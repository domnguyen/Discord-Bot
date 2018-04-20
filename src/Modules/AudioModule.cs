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
    [Name("Audio")]
    [Summary("Audio module to interact with voice chat. Currently, used to playback audio in a stream.")]
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
            m_Service.SetParentModule(this); // Reference to this from the service.
        }

        // Reply will allow the AudioService to reply in the correct text channel.
        public async Task ServiceReplyAsync(string s)
        {
            await ReplyAsync(s);
        }

        // Playing will allow the AudioService to set the current game.
        public async Task ServicePlayingAsync(string s)
        {
            await (Context.Client as DiscordSocketClient).SetGameAsync(s);
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
        [Remarks("!join")]
        [Summary("Joins the user's voice channel.")]
        public async Task JoinVoiceChannel()
        {
            if (m_Service.GetDelayJoin()) return; // Stop multiple attempts to join too quickly.
            await m_Service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Remarks("!leave")]
        [Summary("Leaves the current voice channel.")]
        public async Task LeaveVoiceChannel()
        {
            await m_Service.LeaveAudio(Context.Guild);
        }

        [Command("play", RunMode = RunMode.Async)]
        [Remarks("!play [url]")]
        [Summary("Plays a song by url or local path.")]
        public async Task PlayVoiceChannel([Remainder] string song)
        {
            // Extract the audio. Download here if necessary. TODO: Catch if youtube-dl can't read the header.
            AudioFile audio = await m_Service.ExtractPathAsync(song);

            // Play the audio. This function is BLOCKING. Call this last!
            await m_Service.ForcePlayAudioAsync(Context.Guild, Context.Channel, audio);

            bool autoplay = m_Service.GetAutoPlay();
            // Start the autoplay service if already on, but not started. This function is BLOCKING.
            if (autoplay && m_Service.SetAutoPlay(autoplay))
                await m_Service.AutoPlayAudioAsync(Context.Guild, Context.Channel);
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Remarks("!pause")]
        [Summary("Pauses the current song, if playing.")]
        public async Task PauseVoiceChannel()
        {
            m_Service.PauseAudio();
            await Task.Delay(0);
        }

        [Command("resume", RunMode = RunMode.Async)]
        [Remarks("!resume")]
        [Summary("Pauses the current song, if paused.")]
        public async Task ResumeVoiceChannel()
        {
            m_Service.ResumeAudio();
            await Task.Delay(0);
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Remarks("!stop")]
        [Summary("Stops the current song, if playing or paused.")]
        public async Task StopVoiceChannel()
        {
            m_Service.StopAudio();
            await Task.Delay(0);
        }

        [Command("volume")]
        [Remarks("!volume [num]")]
        [Summary("Changes the volume to [0.0, 1.0].")]
        public async Task VolumeVoiceChannel([Remainder] float volume)
        {
            m_Service.AdjustVolume(volume);
            await Task.Delay(0);
        }

        [Command("add", RunMode = RunMode.Async)]
        [Remarks("!add [url]")]
        [Summary("Adds a song by url or local path to the playlist.")]
        public async Task AddVoiceChannel([Remainder] string song)
        {
            await m_Service.PlaylistAdd(song);

            bool autoplay = m_Service.GetAutoPlay();
            // Start the autoplay service if already on, but not started. This function is BLOCKING.
            if (autoplay && m_Service.SetAutoPlay(autoplay))
                await m_Service.AutoPlayAudioAsync(Context.Guild, Context.Channel);
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Alias("skip", "next")]
        [Remarks("!skip")]
        [Summary("Skips the current song, if playing from the playlist.")]
        public async Task SkipVoiceChannel()
        {
            m_Service.PlaylistSkip();
            await Task.Delay(0);
        }

        [Command("playlist", RunMode = RunMode.Async)]
        [Remarks("!playlist")]
        [Summary("Shows what's currently in the playlist.")]
        public async Task PrintPlaylistVoiceChannel()
        {
            await ServiceReplyAsync(m_Service.PlaylistString()); // Reply with a print out.
        }

        [Command("autoplay", RunMode = RunMode.Async)]
        [Remarks("!autoplay [enable]")]
        [Summary("Starts the autoplay service on the current playlist.")]
        public async Task AutoPlayVoiceChannel([Remainder] bool enable)
        {
            // Start the autoplay service. This function is BLOCKING.
            if (m_Service.SetAutoPlay(enable))
                await m_Service.AutoPlayAudioAsync(Context.Guild, Context.Channel);
        }

    }
}
