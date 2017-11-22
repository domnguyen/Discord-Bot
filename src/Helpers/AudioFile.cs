using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WhalesFargo
{
    /**
     * AudioFile
     * Class that holds properties from the audio file.
     * Add more when necessary, but the only thing we're using for it now is the title field.
     */
    public class AudioFile
    {
        private string m_FileName;
        private string m_Title;
        private bool m_IsNetwork;
        private bool m_IsValid;
        private bool m_IsDownloaded;
        private bool m_IsCurrentlyDownloading = false;

        public AudioFile()
        {
            m_FileName = "";
            m_Title = "";
            m_IsNetwork = true; // True by default, streamed from the network
            m_IsValid = false; // False by default.
            m_IsDownloaded = false;
            m_IsCurrentlyDownloading = false;
        }

        public override string ToString()
        {
            return m_Title;
        }

        public string FileName
        {
            get { return m_FileName; }
            set { m_FileName = value; }
        }

        public string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }

        public bool IsNetwork
        {
            get { return m_IsNetwork; }
            set { m_IsNetwork = value; }
        }

        public bool IsValid
        {
            get { return m_IsValid; }
            set { m_IsValid = value; }
        }

        public bool IsDownloaded
        {
            get { return m_IsDownloaded; }
            set { m_IsDownloaded = value; }
        }

        public bool IsDownloading
        {
            get { return m_IsCurrentlyDownloading; }
        }

        /**
         *  DownloadAsync
         *  Downloads the file in the background and sets downloaded to true when done.
         *  This can be used to optimize network audio sources.
         */
        public async Task DownloadAsync()
        {
            // First we check if it's a network file that needs to be downloaded.
            if (!IsNetwork || File.Exists(m_FileName)) return;

            new Thread(() =>
            {
                string filename;
                int count = 0;
                do
                {
                    filename = Path.Combine("tmp", "d_" + ++count + ".mp3");
                } while (File.Exists(filename));

                // youtube-dl.exe
                Process youtubedl;

                // Download Video
                ProcessStartInfo youtubedlFile = new ProcessStartInfo()
                {
                    FileName = "youtube-dl",
                    Arguments = $"-x --audio-format mp3 -o \"{filename.Replace(".mp3", ".%(ext)s")}\" {m_FileName}",
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                m_IsCurrentlyDownloading = true; // Set to true so we know we're in this loop.
                youtubedl = Process.Start(youtubedlFile);
                youtubedl.WaitForExit();

                // Update the filename with the local directory, set it to local and downloaded to true.
                m_FileName = filename;
                m_IsNetwork = false;
                m_IsDownloaded = true;
                m_IsCurrentlyDownloading = false; // Set to know we're done!

            }).Start();

            await Task.Delay(0);
        }

    }
}
