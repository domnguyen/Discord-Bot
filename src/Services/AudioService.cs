using Discord;
using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WhalesFargo.Helpers;

namespace WhalesFargo.Services
{
    /**
     * AudioService
     * Class that handles a single audio service.
     */
    public class AudioService : CustomService
    {
        // Concurrent dictionary for multithreaded environments.
        private readonly ConcurrentDictionary<ulong, IAudioClient> m_ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        // Playlist.
        private readonly ConcurrentQueue<AudioFile> m_Playlist = new ConcurrentQueue<AudioFile>();

        // Downloader.
        private readonly AudioDownloader m_AudioDownloader = new AudioDownloader(); // Only downloaded on playlist add.

        // Player.
        private readonly AudioPlayer m_AudioPlayer = new AudioPlayer();

        // Private variables.
        private bool m_DelayAction = false;         // Temporary Semaphore to control leaving and joining too quickly.
        private bool m_AutoPlay = false;            // Flag to check if autoplay is currently on or not.
        private bool m_AutoPlayRunning = false;
        private bool m_AutoDownload = true;         // Flag to auto download network items in the playlist.
        private bool m_AutoStop = false;

        /**
         * DelayAction
         * Using the flag, we pass in a function to lock in between the semaphore. Added for better practice.
         * 
         * @param f - Function called in between the semaphore.
         */
        private async Task DelayAction(Action f)
        {
            m_DelayAction = true; // Lock.
            f();
            await Task.Delay(10000); // Delay to prevent error condition. TEMPORARY.
            m_DelayAction = false; // Unlock.
        }

        /**
         *  GetDelayAction
         *  Gets m_DelayAction, this is a temporary semaphore to prevent joining too quickly after leaving a channel.
         */
        public bool GetDelayAction()
        {
            if (m_DelayAction)
                Log("This action is delayed. Please try again later.");
            return m_DelayAction;
        }

        /**
         *  JoinAudio
         *  Join the voice channel of the target.
         *  Adds a new client to the ConcurrentDictionary.
         *  
         *  @param guild
         *  @param target
         */
        public async Task JoinAudioAsync(IGuild guild, IVoiceChannel target)
        {
            // Delayed join if the client recently left a voice channel.
            if (m_DelayAction)
            {
                Log("The client is currently disconnecting from a voice channel. Please try again later.");
                return;
            }

            // Try to get the current audio client. If it's already there, we're already joined.
            if (m_ConnectedChannels.TryGetValue(guild.Id, out var connectedAudioClient))
            {
                Log("The client is already connected to the current voice channel.");
                return;
            }

            // If the target guild id doesn't match the guild id we want, return.
            if (target.Guild.Id != guild.Id)
            {
                Log("Are you sure the current voice channel is correct?");
                return;
            }

            // Attempt to connect to this audio channel.
            var audioClient = await target.ConnectAsync();

            // Add it to the dictionary of connected channels.
            if (m_ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                Log("The client is now connected to the current voice channel.");
                return;
            }

            // If we can't add it to the dictionary or connecting didn't work properly, error.
            Log("Unable to join the current voice channel.");
        }

        /**
         *  LeaveAudio
         *  Leave the current voice channel.
         *  Removes the client from the ConcurrentDictionary.
         *  
         *  @param guild
         */
        public async Task LeaveAudioAsync(IGuild guild)
        {
            // To avoid any issues, we stop the player before leaving the channel.
            if (m_AudioPlayer.IsRunning()) StopAudio();
            while (m_AudioPlayer.IsRunning()) await Task.Delay(1000);

            // Attempt to remove from the current dictionary, and if removed, stop it.
            if (m_ConnectedChannels.TryRemove(guild.Id, out var audioClient))
            {
                //await audioClient.StopAsync();
                Log("The client is now disconnected from the current voice channel.");
                await DelayAction(() => audioClient.StopAsync()); // Can change once this error is resolved.
                return;
            }

            // If we can't remove it from the dictionary, error.
            Log("Unable to disconnect from the current voice channel. Are you sure that it is currently connected?");
        }

        /**
         *  ExtractPathAsync
         *  Extracts from the path and fills an AudioFile with metadata information about the audio source.
         *  
         *  @param path - string of the source path
         */
        public async Task<AudioFile> ExtractPathAsync(string path)
        {
            try // We put this in a try catch block.
            {
                return await m_AudioDownloader.GetAudioFileInfo(path);
            }
            catch
            {
                return null;
            }
        }

