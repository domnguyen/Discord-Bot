using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace WhalesFargo
{
    public static class WhaleHelp
    {
        /* Helper function to obtain the next DayofWeek from the current */
        public static DateTime Next(this DateTime from, DayOfWeek dayOfWeek)
        {
            int start = (int)from.DayOfWeek;
            int target = (int)dayOfWeek;
            if (target <= start)
                target += 7;
            return from.AddDays(target - start);
        }

        /* The function checks to see if the current time is 5 mintues before Guild Battle and Colo.
        *  If it is, it returns a string containing which event is about to occur.
        *  Otherwise, it returns none.
        */
        public static string TimeIsReady()
        {
            DateTime currentUTC = DateTime.UtcNow;
            // 0800-0900
            // 1500-1600
            // 1830-1930
            // 2300-2400
            // Check 5 mintues before.
            DateTime Colo1 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 2, 55, 0);
            DateTime Colo1_End = Colo1.AddMinutes(5);

            DateTime Colo2 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 11, 55, 0);
            DateTime Colo2_End = Colo2.AddMinutes(5);

            DateTime Colo3 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 18, 55, 0);
            DateTime Colo3_End = Colo3.AddMinutes(5);

            DateTime Colo4 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 22, 25, 0);
            DateTime Colo4_End = Colo4.AddMinutes(5);


            DateTime GuildBattle_A = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 17, 55, 0);
            DateTime GuildBattle_A_End = GuildBattle_A.AddMinutes(5);

            DateTime GuildBattle_B = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 22, 55, 0);
            DateTime GuildBattle_B_End = GuildBattle_B.AddMinutes(5);

            DateTime GuildBattle_C = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 1, 55, 0);
            DateTime GuildBattle_C_End = GuildBattle_C.AddMinutes(5);

            // Disable Colo for now
           /* if (DateTime.Compare(currentUTC, Colo1) > 0 & DateTime.Compare(currentUTC, Colo1_End) < 0)
            {
                return "colo";
            }
            else if (DateTime.Compare(currentUTC, Colo2) > 0 & DateTime.Compare(currentUTC, Colo2_End) < 0)
            {
                return "colo";
            }
            else if (DateTime.Compare(currentUTC, Colo3) > 0 & DateTime.Compare(currentUTC, Colo3_End) < 0)
            {
                return "colo";
            }
            else if (DateTime.Compare(currentUTC, Colo4) > 0 & DateTime.Compare(currentUTC, Colo4_End) < 0)
            {
                return "colo";
            } */

            if (DateTime.Compare(currentUTC, GuildBattle_A) > 0 & DateTime.Compare(currentUTC, GuildBattle_A_End) < 0)
            {
                return "gb";
            }
            else if (DateTime.Compare(currentUTC, GuildBattle_B) > 0 & DateTime.Compare(currentUTC, GuildBattle_B_End) < 0)
            {
                return "gb";
            }
            else if (DateTime.Compare(currentUTC, GuildBattle_C) > 0 & DateTime.Compare(currentUTC, GuildBattle_C_End) < 0)
            {
                return "gb";
            }
            else
            {
                return "none";
            }

        }

        public static string getTrollUserMessage()
        {
            Random rnd = new Random();
                int rannum = rnd.Next(1, 5);
                if (rannum == 1)
                    {
                        return "Rogue, I think you're cute :D";
                    }
                else if (rannum == 2)
                    {
                 return "Rogue's the cute one :wink:";
                    }
                else if (rannum == 3)
                {
                return "Reon is a crayon";
                }
                else if (rannum == 4)
                {
                return "Cute sleepy rogue";
                }
                else if (rannum == 5){
                return "Noob Lancer";
                }
            else
            {
                return "none";
            }
           
        }

        public static TimeSpan CheckNextDay(TimeSpan current)
        {
            // If the current - today is a negative time, add a day.
            Console.WriteLine("Current time is : " + current);
            Console.WriteLine("The zero time is : " + new TimeSpan(0, 0, 0));
            if (TimeSpan.Compare(current, new TimeSpan(0, 0, 0)) == -1)
            {
                Console.WriteLine("We added an day");
                TimeSpan toReturn = current.Add(new TimeSpan(1, 0, 0, 0));
                return toReturn;
            }
            else
            {
                return current;
            }
            
        }

        public static bool IsRunning(this Process process)
        {
            try { Process.GetProcessById(process.Id); }
            catch (InvalidOperationException) { return false; }
            catch (ArgumentException) { return false; }
            return true;
        }

        public static Process CreateStream(string url)
        {
            Process currentsong = new Process();

            currentsong.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C youtube-dl.exe -o - {url} | ffmpeg -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            
            currentsong.Start();
            return currentsong;
        }


        public static byte[] ScaleVolumeSafeAllocateBuffers(byte[] audioSamples, float volume)
        {
            Contract.Requires(audioSamples != null);
            Contract.Requires(audioSamples.Length % 2 == 0);
            Contract.Requires(volume >= 0f && volume <= 1f);

            var output = new byte[audioSamples.Length];
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
    }
}
