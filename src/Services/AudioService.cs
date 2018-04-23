using Discord;
using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WhalesFargo.Helpers;

namespace WhalesFargo.Services
{
    /**
     * AudioService
     * Class that handles a single audio service.
     */
    public class AudioService : CustomService
    {
        // Concurrent dictionary for multithreaded environments.
        private readonly ConcurrentDictionary<ulong, IAudioClient> m_ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        // Playlist.
        private readonly ConcurrentQueue<AudioFile> m_Playlist = new ConcurrentQueue<AudioFile>();

        // Downloader.
        private readonly AudioDownloader m_AudioDownloader = new AudioDownloader(); // Only downloaded on playlist add.

        // Private variables.
        private Process m_Process = null;           // Process that runs when playing.
        private Stream m_Stream = null;             // Stream output when playing.
        private bool m_IsPlaying = false;           // Flag to change to play or pause the audio.
        private float m_Volume = 1.0f;              // Volume value that's checked during playback. Reference: PlayAudioAsync.
        private bool m_DelayAction = false;         // Temporary Semaphore to control leaving and joining too quickly.
        private bool m_AutoPlay = false;            // Flag to check if autoplay is currently on or not.

        private bool m_AutoDownload = true;         // Flag to auto download network items in the playlist.
        private bool m_AutoDelete = false;          // Flag to delete downloaded network items when stopped.
        private bool m_AllowNetwork = false;         // Flag to allow network streaming capability.
        private bool m_AutoStop = false;

        private int m_BLOCK_SIZE = 3840;            // Custom block size for playback, in bytes.

        /**
         * DelayAction
         * Using the flag, we pass in a function to lock in between the semaphore. Added for better practice.
         * 
         * @param f - Function called in between the semaphore.
         */
        private async Task DelayAction(Action f)
        {
            m_DelayAction = true; // Lock.
            f();
            await Task.Delay(10000); // Delay to prevent error condition. TEMPORARY.
            m_DelayAction = false; // Unlock.
        }

        /**
         *  GetDelayAction
         *  Gets m_DelayAction, this is a temporary semaphore to prevent joining too quickly after leaving a channel.
         */
        public bool GetDelayAction()
        {
            if (m_DelayAction)
                Log("The client is currently disconnecting from a voice channel. Please try again later.");
            return m_DelayAction;
        }

        /**
         *  JoinAudio
         *  Join the voice channel of the target.
         *  Adds a new client to the ConcurrentDictionary.
         *  
         *  @param guild
         *  @param target
         */
        public async Task JoinAudioAsync(IGuild guild, IVoiceChannel target)
        {
            // Delayed join if the client recently left a voice channel.
            if (m_DelayAction)
            {
                Log("The client is currently disconnecting from a voice channel. Please try again later.");
                return;
            }

            // Try to get the current audio client. If it's already there, we're already joined.
            if (m_ConnectedChannels.TryGetValue(guild.Id, out var connectedAudioClient))
            {
                Log("The client is already connected to the current voice channel.");
                return;
            }

            // If the target guild id doesn't match the guild id we want, return.
            if (target.Guild.Id != guild.Id)
            {
                Log("Are you sure the current voice channel is correct?");
                return;
            }

            // Attempt to connect to this audio channel.
            var audioClient = await target.ConnectAsync();

            // Add it to the dictionary of connected channels.
            if (m_ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                Log("The client is now connected to the current voice channel.");
                return;
            }

            // If we can't add it to the dictionary or connecting didn't work properly, error.
            Log("Unable to join the current voice channel.");
        }

