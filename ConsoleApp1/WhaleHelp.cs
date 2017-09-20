using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;

namespace WhalesFargo
{
    public static class WhaleHelp
    {
        public static DateTime Next(this DateTime from, DayOfWeek dayOfWeek)
        {
            int start = (int)from.DayOfWeek;
            int target = (int)dayOfWeek;
            if (target <= start)
                target += 7;
            return from.AddDays(target - start);
        }

        /* The function checks to see if the current time is 5 mintues before COLO.
  * If it is, it returns true.
  * Otherwise, it returns false
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
            }

            else if (DateTime.Compare(currentUTC, GuildBattle_A) > 0 & DateTime.Compare(currentUTC, GuildBattle_A_End) < 0)
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
    }
}
