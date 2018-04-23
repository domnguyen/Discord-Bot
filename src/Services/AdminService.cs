using Discord;
using System;
using System.Threading.Tasks;

namespace WhalesFargo.Services
{
    public class AdminService : CustomService
    {
        // Private variables.
      
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
                Log($"{user.Mention} has been muted.", (int)E_LogOutput.Reply);
            }
            catch
            {
                Log($"Error while trying to mute {user}.");
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
                Log($"{user.Mention} has been unmuted.", (int)E_LogOutput.Reply);
            }
            catch
            {
                Log($"Error while trying to unmute {user}." );
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
                Log($"Error while trying to kick {user}.");
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
                Log($"Error while trying to ban {user}.");
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
                Log($"Error while trying to add the role {name} to {user}.");
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
                Log($"Error while trying to remove the role {name} to {user}.");
            }
        }

    }
}