        /**
         *  LeaveAudio
         *  Leave the current voice channel.
         *  Removes the client from the ConcurrentDictionary.
         *  
         *  @param guild
         */
        public async Task LeaveAudioAsync(IGuild guild)
        {
            // To avoid any issues, we stop the player before leaving the channel.
            if (m_IsPlaying)
            {
                StopAudio();

                // Wait for it to be stopped.
                bool stopped = false;
                await DelayAction(() => stopped = true);
                while (!stopped)
                {
                    if (!m_IsPlaying) stopped = true;
                }
            }

            // Attempt to remove from the current dictionary, and if removed, stop it.
            if (m_ConnectedChannels.TryRemove(guild.Id, out var audioClient))
            {
                //await audioClient.StopAsync();
                Log("The client is now disconnected from the current voice channel.");
                await DelayAction(() => audioClient.StopAsync()); // Can change once this error is resolved.
                return;
            }

            // If we can't remove it from the dictionary, error.
            Log("Unable to disconnect from the current voice channel. Are you sure that it is currently connected?");
        }

        /**
         *  ExtractPathAsync
         *  Extracts from the path and fills an AudioFile with metadata information about the audio source.
         *  
         *  @param path - string of the source path
         */
        public async Task<AudioFile> ExtractPathAsync(string path)
        {
            try // We put this in a try catch block.
            {
                return await m_AudioDownloader.GetAudioFileInfo(path);
            }
            catch
            {
                return null;
            }
        }

        /**
         *  CreateLocalStream
         *  Creates a local stream using the file path specified and ffmpeg to stream it directly.
         *  The format Discord takes is 16-bit 48000Hz PCM
         *  TODO: Catch any errors that happen when creating PCM streams.
         *  
         *  @param path - string of the source path (local)
         */
        private Process CreateLocalStream(string path)
        {
            Log($"Creating Local Stream : {path}");
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
        }

        /**
         *  CreateNetworkStream
         *  Creates a network stream using youtube-dl.exe, then piping it to ffmpeg to stream it directly.
         *  The format Discord takes is 16-bit 48000Hz PCM
         *  TODO: Catch any errors that happen when creating PCM streams.
         *  
         *  @param path - string of the source path (network)
         */
        private Process CreateNetworkStream(string path)
        { // TODO: Configure this to handle errors as well. A lot of links cannot be opened for some reason.
            Log($"Creating Network Stream : {path}");
            return Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe -o - {path} | ffmpeg -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
        }

        /**
         *  ForcePlayAudioAsync
         *  Force Play the current audio by string in the voice channel of the target.
         *  Right now, playing by local file name.
         *  
         *  @param guild
         *  @param channel
         *  @param path
         *  
         *  TODO: Parse for youtube downloader and ffmpeg for different strings.
         *  TODO: Have a way to check if the file has been downloaded and play a local version instead.
         *  TODO: Consider adding it to autoplay list if it is already playing.
         */
        public async Task ForcePlayAudioAsync(IGuild guild, IMessageChannel channel, AudioFile song)
        {
            bool autoplay = m_AutoPlay; // Store the old autoplay option.

            // If there was an error with extracting the path, return.
            if (song == null)
            {
                Log($"Cannot play the audio source specified : {song}");
                return;
            }

            // Stop the current audio source if one is already running, then give it time to finish it's process.
            if (m_Process != null && m_Process.IsRunning())
            {
                Log("Another audio source is currently playing.");
                if (autoplay) Log("Pausing autoplay service.", (int)E_LogOutput.Reply);
                StopAudio();
                while (m_IsPlaying) await Task.Delay(1000); // Important!! The last statement of the previous process.
            }

            // Start the stream, this is the main part of 'play'
            // Moved to a separate function called AudioPlaybackAsync()
            if (m_ConnectedChannels.TryGetValue(guild.Id, out var audioClient))
            {
                await AudioPlaybackAsync(audioClient, song);
                // We update autoplay since it's reset in StopAudio.
                if (m_AutoPlay = autoplay) Log("Resuming autoplay service.", (int)E_LogOutput.Reply); 
                return;
            }

            // If we can't get it from the dictionary, we're probably not connected to it yet.
            Log("Unable to play in the proper channel. Make sure the audio client is connected.");
        }

