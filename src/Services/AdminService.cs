using System;
using System.Threading.Tasks;
using WhalesFargo.Modules;
using Discord;

namespace WhalesFargo.Services
{
    public class AdminService
    {
        // We have a reference to the parent module to perform actions like replying and setting the current game properly.
        private AdminModule m_ParentModule = null;

        // Private variables.
      
        /**
         *  SetParentModule
         *  Sets the parent module when we start the client in AudioModule.
         *  This should always be called in the module constructor to 
         *  provide a direct reference to the parent module.
         *  
         *  @param parent - Parent AudioModule    
         */
        public void SetParentModule(AdminModule parent) { m_ParentModule = parent; }

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
         *  MuteUser
         *  Mutes the specific user.
         *  
         *  @param guild
         *  @param user
         *  @param channel
         */
        public async Task MuteUser(IGuild guild, IUser user)
        {
            try
            {
                await (user as IGuildUser).ModifyAsync(x => x.Mute = true);
                DiscordReply(user.Mention + " has been unmuted.");
            }
            catch
            {
                Console.WriteLine("Error while trying to mute " + user);
            }
        }

        /**
         *  UnmuteUser
         *  Unmutes the specific user.
         *  
         *  @param guild
         *  @param user
         *  @param channel
         */
        public async Task UnmuteUser(IGuild guild, IUser user)
        {
            try
            {
                await (user as IGuildUser).ModifyAsync(x => x.Mute = false);
                DiscordReply(user.Mention + " has been muted.");
            }
            catch
            {
                Console.WriteLine("Error while trying to mute " + user);
            }
        }

        /**
         *  KickUser
         *  kicks the specific user.
         *  
         *  @param guild
         *  @param user
         *  @param reason
         */
        public async Task KickUser(IGuild guild, IUser user, string reason = null)
        {
            try
            {
                await (user as IGuildUser).KickAsync(reason);
            }
            catch
            {
                Console.WriteLine("Error while trying to kick " + user);
            }
        }

        /**
         *  BanUser
         *  bans the specific user.
         *  
         *  @param guild
         *  @param user
         *  @param reason
         */
        public async Task BanUser(IGuild guild, IUser user, string reason = null)
        {
            try
            {
                await guild.AddBanAsync(user, 0, reason);
            }
            catch
            {
                Console.WriteLine("Error while trying to ban " + user);
            }
        }

        private IRole FindRole(IGuild guild, string name)
        {
            var roles = guild.Roles;
            foreach (IRole role in roles)
            {
                if (role.Name.Equals(name))
                    return role;
            }
            return null;
        }

        private async Task CreateRole(IGuild guild, string name)
        {
            // Let's see if the role exists.
            var role = FindRole(guild, name);
            if (role == null)
                role = await guild.CreateRoleAsync(name, GuildPermissions.All);
        }

        public async Task AddRoleUser(IGuild guild, IUser user, string name)
        {
            var role = FindRole(guild, name);
            try
            {
                if (role != null) await (user as IGuildUser).AddRoleAsync(role);
            }
            catch
            {
                Console.WriteLine("Error while trying to add the role " + name + " to " + user);
            }
        }

        public async Task RemoveRoleUser(IGuild guild, IUser user, string name)
        {
            var role = FindRole(guild, name);
            try
            {
                if (role != null) await (user as IGuildUser).RemoveRoleAsync(role);
            }
            catch
            {
                Console.WriteLine("Error while trying to remove the role " + name + " to " + user);
            }
        }

    }
}
