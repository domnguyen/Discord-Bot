using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Audio;
using Discord.WebSocket;

namespace WhalesFargo
{
   
    /**
     * AudioModule
     * Class that handles the audio portion of the program.
     * An audio module is created here with commands that interact with an AudioService.
     */
    public class AudioModule : ModuleBase
    {
        // Private variables
        private readonly AudioService m_Service;

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot!
        public AudioModule(AudioService service)
        {
            m_Service = service;
        }

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinVoiceChannel()
        {
            await m_Service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveVoiceChannel()
        {
            await m_Service.LeaveAudio(Context.Guild);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayVoiceChannel([Remainder] string song)
        {
            // Get the stream information and display necessary information.
            AudioFile info = await m_Service.GetStreamData(song);
            await (Context.Client as DiscordSocketClient).SetGameAsync(info.Title); // Set 'playing' as the song title.
            await ReplyAsync("Now Playing : " + info.Title);

            // Play the audio. This function is BLOCKING. Call this last!
            await m_Service.PlayAudioAsync(Context.Guild, Context.Channel, song);
        }

        [Command("pause", RunMode = RunMode.Async)]
        public async Task PauseVoiceChannel()
        {
            m_Service.PauseAudio();
            await Task.Delay(0);
        }

        [Command("resume", RunMode = RunMode.Async)]
        public async Task ResumeVoiceChannel()
        {
            m_Service.ResumeAudio();
            await Task.Delay(0);
        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopVoiceChannel()
        {
            m_Service.StopAudio();
            await Task.Delay(0);
        }

        [Command("volume")]
        public async Task VolumeVoiceChannel([Remainder] float volume)
        {
            m_Service.AdjustVolume(volume);
            await Task.Delay(0);
        }

        // Add more commands here.

    }
}