        /**
         *  ForcePlayAudioAsync
         *  Force Play the current audio by string in the voice channel of the target.
         *  Right now, playing by local file name.
         *  
         *  @param guild
         *  @param channel
         *  @param path
         *  
         *  TODO: Parse for youtube downloader and ffmpeg for different strings.
         *  TODO: Have a way to check if the file has been downloaded and play a local version instead.
         *  TODO: Consider adding it to autoplay list if it is already playing.
         */
        public async Task ForcePlayAudioAsync(IGuild guild, IMessageChannel channel, AudioFile song)
        {
            // If there was an error with extracting the path, return.
            if (song == null)
            {
                Log($"Cannot play the audio source specified.");
                return;
            }

            // Stop any other audio running.
            if (m_AudioPlayer.IsRunning()) StopAudio();
            while (m_AudioPlayer.IsRunning()) await Task.Delay(1000);

            // Start the stream, this is the main part of 'play'. This will stop the current song.
            if (m_ConnectedChannels.TryGetValue(guild.Id, out var audioClient))
            {
                Log($"Now Playing: {song.Title}", (int)E_LogOutput.Reply); // Reply in the text channel.
                Log(song.Title, (int)E_LogOutput.Playing); // Set playing.
                await m_AudioPlayer.Play(audioClient, song);
            }
            else
            {
                // If we can't get it from the dictionary, we're probably not connected to it yet.
                Log("Unable to play in the proper channel. Make sure the audio client is connected.");
            }
        }

        /**
         *  AutoPlayAudioAsync
         *  This is for the autoplay function which waits after each playback and pulls from the playlist.
         *  
         *  @param guild
         *  @param channel
         *  
         *  TODO: Parse for youtube downloader and ffmpeg for different strings.
         *  TODO: Have a way to check if the file has been downloaded and play a local version instead.
         */
        public async Task AutoPlayAudioAsync(IGuild guild, IMessageChannel channel)
        {
            if (m_AutoPlayRunning) return; // Only allow one instance of autoplay.
            while (m_AutoPlayRunning = m_AutoPlay)
            {
                // If something else is already playing, we need to wait until it's fully finished.
                if (m_AudioPlayer.IsRunning()) await Task.Delay(1000);

                // We do some checks before entering this loop.
                if (m_Playlist.IsEmpty || !m_AutoPlayRunning || !m_AutoPlay) break;

                // If there's nothing playing, start the stream, this is the main part of 'play'
                if (m_ConnectedChannels.TryGetValue(guild.Id, out var audioClient))
                {
                    AudioFile song = PlaylistNext(); // If null, nothing in the playlist. We can wait in this loop until there is.
                    if (song != null)
                    {
                        Log($"Now Playing: {song.Title}", (int)E_LogOutput.Reply); // Reply in the text channel.
                        Log(song.Title, (int)E_LogOutput.Playing); // Set playing.
                        await m_AudioPlayer.Play(audioClient, song);
                    }
                    else
                        Log($"Cannot play the audio source specified : {song}");

                    // We do the same checks again to make sure we exit right away.
                    if (m_Playlist.IsEmpty || !m_AutoPlayRunning || !m_AutoPlay) break;

                    // Is null or done with playback.
                    continue;
                }

                // If we can't get it from the dictionary, we're probably not connected to it yet.
                Log("Unable to play in the proper channel. Make sure the audio client is connected.");
                break;
            }

            // Stops autoplay once we're done with it.
            if (m_AutoStop) m_AutoPlay = false;
            m_AutoPlayRunning = false;
        }

        /**
         *  IsAudioPlaying
         */
        public bool IsAudioPlaying() { return m_AudioPlayer.IsPlaying(); }

        /**
         *  CheckAutoPlayAsync
         *  Checks if autoplay is true, but not started yet. If not started, we start autoplay.
         *  
         *  @param guild
         *  @param channel
         */
        public async Task CheckAutoPlayAsync(IGuild guild, IMessageChannel channel)
        {
            if (m_AutoPlay && !m_AutoPlayRunning && !m_AudioPlayer.IsRunning()) // if autoplay or force play isn't playing.
                await AutoPlayAudioAsync(guild, channel);
        }

        /**
         *  SetAutoPlay
         *  Sets autplay to enable. Returns if the autoplay service should be started or not.
         *  
         *  @param enable - bool toggle for autoplay.
         */
        public void SetAutoPlay(bool enable) { m_AutoPlay = enable; }

        /**
         *  GetAutoPlay
         */
        public bool GetAutoPlay() { return m_AutoPlay; }

        /**
         *  AdjustVolume
         *  Adjusts the current volume to the value passed. This affects the current AudioPlaybackAsync.
         *  
         *  @param volume - A value from 0.0f - 1.0f.
         */
        public void AdjustVolume(float volume) { m_AudioPlayer.AdjustVolume(volume); }

        /**
         *  PauseAudio
         *  Stops the stream if it's playing. This affects the current AudioPlaybackAsync.
         *  Uses m_Process and m_IsPlaying.
         */
        public void PauseAudio() { m_AudioPlayer.Pause(); }

        /**
         *  ResumeAudio
         *  Stops the stream if it's playing. This affects the current AudioPlaybackAsync.
         *  Uses m_Process and m_IsPlaying.
         */
        public void ResumeAudio() { m_AudioPlayer.Resume(); }

        /**
         *  StopAudio
         *  Stops the stream if it's playing. This affects the current AudioPlaybackAsync.
         *  Uses m_Process and m_IsPlaying.
         */
        public void StopAudio() { m_AutoPlayRunning = false; m_AudioPlayer.Stop(); }

