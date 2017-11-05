using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using System.Threading;

namespace WhalesFargo
{
    /**
    * Audio File
    * Class that holds properties from the audio file.
    * Add more when necessary, but the only thing we're using for it now is the title field.
    */
    public class AudioFile
    {
        private string m_FileName;
        private string m_Title;
        private string m_Author;
        private bool m_IsNetwork;

        public AudioFile()
        {
            m_FileName = "";
            m_Title = "";
            m_Author = "";
            m_IsNetwork = true;
        }
        public string FileName
        {
            get { return m_FileName; }
            set { m_FileName = value; }
        }

        public string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }
        public string Author
        {
            get { return m_Author; }
            set { m_Author = value; }
        }
        public bool IsNetwork
        {
            get { return m_IsNetwork; }
            set { m_IsNetwork = value; }
        }
    }

    /**
     * AudioService
     * Class that handles a single audio service.
     */
    public class AudioService
    {
        // This makes the whole thing work, still figuring it out.
        private readonly ConcurrentDictionary<ulong, IAudioClient> m_ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        private IAudioClient m_Client; // The main audio client.

        // Private variables.
        private Process m_Process; // Process that runs when playing.
        private Stream m_Stream; // Stream output when playing.

        private bool m_IsPlaying = false; // Flag to change to play or pause the audio.
        private float m_Volume = 1.0f; // Volume value that's checked during playback. Reference: PlayAudioAsync.

        /**
         *  JoinAudio
         *  Join the voice channel of the target.
         *  @param guild
         *  @param target
         */
        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            if (m_ConnectedChannels.TryGetValue(guild.Id, out m_Client))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            // Join voice channel.
            if (m_ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                return;
            }

            Console.WriteLine("Unable to join channel.");
        }

        /**
         *  LeaveAudio
         *  Leave the current voice channel.
         *  @param guild
         */
        public async Task LeaveAudio(IGuild guild)
        {
            if (m_ConnectedChannels.TryRemove(guild.Id, out m_Client))
            {
                await m_Client.StopAsync();
            }
        }

        /**
         *  SendAudioAsync
         *  Play the current audio by string in the voice channel of the target.
         *  Right now, playing by local file name.
         *  TODO: Parse for youtube downloader and ffmpeg for different strings.
         *  @param guild
         *  @param channel
         *  @param path
         */
        public async Task PlayAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {
            bool isNetwork = VerifyNetworkPath(path); // Check if network path.

            // Stop the current audio source if one is already running, then give it time to finish it's process.
            if (m_Process != null && m_Process.IsRunning())
            {
                Console.WriteLine("Another audio source is currently playing.");
                StopAudio();
                while (m_Process != null) await Task.Delay(1000); // We'll wait until it's null again.
            }

            // Check if network or local path, if local file doesn't exist, return.
            if (!isNetwork && !File.Exists(path))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }

            // Start the stream, this is the main part of 'play'
            if (m_ConnectedChannels.TryGetValue(guild.Id, out m_Client))
            {
                // Start a new process and create an output stream.
                m_Process = isNetwork ? CreateNetworkStream(path) : CreateLocalStream(path);
                m_Stream = m_Client.CreatePCMStream(AudioApplication.Music);

                await Task.Delay(5000); // We should wait for ffmpeg to buffer some of the audio first.

                m_IsPlaying = true; // Set this to true to start the loop properly.

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

                    // Write out to the stream.
                    await m_Stream.WriteAsync(WhaleHelp.ScaleVolumeSafeAllocateBuffers(buffer, m_Volume), 0, byteCount);
                }
                await m_Stream.FlushAsync();

                // Reset values.
                await Task.Delay(500);
                if (isNetwork && File.Exists(path))
                    File.Delete(path);
                m_Process = null;
                m_Stream = null;
                m_IsPlaying = false;
            }
        }

        /**
         *  PauseAudio
         *  Stops the stream if it's playing.
         */
        public void PauseAudio()
        {
            if (m_Process == null)
            {
                Console.WriteLine("There's no audio currently playing.");
                return;
            }

            if (m_IsPlaying)
                m_IsPlaying = false;
        }

        /**
         *  ResumeAudio
         *  Stops the stream if it's playing.
         */
        public void ResumeAudio()
        {
            if (m_Process == null)
            {
                Console.WriteLine("There's no audio currently playing.");
                return;
            }

            if (!m_IsPlaying)
                m_IsPlaying = true;
        }

        /**
         *  StopAudio
         *  Stops the stream if it's playing.
         */
        public void StopAudio()
        {
            if (m_Process == null)
            {
                Console.WriteLine("There's no audio currently playing.");
                return;
            }

            m_Process.Kill();
        }

        /**
         *  AdjustVolume
         *  Adjusts the current volume to the value passed.
         *  @param volume   A value from 0.0f - 1.0f.
         */
        public void AdjustVolume(float volume)
        {
            // Adjust bounds
            if (volume < 0.0f)
                volume = 0.0f;
            else if (volume > 1.0f)
                volume = 1.0f;

            // Update the volume
            m_Volume = volume;
        }

        /**
         *  GetStreamData
         *  Opens the stream data and fills an AudioFile with metadata information about the audio source.
         *  @param url   string of the source path
         */
        public async Task<AudioFile> GetStreamData(string path)
        {
            TaskCompletionSource<AudioFile> taskSrc = new TaskCompletionSource<AudioFile>();

            bool IsNetwork = VerifyNetworkPath(path);

            // Local file.
            if (!IsNetwork)
            {
                // stream data
                AudioFile StreamData = new AudioFile();
                StreamData.FileName = path;
                StreamData.Title = path.Split('/').Last();
                if (StreamData.Title.CompareTo("") == 0) StreamData.Title = path;
                return StreamData;
            }

            // Network file.
            new Thread(() => {

                // stream data
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

                taskSrc.SetResult(StreamData);
            }).Start();

            AudioFile result = await taskSrc.Task;
            if (result == null)
                throw new Exception("youtube-dl.exe failed to extract the data!");
            return result;
        }

        /* Add more arguments here, but we'll just check based on http and assume a network link. */
        private bool VerifyNetworkPath(string path)
        {
            return path.StartsWith("http");
        }

        /* Create a local stream. */
        private Process CreateLocalStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }

        /* Create a network stream.*/
        private Process CreateNetworkStream(string path)
        { // TODO: Configure this to handle errors as well. A lot of links cannot be opened for some reason.
            return Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe -o - {path} | ffmpeg -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
        }


    }
}
