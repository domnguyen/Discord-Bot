using System.Linq;
using System.Threading.Tasks;
using Discord;
using WhalesFargo.Modules;

namespace WhalesFargo.Services
{
    /**
    * Chat Services
    */
    public class ChatService
    {
        // We have a reference to the parent module to perform actions like replying and setting the current game properly.
        private ChatModule m_ParentModule = null;

        /**
         *  SetParentModule
         *  Sets the parent module when we start the client in AudioModule.
         *  This should always be called in the module constructor to 
         *  provide a direct reference to the parent module.
         *  
         *  @param parent - Parent AudioModule    
         */
        public void SetParentModule(ChatModule parent) { m_ParentModule = parent; }

        /**
         *  DiscordReply
         *  Replies in the text channel using the parent module.
         *  
         *  @param s - Message to reply in the channel
         */
        private async void DiscordReply(string s)
        {
            if (m_ParentModule == null) return;
            await m_ParentModule.ServiceReplyAsync(s);
        }

        /**
         *  DiscordPlaying
         *  Sets the playing string using the parent module.
         *  
         *  @param s - Message to set the playing message to.
         */
        private async void DiscordPlaying(string s)
        {
            if (m_ParentModule == null) return;
            await m_ParentModule.ServicePlayingAsync(s);
        }

        /**
         *  SayMessage
         *  Replies in the text channel using the parent module.
         *  
         *  @param s - Message to reply in the channel
         */
        public void SayMessage(string s)
        {
            DiscordReply(s);
        }

        /**
         *  SetStatus
         *  Sets the bot playikng status.
         *  
         *  @param s - Message to set the playing message to.
         */
        public void SetStatus(string s)
        {
            DiscordPlaying(s);
        }

        /**
         *  ClearMessages
         *  Clears [num] number of messages from the current text channel.
         *  
         *  @param guild
         *  @param channel
         *  @param num
         */
        public async Task ClearMessages(IGuild guild, IMessageChannel channel, IUser user , int num)
        {
            // Check usage case.
            if (num == 0) // Check if Delete is 0, int cannot be null.
            {
                await channel.SendMessageAsync("`You need to specify the amount | !clear (amount) | Replace (amount) with anything`");
                return;
            }

            // Check permissions.
            var GuildUser = await guild.GetUserAsync(user.Id);
            if (!GuildUser.GetPermissions(channel as ITextChannel).ManageMessages)
            {
                await channel.SendMessageAsync("`You do not have enough permissions to manage messages`");
                return;
            }

            // Delete.
            var messages = await channel.GetMessagesAsync((int)num + 1).Flatten();
            await channel.DeleteMessagesAsync(messages);

            // Reply with status.
            await channel.SendMessageAsync($"`{user.Username} deleted {num} messages`");
        }

        /**
         *  MuteUser
         *  Mutes the specific user.
         *  
         *  @param guild
         *  @param user
         *  @param channel
         */
        public async Task MuteUser(IGuild guild, IUser user, IMessageChannel channel)
        {
            var role = guild.Roles.FirstOrDefault(x => x.Name == "mute");
            await (user as IGuildUser).AddRoleAsync(role);
            await channel.SendMessageAsync(user.Mention + " has been muted.");
        }

        /**
         *  UnmuteUser
         *  Unmutes the specific user.
         *  
         *  @param guild
         *  @param user
         *  @param channel
         */
        public async Task UnmuteUser(IGuild guild, IUser user, IMessageChannel channel)
        {

            var role = guild.Roles.FirstOrDefault(x => x.Name == "mute");
            await (user as IGuildUser).RemoveRoleAsync(role);
            await channel.SendMessageAsync(user.Mention + " has been unmuted.");
        }
    }

}