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
     * This handles the entire audio service functionality. 
     * This service used to perform all the tasks required by the module, but most have been separated
     * into helper functions.
     * 
     * AudioDownloader handles reading simple meta data from network links and local songs. If specified,
     * it'll download network songs into a default folder.
     * 
     * AudioPlayer handles the local and network streams then passes it into FFmpeg to output to the voice channel.
     * 
     * Right now the playlist is maintained in the service, but may be abstracted or moved into another
     * class in the future.
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
        private int m_DelayActionLength = 10000;    // To prevent connection issues, we set it to a fairly 'large' value.
        private bool m_DelayAction = false;         // Temporary Semaphore to control leaving and joining too quickly.
        private bool m_AutoPlay = false;            // Flag to check if autoplay is currently on or not.
        private bool m_AutoPlayRunning = false;     // Flag to check if autoplay is currently running or not. More of a 'sanity' check really.
        private bool m_AutoDownload = true;         // Flag to auto download network items in the playlist.
        private bool m_AutoStop = false;            // Flag to stop the autoplay service when we're done playing all songs in the playlist.

        // Using the flag as a semaphore, we pass in a function to lock in between it. Added for better practice.
        // Any async function that's called after this, if required can check for m_DelayAction before continuing.
        private async Task DelayAction(Action f)
        {
            m_DelayAction = true; // Lock.
            f();
            await Task.Delay(m_DelayActionLength); // Delay to prevent error condition. TEMPORARY.
            m_DelayAction = false; // Unlock.
        }

        // Gets m_DelayAction, this is a temporary semaphore to prevent joining too quickly after leaving a channel.
        public bool GetDelayAction()
        {
            if (m_DelayAction) Log("This action is delayed. Please try again later.");
            return m_DelayAction;
        }

        // Joins the voice channel of the target.
        // Adds a new client to the ConcurrentDictionary.
        public async Task JoinAudioAsync(IGuild guild, IVoiceChannel target)
        {
            // We can't connect to an empty guilds or targets.
            if (guild == null || target == null) return;

            // Delayed join if the client recently left a voice channel. This is to prevent reconnection issues.
            if (m_DelayAction)
            {
                Log("The client is currently disconnecting from a voice channel. Please try again later.");
                return;
            }

            // Try to get the current audio client. If it's already there, we've already joined.
            if (m_ConnectedChannels.TryGetValue(guild.Id, out var connectedAudioClient))
            {
                Log("The client is already connected to the current voice channel.");
                return;
            }

            // If the target guild id doesn't match the guild id we want, return.
            // This will likely never happen, but the source message could refer to the incorrect server.
            if (target.Guild.Id != guild.Id)
            {
                Log("Are you sure the current voice channel is correct?");
                return;
            }

            // Attempt to connect to this audio channel.
            var audioClient = await target.ConnectAsync();

            try // We should put a try block in case audioClient is null or some other error occurs.
            {
                // Once connected, add it to the dictionary of connected channels.
                if (m_ConnectedChannels.TryAdd(guild.Id, audioClient))
                {
                    Log("The client is now connected to the current voice channel.");
                    return;
                }
            }
            catch
            {
                Log("The client failed to connect to the target voice channel.");
            }

            // If we can't add it to the dictionary or connecting didn't work properly, error.
            Log("Unable to join the current voice channel.");
        }

        // Leaves the current voice channel.
        // Removes the client from the ConcurrentDictionary.
        public async Task LeaveAudioAsync(IGuild guild)
        {
            // We can't disconnect from an empty guild.
            if (guild == null) return;

            // To avoid any issues, we stop the player before leaving the channel.
            if (m_AudioPlayer.IsRunning()) StopAudio();
            while (m_AudioPlayer.IsRunning()) await Task.Delay(1000); // Wait until it's fully stopped.

            // Attempt to remove from the current dictionary, and if removed, stop it.
            if (m_ConnectedChannels.TryRemove(guild.Id, out var audioClient))
            {
                Log("The client is now disconnected from the current voice channel.");
                await DelayAction(() => audioClient.StopAsync()); // Wait until the audioClient is properly disconnected.
                return;
            }

            // If we can't remove it from the dictionary, error.
            Log("Unable to disconnect from the current voice channel. Are you sure that it is currently connected?");
        }

        // Force Play the current audio in the voice channel of the target.
        // TODO: Consider adding it to autoplay list if it is already playing.
        public async Task ForcePlayAudioAsync(IGuild guild, IMessageChannel channel, AudioFile song)
        {
            // We can't play from an empty guild.
            if (guild == null) return;

            // Can't play an empty song.
            if (song == null) return;
            
            // Stop any other audio running.
            if (m_AudioPlayer.IsRunning()) StopAudio();
            while (m_AudioPlayer.IsRunning()) await Task.Delay(1000);

            // Start the stream, this is the main part of 'play'. This will stop the current song.
            if (m_ConnectedChannels.TryGetValue(guild.Id, out var audioClient))
            {
                Log($"Now Playing: {song.Title}", (int)E_LogOutput.Reply); // Reply in the text channel.
                Log(song.Title, (int)E_LogOutput.Playing); // Set playing.
                await m_AudioPlayer.Play(audioClient, song); // The song should already be identified as local or network.
                Log(Strings.NotPlaying, (int)E_LogOutput.Playing);
            }
            else
            {
                // If we can't get it from the dictionary, we're probably not connected to it yet.
                Log("Unable to play in the proper channel. Make sure the audio client is connected.");
            }
        }

        // This is for the autoplay function which waits after each playback and pulls from the playlist.
        public async Task AutoPlayAudioAsync(IGuild guild, IMessageChannel channel)
        {
            // We can't play from an empty guild.
            if (guild == null) return;

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
                        await m_AudioPlayer.Play(audioClient, song); // The song should already be identified as local or network.
                        Log(Strings.NotPlaying, (int)E_LogOutput.Playing);
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

        // Returns if the audio player is currently playing or not.
        public bool IsAudioPlaying() { return m_AudioPlayer.IsPlaying(); }

        // Checks if autoplay is true, but not started yet. If not started, we start autoplay here.
        public async Task CheckAutoPlayAsync(IGuild guild, IMessageChannel channel)
        {
            if (m_AutoPlay && !m_AutoPlayRunning && !m_AudioPlayer.IsRunning()) // if autoplay or force play isn't playing.
                await AutoPlayAudioAsync(guild, channel);
        }

        // Sets the autoplay service to be true. Likely, wherever this is set, we also check and start auto play.
        public void SetAutoPlay(bool enable) { m_AutoPlay = enable; }
        
        // Returns the current state of the autoplay service.
        public bool GetAutoPlay() { return m_AutoPlay; }

        // Adjusts the current volume to the value passed. This affects the current AudioPlaybackAsync.
        // Takes in a value from [0.0f - 1.0f].
        public void AdjustVolume(float volume) { m_AudioPlayer.AdjustVolume(volume); }

        // AudioPlayback Functions. Pause, Resume, Stop.
        public void PauseAudio() { m_AudioPlayer.Pause(); }
        public void ResumeAudio() { m_AudioPlayer.Resume(); }
        public void StopAudio() { m_AutoPlayRunning = false; m_AudioPlayer.Stop(); }

        // Prints the playlist information.
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

        // Adds a song to the playlist.
        public async Task PlaylistAddAsync(string path)
        {
            // Get audio info.
            AudioFile audio = await GetAudioFileAsync(path);
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

        // Gets the next song in the playlist queue.
        private AudioFile PlaylistNext()
        {
            if (m_Playlist.TryDequeue(out AudioFile nextSong))
                return nextSong;

            if (m_Playlist.Count <= 0) Log("We reached the end of the playlist.");
            else Log("The next song could not be opened.");
            return nextSong;
        }

        // Skips the current playlist song if autoplay is on.
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

        // Extracts simple meta data from the path and fills a new AudioFile
        // information about the audio source. If it fails in the downloader or here,
        // we simply return null.
        public async Task<AudioFile> GetAudioFileAsync(string path)
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

        // Finds all the local songs and prints out a set at a time by page number.
        public void PrintLocalSongs(int page)
        {
            // Get all the songs in this directory.
            string[] items = m_AudioDownloader.GetAllItems();
            int itemCount = items.Length;
            if (itemCount == 0)
            {
                Log("No local files found.", (int)E_LogOutput.Reply);
                return;
            }

            // Count the number of total digits.
            int countDigits = (int)(Math.Floor(Math.Log10(items.Length) + 1));

            // Set pages to print.
            int pageSize = 20;
            int pages = (itemCount / pageSize) + 1;
            if (page < 1 || page > pages)
            {
                Log($"There are {pages} pages. Select page 1 to {pages}.", (int)E_LogOutput.Reply);
                return;
            }

            // Start printing.
            for (int p = page - 1; p < page; p++)
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
                    emb.AddField(zeros + index, file);
                }

                DiscordReply($"Page {p+1}", emb);
            }
        }

        // Returns the path with the specified song by filename.
        // Otherwise, returns the input string.
        public string GetLocalSong(string filename) { string local = m_AudioDownloader.GetItem(filename); return (local != null) ? local : filename; }

        // Returns the name with the specified song by index.
        // Returns null if a local song doesn't exist.
        public string GetLocalSong(int index) { return m_AudioDownloader.GetItem(index); }

        // Adds a song to the download queue.
        public async Task DownloadSongAsync(string path)
        {
            AudioFile audio = await GetAudioFileAsync(path);
            if (audio != null)
            {
                Log($"Added to the download queue : {audio.Title}", (int)E_LogOutput.Reply);

                // If the downloader is set to true, we start the autodownload helper.
                if (audio.IsNetwork) m_AudioDownloader.Push(audio); // Auto download while in playlist.
                await m_AudioDownloader.StartDownloadAsync(); // Start the downloader if it's off.
            }
        }

        // Removes any duplicates in our download folder.
        public async Task RemoveDuplicateSongsAsync()
        {
            m_AudioDownloader.RemoveDuplicateItems();
            await Task.Delay(0);
        }

    }
}
