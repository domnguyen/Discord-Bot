using Discord;
using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public AudioFile()
        {
            m_FileName = "";
            m_Title = "";
            m_IsNetwork = true; // True by default, streamed from the network
            m_IsValid = false; // False by default.
            m_IsDownloaded = false;
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

        /**
        *  DownloadAsync
        *  
        */
        public async Task DownloadAsync()
        {
            await Task.Delay(0); // TODO: Write the download function here.
            string FileName = "temp";

            // Update the filename with the local directory, set it to local and downloaded to true.
            m_FileName = FileName;
            m_IsNetwork = false;
            m_IsDownloaded = true;
        }

    }
}
