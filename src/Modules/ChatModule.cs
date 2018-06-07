using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using WhalesFargo.Services;

namespace WhalesFargo.Modules
{

    /**
     * ChatModule
     * Class that handles the Chat response portion of the program.
     * A chat module is created here with commands that interact with the ChatService.
     */
    [Name("Chat")]
    [Summary("Chat module to interact with text chat.")]
    public class ChatModule : CustomModule
    {
        // Private variables
        private readonly ChatService m_Service;

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot!
        public ChatModule(ChatService service)
        {
            m_Service = service;
            m_Service.SetParentModule(this); // Reference to this from the service.
        }

        [Command("botStatus")]
        [Alias("botstatus")]
        [Remarks("!botstatus [status]")]
        [Summary("Allows admins to set the bot's current game to [status]")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task SetBotStatus([Remainder] string botStatus)
        {
            m_Service.SetStatus(botStatus);
            await Task.Delay(0);
        }

        [Command("say")]
        [Alias("say")]
        [Remarks("!say [msg]")]
        [Summary("The bot will respond in the same channel with the message said.")]
        public async Task Say([Remainder] string usr_msg = "")
        {
            m_Service.SayMessage(usr_msg);
            await Task.Delay(0);
        }

        [Command("Clear")]
        [Remarks("!clear [num]")]
        [Summary("Allows admins to clear [num] amount of messages from current channel")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task ClearMessages([Remainder] int num = 0)
        {
            await m_Service.ClearMessagesAsync(Context.Guild, Context.Channel, Context.User, num);
        }

    }
}


       