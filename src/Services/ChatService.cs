using Discord;
using System.Threading.Tasks;

namespace WhalesFargo.Services
{
    /**
    * ChatService
    * Handles the simple chat services like responses and manipulating chat text.
    */
    public class ChatService : CustomService
    {
        // Replies in the text channel using the parent module.
        public void SayMessage(string s)
        {
            DiscordReply(s);
        }

        // Sets the bot playing status.
        public void SetStatus(string s)
        {
            DiscordPlaying(s);
        }

        // Clears [num] number of messages from the current text channel.
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
            var messages = await channel.GetMessagesAsync((int)num + 1).FlattenAsync();
            foreach(IMessage m in messages)
            {
                await channel.DeleteMessageAsync(m.Id);
            }
           

            // Reply with status.
            Log($"{user.Username} deleted {num} messages", (int)E_LogOutput.Reply);
        }
    }
}