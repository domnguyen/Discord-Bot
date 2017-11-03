using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Audio;

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
        [Command("joinvoice", RunMode = RunMode.Async)]
        public async Task JoinVoiceChannel()
        {
            await m_Service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
            Console.WriteLine("Connected to voice channel.");
        }

        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.
        [Command("leavevoice", RunMode = RunMode.Async)]
        public async Task LeaveVoiceChannel()
        {
            await m_Service.LeaveAudio(Context.Guild);
            Console.WriteLine("Left voice channel.");
        }

        [Command("playvoice", RunMode = RunMode.Async)]
        public async Task PlayVoiceChannel([Remainder] string song)
        {
           await m_Service.SendAudioAsync(Context.Guild, Context.Channel, song);
        }

        // Add more commands here.
    }
}