        /**
         *  AutoPlayAudioAsync
         *  This is for the autoplay function which waits after each playback and pulls from the playlist.
         *  
         *  @param guild
         *  @param channel
         *  
         *  TODO: Parse for youtube downloader and ffmpeg for different strings.
         *  TODO: Have a way to check if the file has been downloaded and play a local version instead.
         */
        public async Task AutoPlayAudioAsync(IGuild guild, IMessageChannel channel)
        {
            while (m_AutoPlay)
            {
                // Wait for any previous songs.
                if (m_Process != null && m_Process.IsRunning())
                {
                    while (m_IsPlaying && m_AutoPlay) // Important!! The last statement of the previous process.
                        await Task.Delay(1000);
                }

                if (!m_AutoPlay) return; // If we turn off autoplay prematurely or in the middle of a song.

                // If there's nothing playing, start the stream, this is the main part of 'play'
                if (m_ConnectedChannels.TryGetValue(guild.Id, out var audioClient))
                {
                    AudioFile song = PlaylistNext(); // If null, nothing in the playlist. We can wait in this loop until there is.
                    if (song != null) await AudioPlaybackAsync(audioClient, song);
                    if (m_Playlist.IsEmpty) break;
                    continue; // Is null or done with playback.
                }
                // If we can't get it from the dictionary, we're probably not connected to it yet.
                Log("Unable to play in the proper channel. Make sure the audio client is connected.");
                break;
            }

            // Stops autoplay once we're done with it.
            if (m_AutoStop)
                m_AutoPlay = false;
        }

        /**
         *  AudioPlaybackAsync
         *  Async function that handles the playback of the audio. This function is technically blocking in it's for loop.
         *  It can be broken by cancelling m_Process or when it reads to the end of the file. 
         *  At the start, m_Process, m_Stream, amd m_IsPlaying is flushed.
         *  While it is playing, these will hold values of the current playback audio. It will depend on m_Volume for the volume.
         *  In the end, the three are flushed again.
         *  
         *  @param client
         *  @param song
         */
        private async Task AudioPlaybackAsync(IAudioClient client, AudioFile song)
        {
            // Clear out any old values from class variables (Flush).
            m_Process = null;
            m_Stream = null;
            m_IsPlaying = false;

            // Check if network path. This will change if it's an audiofile.
            bool? isNetwork = m_AudioDownloader.VerifyNetworkPath(song.FileName);
            if (isNetwork == null)
            {
                Log("Path invalid.");
                return;
            }
            if (isNetwork == true && !m_AllowNetwork)
            {
                Log("Network not allowed.");
                return;
            }

            // Start a new process and create an output stream. Decide between network or local.
            // Check if a network file is downloaded, to use local version. Upon download finished, the downloader sets network to false.
            m_Process = (bool)isNetwork ? CreateNetworkStream(song.FileName) : CreateLocalStream(song.FileName);
            m_Stream = client.CreatePCMStream(AudioApplication.Music);
            m_IsPlaying = true; // Set this to true to start the loop properly.

            await Task.Delay(5000); // We should wait for ffmpeg to buffer some of the audio first.

            Log($"Now Playing: {song.Title}", (int)E_LogOutput.Reply); // Reply in the text channel.
            Log(song.Title, (int)E_LogOutput.Playing); // Set playing.

            // While true, we stream the audio in chunks.
            while (true)
            {
                // If the process is already over, we're finished.
                if (m_Process.HasExited)
                    break;

                while (!m_IsPlaying) await Task.Delay(1000); // We pause within this function while it's 'not playing'.

                // Read the stream in chunks.
                int blockSize = m_BLOCK_SIZE; // Size of bytes to read per frame.
                byte[] buffer = new byte[blockSize];
                int byteCount;
                byteCount = await m_Process.StandardOutput.BaseStream.ReadAsync(buffer, 0, blockSize);

                // If the stream cannot be read or we reach the end of the file, we're finished.
                if (byteCount <= 0)
                    break;

                try
                {
                    // Write out to the stream. Relies on m_Volume to adjust bytes accordingly.
                    await m_Stream.WriteAsync(ScaleVolumeSafeAllocateBuffers(buffer, m_Volume), 0, byteCount);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    break;
                }
            }

            // Flush the stream and wait until it's fully done before continuing.
            m_Stream.FlushAsync().Wait();

            // Delete if it's still in the directory. Only if it's a downloaded network file.
            if (song.IsDownloaded && File.Exists(song.FileName) && m_AutoDelete)
            {
                Log($"Deleted {song.FileName}");
                File.Delete(song.FileName);
            }

            // Reset values. Basically clearing out values again (Flush).
            m_Process = null;
            m_Stream = null;
            m_IsPlaying = false; // We make sure this is last so we can exit properly.
            Log("", (int)E_LogOutput.Playing); // Set playing off.
        }

