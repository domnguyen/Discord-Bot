using Discord;
using System.Threading.Tasks;

namespace WhalesFargo.Services
{
    /**
    * Chat Services
    */
    public class ChatService : CustomService
    {
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
        public async Task ClearMessagesAsync(IGuild guild, IMessageChannel channel, IUser user , int num)
        {
            // Check usage case.
            if (num == 0) // Check if Delete is 0, int cannot be null.
            {
                Log("You need to specify the amount | !clear (amount) | Replace (amount) with anything", (int)E_LogOutput.Reply);
                return;
            }

            // Check permissions.
            var GuildUser = await guild.GetUserAsync(user.Id);
            if (!GuildUser.GetPermissions(channel as ITextChannel).ManageMessages)
            {
                Log("You do not have enough permissions to manage messages", (int)E_LogOutput.Reply);
                return;
            }

            // Delete.
            var messages = await channel.GetMessagesAsync((int)num + 1).Flatten();
            await channel.DeleteMessagesAsync(messages);

            // Reply with status.
            Log($"{user.Username} deleted {num} messages", (int)E_LogOutput.Reply);
        }
    }

}