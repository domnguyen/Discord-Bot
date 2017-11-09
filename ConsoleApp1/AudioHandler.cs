using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhalesFargo
{
    /**
     * AudioHandler
     * Class that handles the audio files and processes.
     * This will hold the queue for a playlist and other functions.
     */
    public class AudioHandler
    {
        private ConcurrentQueue<AudioFile> m_PlaylistQueue; // Hold a concurrent queue for shared files.

        public AudioHandler()
        {
            m_PlaylistQueue = new ConcurrentQueue<AudioFile>();
        }

        public void AddSong(AudioFile song)
        {
            m_PlaylistQueue.Enqueue(song);
        }

        public AudioFile GetNextSong()
        {
            AudioFile nextSong = null;
            if (m_PlaylistQueue.TryDequeue(out nextSong))
                return nextSong;
            Console.WriteLine("Couldn't get the next song.");
            return nextSong;
        }

    }
}
