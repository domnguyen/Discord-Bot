using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

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

        // Dependencies are automatically injected via this constructor.
        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot!
        public AudioModule(AudioService service)
        {
            m_Service = service;
        }

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        //
        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        //
        // 'Avoid using long-running code in your modules wherever possible. 
        // You should not be implementing very much logic into your modules,
        // instead, outsource to a service for that.'

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinVoiceChannel()
        {
            if (m_Service.GetDelayJoin()) return; // Stop multiple attempts to join too quickly.
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
            // Extract the audio. Download here if necessary. TODO: Catch if youtube-dl can't read the header.
            AudioFile audio = await m_Service.ExtractPathAsync(song);

            // Display necessary information.
            await (Context.Client as DiscordSocketClient).SetGameAsync(audio.Title); // Set 'playing' as the song title.
            await ReplyAsync("Now Playing : " + audio.Title); // Reply with a 'Now Playing' message.

            // Play the audio. This function is BLOCKING. Call this last!
            await m_Service.ForcePlayAudioAsync(Context.Guild, Context.Channel, audio);
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

        [Command("add", RunMode = RunMode.Async)]
        public async Task AddVoiceChannel([Remainder] string song)
        {
            await m_Service.PlaylistAdd(song);

            // Start autoplay on add if it's on.
            bool autoplay = m_Service.GetAutoPlay();
            if (autoplay && m_Service.SetAutoPlay(autoplay))
                await m_Service.AutoPlayAudioAsync(Context.Guild, Context.Channel);
        }

        [Command("skip", RunMode = RunMode.Async)]
        public async Task SkipVoiceChannel()
        {
            m_Service.PlaylistSkip();
            await Task.Delay(0);
        }

        [Command("playlist", RunMode = RunMode.Async)]
        public async Task PrintPlaylistVoiceChannel()
        {
            await ReplyAsync(m_Service.PlaylistString()); // Reply with a print out.
        }

        [Command("autoplay", RunMode = RunMode.Async)]
        public async Task AutoPlayVoiceChannel([Remainder] bool enable)
        {
            // Start autoplay only when it's toggled from off to on.
            if (m_Service.SetAutoPlay(enable))
                await m_Service.AutoPlayAudioAsync(Context.Guild, Context.Channel);
        }

    }
}
