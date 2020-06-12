using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WhalesFargo.Helpers;

namespace WhalesFargo.Services
{
    /**
    * ChatService
    * Handles the simple chat services like responses and manipulating chat text.
    */
    public class MapleService : CustomService
    {
        // Replies in the text channel using the parent module.
        public async Task CheckFlagRace(DiscordSocketClient _client)
        {
            DateTime currentTime = DateTime.Now;
            List<DateTime> flagTimes = GetFlagRaceTimes();
            Log(currentTime.ToLongTimeString());

          


            foreach (DateTime d in flagTimes)
            {
                // get 5 minutes before
                DateTime Time_One = d.AddMinutes(-4); // Make sure this is Less than the other time.
                DateTime Time_Two = d.AddMinutes(-3);
                // If the time is between 5 minutes of the Flag, we'll send a message.
                if (currentTime > Time_One && currentTime < Time_Two)
                {


                    // ulong id = 695711628464750662; // Lily General
                    ulong debug_id = 365341595412725771; // Dom's debug
                    var debug_chnl = _client.GetChannel(debug_id) as IMessageChannel; // 4

                    ulong lilyID = 695711628464750662; // Lily General
                    var lily_chnl = _client.GetChannel(lilyID) as IMessageChannel; // 4

                  

                    await debug_chnl.SendMessageAsync("<@&"+381673804612501515+">" + "Announcement! Flag Race Starting Soon!"); // 5

                    await lily_chnl.SendMessageAsync("<@&"+701556072284160061+"> Flag Race Starting Soon!"); // 5

                    return;

                }


            }

            return;







        }


        private List<DateTime> GetFlagRaceTimes()
        {
            List<DateTime> toRet = new List<DateTime>();

            DateTime utcNow = DateTime.Now.Date;

            DateTime first = utcNow.AddHours(5);

            //DateTime first = utcNow.AddHours(12);
            DateTime second = utcNow.AddHours(12);

            DateTime third = utcNow.AddHours(14);

            DateTime fourth = utcNow.AddHours(15);

            DateTime fifth = utcNow.AddHours(16);

            toRet.Add(first);
            toRet.Add(second);
            toRet.Add(third);
            toRet.Add(fourth);
            toRet.Add(fifth);

            return toRet;

        }

        // Sets the bot playing status.

    }

}