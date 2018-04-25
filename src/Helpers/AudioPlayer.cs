using Discord.Audio;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WhalesFargo.Helpers
{
    /**
     *  AudioPlayer
     *  Helper class to handle a single audio playback.
     */
    class AudioPlayer
    {
        // Private variables.
        private bool m_IsRunning = false;           // Boolean to wrap the audio playback method.
        private Process m_Process = null;           // Process that runs when playing.
        private Stream m_Stream = null;             // Stream output when playing.
        private bool m_IsPlaying = false;           // Flag to change to play or pause the audio.
        private float m_Volume = 1.0f;              // Volume value that's checked during playback. Reference: PlayAudioAsync.
        private int m_BLOCK_SIZE = 3840;            // Custom block size for playback, in bytes.

        /**
         *  CreateLocalStream
         *  Creates a local stream using the file path specified and ffmpeg to stream it directly.
         *  The format Discord takes is 16-bit 48000Hz PCM
         *  
         *  @param path - string of the source path (local)
         */
        private Process CreateLocalStream(string path)
        {
            try
            {
                return Process.Start(new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });
            }
            catch
            {
                Console.WriteLine($"Error while opening local stream : {path}");
                return null;
            }
        }

        /**
         *  CreateNetworkStream
         *  Creates a network stream using youtube-dl.exe, then piping it to ffmpeg to stream it directly.
         *  The format Discord takes is 16-bit 48000Hz PCM
         *  TODO: Catch any errors that happen when creating PCM streams.
         *  
         *  @param path - string of the source path (network)
         */
        private Process CreateNetworkStream(string path)
        {
            try
            {
                return Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C youtube-dl.exe -o - {path} | ffmpeg -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                });
            }
            catch
            {
                Console.WriteLine($"Error while opening network stream : {path}");
                return null;
            }
        }

        /**
         *  AudioPlaybackAsync
         *  Async function that handles the playback of the audio. This function is technically blocking in it's for loop.
         *  It can be broken by cancelling m_Process or when it reads to the end of the file. 
         *  At the start, m_Process, m_Stream, amd m_IsPlaying is flushed.
         *  While it is playing, these will hold values of the current playback audio. It will depend on m_Volume for the volume.
         *  In the end, the three are flushed again.
         *  
         *  @param client
         *  @param song
         */
        private async Task AudioPlaybackAsync(IAudioClient client, AudioFile song)
        {
            // Set running to true.
            m_IsRunning = true;

            // Start a new process and create an output stream. Decide between network or local.
            m_Process = (bool)song.IsNetwork ? CreateNetworkStream(song.FileName) : CreateLocalStream(song.FileName);
            m_Stream = client.CreatePCMStream(AudioApplication.Music);
            m_IsPlaying = true; // Set this to true to start the loop properly.

            await Task.Delay(5000); // We should wait for ffmpeg to buffer some of the audio first.

            // We stream the audio in chunks.
            while (true)
            {
                // If the process is already over, we're finished. If something else kills this process, we stop.
                if (m_Process == null || m_Process.HasExited) break;

                // If the stream is broken, we exit.
                if (m_Stream == null) break;

                // We pause within this function while it's 'not playing'.
                if (!m_IsPlaying) continue;

                // Read the stream in chunks.
                int blockSize = m_BLOCK_SIZE; // Size of bytes to read per frame.
                byte[] buffer = new byte[blockSize];
                int byteCount;
                byteCount = await m_Process.StandardOutput.BaseStream.ReadAsync(buffer, 0, blockSize);

                // If the stream cannot be read or we reach the end of the file, we exit.
                if (byteCount <= 0) break;

                try
                {
                    // Write out to the stream. Relies on m_Volume to adjust bytes accordingly.
                    await m_Stream.WriteAsync(ScaleVolumeSafeAllocateBuffers(buffer, m_Volume), 0, byteCount);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    break;
                }
            }

            // Kill the process, if it's lingering.
            if (m_Process != null && !m_Process.HasExited) m_Process.Kill();

            // Flush the stream and wait until it's fully done before continuing.
            if (m_Stream != null) m_Stream.FlushAsync().Wait();

            // Reset values. Basically clearing out values (Flush).
            m_Process = null;
            m_Stream = null;
            m_IsPlaying = false;

            // Set running to false.
            m_IsRunning = false;
        }

        /**
         * ScaleVolumeSafeAllocateBuffers
         * Adjusts the byte array by the volume, scaling it by a factor [0.0f,1.0f]
         * 
         * @param audioSamples - The source audio sample from the ffmpeg stream
         * @param volume - The volume to adjust to, ranges [0.0f,1.0f]
         */
        private byte[] ScaleVolumeSafeAllocateBuffers(byte[] audioSamples, float volume)
        {
            if (audioSamples == null) return null;
            if (audioSamples.Length % 2 != 0) return null;
            if (volume < 0.0f || volume > 1.0f) return null;

            // Adjust the output for the volume.
            var output = new byte[audioSamples.Length];
            try
            {
                // If it's close to full volume, we just copy it.
                if (Math.Abs(volume - 1f) < 0.0001f)
                {
                    Buffer.BlockCopy(audioSamples, 0, output, 0, audioSamples.Length);
                    return output;
                }

                // 16-bit precision for the multiplication
                int volumeFixed = (int)Math.Round(volume * 65536d);
                for (var i = 0; i < output.Length; i += 2)
                {
                    // The cast to short is necessary to get a sign-extending conversion
                    int sample = (short)((audioSamples[i + 1] << 8) | audioSamples[i]);
                    int processed = (sample * volumeFixed) >> 16;

                    output[i] = (byte)processed;
                    output[i + 1] = (byte)(processed >> 8);
                }
                return output;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        /**
         *  AdjustVolume
         *  Adjusts the current volume to the value passed. This affects the current AudioPlaybackAsync.
         *  
         *  @param volume - A value from 0.0f - 1.0f.
         */
        public void AdjustVolume(float volume)
        {
            // Adjust bounds
            if (volume < 0.0f)
                volume = 0.0f;
            else if (volume > 1.0f)
                volume = 1.0f;

            m_Volume = volume; // Update the volume
        }

        /**
         *  IsRunning
         *  Returns if AudioPlaybackAsync is currently running.
         */
        public bool IsRunning() { return m_IsRunning; }

        /**
         *  IsPlaying
         *  Returns if the process is in the middle of AudioPlaybackAsync.
         */
        public bool IsPlaying() { return ((m_Process != null) && m_IsPlaying); }

        /**
         *  Play
         *           
         *  @param client
         *  @param song
         */
        public async Task Play(IAudioClient client, AudioFile song)
        {
            // Stop the current song. We wait until it's done to play the next song.
            if (m_IsRunning) Stop();
            while (m_IsRunning) await Task.Delay(1000);

            // Start playback.
            await AudioPlaybackAsync(client, song);
        }

        /**
         *  Pause
         *  Stops the stream if it's playing. This affects the current AudioPlaybackAsync.
         */
        public void Pause() { m_IsPlaying = false; }

        /**
         *  Resume
         *  Stops the stream if it's playing. This affects the current AudioPlaybackAsync.
         */
        public void Resume() { m_IsPlaying = true; }

        /**
         *  Stop
         *  Stops the stream if it's playing. This affects the current AudioPlaybackAsync.
         */
        public void Stop() { if (m_Process != null) m_Process.Kill(); } // This basically stops the current loop by exiting the process.

    }
}
