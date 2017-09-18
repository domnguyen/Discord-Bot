
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Linq;


namespace WhalesFargo
{
    public class Commands : ModuleBase
    {

        public ConcurrentDictionary<ulong, string> GuildMuteRoles { get; }



        /* Check Next Quest */
        [Command("help")]
        [Summary("Sends all possible commands")]
        [Alias("help", "h")]
        public async Task Help()
        {
            await Context.Message.DeleteAsync();

      
            var emb = new EmbedBuilder();
            emb.WithTitle("This is the help command");
            emb.Color = new Color(250,20,20);
            emb.AddField("**Admin Commands**", "*Requires admin role*", false);
            emb.AddField("!mute (user) /!unmute (user)", "This allows admins to mute/unmute annoying users.", true);
            emb.AddField("!rogue", "This turns on/off the rogue chat detection.", true);

            emb.AddField("**Regular Commands**", "*Coming soon!*", false);
            emb.AddField("!next (egg/aug/augment/keymin/kesa/pasa/kesapasa/super/gold)", "This returns the next augment/egg/keymin/super/gold quest.", true);
            
         
            await ReplyAsync("", false, emb);



        }


        /* Check Next Quest */
        [Command("enhance")]
        [Summary("Check next event")]
        [Alias("next", "future")]
        public async Task Enhance([Remainder] string event_name = "")
        {

            /* Get current time in UTC */
            DateTime currentETC = DateTime.UtcNow;

            /* Check which command was run */
            bool egg = String.Equals(event_name, "egg", StringComparison.Ordinal);
            bool keymin = String.Equals(event_name, "keymin", StringComparison.Ordinal);
            bool augment = (String.Equals(event_name, "augment", StringComparison.Ordinal) || String.Equals(event_name, "aug", StringComparison.Ordinal));
            bool kesapasa = (String.Equals(event_name, "kesa", StringComparison.Ordinal) || String.Equals(event_name, "pasa", StringComparison.Ordinal) || String.Equals(event_name, "kesapasa", StringComparison.Ordinal));
            bool gold = String.Equals(event_name, "gold", StringComparison.Ordinal);
            bool super = String.Equals(event_name, "super", StringComparison.Ordinal);

            // Egg & Pasa
            DateTime EP1 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 10, 0, 0);
            DateTime EP1_End = EP1.AddHours(2);
            DateTime EP2 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 18, 0, 0);
            DateTime EP2_End = EP2.AddHours(2);
            DateTime EP3 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 2, 0, 0);
            DateTime EP3_End = EP3.AddHours(2);
            // Eggs and Keymin
            DateTime EK1 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 8, 0, 0);
            DateTime EK1_End = EK1.AddHours(2);
            DateTime EK2 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 16, 0, 0);
            DateTime EK2_End = EK2.AddHours(2);
            DateTime EK3 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 0, 0, 0);
            DateTime EK3_End = EK3.AddHours(2);
            // Keymin and Pasa
            DateTime KP1 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 4, 0, 0);
            DateTime KP1_End = KP1.AddHours(2);
            DateTime KP2 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 12, 0, 0);
            DateTime KP2_End = KP2.AddHours(2);
            DateTime KP3 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 20, 0, 0);
            DateTime KP3_End = KP3.AddHours(2);
            // Glorious Kesapasa
            DateTime GP1 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 6, 0, 0);
            DateTime GP1_End = GP1.AddHours(2);
            DateTime GP2 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 14, 0, 0);
            DateTime GP2_End = GP2.AddHours(2);
            DateTime GP3 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 22, 0, 0);
            DateTime GP3_End = GP3.AddHours(2);
            // Augment
            DateTime AUG1 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 4, 0, 0);
            DateTime AUG1_End = AUG1.AddMinutes(90);
            DateTime AUG2 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 10, 0, 0);
            DateTime AUG2_End = AUG2.AddMinutes(90);
            DateTime AUG3 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 16, 0, 0);
            DateTime AUG3_End = AUG3.AddMinutes(90);
            DateTime AUG4 = new DateTime(currentETC.Year, currentETC.Month, currentETC.Day, 22, 0, 0);
            DateTime AUG4_End = AUG4.AddMinutes(90);

            if (MyGlobals.Debug)
            {
                Console.WriteLine("Egg is " + egg);
                Console.WriteLine("Kesa is " + kesapasa);
            }
            if (egg)
            {
                await Context.Message.DeleteAsync();
                var emb = new EmbedBuilder();
                emb.WithTitle("**Upcoming Egg Quest:**");
                emb.WithDescription("Requested by :" + Context.Message.Author.Mention);
                emb.Color = new Color(250, 20, 20);
                // from 0:00:00 - 2:00:00 and ON
                // If greater than EK3 and less than EK3_end.
                if (DateTime.Compare(currentETC, EK3) > 0 & DateTime.Compare(currentETC, EK3_End) < 0)
                {
                    System.TimeSpan diff = EK3_End.Subtract(currentETC);


                    emb.AddField("**Current : **Egg and Keymin (EK03)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EP3.Subtract(currentETC);
                    emb.AddField("**Next Egg (EP03):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 2-4 and ON
                else if (DateTime.Compare(currentETC, EP3) > 0 & DateTime.Compare(currentETC, EP3_End) < 0)
                {
                    System.TimeSpan diff = EP3_End.Subtract(currentETC);
                    emb.AddField("**Current : **Egg and Kesapasa (EP03)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EK1.Subtract(currentETC);
                    emb.AddField("**Next Egg (EK01):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 4:00:00 - 8:00:00 and OFF
                else if (DateTime.Compare(currentETC, EP3_End) > 0 & DateTime.Compare(currentETC, EK1) < 0)
                {
                    System.TimeSpan diff = EK1.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Egg starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                // 8:00:00 - 10:00:00 and ON
                else if (DateTime.Compare(currentETC, EK1) > 0 & DateTime.Compare(currentETC, EK1_End) < 0)
                {
                    System.TimeSpan diff = EK1_End.Subtract(currentETC);
                    emb.AddField("**Current : **Egg and Keymin (EK01)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EP1.Subtract(currentETC);
                    emb.AddField(" **Next Egg (EP01):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }
                // 10:00:00 - 12:00:00 and ON
                else if (DateTime.Compare(currentETC, EP1) > 0 & DateTime.Compare(currentETC, EP1_End) < 0)
                {
                    System.TimeSpan diff = EP1_End.Subtract(currentETC);
                    emb.AddField("**Current : **Egg and Kesapasa (EP01)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");

                    System.TimeSpan diff2 = EK2.Subtract(currentETC);
                    emb.AddField("**Next Egg (EK02) :** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }
                // 12:00:00 - 16:00:00 and OFF
                else if (DateTime.Compare(currentETC, EP1_End) > 0 & DateTime.Compare(currentETC, EK2) < 0)
                {
                    System.TimeSpan diff = EK2.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Egg starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");
                }
                // 16:00:00 - 18:00:00 and ON
                else if (DateTime.Compare(currentETC, EK2) > 0 & DateTime.Compare(currentETC, EK2_End) < 0)
                {
                    System.TimeSpan diff = EK2_End.Subtract(currentETC);
                    emb.AddField("**Current : **Egg and Keymin (EK02)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");

                    System.TimeSpan diff2 = EP2.Subtract(currentETC);
                    emb.AddField("**Next Egg (EP02):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }
                // 18:00:00 - 20:00:00 and ON
                else if (DateTime.Compare(currentETC, EP2) > 0 & DateTime.Compare(currentETC, EP2_End) < 0)
                {
                    System.TimeSpan diff = EP2_End.Subtract(currentETC);
                    emb.AddField("**Current : **Egg and Pasa (EP02)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");

                    System.TimeSpan diff2 = EK3.Subtract(currentETC);
                    emb.AddField("**Next Egg (EK03):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 20:00:00 - 0:00:00 and OFF
                else if (DateTime.Compare(currentETC, EP2_End) > 0 & DateTime.Compare(currentETC, EK3) < 0)
                {
                    System.TimeSpan diff = EK3.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Egg starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");
                }

                await ReplyAsync("", false, emb);

            } // End of Egg 
            else if (keymin)
            {
                await Context.Message.DeleteAsync();
                var emb = new EmbedBuilder();
                emb.WithTitle("**Upcoming Keymin Quest:**");
                emb.WithDescription("Requested by :" + Context.Message.Author.Mention);
                emb.Color = new Color(250, 20, 20);
                // from 0:00:00 - 2:00:00 and ON
                // If greater than EK3 and less than EK3_end.
                if (DateTime.Compare(currentETC, EK3) > 0 & DateTime.Compare(currentETC, EK3_End) < 0)
                {
                    System.TimeSpan diff = EK3_End.Subtract(currentETC);


                    emb.AddField("**Current : **Egg and Keymin (EK03)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EP3.Subtract(currentETC);
                    emb.AddField("**Next Keymin (KP01):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 2-4 and OFF
                else if (DateTime.Compare(currentETC, EK3_End) > 0 & DateTime.Compare(currentETC, KP1) < 0)
                {
                    System.TimeSpan diff = KP1.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                // 4:00:00 - 6:00:00 and ON
                else if (DateTime.Compare(currentETC, KP1) > 0 & DateTime.Compare(currentETC, KP1_End) < 0)
                {
                    System.TimeSpan diff = KP1_End.Subtract(currentETC);
                    emb.AddField("**Current : **Keymin and Pasa (KP01)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EK1.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).", true);
                }

                // 6-8 and OFF
                else if (DateTime.Compare(currentETC, KP1_End) > 0 & DateTime.Compare(currentETC, EK1) < 0)
                {
                    System.TimeSpan diff = EK1.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }


                // 8:00:00 - 10:00:00 and ON
                else if (DateTime.Compare(currentETC, EK1) > 0 & DateTime.Compare(currentETC, EK1_End) < 0)
                {
                    System.TimeSpan diff = EK1_End.Subtract(currentETC);
                    emb.AddField("**Current : **Egg and Keymin (EK01)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = KP2.Subtract(currentETC);
                    emb.AddField(" **Next Keymin (KP02):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }
                // 10:00:00 - 12:00:00 and OFF
                else if (DateTime.Compare(currentETC, EK1_End) > 0 & DateTime.Compare(currentETC, KP2) < 0)
                {
                    System.TimeSpan diff = KP2.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                // 12-14 and ON
                else if (DateTime.Compare(currentETC, KP2) > 0 & DateTime.Compare(currentETC, KP2_End) < 0)
                {
                    System.TimeSpan diff = KP2_End.Subtract(currentETC);
                    emb.AddField("**Current : **Keymin & Kesapasa (KP02)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EK2.Subtract(currentETC);
                    emb.AddField(" **Next Egg (EK02):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }


                // 14:00:00 - 16:00:00 and OFF
                else if (DateTime.Compare(currentETC, KP2_End) > 0 & DateTime.Compare(currentETC, EK2) < 0)
                {
                    System.TimeSpan diff = EK2.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");
                }
                // 16:00:00 - 18:00:00 and ON
                else if (DateTime.Compare(currentETC, EK2) > 0 & DateTime.Compare(currentETC, EK2_End) < 0)
                {
                    System.TimeSpan diff = EK2_End.Subtract(currentETC);
                    emb.AddField("**Current : **Egg and Keymin (EK02)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");

                    System.TimeSpan diff2 = KP3.Subtract(currentETC);
                    emb.AddField("**Next Keymin (KP03):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }


                // 18:00:00 - 20:00:00 and OFF
                else if (DateTime.Compare(currentETC, EK2_End) > 0 & DateTime.Compare(currentETC, KP3) < 0)
                {
                    System.TimeSpan diff = KP3.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");
                }

                // 20:00:00 - 22:00:00 and ON
                else if (DateTime.Compare(currentETC, KP3) > 0 & DateTime.Compare(currentETC, KP3_End) < 0)
                {
                    System.TimeSpan diff = KP3_End.Subtract(currentETC);
                    emb.AddField("**Current : **Keymin and Pasa (KP03)", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");

                    System.TimeSpan diff2 = EK3.Subtract(currentETC);
                    emb.AddField("**Next Keymin (EK03):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 22:00:00 - 0:00:00 and OFF
                else if (DateTime.Compare(currentETC, KP3_End) > 0 & DateTime.Compare(currentETC, EK3) < 0)
                {
                    System.TimeSpan diff = EK3.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Keymin starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).");
                }

                await ReplyAsync("", false, emb);

            } 
            else if (kesapasa)
            {
                await Context.Message.DeleteAsync();
                var emb = new EmbedBuilder();
                emb.WithTitle("**Upcoming Kesapasa Quest:**");
                emb.WithDescription("Requested by :" + Context.Message.Author.Mention);
                emb.Color = new Color(250, 20, 20);
                // from 2-4  and ON
                if (DateTime.Compare(currentETC, EP3) > 0 & DateTime.Compare(currentETC, EP3_End) < 0)
                {
                    System.TimeSpan diff = EP3_End.Subtract(currentETC);


                    emb.AddField("**Current : **Egg and Kesapasa (EP03)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EP3.Subtract(currentETC);
                    emb.AddField("**Next Pasa (KP01):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 4-6
                else if (DateTime.Compare(currentETC, KP1) > 0 & DateTime.Compare(currentETC, KP1_End) < 0)
                {
                    System.TimeSpan diff = KP1_End.Subtract(currentETC);


                    emb.AddField("**Current : **Keymin and Kesapasa (KP01)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = GP1.Subtract(currentETC);
                    emb.AddField("**Next Pasa (GP01):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }
                // 6-8
                else if (DateTime.Compare(currentETC, GP1) > 0 & DateTime.Compare(currentETC, GP1_End) < 0)
                {
                    System.TimeSpan diff = GP1_End.Subtract(currentETC);


                    emb.AddField("**Current : **Glorious Kesapasa (GP01)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EP1.Subtract(currentETC);
                    emb.AddField("**Next Pasa (EP01):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 8-10 and OFF
                else if (DateTime.Compare(currentETC, GP1_End) > 0 & DateTime.Compare(currentETC, EP1) < 0)
                {
                    System.TimeSpan diff = EP1.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Kesapasa starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                // from 10-12  and ON
                else if (DateTime.Compare(currentETC, EP1) > 0 & DateTime.Compare(currentETC, EP1_End) < 0)
                {
                    System.TimeSpan diff = EP1_End.Subtract(currentETC);


                    emb.AddField("**Current : **Egg and Kesapasa (EP01)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = KP2.Subtract(currentETC);
                    emb.AddField("**Next Pasa (KP02):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }


                // from 12-14  and ON
                else if (DateTime.Compare(currentETC, KP2) > 0 & DateTime.Compare(currentETC, KP2_End) < 0)
                {
                    System.TimeSpan diff = KP2_End.Subtract(currentETC);


                    emb.AddField("**Current : **Keymin and Kesapasa (KP2)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = GP2.Subtract(currentETC);
                    emb.AddField("**Next Pasa (GP02):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // from 14-16  and ON
                else if (DateTime.Compare(currentETC, GP2) > 0 & DateTime.Compare(currentETC, GP2_End) < 0)
                {
                    System.TimeSpan diff = GP2_End.Subtract(currentETC);


                    emb.AddField("**Current : **Keymin and Kesapasa (KP2)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EP2.Subtract(currentETC);
                    emb.AddField("**Next Pasa (EP02):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 16-18 and OFF
                else if (DateTime.Compare(currentETC, GP2_End) > 0 & DateTime.Compare(currentETC, EP2) < 0)
                {
                    System.TimeSpan diff = EP2.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Kesapasa starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                // from 18-20  and ON
                else if (DateTime.Compare(currentETC, EP2) > 0 & DateTime.Compare(currentETC, EP2_End) < 0)
                {
                    System.TimeSpan diff = EP2_End.Subtract(currentETC);


                    emb.AddField("**Current : **Egg and Kesapasa (EP2)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = KP3.Subtract(currentETC);
                    emb.AddField("**Next Pasa (KP3):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // from 20-22  and ON
                else if (DateTime.Compare(currentETC, KP3) > 0 & DateTime.Compare(currentETC, KP3_End) < 0)
                {
                    System.TimeSpan diff = KP3_End.Subtract(currentETC);


                    emb.AddField("**Current : **Keymin and Kesapasa (KP3)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = GP3.Subtract(currentETC);
                    emb.AddField("**Next Pasa (GP3):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // from 22-0  and ON
                else if (DateTime.Compare(currentETC, GP3) > 0 & DateTime.Compare(currentETC, GP3_End) < 0)
                {
                    System.TimeSpan diff = GP3_End.Subtract(currentETC);


                    emb.AddField("**Current : **Glorious Kesapasa (GP3)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = EP3.Subtract(currentETC);
                    emb.AddField("**Next Pasa (EP3):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // 0-2 and OFF
                else if (DateTime.Compare(currentETC, GP3_End) > 0 & DateTime.Compare(currentETC, EP3) < 0)
                {
                    System.TimeSpan diff = EP3.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Kesapasa starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }
                await ReplyAsync("", false, emb);
            }

           
            /* TODO: Need to add super augment */
            else if (augment)
            {
                await Context.Message.DeleteAsync();
                var emb = new EmbedBuilder();
                emb.WithTitle("**Upcoming Augment Quest:**");
                emb.WithDescription("Requested by :" + Context.Message.Author.Mention);
                emb.Color = new Color(250, 20, 20);
                // between first aug
                if (DateTime.Compare(currentETC, AUG1) > 0 & DateTime.Compare(currentETC, AUG1_End) < 0)
                {
                    System.TimeSpan diff = AUG1_End.Subtract(currentETC);


                    emb.AddField("**Current : **Augment (AUG1)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = AUG2.Subtract(currentETC);
                    emb.AddField("**Next Augment (AUG2):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // Off between 1st
                else if (DateTime.Compare(currentETC, AUG1_End) > 0 & DateTime.Compare(currentETC, AUG2) < 0)
                {
                    System.TimeSpan diff = AUG2.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Augment starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }


                else if (DateTime.Compare(currentETC, AUG2) > 0 & DateTime.Compare(currentETC, AUG2_End) < 0)
                {
                    System.TimeSpan diff = AUG2_End.Subtract(currentETC);


                    emb.AddField("**Current : **Augment (AUG2)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = AUG3.Subtract(currentETC);
                    emb.AddField("**Next Augment (AUG3):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }


                // Off between 1st
                else if (DateTime.Compare(currentETC, AUG2_End) > 0 & DateTime.Compare(currentETC, AUG3) < 0)
                {
                    System.TimeSpan diff = AUG3.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Augment starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }


                else if (DateTime.Compare(currentETC, AUG3) > 0 & DateTime.Compare(currentETC, AUG3_End) < 0)
                {
                    System.TimeSpan diff = AUG3_End.Subtract(currentETC);


                    emb.AddField("**Current : **Augment (AUG3)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = AUG4.Subtract(currentETC);
                    emb.AddField("**Next Augment (AUG4):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // Off between 1st
                else if (DateTime.Compare(currentETC, AUG3_End) > 0 & DateTime.Compare(currentETC, AUG4) < 0)
                {
                    System.TimeSpan diff = AUG4.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Augment starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }


                else if (DateTime.Compare(currentETC, AUG4) > 0 & DateTime.Compare(currentETC, AUG4_End) < 0)
                {
                    System.TimeSpan diff = AUG4_End.Subtract(currentETC);


                    emb.AddField("**Current : **Augment (AUG3)  ", "**Remaining Time :** " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);

                    System.TimeSpan diff2 = AUG1.Subtract(currentETC);
                    emb.AddField("**Next Augment (AUG1):** ", "**Starts in : **" + diff2.ToString(@"hh") + " hour(s) " + "and " + diff2.ToString(@"mm") + " minute(s).");
                }

                // Off between 1st
                else if (DateTime.Compare(currentETC, AUG4_End) > 0 & DateTime.Compare(currentETC, AUG1) < 0)
                {
                    System.TimeSpan diff = AUG1.Subtract(currentETC);
                    emb.AddField("None are going on right now.", "**Next Augment starts in** : " + diff.ToString(@"hh") + " hour(s) " + "and " + diff.ToString(@"mm") + " minute(s).", true);
                }

                await ReplyAsync("", false, emb);
            }
            else if (gold)
            {

            }
            else if (super)
            {

            }
            else
            {
                await Context.Message.DeleteAsync();
                var emb = new EmbedBuilder();
                emb.WithTitle("*Error:**");
                emb.WithDescription("Requested by :" + Context.Message.Author.Mention);
                emb.Color = new Color(250, 20, 20);
                emb.AddField("Error. Invalid argument.", "Please type !help for assistance.", true);
                await ReplyAsync("", false, emb);
               
            }
        }
        

        /* Add I am */



        /* Turn off their chat if they spam. */
        [Command("mute")]
        [Summary("Turn on Mute")]
        [Alias("mute")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
      
        public async Task Mute([Remainder] IGuildUser user = null)
        {



            Console.WriteLine(user);
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "mute");
            Console.WriteLine("The mute role is : " + role);
            await (user as IGuildUser).AddRoleAsync(role);
            await ReplyAsync(user.Mention + " has been muted.");

           
        }

        /* Turn off their chat if they spam. */
        [Command("unmute")]
        [Summary("Turn off Mute")]
        [Alias("unmute")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]

        public async Task Unmute([Remainder] IGuildUser user = null)
        {



            Console.WriteLine(user);
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "mute");
            Console.WriteLine("The mute role is : " + role);
            await (user as IGuildUser).RemoveRoleAsync(role);
            await ReplyAsync(user.Mention + " has been unmuted.");


        }




        /* Remind Colo On */
        [Command("colo")]
        [Summary("Turn on Colo Reminders")]
        [Alias("colo", "coliremind", "coli")]

        public async Task Colo()
        {
                var now = DateTime.Now;
                // 5am 
                var colo_time_1 = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0);
                var colo_time_2 = new DateTime(now.Year, now.Month, now.Day, 15, 0, 0);
                var colo_time_3 = new DateTime(now.Year, now.Month, now.Day, 18, 30, 0);
                var colo_time_4 = new DateTime(now.Year, now.Month, now.Day, 23, 34, 0);

                /* Establish the UTC Timezone */
                TimeZoneInfo eastInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

                /* Get current time, and next time with 5 minute interval */
                DateTime currentETC = DateTime.UtcNow;
                DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(currentETC, eastInfo);
                DateTime currentETC5 = easternTime.AddMinutes(5);
                if (
                      // if current, ex 4:55 and 5:00 and 5:00 and 5:00 , so exactly 5 minutes before
                      (DateTime.Compare(currentETC, colo_time_1) < 0 & DateTime.Compare(currentETC5, colo_time_1) == 0) ||
                      (DateTime.Compare(currentETC, colo_time_2) < 0 & DateTime.Compare(currentETC5, colo_time_2) == 0) ||
                      (DateTime.Compare(currentETC, colo_time_3) < 0 & DateTime.Compare(currentETC5, colo_time_3) == 0) ||
                      (DateTime.Compare(currentETC, colo_time_4) < 0 & DateTime.Compare(currentETC5, colo_time_4) == 0))
                {
                    //If we get a valid time, we return one.
                    await Context.Channel.SendMessageAsync("@Everyone, Coliseum will begin shortly.");
                }
                else
                {
                    // If the time is not valid, then return a 0.
                    Console.WriteLine("Colisium not up.");

                }
                await Task.Delay(10000);
            
        }

        [Command("rogue")]
        [Summary("turn on and off Rogue")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Alias("rogue")]
        public async Task Rogue()
        {
            var rogue = MyGlobals.RTotal;
            var GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
            if (MyGlobals.Debug)
            {
                Console.WriteLine(rogue);
            }
                // Was 0 before, so off
                if (rogue == 0)
                {
                    MyGlobals.RTotal = 1;
                    await Task.Delay(1000);
                    await ReplyAsync(Context.Message.Author.Mention + " Rogue has been activated.");
                }
                //It was on before, so now 0
                if (rogue == 1)
                {
                    MyGlobals.RTotal = 0;
                    await Task.Delay(1000);
                    await ReplyAsync(Context.Message.Author.Mention + " Rogue has been deactivated.");
                }
            
        }
        }
    }

