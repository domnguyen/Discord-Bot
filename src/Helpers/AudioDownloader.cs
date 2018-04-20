using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WhalesFargo
{
    /**
    *  AudioDownloader
    *  Helper class to download files in the background.
    *  This can be used to optimize network audio sources.
    */
    public class AudioDownloader
    {
        // Concurrent Libraries to keep track of the current downloads order and duplicates.
        private readonly ConcurrentQueue<AudioFile> m_DownloadQueue = new ConcurrentQueue<AudioFile>();
        private readonly ConcurrentDictionary<string, int> m_DownloadDuplicates = new ConcurrentDictionary<string, int>();

        // Private variables.
        private string m_FolderPath = "tmp";            // Default folder path. This is relative to the running directory of the bot.
        private bool m_IsRunning = false;               // Flag to check if it's in the middle of downloading already.

        /**
         * Add
         * Adds a song to the queue for download.
         * 
         * @param song - song to be downloaded in the future.
         */
        public async Task AddAsync(AudioFile song)
        {
            m_DownloadQueue.Enqueue(song); // Only add if there's no errors.
            await Task.Delay(0);
        }

        /**
         * Next
         * Gets the next song in the queue for download.
         */
        public AudioFile Next()
        {
            AudioFile nextSong = null;
            m_DownloadQueue.TryDequeue(out nextSong);
            return nextSong;
        }

        /**
         * StartDownloadAsync
         * Starts the download loop and downloads from the front of the queue.
         */
        public async Task StartDownloadAsync()
        {
            if (m_IsRunning) return; // Download is already running, stop to avoid conflicts/race conditions.

            m_IsRunning = true;
            while (m_DownloadQueue.Count > 0)
            {
                if (!m_IsRunning) return; // Stop downloading.
                await DownloadAsync(Next());
            }
            m_IsRunning = false;
        }

        /**
         * StopDownload
         * Stop the download loop.
         */
        public void StopDownload()
        {
            m_IsRunning = false;
        }

        /**
         *  DownloadAsync
         *  Downloads the file in the background and sets downloaded to true when done.
         *  This can be used to optimize network audio sources.
         *  
         *  @param song - AudioFile from the concurrentqueue
         *  
         *  TODO: Catch any errors if the file cannot be downloaded or opened?
         */
        public async Task DownloadAsync(AudioFile song)
        {
            // First we check if it's a network file that needs to be downloaded.
            if (!song.IsNetwork || File.Exists(song.FileName)) return;

            // Start downloading on a separate thread.
            new Thread(() =>
            {
                // Get the proper filename.
                string filename = GetProperFilename(song.Title);

                // youtube-dl.exe
                Process youtubedl;

                // Download Video
                ProcessStartInfo youtubedlFile = new ProcessStartInfo()
                {
                    FileName = "youtube-dl",
                    Arguments = $"-x --audio-format mp3 -o \"{filename.Replace(".mp3", ".%(ext)s")}\" {song.FileName}",
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                youtubedl = Process.Start(youtubedlFile);
                youtubedl.WaitForExit();

                // Update the filename with the local directory, set it to local and downloaded to true.
                song.FileName = filename;
                song.IsNetwork = false;
                song.IsDownloaded = true;

            }).Start();

            await Task.Delay(0);
        }

        /**
         * IsRunning
         * Returns the status of the downloader.
         */
        public bool IsRunning()
        {
            return m_IsRunning;
        }

        /**
         * GetProperFilename
         * Returns the proper filename by searching the path for an existing file.
         * 
         * @param title - The song title we're searching for, or if there's an existing filename.
         */
        private string GetProperFilename(string title)
        {
            string filename = "";
            filename = Path.Combine(m_FolderPath, title + ".mp3");

            // It already exists, so we update it's value.
            if (m_DownloadDuplicates.TryRemove(filename, out var count))
            {
                m_DownloadDuplicates.TryAdd(filename, ++count);
                filename = Path.Combine(m_FolderPath, title + "_" + (count) + ".mp3");
            }

            // This is the first time seeing it, so we add it for the first time.
            else
            {
                m_DownloadDuplicates.TryAdd(filename, 0);
            }

            return filename;
        }
    }
}
