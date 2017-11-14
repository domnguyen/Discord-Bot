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
        /* Clears X amount of messages from the current discord channel  */
        [Command("Clear")]
        public async Task clear([Remainder] int Delete = 0)
        {
           await m_Service.clearMsg(Context.Guild, Context.Channel, Context.User ,  Delete);
        }
        // Sets the bot's playing status to whatever we want.
        [Command("botStatus")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Alias("botstatus")]
        public async Task SetBotStatus([Remainder] string botStatus)
        {
            await (Context.Client as DiscordSocketClient).SetGameAsync(botStatus);
        }
        /* This activates a response detection commmand upon certain phrases being said. The bot will respond with various sassy comments */
        [Command("ChatRespond")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Alias("sass")]
        public async Task ChatRespond()
        {
            await m_Service.checkChatRespond(Context.Channel);
        }
        /* This activates a response detection commmand upon a certain user talking. The bot will respond with various sassy comments */
        [Command("TrollUser")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Alias("troll")]
        public async Task UserRespond()
        {
            await m_Service.checkUserRespond(Context.Channel);
        }
        /* This mutes a user based on their roles. They will be added to a role where they cannot send messages.*/
        [Command("mute")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task Mute([Remainder] IGuildUser user = null)
        {
            await m_Service.mute(Context.Guild, Context.User, Context.Channel);      
        }

        /* Turn off their chat if they spam. */
        [Command("unmute")]   
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]

        public async Task Unmute([Remainder] IGuildUser user = null)
        {
            await m_Service.unmute(Context.Guild, Context.User, Context.Channel);
        }

    }
}


       