        /**
         *  PauseAudio
         *  Stops the stream if it's playing. This affects the current AudioPlaybackAsync.
         *  Uses m_Process and m_IsPlaying.
         */
        public void PauseAudio()
        {
            if (m_Process == null) return;
            if (m_IsPlaying) m_IsPlaying = false;
            Log("Pausing the current audio source.");
        }

        /**
         *  ResumeAudio
         *  Stops the stream if it's playing. This affects the current AudioPlaybackAsync.
         *  Uses m_Process and m_IsPlaying.
         */
        public void ResumeAudio()
        {
            if (m_Process == null) return;
            if (!m_IsPlaying) m_IsPlaying = true;
            Log("Resuming the current audio source.");
        }

        /**
         *  StopAudio
         *  Stops the stream if it's playing. This affects the current AudioPlaybackAsync.
         *  Uses m_Process and m_IsPlaying.
         */
        public void StopAudio()
        {
            if (m_Process == null) return;
            if (m_AutoPlay) m_AutoPlay = false; // Stop autoplay service as well to prevent reloading.
            m_Process.Kill(); // This basically stops the current loop by exiting the process.
            Log("Stopping the current audio source.");
        }

        /**
         *  AdjustVolume
         *  Adjusts the current volume to the value passed. This affects the current AudioPlaybackAsync.
         *  
         *  @param volume - A value from 0.0f - 1.0f.
         */
        public void AdjustVolume(float volume)
        {
            // Adjust bounds
            if (volume < 0.0f)
                volume = 0.0f;
            else if (volume > 1.0f)
                volume = 1.0f;

            m_Volume = volume; // Update the volume
            Log("Adjusting volume : " + volume);
        }

        /**
         *  CheckAutoPlayAsync
         *  Checks if autoplay is true, but not started yet. If not started, we start autoplay.
         *  
         *  @param guild
         *  @param channel
         */
        public async Task CheckAutoPlayAsync(IGuild guild, IMessageChannel channel)
        {
            if (m_AutoPlay && !m_IsPlaying)
                await AutoPlayAudioAsync(guild, channel);
        }

        /**
         *  SetAutoPlay
         *  Sets autplay to enable. Returns if the autoplay service should be started or not.
         *  
         *  @param enable - bool toggle for autoplay.
         */
        public void SetAutoPlay(bool enable)
        {
            m_AutoPlay = enable;
            Log($"Setting autoplay : {enable}");
        }

        /**
         *  GetAutoPlay
         */
        public bool GetAutoPlay()
        {
            return m_AutoPlay;
        }

        /**
         *  GetIsPlaying
         */
        public bool GetIsPlaying()
        {
            return m_IsPlaying;
        }

        /**
         *  PrintPlaylist
         *  Returns a string with the playlist information.
         */
        public string PlaylistString()
        {
            int count = m_Playlist.Count;
            if (count == 0) return "There are currently no items in the playlist.";

            // Count the number of total digits.
            int countDigits = (int)(Math.Floor(Math.Log10(count) + 1));

            string playlist = "";
            for (int i = 0; i < count; i++)
            {
                // Prepend 0's so it matches in length.
                string zeros = "";
                int numDigits = (i == 0) ? 1 : (int)(Math.Floor(Math.Log10(i) + 1));
                while (numDigits < countDigits)
                {
                    zeros += "0";
                    ++numDigits;
                }
                // Print out the current audio file string.
                AudioFile current = m_Playlist.ElementAt(i);
                playlist += zeros + i + " : " + current + "\n";
            }

            return playlist;
        }

