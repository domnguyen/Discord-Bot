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
        IAudioClient m_Client;
        ConcurrentDictionary<ulong, IAudioClient> m_ConnectedChannels;

        Process m_Process;
        Stream m_Stream;
        float m_Volume = 1.0f;

        public void Init(IAudioClient client, ConcurrentDictionary<ulong, IAudioClient> connectedChannels)
        {
            m_Client = client;
            m_ConnectedChannels = connectedChannels;
        }

        /**
         *  JoinAudio
         *  Join the voice channel of the target.
         *  @param guild
         *  @param target
         */
        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            // Get the current Audio Client and connected channels.
            IAudioClient client = GetAudioClient();
            ConcurrentDictionary<ulong, IAudioClient> connectedChannels = GetConnectedChannels();

            if (connectedChannels.TryGetValue(guild.Id, out client))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            // Join voice channel.
            if (connectedChannels.TryAdd(guild.Id, audioClient))
            {
                // await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
            }
        }

        /**
         *  LeaveAudio
         *  Leave the current voice channel.
         *  @param guild
         */
        public async Task LeaveAudio(IGuild guild)
        {
            // Get the current Audio Client and connected channels.
            IAudioClient client = GetAudioClient();
            ConcurrentDictionary<ulong, IAudioClient> connectedChannels = GetConnectedChannels();

            if (connectedChannels.TryRemove(guild.Id, out client))
            {
                await client.StopAsync();
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
            // Get the current Audio Client and connected channels.
            IAudioClient client = GetAudioClient();
            ConcurrentDictionary<ulong, IAudioClient> connectedChannels = GetConnectedChannels();

            bool isNetwork = VerifyNetworkPath(path); // Check if network path.

            // Check if network or local path, if local file doesn't exist, return.
            if (!isNetwork && !File.Exists(path))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }

            if (connectedChannels.TryGetValue(guild.Id, out client))
            {
                // Start a new process and create an output stream.
                m_Process = isNetwork ? CreateNetworkStream(path) : CreateLocalStream(path);
                m_Stream = client.CreatePCMStream(AudioApplication.Music);

                await Task.Delay(4000); // We should wait for ffmpeg to buffer some of the audio first.

                // While true, we stream the audio in chunks.
                while (true)
                {
                    // If the process is already over, we're finished.
                    if (m_Process.HasExited)
                        break;

                    // Read the stream in chunks.
                    int blockSize = 2880;
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
                m_Process = null;
                m_Stream = null;
            }
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

        /* Get the current audio client. TODO: Replace once global is out!! */
        private IAudioClient GetAudioClient()
        {
            if (m_Client != null) return m_Client;
            return MyGlobals.BotAudioClient;
        }

        /* Get the current connected channels dictionary. TODO: Replace once global is out!! */
        private ConcurrentDictionary<ulong, IAudioClient> GetConnectedChannels()
        {
            if (m_ConnectedChannels != null) return m_ConnectedChannels;
            return MyGlobals.ConnectedChannels;
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
