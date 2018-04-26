using System;
using Discord;
using WhalesFargo.Modules;

namespace WhalesFargo.Services
{
    // Enum to direct the string to output. Reference Log()
    public enum E_LogOutput { Console, Reply, Playing };

    /**
     * AudioService
     * Class that handles a single audio service.
     */
    public class CustomService
    {
        // We have a reference to the parent module to perform actions like replying and setting the current game properly.
        private CustomModule m_ParentModule = null;

        /**
         *  SetParentModule
         *  Sets the parent module when we start the client in AudioModule.
         *  This should always be called in the module constructor to 
         *  provide a direct reference to the parent module.
         *  
         *  @param parent - Parent AudioModule    
         */
        public void SetParentModule(CustomModule parent) { m_ParentModule = parent; }

        /**
         *  DiscordReply
         *  Replies in the text channel using the parent module.
         *  
         *  @param s - Message to reply in the channel
         */
        protected async void DiscordReply(string s)
        {
            if (m_ParentModule == null) return;
            await m_ParentModule.ServiceReplyAsync(s);
        }

        protected async void DiscordReply(string title, EmbedBuilder emb)
        {
            await m_ParentModule.ServiceReplyAsync(title, emb);
        }

        /**
         *  DiscordPlaying
         *  Sets the playing string using the parent module.
         *  
         *  @param s - Message to set the playing message to.
         */
        protected async void DiscordPlaying(string s)
        {
            if (m_ParentModule == null) return;
            await m_ParentModule.ServicePlayingAsync(s);
        }

        /**
         *  Log
         *  A Custom logger which can send messages to 
         *  console, reply in module, or set to playing.
         *  By default, we log everything to the console.
         *  TODO: Write so that it's an ENUM, where we can use | or &.
         *  
         *  @param s - Message to log
         *  @param output - Output source
         */
        protected void Log(string s, int output = (int)E_LogOutput.Console)
        {
            string str = $"{DateTime.Now.ToString("hh:mm:ss")} DiscordBot {s}";
#if (DEBUG_VERBOSE)
            Console.WriteLine("AudioService [DEBUG] -- " + str);
#endif
            if (output == (int)E_LogOutput.Console)
            {
                if (Program.UI != null) Program.UI.SetConsoleText(str);
                Console.WriteLine("DEBUG -- " + str);
            }
            if (output == (int)E_LogOutput.Reply) DiscordReply($"`{str}`");
            if (output == (int)E_LogOutput.Playing)
            {
                if (Program.UI != null) Program.UI.SetAudioText(str);
                DiscordPlaying(str);
            }
        }

    }
}
