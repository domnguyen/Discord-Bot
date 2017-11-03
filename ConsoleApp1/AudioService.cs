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
        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
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
                using (var output = isNetwork ? CreateNetworkStream(path).StandardOutput.BaseStream : CreateLocalStream(path).StandardOutput.BaseStream)
                using (var stream = client.CreatePCMStream(AudioApplication.Music))
                {
                    try     { await output.CopyToAsync(stream); }
                    finally { await stream.FlushAsync(); }
                }
            }
        }

        /* Get the current audio client. TODO: Replace once global is out!! */
        private IAudioClient GetAudioClient()
        {
            return MyGlobals.BotAudioClient;
        }

        /* Get the current connected channels dictionary. TODO: Replace once global is out!! */
        private ConcurrentDictionary<ulong, IAudioClient> GetConnectedChannels()
        {
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