        /**
         *  PrintPlaylist
         *  Prints the playlist information.
         */
        public void PrintPlaylist()
        {
            // If none, we return.
            int count = m_Playlist.Count;
            if (count == 0)
            {
                Log("There are currently no items in the playlist.", (int)E_LogOutput.Reply);
                return;
            }

            // Count the number of total digits.
            int countDigits = (int)(Math.Floor(Math.Log10(count) + 1));

            // Create an embed builder.
            var emb = new EmbedBuilder();

            for (int i = 0; i < count; i++)
            {
                // Prepend 0's so it matches in length.
                string zeros = "";
                int numDigits = (i == 0) ? 1 : (int)(Math.Floor(Math.Log10(i) + 1));
                while (numDigits < countDigits)
                {
                    zeros += "0";
                    ++numDigits;
                }

                // Filename.
                AudioFile current = m_Playlist.ElementAt(i);
                emb.AddField(zeros + i, current);
            }

            DiscordReply("Playlist", emb);
        }

        /**
         *  PlaylistAdd
         *  Adds a song to the playlist.
         *  
         *  @param path   
         */
        public async Task PlaylistAddAsync(string path)
        {
            // Get audio info.
            AudioFile audio = await m_AudioDownloader.GetAudioFileInfo(GetLocalSong(path));
            if (audio != null)
            {
                m_Playlist.Enqueue(audio); // Only add if there's no errors.
                Log($"Added to playlist : {audio.Title}", (int)E_LogOutput.Reply);

                // If the downloader is set to true, we start the autodownload helper.
                if (m_AutoDownload)
                {
                    if (audio.IsNetwork) m_AudioDownloader.Push(audio); // Auto download while in playlist.
                    await m_AudioDownloader.StartDownloadAsync(); // Start the downloader if it's off.
                }
            }
        }

        /**
         *  PlaylistNext
         *  Gets the next song in the queue.
         */
        private AudioFile PlaylistNext()
        {
            if (m_Playlist.TryDequeue(out AudioFile nextSong))
                return nextSong;

            if (m_Playlist.Count <= 0) Log("We reached the end of the playlist.");
            else Log("The next song could not be opened.");
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
                Log("Autoplay service hasn't been started.");
                return;
            }
            if (!m_AudioPlayer.IsRunning())
            {
                Log("There's no audio currently playing.");
                return;
            }
            m_AudioPlayer.Stop();
        }

        /**
         *  PrintLocalSongs
         *  Finds all the local songs and prints out a set at a time by page number.
         */
        public void PrintLocalSongs(int page)
        {
            // Get all the songs in this directory.
            string[] items = m_AudioDownloader.GetAllItems();
            int itemCount = items.Length;
            if (itemCount == 0) DiscordReply("No local files found.");

            // Count the number of total digits.
            int countDigits = (int)(Math.Floor(Math.Log10(items.Length) + 1));

            // Set pages to print.
            int pageSize = 20;
            int pages = (page == 0) ? (itemCount / pageSize) + 1 : page;
            if (page < 1) page = 1;

            // Start printing.
            for (int p = page - 1; p < pages; p++)
            {
                // Create an embed builder.
                var emb = new EmbedBuilder();

                for (int i = 0; i < pageSize; i++)
                {
                    // Get the index for the file.
                    int index = (p * pageSize) + i;
                    if (index >= itemCount) break;

                    // Prepend 0's so it matches in length. This will be the 'index'.
                    string zeros = "";
                    int numDigits = (index == 0) ? 1 : (int)(Math.Floor(Math.Log10(index) + 1));
                    while (numDigits < countDigits)
                    {
                        zeros += "0";
                        ++numDigits;
                    }

                    // Filename.
                    string file = items[index].Split(Path.DirectorySeparatorChar).Last(); // Get just the file name.
                    emb.AddInlineField(zeros + index, file);
                }

                DiscordReply($"Page {p+1}", emb);
            }
        }

        /**
          *  GetLocalSong
          *  Returns a string with the specified song by index.
          */
        public string GetLocalSong(int index) { return m_AudioDownloader.GetItem(index); }

        /**
          *  GetLocalSong
          *  Returns a string with the specified song by filename.
          */
        public string GetLocalSong(string filename) { string local = m_AudioDownloader.GetItem(filename); return (local != null) ? local : filename; }

        /**
         *  DownloadSongAsync
         *  Adds a song to the download queue.
         *  
         *  @param path
         */
        public async Task DownloadSongAsync(string path)
        {
            AudioFile audio = await m_AudioDownloader.GetAudioFileInfo(path);
            if (audio != null)
            {
                Log($"Added to the download queue : {audio.Title}", (int)E_LogOutput.Reply);

                // If the downloader is set to true, we start the autodownload helper.
                if (audio.IsNetwork) m_AudioDownloader.Push(audio); // Auto download while in playlist.
                await m_AudioDownloader.StartDownloadAsync(); // Start the downloader if it's off.
            }
        }

        /**
         *  RemoveDuplicateSongsAsync
         *  Removes any duplicates in our download folder.
         */
        public async Task RemoveDuplicateSongsAsync()
        {
            m_AudioDownloader.RemoveDuplicateItems();
            await Task.Delay(0);
        }

    }
}
