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
    * Chat Services
    */
    public class ChatService
    {
       

        /* Returns the various commands built into the bot */
        public async Task getHelp(IGuild guild, IMessageChannel channel)
        {
            var emb = new EmbedBuilder();
            emb.WithTitle("This is the help command");
            emb.Color = new Color(250, 20, 20);
			/* Admin Commands */
            emb.AddField("**Admin Commands**", "*Requires admin role*", false);
            emb.AddField("!mute (user) /!unmute (user)", "This allows admins to mute/unmute annoying users.", true);
            emb.AddField("!rogue", "This turns on/off the Rogue chat detection.", true);
            emb.AddField("!sass", "Turns on and off the bot's sassy responses.", true);
            emb.AddField("!clear [num]", "Clears [num] amount of messages from current channel", true);
            emb.AddField("!botstatus [status]", "Sets the bot's current game to [status]", true);
			/* Regular Commands */
            emb.WithTitle("Regular Commands");
            emb.Color = new Color(250, 20, 20);
            emb.AddField("!next (egg/aug/augment/keymin/kesa/pasa/kesapasa/super/gold)", "This returns the next augment/egg/keymin/super/gold quest.", true);
            emb.AddField("!say [msg]", "The bot will respond in the same channel with the message said.", true);
            emb.AddField("!join ", "The bot will join the voice channel of who said the command.", true);
            emb.AddField("!stop/leave ", "Bot will end song and leave..", true);
            emb.AddField("!play [url]", "Bot will play youtube URL song.", true);
            await channel.SendMessageAsync("", false, emb);
        }

		public async Task getEnhance(IGuild guild, IMessageChannel channel,  string event_name)
        {
            /* Get current time in UTC */
            DateTime currentUTC = DateTime.UtcNow;
            /* Daylight savings adjustment */
            

            

          

            /* Check which command was run */
            bool egg = String.Equals(event_name, "egg", StringComparison.Ordinal);
            bool keymin = String.Equals(event_name, "keymin", StringComparison.Ordinal);
            bool augment = (String.Equals(event_name, "augment", StringComparison.Ordinal) || String.Equals(event_name, "aug", StringComparison.Ordinal));
            bool kesapasa = (String.Equals(event_name, "kesa", StringComparison.Ordinal) || String.Equals(event_name, "pasa", StringComparison.Ordinal) || String.Equals(event_name, "kesapasa", StringComparison.Ordinal));
            bool gold = String.Equals(event_name, "gold", StringComparison.Ordinal);
            bool super = String.Equals(event_name, "super", StringComparison.Ordinal);

            // Egg & Pasa
            DateTime EP1 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 10, 0, 0);
            DateTime EP1_End = EP1.AddHours(2);
            DateTime EP2 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 18, 0, 0);
            DateTime EP2_End = EP2.AddHours(2);
            DateTime EP3 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 2, 0, 0);
            DateTime EP3_End = EP3.AddHours(2);
            // Eggs and Keymin
            DateTime EK1 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 8, 0, 0);
            DateTime EK1_End = EK1.AddHours(2);
            DateTime EK2 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 16, 0, 0);
            DateTime EK2_End = EK2.AddHours(2);
            DateTime EK3 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 0, 0, 0);
            DateTime EK3_End = EK3.AddHours(2);
            // Keymin and Pasa
            DateTime KP1 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 4, 0, 0);
            DateTime KP1_End = KP1.AddHours(2);
            DateTime KP2 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 12, 0, 0);
            DateTime KP2_End = KP2.AddHours(2);
            DateTime KP3 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 20, 0, 0);
            DateTime KP3_End = KP3.AddHours(2);
            // Glorious Kesapasa
            DateTime GP1 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 6, 0, 0);
            DateTime GP1_End = GP1.AddHours(2);
            DateTime GP2 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 14, 0, 0);
            DateTime GP2_End = GP2.AddHours(2);
            DateTime GP3 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 22, 0, 0);
            DateTime GP3_End = GP3.AddHours(2);
            // Augment
            DateTime AUG1 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 4, 0, 0);
            DateTime AUG1_End = AUG1.AddMinutes(90);
            DateTime AUG2 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 10, 0, 0);
            DateTime AUG2_End = AUG2.AddMinutes(90);
            DateTime AUG3 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 16, 0, 0);
            DateTime AUG3_End = AUG3.AddMinutes(90);
            DateTime AUG4 = new DateTime(currentUTC.Year, currentUTC.Month, currentUTC.Day, 22, 0, 0);
            DateTime AUG4_End = AUG4.AddMinutes(90);

            /* Variables for Augment quest */
            DateTime NextSat = WhaleHelp.Next(currentUTC, DayOfWeek.Saturday);
            DateTime NextSun = WhaleHelp.Next(currentUTC, DayOfWeek.Sunday);
            DateTime NextMon = WhaleHelp.Next(currentUTC, DayOfWeek.Monday);

            //IF today is Saturday, we want to tell u dont need the next saturday
            if (currentUTC.DayOfWeek == DayOfWeek.Saturday)
            {
                NextSat = currentUTC;
            }
            // If today is sunday, set nextSun equal to today
            else if ((currentUTC.DayOfWeek == DayOfWeek.Sunday))
            {
                NextSat = currentUTC.AddDays(-1);
                NextSun = currentUTC;
            }
            else if ((currentUTC.DayOfWeek == DayOfWeek.Monday))
            {
                NextSat = currentUTC.AddDays(-2);
                NextSun = currentUTC.AddDays(-1);
                NextMon = currentUTC;
            }



            DateTime SA1 = new DateTime(NextSat.Year, NextSat.Month, NextSat.Day, 13, 0, 0);
            DateTime SA1_End = SA1.AddMinutes(20);
            DateTime SA2 = new DateTime(NextSun.Year, NextSun.Month, NextSun.Day, 1, 0, 0);
            DateTime SA2_End = SA2.AddMinutes(20);
            DateTime SA3 = new DateTime(NextSun.Year, NextSun.Month, NextSun.Day, 13, 0, 0);
            DateTime SA3_End = SA3.AddMinutes(20);
            DateTime SA4 = new DateTime(NextMon.Year, NextMon.Month, NextMon.Day, 1, 0, 0);
            DateTime SA4_End = SA4.AddMinutes(20);

            DateTime SG1 = new DateTime(NextSat.Year, NextSat.Month, NextSat.Day, 17, 30, 0);
            DateTime SG1_End = SG1.AddMinutes(20);
            DateTime SG2 = new DateTime(NextSat.Year, NextSat.Month, NextSat.Day, 23, 30, 0);
            DateTime SG2_End = SG2.AddMinutes(20);
            DateTime SG3 = new DateTime(NextSun.Year, NextSun.Month, NextSun.Day, 17, 30, 0);
            DateTime SG3_End = SG3.AddMinutes(20);
            DateTime SG4 = new DateTime(NextSun.Year, NextSun.Month, NextSun.Day, 23, 30, 0);
            DateTime SG4_End = SG4.AddMinutes(20);

            /* Create Embeded Text Discord builder */

            var emb = new EmbedBuilder();

            /* Now we check which command was called */
            if (egg)
            {


                emb.WithTitle("**Upcoming Egg Quest:**");
                
                emb.Color = new Color(250, 20, 20);
                // from 0:00:00 - 2:00:00 and ON
                // If greater than EK3 and less than EK3_end.
                if (DateTime.Compare(currentUTC, EK3) > 0 & DateTime.Compare(currentUTC, EK3_End) < 0)
                {
                    System.TimeSpan diff = EK3_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Egg and Keymin (EK03)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EP3.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);

                    emb.AddField("**Next Egg (EP03):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 2-4 and ON
                else if (DateTime.Compare(currentUTC, EP3) > 0 & DateTime.Compare(currentUTC, EP3_End) < 0)
                {
                    System.TimeSpan diff = EP3_End.Subtract(currentUTC);
                    emb.AddField("**Current : **Egg and Kesapasa (EP03)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EK1.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Egg (EK01):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 4:00:00 - 8:00:00 and OFF
                else if (DateTime.Compare(currentUTC, EP3_End) > 0 & DateTime.Compare(currentUTC, EK1) < 0)
                {
                    System.TimeSpan diff = EK1.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Egg starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                // 8:00:00 - 10:00:00 and ON
                else if (DateTime.Compare(currentUTC, EK1) > 0 & DateTime.Compare(currentUTC, EK1_End) < 0)
                {
                    System.TimeSpan diff = EK1_End.Subtract(currentUTC);
                    emb.AddField("**Current : **Egg and Keymin (EK01)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EP1.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField(" **Next Egg (EP01):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }
                // 10:00:00 - 12:00:00 and ON
                else if (DateTime.Compare(currentUTC, EP1) > 0 & DateTime.Compare(currentUTC, EP1_End) < 0)
                {
                    System.TimeSpan diff = EP1_End.Subtract(currentUTC);
                    emb.AddField("**Current : **Egg and Kesapasa (EP01)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");

                    System.TimeSpan diff2 = EK2.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Egg (EK02) :** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }
                // 12:00:00 - 16:00:00 and OFF
                else if (DateTime.Compare(currentUTC, EP1_End) > 0 & DateTime.Compare(currentUTC, EK2) < 0)
                {
                    System.TimeSpan diff = EK2.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Egg starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");
                }
                // 16:00:00 - 18:00:00 and ON
                else if (DateTime.Compare(currentUTC, EK2) > 0 & DateTime.Compare(currentUTC, EK2_End) < 0)
                {
                    System.TimeSpan diff = EK2_End.Subtract(currentUTC);
                    emb.AddField("**Current : **Egg and Keymin (EK02)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");

                    System.TimeSpan diff2 = EP2.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Egg (EP02):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }
                // 18:00:00 - 20:00:00 and ON
                else if (DateTime.Compare(currentUTC, EP2) > 0 & DateTime.Compare(currentUTC, EP2_End) < 0)
                {
                    System.TimeSpan diff = EP2_End.Subtract(currentUTC);
                    emb.AddField("**Current : **Egg and Pasa (EP02)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");

                    System.TimeSpan diff2 = EK3.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);




                    emb.AddField("**Next Egg (EK03):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 20:00:00 - 0:00:00 and OFF
                else if (DateTime.Compare(currentUTC, EP2_End) > 0 & DateTime.Compare(currentUTC, EK3) < 0)
                {
                    System.TimeSpan diff = EK3.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Egg starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");
                }


            } // End of Egg 
            else if (keymin)
            {


                emb.WithTitle("**Upcoming Keymin Quest:**");
               
                emb.Color = new Color(250, 20, 20);
                // from 0:00:00 - 2:00:00 and ON
                // If greater than EK3 and less than EK3_end.
                if (DateTime.Compare(currentUTC, EK3) > 0 & DateTime.Compare(currentUTC, EK3_End) < 0)
                {
                    System.TimeSpan diff = EK3_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Egg and Keymin (EK03)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EP3.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Keymin (KP01):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 2-4 and OFF
                else if (DateTime.Compare(currentUTC, EK3_End) > 0 & DateTime.Compare(currentUTC, KP1) < 0)
                {
                    System.TimeSpan diff = KP1.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                // 4:00:00 - 6:00:00 and ON
                else if (DateTime.Compare(currentUTC, KP1) > 0 & DateTime.Compare(currentUTC, KP1_End) < 0)
                {
                    System.TimeSpan diff = KP1_End.Subtract(currentUTC);
                    emb.AddField("**Current : **Keymin and Pasa (KP01)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EK1.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).", true);
                }

                // 6-8 and OFF
                else if (DateTime.Compare(currentUTC, KP1_End) > 0 & DateTime.Compare(currentUTC, EK1) < 0)
                {
                    System.TimeSpan diff = EK1.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }


                // 8:00:00 - 10:00:00 and ON
                else if (DateTime.Compare(currentUTC, EK1) > 0 & DateTime.Compare(currentUTC, EK1_End) < 0)
                {
                    System.TimeSpan diff = EK1_End.Subtract(currentUTC);
                    emb.AddField("**Current : **Egg and Keymin (EK01)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = KP2.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField(" **Next Keymin (KP02):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }
                // 10:00:00 - 12:00:00 and OFF
                else if (DateTime.Compare(currentUTC, EK1_End) > 0 & DateTime.Compare(currentUTC, KP2) < 0)
                {
                    System.TimeSpan diff = KP2.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                // 12-14 and ON
                else if (DateTime.Compare(currentUTC, KP2) > 0 & DateTime.Compare(currentUTC, KP2_End) < 0)
                {
                    System.TimeSpan diff = KP2_End.Subtract(currentUTC);
                    emb.AddField("**Current : **Keymin & Kesapasa (KP02)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EK2.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField(" **Next Egg (EK02):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }


                // 14:00:00 - 16:00:00 and OFF
                else if (DateTime.Compare(currentUTC, KP2_End) > 0 & DateTime.Compare(currentUTC, EK2) < 0)
                {
                    System.TimeSpan diff = EK2.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");
                }
                // 16:00:00 - 18:00:00 and ON
                else if (DateTime.Compare(currentUTC, EK2) > 0 & DateTime.Compare(currentUTC, EK2_End) < 0)
                {
                    System.TimeSpan diff = EK2_End.Subtract(currentUTC);
                    emb.AddField("**Current : **Egg and Keymin (EK02)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");

                    System.TimeSpan diff2 = KP3.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Keymin (KP03):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }


                // 18:00:00 - 20:00:00 and OFF
                else if (DateTime.Compare(currentUTC, EK2_End) > 0 & DateTime.Compare(currentUTC, KP3) < 0)
                {
                    System.TimeSpan diff = KP3.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");
                }

                // 20:00:00 - 22:00:00 and ON
                else if (DateTime.Compare(currentUTC, KP3) > 0 & DateTime.Compare(currentUTC, KP3_End) < 0)
                {
                    System.TimeSpan diff = KP3_End.Subtract(currentUTC);
                    emb.AddField("**Current : **Keymin and Pasa (KP03)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");

                    System.TimeSpan diff2 = EK3.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Keymin (EK03):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 22:00:00 - 0:00:00 and OFF
                else if (DateTime.Compare(currentUTC, KP3_End) > 0 & DateTime.Compare(currentUTC, EK3) < 0)
                {
                    System.TimeSpan diff = EK3.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");
                }




            }
            else if (kesapasa)
            {


                emb.WithTitle("**Upcoming Kesapasa Quest:**");
               
                emb.Color = new Color(250, 20, 20);
                // from 2-4  and ON
                if (DateTime.Compare(currentUTC, EP3) < 0)
               
                {
                    System.TimeSpan diff = EP3.Subtract(currentUTC);


                    emb.AddField("None are going on right now.", "**Next Kesapasa starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                   }


                else if (DateTime.Compare(currentUTC, EP3) > 0 & DateTime.Compare(currentUTC, EP3_End) < 0)
                {
                    System.TimeSpan diff = EP3_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Egg and Kesapasa (EP03)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = KP1.Subtract(currentUTC);
                    //diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Pasa (KP01):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 4-6
                else if (DateTime.Compare(currentUTC, KP1) > 0 & DateTime.Compare(currentUTC, KP1_End) < 0)
                {
                    System.TimeSpan diff = KP1_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Keymin and Kesapasa (KP01)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = GP1.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Pasa (GP01):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }
                // 6-8
                else if (DateTime.Compare(currentUTC, GP1) > 0 & DateTime.Compare(currentUTC, GP1_End) < 0)
                {
                    System.TimeSpan diff = GP1_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Glorious Kesapasa (GP01)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EP1.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Pasa (EP01):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 8-10 and OFF
                else if (DateTime.Compare(currentUTC, GP1_End) > 0 & DateTime.Compare(currentUTC, EP1) < 0)
                {
                    System.TimeSpan diff = EP1.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Kesapasa starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                // from 10-12  and ON
                else if (DateTime.Compare(currentUTC, EP1) > 0 & DateTime.Compare(currentUTC, EP1_End) < 0)
                {
                    System.TimeSpan diff = EP1_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Egg and Kesapasa (EP01)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = KP2.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Pasa (KP02):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }


                // from 12-14  and ON
                else if (DateTime.Compare(currentUTC, KP2) > 0 & DateTime.Compare(currentUTC, KP2_End) < 0)
                {
                    System.TimeSpan diff = KP2_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Keymin and Kesapasa (KP2)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = GP2.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Pasa (GP02):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // from 14-16  and ON
                else if (DateTime.Compare(currentUTC, GP2) > 0 & DateTime.Compare(currentUTC, GP2_End) < 0)
                {
                    System.TimeSpan diff = GP2_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Keymin and Kesapasa (KP2)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EP2.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Pasa (EP02):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 16-18 and OFF
                else if (DateTime.Compare(currentUTC, GP2_End) > 0 & DateTime.Compare(currentUTC, EP2) < 0)
                {
                    System.TimeSpan diff = EP2.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Kesapasa starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                // from 18-20  and ON
                else if (DateTime.Compare(currentUTC, EP2) > 0 & DateTime.Compare(currentUTC, EP2_End) < 0)
                {
                    System.TimeSpan diff = EP2_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Egg and Kesapasa (EP2)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = KP3.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Pasa (KP3):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // from 20-22  and ON
                else if (DateTime.Compare(currentUTC, KP3) > 0 & DateTime.Compare(currentUTC, KP3_End) < 0)
                {
                    System.TimeSpan diff = KP3_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Keymin and Kesapasa (KP3)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = GP3.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Pasa (GP3):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // from 22-0  and ON
                else if (DateTime.Compare(currentUTC, GP3) > 0 & DateTime.Compare(currentUTC, GP3_End) < 0)
                {
                    System.TimeSpan diff = GP3_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Glorious Kesapasa (GP3)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EP3.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Pasa (EP3):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 0-2 and OFF
                else if (DateTime.Compare(currentUTC, GP3_End) > 0 & DateTime.Compare(currentUTC, EP3) < 0)
                {
                    System.TimeSpan diff = EP3.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Kesapasa starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }



            }
            else if (augment)
            {


                emb.WithTitle("**Upcoming Augment Quest:**");
                
                emb.Color = new Color(250, 20, 20);
                 if (DateTime.Compare(currentUTC, AUG1) < 0)
                {
                    System.TimeSpan diff = AUG1.Subtract(currentUTC);


                    emb.AddField("None are going on right now.", "**Next Augment starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    
                }
                // between first aug
                else if (DateTime.Compare(currentUTC, AUG1) > 0 & DateTime.Compare(currentUTC, AUG1_End) < 0)
                {
                    System.TimeSpan diff = AUG1_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Augment (AUG1)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = AUG2.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Augment (AUG2):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // Off between 1st
                else if (DateTime.Compare(currentUTC, AUG1_End) > 0 & DateTime.Compare(currentUTC, AUG2) < 0)
                {
                    System.TimeSpan diff = AUG2.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Augment starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }


                else if (DateTime.Compare(currentUTC, AUG2) > 0 & DateTime.Compare(currentUTC, AUG2_End) < 0)
                {
                    System.TimeSpan diff = AUG2_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Augment (AUG2)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = AUG3.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Augment (AUG3):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }


                // Off between 1st
                else if (DateTime.Compare(currentUTC, AUG2_End) > 0 & DateTime.Compare(currentUTC, AUG3) < 0)
                {
                    System.TimeSpan diff = AUG3.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Augment starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }


                else if (DateTime.Compare(currentUTC, AUG3) > 0 & DateTime.Compare(currentUTC, AUG3_End) < 0)
                {
                    System.TimeSpan diff = AUG3_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Augment (AUG3)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = AUG4.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Augment (AUG4):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // Off between 1st
                else if (DateTime.Compare(currentUTC, AUG3_End) > 0 & DateTime.Compare(currentUTC, AUG4) < 0)
                {
                    System.TimeSpan diff = AUG4.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Augment starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }


                else if (DateTime.Compare(currentUTC, AUG4) > 0 & DateTime.Compare(currentUTC, AUG4_End) < 0)
                {
                    System.TimeSpan diff = AUG4_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Augment (AUG3)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = AUG1.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next Augment (AUG1):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // Off between 1st
                else if (DateTime.Compare(currentUTC, AUG4_End) > 0 & DateTime.Compare(currentUTC, AUG1) < 0)
                {
                    System.TimeSpan diff = AUG1.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Augment starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }
                emb.AddField("Super Augment", "If you are looking for super augment, do !next super", true);

            }
            else if (gold)
            {


                emb.WithTitle("**Upcoming Gold Quest:**");
                
                emb.Color = new Color(250, 250, 20);
                // Off between 1st
                if (DateTime.Compare(currentUTC, SG1) < 0)
                {
                    System.TimeSpan diff = SG1.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Gold starts in** : " + diff.ToString(@"dd") + " day(s) " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }
                // between first aug
                else if (DateTime.Compare(currentUTC, SG1) > 0 & DateTime.Compare(currentUTC, SG1_End) < 0)
                {
                    System.TimeSpan diff = SG1_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Gold (SG1)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = SG2.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next SG2 (SG2):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }
                // Off between 1st
                else if (DateTime.Compare(currentUTC, SG1_End) > 0 & DateTime.Compare(currentUTC, SG2) < 0)
                {
                    System.TimeSpan diff = SG2.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Gold starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                else if (DateTime.Compare(currentUTC, SG2) > 0 & DateTime.Compare(currentUTC, SG2_End) < 0)
                {
                    System.TimeSpan diff = SG3_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Gold (SG2)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = SG3.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next SG3 (SG3):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // Off between 1st
                else if (DateTime.Compare(currentUTC, SG2_End) > 0 & DateTime.Compare(currentUTC, SG3) < 0)
                {
                    System.TimeSpan diff = SG3.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Gold starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                else if (DateTime.Compare(currentUTC, SG3) > 0 & DateTime.Compare(currentUTC, SG3_End) < 0)
                {
                    System.TimeSpan diff = SG3_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Gold (SG3)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = SG4.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next SG4 (SG4):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // Off between 1st
                else if (DateTime.Compare(currentUTC, SG3_End) > 0 & DateTime.Compare(currentUTC, SG4) < 0)
                {
                    System.TimeSpan diff = SG4.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Gold starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                else if (DateTime.Compare(currentUTC, SG4) > 0 & DateTime.Compare(currentUTC, SG4_End) < 0)
                {
                    System.TimeSpan diff = SG4_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Gold (SG4)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = SG1.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next SG1 (SG1):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // Off between 1st
                else if (DateTime.Compare(currentUTC, SG4_End) > 0)
                {
                    System.TimeSpan diff = SG1.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Gold starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }



            }
            else if (super)
            {


                emb.WithTitle("**Upcoming Super Augment Quest:**");
                
                emb.Color = new Color(250, 250, 20);
                // between first aug
                // First case, Before 1st SA and last, so M-F times.
                if (DateTime.Compare(currentUTC, SA1) < 0 && DateTime.Compare(currentUTC, SA4) < 0)
                {
                    System.TimeSpan diff = SA1.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Super Augment starts in** : " + diff.ToString(@"dd") + " day(s) " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                else if (DateTime.Compare(currentUTC, SA1) > 0 & DateTime.Compare(currentUTC, SA1_End) < 0)
                {
                    System.TimeSpan diff = SA1_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Super Augment (SA1)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = SA2.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next SA2 (SA2):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }
                // Off between 1st
                else if (DateTime.Compare(currentUTC, SA1_End) > 0 & DateTime.Compare(currentUTC, SA2) < 0)
                {
                    System.TimeSpan diff = SA2.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Super Augment starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                else if (DateTime.Compare(currentUTC, SA2) > 0 & DateTime.Compare(currentUTC, SA2_End) < 0)
                {
                    System.TimeSpan diff = SA3_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Super Augment (SA2)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = SA3.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next SA3 (SA3):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // Off between 1st
                else if (DateTime.Compare(currentUTC, SA2_End) > 0 & DateTime.Compare(currentUTC, SA3) < 0)
                {
                    System.TimeSpan diff = SA3.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Super Augment starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                else if (DateTime.Compare(currentUTC, SA3) > 0 & DateTime.Compare(currentUTC, SA3_End) < 0)
                {
                    System.TimeSpan diff = SA3_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Super Augment (SA3)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = SA4.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next SA4 (SA4):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // Off between 1st
                else if (DateTime.Compare(currentUTC, SA3_End) > 0 & DateTime.Compare(currentUTC, SA4) < 0)
                {
                    System.TimeSpan diff = SA4.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Super Augment starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                else if (DateTime.Compare(currentUTC, SA4) > 0 & DateTime.Compare(currentUTC, SA4_End) < 0)
                {
                    System.TimeSpan diff = SA4_End.Subtract(currentUTC);


                    emb.AddField("**Current : **Super Augment (SA4)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = SA1.Subtract(currentUTC);
                    diff2 = WhaleHelp.CheckNextDay(diff2);
                    emb.AddField("**Next SA1 (SA1):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // Off between 1st
                else if (DateTime.Compare(currentUTC, SA4_End) > 0 & DateTime.Compare(currentUTC, SA1) < 0)
                {
                    System.TimeSpan diff = SA1.Subtract(currentUTC);
                    emb.AddField("None are going on right now.", "**Next Super Augment starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }


            }
            else if (!egg && !keymin && !augment && !gold && !super)
            {


                emb.WithTitle("*Error:**");
                
                emb.Color = new Color(250, 20, 20);
                emb.AddField("Error. Invalid argument.", "Please type !help for assistance.", true);


            }

            /* Then we send the information  */

            await channel.SendMessageAsync("", false, emb);
        }


        public async Task clearMsg(IGuild guild, IMessageChannel channel, IUser user , int number){

            
            
            

            var GuildUser = await guild.GetUserAsync(user.Id);
            if (!GuildUser.GetPermissions(channel as ITextChannel).ManageMessages)
            {
                await channel.SendMessageAsync("`You do not have enough permissions to manage messages`");
                return;
            }
            if (number == 0) // Check if Delete is 0, int cannot be null.
            {
                await channel.SendMessageAsync("`You need to specify the amount | !clear (amount) | Replace (amount) with anything`");
            }

            int Amount = 0;
            foreach (var Item in await channel.GetMessagesAsync(number).Flatten())
            {

                Amount++;
                await Item.DeleteAsync();

            }
            await channel.SendMessageAsync($"`{user.Username} deleted {Amount} messages`");
}

        public async Task checkChatRespond(IMessageChannel channel)
        {
            if (!MyGlobals.PhraseRespond)
            {
                MyGlobals.PhraseRespond = true;
                await Task.Delay(1000);
                await channel.SendMessageAsync("Bot Response has been activated.`");
            }

            else if (MyGlobals.PhraseRespond)
            {
                MyGlobals.PhraseRespond = false;
                await Task.Delay(1000);
                await channel.SendMessageAsync("`Bot Response has been deactivated.`");
            }
        }

        public async Task checkUserRespond(IMessageChannel channel)
        {
            if (!MyGlobals.TrollUser)
            {
                MyGlobals.TrollUser = true;
                await Task.Delay(1000);
                await channel.SendMessageAsync("`TrollUser has been activated.`");
            }
            else
            {
                MyGlobals.TrollUser = false;
                await Task.Delay(1000);
                await channel.SendMessageAsync("`TrollUser has been deactivated.`");
            }
        }

        public async Task mute(IGuild guild, IUser user, IMessageChannel channel)
        {

            var role = guild.Roles.FirstOrDefault(x => x.Name == "mute");
            await (user as IGuildUser).AddRoleAsync(role);
            await channel.SendMessageAsync(user.Mention + " has been muted.");
        }

        public async Task unmute(IGuild guild, IUser user, IMessageChannel channel)
        {

            var role = guild.Roles.FirstOrDefault(x => x.Name == "mute");
            await (user as IGuildUser).RemoveRoleAsync(role);
            await channel.SendMessageAsync(user.Mention + " has been muted.");
        }
    }

}