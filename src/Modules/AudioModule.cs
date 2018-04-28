using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using WhalesFargo.Helpers;
using WhalesFargo.Services;

namespace WhalesFargo.Modules
{

    /**
     * AudioModule
     * This handles all the audio commands for the bot.
     * 
     * This supports playing local songs (from the machine the bot is running on) and most network
     * songs that youtube-dl supports.
     * 
     * It also maintains it's own playlist and can be mixed between local and network sources.
     * 
     * Since this is meant to run on a single server, it should only be joined in a single
     * voice channel at a time. We set it up to allow multiple channel connections, but we only
     * have a single instance of the audioplayer. If the bot exists in multiple servers, it will only
     * interact with the voice chat in the last server it received the commands in.
     * 
     * As a module, this will interact with AudioService, using commands from Discord.
     */
    [Name("Audio")]
    [Summary("Audio module to interact with voice chat. Currently, used to playback audio in a stream.")]
    public class AudioModule : CustomModule
    {
        // Private variables
        private readonly AudioService m_Service;        // Reference to the service.

        // Dependencies are automatically injected via this constructor.
        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot!
        public AudioModule(AudioService service)
        {
            m_Service = service;
            m_Service.SetParentModule(this); // Reference to this from the service.
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
            if (m_Service.GetDelayAction()) return; // Stop multiple attempts to join too quickly.
            await m_Service.JoinAudioAsync(Context.Guild, (Context.User as IVoiceState).VoiceChannel);

            // Start the autoplay service if enabled, but not yet started.
            await m_Service.CheckAutoPlayAsync(Context.Guild, Context.Channel);
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Remarks("!leave")]
        [Summary("Leaves the current voice channel.")]
        public async Task LeaveVoiceChannel()
        {
            await m_Service.LeaveAudioAsync(Context.Guild);
        }

        [Command("play", RunMode = RunMode.Async)]
        [Remarks("!play [url/index]")]
        [Summary("Plays a song by url or local path.")]
        public async Task PlayVoiceChannel([Remainder] string song)
        {
            // Extract the audio.
            AudioFile audio = await m_Service.GetAudioFileAsync(m_Service.GetLocalSong(song));

            // Play the audio. We check if audio is null when we attempt to play. This function is BLOCKING.
            await m_Service.ForcePlayAudioAsync(Context.Guild, Context.Channel, audio);

            // Start the autoplay service if enabled, but not yet started.
            // Once force play is done, if auto play is enabled, we can resume the autoplay here.
            await m_Service.CheckAutoPlayAsync(Context.Guild, Context.Channel);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayVoiceChannelByIndex([Remainder] int index)
        {
            // Play a song by it's local index in the download folder.
            await PlayVoiceChannel(m_Service.GetLocalSong(index)); 
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Remarks("!pause")]
        [Summary("Pauses the current song, if playing.")]
        public async Task PauseVoiceChannel()
        {
            m_Service.PauseAudio();
            await Task.Delay(0); // Suppress async warrnings.
        }

        [Command("resume", RunMode = RunMode.Async)]
        [Remarks("!resume")]
        [Summary("Pauses the current song, if paused.")]
        public async Task ResumeVoiceChannel()
        {
            m_Service.ResumeAudio();
            await Task.Delay(0); // Suppress async warrnings.
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Remarks("!stop")]
        [Summary("Stops the current song, if playing or paused.")]
        public async Task StopVoiceChannel()
        {
            m_Service.StopAudio();
            await Task.Delay(0); // Suppress async warrnings.
        }

        [Command("volume")]
        [Remarks("!volume [num]")]
        [Summary("Changes the volume to [0.0, 1.0].")]
        public async Task VolumeVoiceChannel([Remainder] float volume)
        {
            m_Service.AdjustVolume(volume);
            await Task.Delay(0); // Suppress async warrnings.
        }

        [Command("add", RunMode = RunMode.Async)]
        [Remarks("!add [url/index]")]
        [Summary("Adds a song by url or local path to the playlist.")]
        public async Task AddVoiceChannel([Remainder] string song)
        {
            // Add it to the playlist.
            await m_Service.PlaylistAddAsync(m_Service.GetLocalSong(song));

            // Start the autoplay service if enabled, but not yet started.
            await m_Service.CheckAutoPlayAsync(Context.Guild, Context.Channel);
        }

        [Command("add", RunMode = RunMode.Async)]
        public async Task AddVoiceChannelByIndex([Remainder] int index)
        {
            // Add a song by it's local index in the download folder.
            await AddVoiceChannel(m_Service.GetLocalSong(index));
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
            m_Service.PrintPlaylist();
            await Task.Delay(0);
        }

        [Command("autoplay", RunMode = RunMode.Async)]
        [Remarks("!autoplay [enable]")]
        [Summary("Starts the autoplay service on the current playlist.")]
        public async Task AutoPlayVoiceChannel([Remainder] bool enable)
        {
            m_Service.SetAutoPlay(enable);

            // Start the autoplay service if already on, but not started.
            await m_Service.CheckAutoPlayAsync(Context.Guild, Context.Channel);
        }

        [Command("download", RunMode = RunMode.Async)]
        [Remarks("!download [http]")]
        [Summary("Download songs into our local folder.")]
        public async Task DownloadSong([Remainder] string path)
        {
            await m_Service.DownloadSongAsync(path);
        }

        [Command("songs", RunMode = RunMode.Async)]
        [Remarks("!songs [page]")]
        [Summary("Shows songs in our local folder in pages.")]
        public async Task PrintSongDirectory(int page = 0)
        {
            m_Service.PrintLocalSongs(page);
            await Task.Delay(0);
        }

        [Command("cleanupsongs", RunMode = RunMode.Async)]
        [Remarks("!cleanupsongs")]
        [Summary("Cleans the local folder of duplicate files created by our downloader.")]
        public async Task CleanSongDirectory()
        {
            await m_Service.RemoveDuplicateSongsAsync();
        }

    }
}
