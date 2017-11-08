using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace WhalesFargo
{

    /**
     * Chat
     * Class that handles the Chat response portion of the program.
     * A chat module is created here with commands that interact with the ChatService.
     */
    public class ChatModule : ModuleBase
    {
        // Private variables
        private readonly ChatService m_Service;

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot!
        public ChatModule(ChatService service)
        {
            m_Service = service;
        }


        /* Returns all commands that the bot has */
        [Command("help")]
        [Alias("help", "h")]
        public async Task HelpCommand()
        {
            /* Delete Last Message */
            await m_Service.getHelp(Context.Guild, Context.Channel);
        }

        /* Makes the bot say the message */
        [Command("say")]
        [Alias("say")]
        public async Task Say([Remainder] string usr_msg = "")
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync(usr_msg);
        }

        [Command("enchance")]
        [Alias("next","enhance")]
        public async Task CheckULEventSchedule([Remainder] string event_name = "")
        {
          
            await m_Service.getEnhance(Context.Guild, Context.Channel , event_name);
        }





    }
}


       