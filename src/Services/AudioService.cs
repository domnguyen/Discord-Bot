//#define DEBUG_VERBOSE // Use this to print out all log messages to console. Comment out to disable.

using Discord;
using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WhalesFargo
{
    // Enum to direct the string to output. Reference Log()
    public enum E_LogOutput { Console, Reply, Playing };

    /**
     * AudioService
     * Class that handles a single audio service.
     */
    public class AudioService
    {
        // We have a reference to the parent module to perform actions like replying and setting the current game properly.
        private AudioModule m_ParentModule = null;

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
        private bool m_DelayJoin = false;           // Temporary Semaphore to control leaving and joining too quickly.
        private bool m_AutoPlay = false;            // Flag to check if autoplay is currently on or not.
        private bool m_AutoDownload = true;         // Flag to auto download network items.

        private int m_BLOCK_SIZE = 3840;            // Custom block size for playback, in bytes.

        /**
         *  SetParentModule
         *  Sets the parent module when we start the client in AudioModule.
         *  This should always be called in the module constructor to 
         *  provide a direct reference to the parent module.
         *  
         *  @param parent - Parent AudioModule    
         */
        public void SetParentModule(AudioModule parent) { m_ParentModule = parent; }

        /**
         *  DiscordReply
         *  Replies in the text channel using the parent module.
         *  
         *  @param s - Message to reply in the channel
         */
        private async void DiscordReply(string s)
        {
            if (m_ParentModule == null) return;
            await m_ParentModule.ServiceReplyAsync(s);
        }

        /**
         *  DiscordPlaying
         *  Sets the playing string using the parent module.
         *  
         *  @param s - Message to set the playing message to.
         */
        private async void DiscordPlaying(string s)
        {
            if (m_ParentModule == null) return;
            await m_ParentModule.ServicePlayingAsync(s);
        }

        /**
         *  Log
         *  A Custom logger which can send messages to 
         *  console, reply in module, or set to playing.
         *  By default, we log everything to the console.
         *  TODO: Write so that it's an ENUM, where we can use | or &.
         *  
         *  @param s - Message to log
         *  @param output - Output source
         */
        private void Log(string s, int output = (int)E_LogOutput.Console)
        {
#if (DEBUG_VERBOSE)
            Console.WriteLine("AudioService [DEBUG] -- " + s);
#endif
            if (output == (int)E_LogOutput.Console) Console.WriteLine("AudioService -- " + s);
            if (output == (int)E_LogOutput.Reply) DiscordReply(s);
            if (output == (int)E_LogOutput.Playing) DiscordPlaying(s);
        }

        /**
         * DelayAction
         * Using the flag, we pass in a function to lock in between the semaphore. Added for better practice.
         * 
         * @param f - Function called in between the semaphore.
         */
        private async Task DelayAction(Action f)
        {
            m_DelayJoin = true; // Lock.
            f();
            await Task.Delay(10000); // Delay to prevent error condition. TEMPORARY.
            m_DelayJoin = false; // Unlock.
        }

        /**
         *  JoinAudio
         *  Join the voice channel of the target.
         *  Adds a new client to the ConcurrentDictionary.
         *  
         *  @param guild
         *  @param target
         */
        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            // Delayed join if the client recently left a voice channel.
            if (m_DelayJoin)
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
        public async Task LeaveAudio(IGuild guild)
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
         *  GetDelayJoin
         *  Gets m_DelayJoin, this is a temporary semaphore to prevent joining too quickly after leaving a channel.
         */
        public bool GetDelayJoin()
        {
            if (m_DelayJoin)
                Log("The client is currently disconnecting from a voice channel. Please try again later.");
            return m_DelayJoin;
        }

        /**
         *  ExtractPathAsync
         *  Extracts from the path and fills an AudioFile with metadata information about the audio source.
         *  
         *  @param path - string of the source path
         */
        public async Task<AudioFile> ExtractPathAsync(string path)
        {
            return await ExtractAsync(path);
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
            Log("Creating Local Stream : " + path);
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
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
            Log("Creating Network Stream : " + path);
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
            // If there was an error with extracting the path, return.
            if (song == null)
            {
                Log("Cannot play the audio source specified : " + song);
                return;
            }

            // Stop the current audio source if one is already running, then give it time to finish it's process.
            if (m_Process != null && m_Process.IsRunning())
            {
                Log("Another audio source is currently playing.");
                StopAudio();
                while (m_IsPlaying) await Task.Delay(1000); // Important!! The last statement of the previous process.
            }

            // Start the stream, this is the main part of 'play'
            // Moved to a separate function called AudioPlaybackAsync()
            if (m_ConnectedChannels.TryGetValue(guild.Id, out var audioClient))
            {
                await AudioPlaybackAsync(audioClient, song);
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
            //m_AutoPlay = false; // TODO : Set an option for this. Right now, autoplay stays in autoplay.
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

            bool isNetwork = VerifyNetworkPath(song.FileName); // Check if network path. This will change if it's an audiofile.

            // Start a new process and create an output stream. Decide between network or local.
            // TODO: Check if a network file is downloaded, to use local version.
            m_Process = isNetwork ? CreateNetworkStream(song.FileName) : CreateLocalStream(song.FileName);
            m_Stream = client.CreatePCMStream(AudioApplication.Music);
            m_IsPlaying = true; // Set this to true to start the loop properly.

            await Task.Delay(5000); // We should wait for ffmpeg to buffer some of the audio first.

            Log("Now Playing: " + song.Title, (int)E_LogOutput.Reply); // Reply in the text channel.
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
            await m_Stream.FlushAsync();
            await Task.Delay(500);

            // Delete if it's still in the directory. Only if it's a downloaded network file.
            if (song.IsDownloaded && File.Exists(song.FileName))
                File.Delete(song.FileName);

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
            m_Process.Kill(); // This basically stops the current loop by exiting the process.
            //if (m_IsPlaying) m_IsPlaying = false; // Sets playing to false.
            if (m_AutoPlay) m_AutoPlay = false; // Stop autoplay service as well to prevent reloading.
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
         *  SetAutoPlay
         *  Sets autplay to enable. Returns if the autoplay service should be started or not.
         *  
         *  @param enable - bool toggle for autoplay.
         */
        public bool SetAutoPlay(bool enable)
        {
            m_AutoPlay = enable;
            Log("Setting autoplay : " + enable);

            if (!m_IsPlaying && enable) return true; // Only return true if nothing is playing
            return false;
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
        public async Task PlaylistAdd(string path)
        {
            AudioFile audio = await ExtractAsync(path);
            if (audio != null)
            {
                m_Playlist.Enqueue(audio); // Only add if there's no errors.
                Log("Added to playlist : " + audio.Title, (int)E_LogOutput.Reply);

                // If the downloader is set to true, we start the autodownload helper.
                if (m_AutoDownload)
                {
                    if (audio.IsNetwork) await m_AudioDownloader.AddAsync(audio); // Auto download while in playlist.
                    if (!m_AudioDownloader.IsRunning()) await m_AudioDownloader.StartDownloadAsync(); // Start the downloader if it's off.
                }
            }
        }

        /**
         *  PlaylistNext
         *  Gets the next song in the queue.
         */
        private AudioFile PlaylistNext()
        {
            AudioFile nextSong = null;
            if (m_Playlist.TryDequeue(out nextSong))
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
         * ScaleVolumeSafeAllocateBuffers
         * Adjusts the byte array by the volume, scaling it by a factor [0.0f,1.0f]
         * 
         * @param audioSamples - The source audio sample from the ffmpeg stream
         * @param volume - The volume to adjust to, ranges [0.0f,1.0f]
         */
        private byte[] ScaleVolumeSafeAllocateBuffers(byte[] audioSamples, float volume)
        {
            if (audioSamples == null) return null;
            if (audioSamples.Length == 0) return null;
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

        /**
        *  VerifyNetworkPath
        *  Verifies that the path is a network path and not a local path. Checks here before extracting.
        *  Add more arguments here, but we'll just check based on http and assume a network link.
        *  
        *  @param path - The path to the file
        */
        private bool VerifyNetworkPath(string path)
        {
            return path.StartsWith("http");
        }

        /**
        *  ExtractAsync
        *  Extracts data from the current path, by finding it locally or on the network.
        *  Puts all the information into an AudioFile and returns it.
        *  Returns null if it can't be extracted through it's path.
        *  
        *  @param path - string of the source path
        */
        private async Task<AudioFile> ExtractAsync(string path)
        {
            Log("Extracting Meta Data for : " + path);

            TaskCompletionSource<AudioFile> taskSrc = new TaskCompletionSource<AudioFile>();
            bool verifyNetwork = VerifyNetworkPath(path);

            // Local file.
            if (!verifyNetwork)
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine("File does not exist.");
                    return null;
                }
                // stream data
                AudioFile StreamData = new AudioFile();
                StreamData.FileName = path;
                StreamData.Title = path.Split('/').Last();
                if (StreamData.Title.CompareTo("") == 0) StreamData.Title = path;
                StreamData.IsNetwork = verifyNetwork;
                return StreamData;
            }

            // Network file.
            new Thread(() =>
            {
                // Stream data
                AudioFile StreamData = new AudioFile();

                // youtube-dl.exe
                Process youtubedl;

                // Get Video Title
                ProcessStartInfo youtubedlMetaData = new ProcessStartInfo()
                {
                    FileName = "youtube-dl",
                    Arguments = $"-s -e {path}",// Add more flags for more options.
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                youtubedl = Process.Start(youtubedlMetaData);
                youtubedl.WaitForExit();

                // Read the output of the simulation
                string[] output = youtubedl.StandardOutput.ReadToEnd().Split('\n');

                // Set the file name.
                StreamData.FileName = path;

                // Extract each line printed for it's corresponding data.
                if (output.Length > 0)
                    StreamData.Title = output[0];

                // Set other properties as follows.
                StreamData.IsNetwork = verifyNetwork;

                taskSrc.SetResult(StreamData);
            }).Start();

            AudioFile result = await taskSrc.Task;
            if (result == null) // TODO: We might not need to throw an exception.
                throw new Exception("youtube-dl.exe failed to extract the data!");

            return result;
        }

    }
}
