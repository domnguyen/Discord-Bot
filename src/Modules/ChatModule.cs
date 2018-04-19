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
    [Name("Chat")]
    [Summary("Chat module to interact with text chat.")]
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
        // DEPRECATED, DO NOT USE.
        [Command("help_deprecated_4_19_2018")]
        public async Task HelpCommand()
        {
            /* Delete Last Message */
            await m_Service.getHelp(Context.Guild, Context.Channel);
        }

        // Sets the bot's playing status to whatever we want.
        [Command("botStatus")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Alias("botstatus")]
        [Remarks("!botstatus [status]")]
        [Summary("Allows admins to set the bot's current game to [status]")]
        public async Task SetBotStatus([Remainder] string botStatus)
        {
            await (Context.Client as DiscordSocketClient).SetGameAsync(botStatus);
        }

        /* Makes the bot say the message */
        [Command("say")]
        [Alias("say")]
        [Remarks("!say [msg]")]
        [Summary("The bot will respond in the same channel with the message said.")]
        public async Task Say([Remainder] string usr_msg = "")
        {
            await Context.Message.DeleteAsync();
            await ReplyAsync(usr_msg);
        }

        [Command("enhance")]
        [Alias("next","enhance")]
        [Remarks("!next (egg/aug/augment/keymin/kesa/pasa/kesapasa/super/gold)")]
        [Summary("This returns the next augment/egg/keymin/super/gold quest.")]
        public async Task CheckULEventSchedule([Remainder] string event_name = "")
        {
            await m_Service.GetEnhance(Context.Guild, Context.Channel , event_name);
        }

        /* Clears X amount of messages from the current discord channel  */
        [Command("Clear")]
        [Remarks("!clear [num]")]
        [Summary("Allows admins to clear [num] amount of messages from current channel")]
        public async Task Clear([Remainder] int Delete = 0)
        {
           await m_Service.ClearMsg(Context.Guild, Context.Channel, Context.User ,  Delete);
        }

        /* This activates a response detection commmand upon certain phrases being said. The bot will respond with various sassy comments */
        [Command("ChatRespond")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Alias("sass")]
        [Remarks("!sass")]
        [Summary("Allows admins to turns on and off the bot's sassy responses.")]
        public async Task ChatRespond()
        {
            await m_Service.CheckChatRespond(Context.Channel);
        }

        /* This activates a response detection commmand upon a certain user talking. The bot will respond with various sassy comments */
        [Command("TrollUser")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Alias("troll")]
        [Remarks("!troll")]
        [Summary("Allows admins to activate/deactivate TrollUser")]
        public async Task UserRespond()
        {
            await m_Service.CheckUserRespond(Context.Channel);
        }

        /* This mutes a user based on their roles. They will be added to a role where they cannot send messages.*/
        [Command("mute")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [Remarks("!mute [user]")]
        [Summary("This allows admins to mute users.")]
        public async Task Mute([Remainder] IGuildUser user = null)
        {
            await m_Service.Mute(Context.Guild, Context.User, Context.Channel);      
        }

        /* Turn off mute. */
        [Command("unmute")]   
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        [Remarks("!unmute [user]")]
        [Summary("This allows admins to unmute users.")]
        public async Task Unmute([Remainder] IGuildUser user = null)
        {
            await m_Service.Unmute(Context.Guild, Context.User, Context.Channel);
        }

    }
}


       