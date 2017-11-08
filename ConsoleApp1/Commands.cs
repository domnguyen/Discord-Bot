
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace WhalesFargo
{


    public class Commands : ModuleBase
    {
       


        public ConcurrentDictionary<ulong, string> GuildMuteRoles { get; }


        /* Clears X amount of messages from the current discord channel  */
        [Command("Clear")]
        public async Task clear([Remainder] int Delete = 0)
        {
            IGuildUser Bot = await Context.Guild.GetUserAsync(Context.Client.CurrentUser.Id);
            if (!Bot.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`Bot does not have enough permissions to manage messages`");
                return;
            }
            await Context.Message.DeleteAsync();
            var GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
            if (!GuildUser.GetPermissions(Context.Channel as ITextChannel).ManageMessages)
            {
                await Context.Channel.SendMessageAsync("`You do not have enough permissions to manage messages`");
                return;
            }
            if (Delete == 0) // Check if Delete is 0, int cannot be null.
            {
                await Context.Channel.SendMessageAsync("`You need to specify the amount | !clear (amount) | Replace (amount) with anything`");
            }
            int Amount = 0;
            foreach (var Item in await Context.Channel.GetMessagesAsync(Delete).Flatten())
            {

                Amount++;
                await Item.DeleteAsync();

            }
            await Context.Channel.SendMessageAsync($"`{Context.User.Username} deleted {Amount} messages`");
        }

        /* This mutes a user based on their roles. They will be added to a role where they cannot send messages.*/
        [Command("mute")]
        [Summary("Turn on Mute")]
        [Alias("mute")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task Mute([Remainder] IGuildUser user = null)
        {
            Console.WriteLine(user);
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "mute");
            Console.WriteLine("The mute role is : " + role);
            await (user as IGuildUser).AddRoleAsync(role);
            await ReplyAsync(user.Mention + " has been muted.");
        }

        /* Turn off their chat if they spam. */
        [Command("unmute")]
        [Summary("Turn off Mute")]
        [Alias("unmute")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.MuteMembers)]

        public async Task Unmute([Remainder] IGuildUser user = null)
        {
            Console.WriteLine(user);
            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "mute");
            Console.WriteLine("The mute role is : " + role);
            await (user as IGuildUser).RemoveRoleAsync(role);
            await ReplyAsync(user.Mention + " has been unmuted.");


        }

        /* This activates a response detection commmand upon a certain user talking. The bot will respond with various sassy comments */
        [Command("TrollUser")]
        [Summary("Will respond to a user everytime they speak")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Alias("troll")]
        public async Task Rogue()
        {
            
            var GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
           
            // Was 0 before, so off
            if (!MyGlobals.TrollUser)
            {
                MyGlobals.TrollUser = true;
                await Task.Delay(1000);
                await ReplyAsync("`" + Context.Message.Author.Mention + " TrollUser has been activated.`");
            }
            //It was on before, so now 0
            else
            {
                MyGlobals.TrollUser = false;
                await Task.Delay(1000);
                await ReplyAsync("`" + Context.Message.Author.Mention + " TrollUser has been deactivated.`");
            }

        }

        /* This activates a response detection commmand upon certain phrases being said. The bot will respond with various sassy comments */
        [Command("ChatRespond")]
        [Summary("Turns on and off chat responses.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Alias("sass")]
        public async Task ChatRespond()
        {
            var GuildUser = await Context.Guild.GetUserAsync(Context.User.Id);
        
          
            if (!MyGlobals.PhraseRespond)
            {
                MyGlobals.PhraseRespond = true;
                await Task.Delay(1000);
                await ReplyAsync("`" + Context.Message.Author.Mention + " Bot Response has been activated.`");
            }
            
            else if (MyGlobals.PhraseRespond)
            {
                MyGlobals.PhraseRespond = false;
                await Task.Delay(1000);
                await ReplyAsync("`" + Context.Message.Author.Mention + " Bot Response has been deactivated.`");
            }

        }

        // Sets the bot's playing status to whatever we want.
        [Command("botStatus")]
        [Summary("Sets the bot's status")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Alias("botstatus")]
        public async Task SetBotStatus([Remainder] string botStatus)
        {
            await (Context.Client as DiscordSocketClient).SetGameAsync(botStatus);
        }

    }
}

    

