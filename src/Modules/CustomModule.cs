using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace WhalesFargo.Modules
{
    /**
     * CustomModule
     * Base class that adds overloaded and custom functions to the ModuleBase.
     * Shared functions like reply and set playing.
     * This should be paired with the CustomService to use these functions.
     */
    public class CustomModule : ModuleBase
    {
        // Reply will allow the AudioService to reply in the correct text channel.
        public async Task ServiceReplyAsync(string s)
        {
            await ReplyAsync(s);
        }

        // Reply is the same as above except it can use the embed builder.
        public async Task ServiceReplyAsync(string title, EmbedBuilder emb)
        {
            await ReplyAsync(title, false, emb.Build()); // Text-To-Speech is off.
        }

        // Playing will allow the AudioService to set the current game.
        public async Task ServicePlayingAsync(string s)
        {
            try
            {
                await (Context.Client as DiscordSocketClient).SetGameAsync(s);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


    }
}
