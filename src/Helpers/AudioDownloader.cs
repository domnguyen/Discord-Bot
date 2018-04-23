using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WhalesFargo.Helpers
{
    /**
    *  AudioDownloader
    *  Helper class to download files in the background.
    *  This can be used to optimize network audio sources.
    */
    public class AudioDownloader
    {
        // Concurrent Library to keep track of the current downloads order.
        private readonly ConcurrentQueue<AudioFile> m_DownloadQueue = new ConcurrentQueue<AudioFile>();

        // Private variables.
        private string m_DownloadPath = "tmp";              // Default folder path. This is relative to the running directory of the bot.
        private bool m_IsRunning = false;                   // Flag to check if it's in the middle of downloading already.
        private string m_CurrentlyDownloading = "";         // Currently downloading file.
        private bool m_AllowDuplicates = true;              // Flag for downloading duplicate items.

        /**
         *  GetDirectory
         *  Returns the current downloading folder.
         */
        public string GetDownloadPath() { return m_DownloadPath; }

        /**
         * IsRunning
         * Returns the status of the downloader.
         */
        public bool IsRunning() { return m_IsRunning; }

        /**
         * CurrentlyDownloading
         * Returns the current download status.
         */
        public string CurrentlyDownloading() { return m_CurrentlyDownloading; }

        /**
         *  GetAllItems
         *  Returns a string with downloaded song names.
         */
        public string GetAllItems()
        {
            // Populate items with the files in the folder.
            string items = "";

            // Check the files in the directory.
            string[] itemEntries = Directory.GetFiles(m_DownloadPath);
            int itemCount = itemEntries.Length;
            if (itemCount == 0) return "There are currently no items downloaded.";

            // Count the number of total digits.
            int countDigits = (int)(Math.Floor(Math.Log10(itemCount) + 1));

            // For each file, we add it to the list.
            for (int i = 0; i < itemCount; i++)
            {
                // Prepend 0's so it matches in length. This will be the 'index'.
                string zeros = "";
                int numDigits = (i == 0) ? 1 : (int)(Math.Floor(Math.Log10(i) + 1));
                while (numDigits < countDigits)
                {
                    zeros += "0";
                    ++numDigits;
                }

                // Print out the current file string.
                string file = itemEntries[i].Split(Path.DirectorySeparatorChar).Last(); // Get just the file name.
                items += zeros + i + " : " + file + "\n";
            }

            return items;
        }

        /**
         *  GetItem
         *  Returns a path to the downloaded item, if already downloaded.
         *  
         *  @param item
         */
        public string GetItem(string item)
        {
            // If it's been downloaded and isn't currently downloading, we can return it.
            if (File.Exists(m_DownloadPath + "\\" + item) && !m_CurrentlyDownloading.Equals(item))
                return m_DownloadPath + "\\" + item;

            // Else we return blank. This means the item doesn't exist in our library.
            return null;
        }

        /**
         *  GetItem
         *  Returns a path to the downloaded item, if already downloaded.
         *  
         *  @param item
         */
        public string GetItem(int index)
        {
            // Check the files in the directory.
            string[] itemEntries = Directory.GetFiles(m_DownloadPath);

            // Return by index.
            if (index < 0 || index >= itemEntries.Length) return null;
            return itemEntries[index].Split(Path.DirectorySeparatorChar).Last();
        }

        /**
         * GetDuplicateItem
         * Returns the proper filename by searching the path for an existing file.
         * 
         * @param item - The song title we're searching for, without the .mp3.
         */
        private string GetDuplicateItem(string item)
        {
            string filename = null;
            int count = 0;

            filename = Path.Combine(m_DownloadPath, item + ".mp3");

            while (File.Exists(filename))
            {
                filename = Path.Combine(m_DownloadPath, item + "_" + (count++) + ".mp3");
            }

            return filename;
        }

        /**
         * RemoveDuplicateItems
         * Remove any duplicates created by the downloader.
         */
        public void RemoveDuplicateItems()
        {
            ConcurrentDictionary<string, int> duplicates = new ConcurrentDictionary<string, int>();

            // Check the files in the directory.
            string[] itemEntries = Directory.GetFiles(m_DownloadPath);
            foreach (string item in itemEntries)
            {
                string filename = Path.GetFileNameWithoutExtension(item);

                // If it's a duplicate, get it's base name.
                var isDuplicate = int.TryParse(filename.Split('_').Last(), out int n);
                if (isDuplicate) filename = filename.Split(new char[] { '_' }, 2)[0];

                duplicates.TryRemove(filename, out int count);
                duplicates.TryAdd(filename, ++count);

                try
                {
                    if (count >= 2) File.Delete(item);
                }
                catch
                {
                    Console.WriteLine("Problem whiler deleting duplicates.");
                }
            }
        }

        /**
         * Next
         * Gets the next song in the queue for download.
         */
        private AudioFile Next()
        {
            m_DownloadQueue.TryDequeue(out AudioFile nextSong);
            return nextSong;
        }

        /**
         * Add
         * Adds a song to the queue for download.
         * 
         * @param song - song to be downloaded in the future.
         */
        public void Add(AudioFile song)
        {
            m_DownloadQueue.Enqueue(song); // Only add if there's no errors.
        }

        /**
         * StartDownloadAsync
         * Starts the download loop and downloads from the front of the queue.
         */
        public async Task StartDownloadAsync()
        {
            if (m_IsRunning) return; // Download is already running, stop to avoid conflicts/race conditions.

            // Loop for downloading.
            m_IsRunning = true;
            while (m_DownloadQueue.Count > 0)
            {
                if (!m_IsRunning) return; // Stop downloading.
                await DownloadAsync(Next());
            }
            m_IsRunning = false;
        }

        /**
         *  DownloadAsync
         *  Downloads the file in the background and sets downloaded to true when done.
         *  This can be used to optimize network audio sources.
         *  
         *  @param song - AudioFile from the concurrentqueue
         */
        private async Task DownloadAsync(AudioFile song)
        {
            // First we check if it's a network file that needs to be downloaded.
            if (!song.IsNetwork) return;

            // Then we check if the file already exists.
            string filename = GetItem(song.Title + ".mp3");
            if (filename != null) // We get the full path.
            {
                if (m_AllowDuplicates) filename = GetDuplicateItem(song.Title);
                else return;
            }
            else // This is our first time seeing it.
            {
                filename = m_DownloadPath + "\\" + song.Title + ".mp3";
            }

            { // Start downloading.
                // Set it as our currently downloading item.
                m_CurrentlyDownloading = filename;
                Console.WriteLine("Currently downloading : " + song.Title);

                // youtube-dl.exe
                Process youtubedl;

                try
                {
                    // Download Video. This replaces the format with the extension .mp3 in the end.
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
                }
                catch
                {
                    // Error while downloading. Remove from folder if exists.
                    Console.WriteLine("Error while downloading " + song.Title);
                    if (GetItem(filename) != null) File.Delete(filename);
                }

                // Update the filename with the local directory, set it to local and downloaded to true.
                song.FileName = filename;
                song.IsNetwork = false; // Network is now false.
                song.IsDownloaded = true;
                m_CurrentlyDownloading = ""; // Reset our currently downloading item.
            }

            await Task.Delay(0);
        }

        /**
         * StopDownload
         * Stops the download loop.
         */
        public void StopDownload()
        {
            m_IsRunning = false;
        }

        /**
         *  VerifyNetworkPath
         *  Verifies that the path is a network path and not a local path. Checks here before extracting.
         *  Add more arguments here, but we'll just check based on http and assume a network link.
         *  
         *  @param path - The path to the file
         */
        public bool? VerifyNetworkPath(string path)
        {
            if (path == null) return null;
            return path.StartsWith("http");
        }

        /**
         *  GetAudioFileInfo
         *  Extracts data from the current path, by finding it locally or on the network.
         *  Puts all the information into an AudioFile and returns it.
         *  Returns null if it can't be extracted through it's path.
         *  
         *  @param path - string of the source path
         */
        public async Task<AudioFile> GetAudioFileInfo(string path)
        {
            Console.WriteLine("Extracting Meta Data for : " + path);

            // Verify if it's a network path or not.
            bool? verifyNetwork = VerifyNetworkPath(path);
            if (verifyNetwork == null)
            {
                Console.WriteLine("Path invalid.");
                return null;
            }

            // Construct audio file.
            AudioFile StreamData = new AudioFile();

            // Local file.
            if (verifyNetwork == false)
            {
                try
                {
                    // Check if we have it in our downloaded directory.
                    string downloaded = GetItem(path);
                    if (downloaded != null) path = downloaded;

                    // If it's downloaded, it'll exist, but if it still doesn't exist, return.
                    if (!File.Exists(path))
                    {
                        Console.WriteLine("File does not exist.");
                        throw new NullReferenceException();
                    }

                    // Set the file name.
                    StreamData.FileName = path;

                    // Extract corresponding data.
                    StreamData.Title = path.Split(Path.DirectorySeparatorChar).Last();
                    if (StreamData.Title.CompareTo("") == 0) StreamData.Title = path;

                    // Set other properties as follows.
                    StreamData.IsNetwork = (bool)verifyNetwork;
                }
                catch
                {
                    Console.WriteLine("Failed to get local file information!");
                    return null;
                }
            }
            // Network file.
            else if (verifyNetwork == true)
            {
                // youtube-dl.exe
                Process youtubedl;

                try
                {
                    // Get Video Title
                    ProcessStartInfo youtubedlMetaData = new ProcessStartInfo()
                    {
                        FileName = "youtube-dl",
                        Arguments = $"-s -e {path}",// Add more flags for more options.
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    youtubedl = Process.Start(youtubedlMetaData);
                    youtubedl.WaitForExit();

                    // Read the output of the simulation
                    string[] output = youtubedl.StandardOutput.ReadToEnd().Split('\n');

                    // Set the file name.
                    StreamData.FileName = path;

                    // Extract each line printed for it's corresponding data.
                    if (output.Length > 0)
                        StreamData.Title = output[0];

                    // Set other properties as follows.
                    StreamData.IsNetwork = (bool)verifyNetwork;
                }
                catch
                {
                    Console.WriteLine("youtube-dl.exe failed to extract the data!");
                    return null;
                }
            }

            await Task.Delay(0);
            return StreamData;
        }

    }
}
