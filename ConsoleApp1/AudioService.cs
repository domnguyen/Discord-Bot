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
    /**
     * AudioService
     * Class that handles a single audio service.
     */
    public class AudioService
    {
        // Concurrent dictionary for multithreaded environments.
        private readonly ConcurrentDictionary<ulong, IAudioClient> m_ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        private bool m_DelayJoin = false; // Temporary Semaphore to control leaving and joining too quickly.

        // Private variables.
        private Process m_Process; // Process that runs when playing.
        private Stream m_Stream; // Stream output when playing.
        private bool m_IsPlaying = false; // Flag to change to play or pause the audio.
        private float m_Volume = 1.0f; // Volume value that's checked during playback. Reference: PlayAudioAsync.

        // Playlist. TODO: Move to separate class later.
        private readonly ConcurrentQueue<AudioFile> m_Playlist = new ConcurrentQueue<AudioFile>();
        private bool m_AutoPlay = false;

        /**
         *  JoinAudio
         *  Join the voice channel of the target.
         *  Adds a new client to the ConcurrentDictionary.
         *  @param guild
         *  @param target
         */
        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            // Delayed join if the client recently left a voice channel.
            if (m_DelayJoin)
            {
                Console.WriteLine("The client is currently disconnecting from a voice channel. Please try again later.");
                return;
            }

            // Try to get the current audio client. If it's already there, we're already joined.
            if (m_ConnectedChannels.TryGetValue(guild.Id, out var connectedAudioClient))
            {
                Console.WriteLine("The current voice channel is already connected.");
                return;
            }

            // If the target guild id doesn't match the guild id we want, return.
            if (target.Guild.Id != guild.Id)
            {
                Console.WriteLine("Are you sure the current voice channel is correct?");
                return;
            }

            // Attempt to connect to this audio channel.
            var audioClient = await target.ConnectAsync();

            // Add it to the dictionary of connected channels.
            if (m_ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                Console.WriteLine("Connected to the voice channel specified.");
                return;
            }

            // If we can't add it to the dictionary or connecting didn't work properly, error.
            Console.WriteLine("Unable to join the channel specified.");
        }

        /**
         *  LeaveAudio
         *  Leave the current voice channel.
         *  Removes the client from the ConcurrentDictionary.
         *  @param guild
         */
        public async Task LeaveAudio(IGuild guild)
        {
            // Attempt to remove from the current dictionary, and if removed, stop it.
            if (m_ConnectedChannels.TryRemove(guild.Id, out var audioClient))
            {
                await audioClient.StopAsync();
                Console.WriteLine("Left the voice channel.");

                m_DelayJoin = true; // Lock.
                await Task.Delay(10000); // Delay to prevent error condition. TEMPORARY.
                m_DelayJoin = false; // Unlock.

                return;
            }

            // If we can't remove it from the dictionary, error.
            Console.WriteLine("Unable to leave the voice channel. Are you sure that it is currently connected?");
        }

        /**
         *  GetDelayJoin
         *  Gets m_DelayJoin, this is a temporary semaphore to prevent joining too quickly after leaving a channel.
         */
        public bool GetDelayJoin()
        {
            if (m_DelayJoin) Console.WriteLine("The client is currently delayed."); // Debug line when blocked.
            return m_DelayJoin;
        }

        /**
        *  ExtractPathAsync
        *  Extracts from the path and fills an AudioFile with metadata information about the audio source.
        *  @param path      string of the source path
        */
        public async Task<AudioFile> ExtractPathAsync(string path)
        {
            return await AudioService.ExtractAsync(path);
        }

        /**
         *  CreateLocalStream
         *  Creates a local stream using the file path specified and ffmpeg to stream it directly.
         *  The format Discord takes is 16-bit 48000Hz PCM
         *  @param path     string of the source path (local)
         */
        private Process CreateLocalStream(string path)
        {
            Console.WriteLine("Creating Local Stream : " + path);
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
         *  @param path     string of the source path (network)
         */
        private Process CreateNetworkStream(string path)
        { // TODO: Configure this to handle errors as well. A lot of links cannot be opened for some reason.
            Console.WriteLine("Creating Network Stream : " + path);
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
         *  @param guild
         *  @param channel
         *  @param path
         *  
         *  TODO: Parse for youtube downloader and ffmpeg for different strings.
         *  TODO: Have a way to check if the file has been downloaded and play a local version instead.
         *  TODO: Consider killing autoplay
         */
        public async Task ForcePlayAudioAsync(IGuild guild, IMessageChannel channel, AudioFile song)
        {
            // If there was an error with extracting the path, return.
            if (song == null)
            {
                await channel.SendMessageAsync("Unable to open.");
                return;
            }

            // Stop the current audio source if one is already running, then give it time to finish it's process.
            if (m_Process != null && m_Process.IsRunning())
            {
                Console.WriteLine("Another audio source is currently playing.");
                StopAudio();
                while (m_IsPlaying) await Task.Delay(1000); // Important!! The last statement of the previous process.
            }

            // Start the stream, this is the main part of 'play'
            // Moved to a separate function.
            if (m_ConnectedChannels.TryGetValue(guild.Id, out var audioClient))
            {
                // TODO: Write it so that we find it by playlist first.
                await AudioPlaybackAsync(audioClient, song);
                return;
            }

            // If we can't get it from the dictionary, we're probably not connected to it yet.
            Console.WriteLine("Unable to play in the proper channel. Make sure the audio client is connected.");
        }

        /**
         *  AutoPlayAudioAsync
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
                Console.WriteLine("Unable to play in the proper channel. Make sure the audio client is connected.");
                break;
            }
            m_AutoPlay = false; // Finish autoplay service.
        }

        /**
         *  AudioPlaybackAsync
         *  Async function that handles the playback of the audio. This function is technically blocking in it's for loop.
         *  It can be broken by cancelling m_Process or when it reads to the end of the file. 
         *  At the start, m_Process, m_Stream, amd m_IsPlaying is flushed.
         *  While it is playing, these will hold values of the current playback audio. It will depend on m_Volume for the volume.
         *  In the end, the three are flushed again.
         *  @param client
         *  @param song
         */
        private async Task AudioPlaybackAsync(IAudioClient client, AudioFile song)
        {
            // Clear out any old values from class variables (Flush).
            m_Process = null;
            m_Stream = null;
            m_IsPlaying = false;

            bool isNetwork = AudioService.VerifyNetworkPath(song.FileName); // Check if network path. This will change if it's an audiofile.

            // Start a new process and create an output stream. Decide between network or local.
            m_Process = isNetwork ? CreateNetworkStream(song.FileName) : CreateLocalStream(song.FileName);
            m_Stream = client.CreatePCMStream(AudioApplication.Music);
            m_IsPlaying = true; // Set this to true to start the loop properly.

            await Task.Delay(5000); // We should wait for ffmpeg to buffer some of the audio first.

            Console.WriteLine("Now playing from : " + song.FileName);

            // While true, we stream the audio in chunks.
            while (true)
            {
                // If the process is already over, we're finished.
                if (m_Process.HasExited)
                    break;

                while (!m_IsPlaying) await Task.Delay(1000); // We pause within this function while it's 'not playing'.

                // Read the stream in chunks.
                int blockSize = 3840;
                byte[] buffer = new byte[blockSize];
                int byteCount;
                byteCount = await m_Process.StandardOutput.BaseStream.ReadAsync(buffer, 0, blockSize);

                // If the stream cannot be read or we reach the end of the file, we're finished.
                if (byteCount == 0)
                    break;

                // Write out to the stream. Relies on m_Volume to adjust bytes accordingly.
                await m_Stream.WriteAsync(WhaleHelp.ScaleVolumeSafeAllocateBuffers(buffer, m_Volume), 0, byteCount);
            }
            await m_Stream.FlushAsync();
            await Task.Delay(500);

            // Delete if it's still in the directory.
            if (isNetwork && File.Exists(song.FileName))
                File.Delete(song.FileName);

            // Reset values. Basically clearing out values again (Flush).
            m_Process = null;
            m_Stream = null;
            m_IsPlaying = false; // We make sure this is last so we can exit properly.
        }

        /**
         *  PauseAudio
         *  Stops the stream if it's playing. This affects the current AudioPlaybackAsync.
         *  Uses m_Process and m_IsPlaying.
         */
        public void PauseAudio()
        {
            if (m_Process == null)
            {
                Console.WriteLine("There's no audio currently playing.");
                return;
            }
            if (m_IsPlaying) m_IsPlaying = false;
            Console.WriteLine("Pausing voice.");
        }

        /**
         *  ResumeAudio
         *  Stops the stream if it's playing. This affects the current AudioPlaybackAsync.
         *  Uses m_Process and m_IsPlaying.
         */
        public void ResumeAudio()
        {
            if (m_Process == null)
            {
                Console.WriteLine("There's no audio currently playing.");
                return;
            }
            if (!m_IsPlaying) m_IsPlaying = true;
            Console.WriteLine("Resuming voice.");
        }

        /**
         *  StopAudio
         *  Stops the stream if it's playing. This affects the current AudioPlaybackAsync.
         *  Uses m_Process and m_IsPlaying.
         */
        public void StopAudio()
        {
            if (m_Process == null)
            {
                Console.WriteLine("There's no audio currently playing.");
                return;
            }
            m_Process.Kill(); // This basically stops the current loop by exiting the process.
            if (m_IsPlaying) m_IsPlaying = false; // Sets playing to false.
            Console.WriteLine("Stopping voice.");
            m_AutoPlay = false; // Stop autplay service as well to prevent reloading.
        }

        /**
         *  AdjustVolume
         *  Adjusts the current volume to the value passed. This affects the current AudioPlaybackAsync.
         *  @param volume   A value from 0.0f - 1.0f.
         */
        public void AdjustVolume(float volume)
        {
            // Adjust bounds
            if (volume < 0.0f)
                volume = 0.0f;
            else if (volume > 1.0f)
                volume = 1.0f;
            
            m_Volume = volume; // Update the volume
            Console.WriteLine("Adjusting volume : " + volume);
        }

        /**
         *  PlaylistAdd
         *  Adds a song to the playlist.
         *  @param path   
         */
        public async Task PlaylistAdd(string path)
        {
            AudioFile audio = await AudioService.ExtractAsync(path);
            m_Playlist.Enqueue(audio);
            Console.WriteLine("Added to playlist : " + path);
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
            Console.WriteLine("Couldn't get the next song. It may not be ready yet or we're at the end.");
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
        *  SetAutoPlay
        *  Sets autplay to enable. Returns if the autoplay service should be started or not.
        */
        public bool SetAutoPlay(bool enable)
        {
            if (m_AutoPlay && enable)
                Console.WriteLine("Autoplay service is already started");

            m_AutoPlay = enable;
            Console.WriteLine("Setting autoplay : " + enable);

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
        *  VerifyNetworkPath
        *  Verifies that the path is a network path and not a local path. Checks here before extracting.
        *  Add more arguments here, but we'll just check based on http and assume a network link.
        *  @param path     The path to the file.
        */
        public static bool VerifyNetworkPath(string path)
        {
            return path.StartsWith("http");
        }

        /**
        *  Extract
        *  Extracts data from the current path, by finding it locally or on the network.
        *  Puts all the information into an AudioFile and returns it.
        *  Returns null if it can't be extracted through it's path.
        *  @param path   string of the source path
        */
        public static async Task<AudioFile> ExtractAsync(string path)
        {
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
            new Thread(() => {

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
                    UseShellExecute = false     //Linux?
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
            if (result == null)
                throw new Exception("youtube-dl.exe failed to extract the data!");
            return result;
        }

    }
}
