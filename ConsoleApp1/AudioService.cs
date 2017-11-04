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


namespace WhalesFargo
{
    /**
     * AudioService
     * Class that handles a single audio service.
     */
    public class AudioService
    {
        // This makes the whole thing work, still figuring it out.5
        private readonly ConcurrentDictionary<ulong, IAudioClient> m_ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        private IAudioClient m_Client;

        // Private variables.
        private Process m_Process; // Process that runs when playing.
        private Stream m_Stream; // Steam output when playing.

        private bool m_IsPlaying = false;
        private float m_Volume = 1.0f;

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

            // Check if network or local path, if local file doesn't exist, return.
            if (!isNetwork && !File.Exists(path))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }

            if (m_ConnectedChannels.TryGetValue(guild.Id, out m_Client))
            {
                // Start a new process and create an output stream.
                m_Process = isNetwork ? CreateNetworkStream(path) : CreateLocalStream(path);
                m_Stream = m_Client.CreatePCMStream(AudioApplication.Music);

                await Task.Delay(4000); // We should wait for ffmpeg to buffer some of the audio first.

                m_IsPlaying = true;

                // While true, we stream the audio in chunks.
                while (true)
                {
                    // If the process is already over, we're finished.
                    if (m_Process.HasExited)
                        break;

                    while (!m_IsPlaying) await Task.Delay(2000);

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
        {
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
