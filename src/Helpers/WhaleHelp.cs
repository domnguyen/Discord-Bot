using System;
using System.Diagnostics;

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
            /* Daylight savings adjustment */
            currentUTC = currentUTC.AddHours(-1);

            // 0800-0900
            // 1500-1600
            // 1830-1930
            // 2300-2400
            // Check 5 mintues before.

            /*
            DateTime Colo1 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 2, 55, 0);
            DateTime Colo1_End = Colo1.AddMinutes(5);

            DateTime Colo2 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 11, 55, 0);
            DateTime Colo2_End = Colo2.AddMinutes(5);

            DateTime Colo3 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 18, 55, 0);
            DateTime Colo3_End = Colo3.AddMinutes(5);

            DateTime Colo4 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 22, 25, 0);
            DateTime Colo4_End = Colo4.AddMinutes(5);
            */

            // 4 AM 
            DateTime Mobius1 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 10, 55, 0);
            DateTime Mobius1_End = Mobius1.AddMinutes(5);
            
            // 11 AM PST
            DateTime Mobius2 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 17, 55, 0);
            DateTime Mobius2_End = Mobius2.AddMinutes(5);

            // 2:30 PST
            DateTime Mobius3 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 21, 25, 0);
            DateTime Mobius3_End = Mobius3.AddMinutes(5);

            // 7 PM
            DateTime Mobius4 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 1, 55, 0);
            DateTime Mobius4_End = Mobius4.AddMinutes(5);


            DateTime GuildBattleC_A = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 16, 55, 0);
            DateTime GuildBattleC_A_End = GuildBattleC_A.AddMinutes(5);

            DateTime GuildBattleC_B = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 21, 55, 0);
            DateTime GuildBattleC_B_End = GuildBattleC_B.AddMinutes(5);

            DateTime GuildBattleC_C = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 0, 55, 0);
            DateTime GuildBattleC_C_End = GuildBattleC_C.AddMinutes(5);



            DateTime GuildBattle_A = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 17, 55, 0);
            DateTime GuildBattle_A_End = GuildBattle_A.AddMinutes(5);

            DateTime GuildBattle_B = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 22, 55, 0);
            DateTime GuildBattle_B_End = GuildBattle_B.AddMinutes(5);

            DateTime GuildBattle_C = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 1, 55, 0);
            DateTime GuildBattle_C_End = GuildBattle_C.AddMinutes(5);

            /* // Disable Colo for now
            if (DateTime.Compare(currentUTC, Colo1) > 0 & DateTime.Compare(currentUTC, Colo1_End) < 0)
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

            
            if (DateTime.Compare(currentUTC, Mobius1) > 0 & DateTime.Compare(currentUTC, Mobius1_End) < 0)
            {
                return "mobius";
            }
            else if (DateTime.Compare(currentUTC, Mobius2) > 0 & DateTime.Compare(currentUTC, Mobius2_End) < 0)
            {
                return "mobius";
            }
            else if (DateTime.Compare(currentUTC, Mobius3) > 0 & DateTime.Compare(currentUTC, Mobius3_End) < 0)
            {
                return "mobius";
            }
            else if (DateTime.Compare(currentUTC, Mobius4) > 0 & DateTime.Compare(currentUTC, Mobius4_End) < 0)
            {
                return "mobius";
            } 

            if (DateTime.Compare(currentUTC, GuildBattleC_A) > 0 & DateTime.Compare(currentUTC, GuildBattleC_A_End) < 0)
            {
                return "gbc";
            }
            else if (DateTime.Compare(currentUTC, GuildBattleC_B) > 0 & DateTime.Compare(currentUTC, GuildBattleC_B_End) < 0)
            {
                return "gbc";
            }
            else if (DateTime.Compare(currentUTC, GuildBattleC_C) > 0 & DateTime.Compare(currentUTC, GuildBattleC_C_End) < 0)
            {
                return "gbc";
            }
        
            else if (DateTime.Compare(currentUTC, GuildBattle_A) > 0 & DateTime.Compare(currentUTC, GuildBattle_A_End) < 0)
            {
                return "gba";
            }
            else if (DateTime.Compare(currentUTC, GuildBattle_B) > 0 & DateTime.Compare(currentUTC, GuildBattle_B_End) < 0)
            {
                return "gba";
            }
            else if (DateTime.Compare(currentUTC, GuildBattle_C) > 0 & DateTime.Compare(currentUTC, GuildBattle_C_End) < 0)
            {
                return "gba";
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

        public static string GetResponseMessage(string str_message)
        {
            // Done 
            bool salt = str_message.IndexOf("salt", StringComparison.OrdinalIgnoreCase) >= 0;
            bool fart = str_message.IndexOf("swoosh", StringComparison.OrdinalIgnoreCase) >= 0;
            bool noob = str_message.IndexOf("noob", StringComparison.OrdinalIgnoreCase) >= 0;
            bool scam = str_message.IndexOf("scam", StringComparison.OrdinalIgnoreCase) >= 0;
            bool spawn = str_message.IndexOf("spawn", StringComparison.OrdinalIgnoreCase) >= 0;

            // Todo
            bool skumbag = str_message.IndexOf("skumbag", StringComparison.OrdinalIgnoreCase) >= 0;
            bool senpai = str_message.IndexOf("senpai", StringComparison.OrdinalIgnoreCase) >= 0;
            bool op = str_message.IndexOf("op", StringComparison.OrdinalIgnoreCase) >= 0;


            // If the bot scan is on
            if (MyGlobals.PhraseRespond)
            {
                if (salt)
                {
                    Console.WriteLine("Salt Response Activated");
                    return  "https://imgur.com/1S9x2fH";
                }
                else if (fart)
                {
                    Console.WriteLine("fart Response Activated");
                    return "https://imgur.com/1hr7CfK";
                }
                else if (noob)
                {
                    Random rnd = new Random();
                    int rannum = rnd.Next(1, 10);
                    if (rannum % 2 == 0)
                    {
                        Console.WriteLine("noob Response Activated");
                        return "https://imgur.com/HxAkrS2";
                    }
                }
                else if (scam)
                {
                    Random rnd = new Random();
                    int rannum = rnd.Next(1, 10);
                    if (rannum % 2 == 0)
                    {
                        Console.WriteLine("scam Response Activated");
                        return "https://imgur.com/QnQCtoN";
                    }
                }
                else if (spawn)
                {
                    Random rnd = new Random();
                    int rannum = rnd.Next(1, 10);
                    if (rannum == 1)
                    {
                        return "https://imgur.com/XoXcx1X \n Are you sure you want to spawn??";
                       

                    }
                    if (rannum == 2)
                    {
                        return " https://vignette2.wikia.nocookie.net/unisonleague/images/5/59/Gear-Behemoth_Icon.png \n If you spawn, you could end up with a behemoth...";
                    }
                }
                else
                {
                    return "";
                }
            }
            
                return "";
            
            
                
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

    }
}