        /**
         *  PlaylistAdd
         *  Adds a song to the playlist.
         *  
         *  @param path   
         */
        public async Task PlaylistAddAsync(string path)
        {
            AudioFile audio = await m_AudioDownloader.GetAudioFileInfo(path);
            if (audio != null)
            {
                m_Playlist.Enqueue(audio); // Only add if there's no errors.
                Log($"Added to playlist : {audio.Title}", (int)E_LogOutput.Reply);

                // If the downloader is set to true, we start the autodownload helper.
                if (m_AutoDownload)
                {
                    if (audio.IsNetwork) m_AudioDownloader.Add(audio); // Auto download while in playlist.
                    await m_AudioDownloader.StartDownloadAsync(); // Start the downloader if it's off.
                }
            }
        }

        /**
         *  PlaylistNext
         *  Gets the next song in the queue.
         */
        private AudioFile PlaylistNext()
        {
            if (m_Playlist.TryDequeue(out AudioFile nextSong))
                return nextSong;

            if (m_Playlist.Count <= 0) Log("We reached the end of the playlist.");
            else Log("The next song could not be opened.");
            return nextSong;
        }

        /**
         *  PlaylistSkip
         *  Skips the current song playing.
         */
        public void PlaylistSkip()
        {
            if (!m_AutoPlay)
            {
                Console.WriteLine("Autoplay service hasn't been started.");
                return;
            }
            if (m_Process == null)
            {
                Console.WriteLine("There's no audio currently playing.");
                return;
            }
            m_Process.Kill(); // This basically stops the current loop by exiting the process.
        }

        /**
         *  GetLocalSongs
         *  Returns a string with the downloaded songs information.
         */
        public string GetLocalSongs()
        {
            return m_AudioDownloader.GetAllItems();
        }

        /**
          *  GetLocalSongs
          *  Returns a string with the specified song by index.
          */
        public string GetLocalSong(int index)
        {
            return m_AudioDownloader.GetItem(index);
        }

        public async Task RemoveDuplicateSongsAsync()
        {
            m_AudioDownloader.RemoveDuplicateItems();
            await Task.Delay(0);
        }

        /**
         * ScaleVolumeSafeAllocateBuffers
         * Adjusts the byte array by the volume, scaling it by a factor [0.0f,1.0f]
         * 
         * @param audioSamples - The source audio sample from the ffmpeg stream
         * @param volume - The volume to adjust to, ranges [0.0f,1.0f]
         */
        private byte[] ScaleVolumeSafeAllocateBuffers(byte[] audioSamples, float volume)
        {
            if (audioSamples == null) return null;
            if (audioSamples.Length % 2 != 0) return null;
            if (volume < 0.0f || volume > 1.0f) return null;

            // Adjust the output for the volume.
            var output = new byte[audioSamples.Length];
            if (Math.Abs(volume - 1f) < 0.0001f)
            {
                try
                {
                    Buffer.BlockCopy(audioSamples, 0, output, 0, audioSamples.Length);
                    return output;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    return null;
                }
            }

            // 16-bit precision for the multiplication
            int volumeFixed = (int)Math.Round(volume * 65536d);

            for (var i = 0; i < output.Length; i += 2)
            {
                // The cast to short is necessary to get a sign-extending conversion
                int sample = (short)((audioSamples[i + 1] << 8) | audioSamples[i]);
                int processed = (sample * volumeFixed) >> 16;

                output[i] = (byte)processed;
                output[i + 1] = (byte)(processed >> 8);
            }

            return output;
        }


    }
}
