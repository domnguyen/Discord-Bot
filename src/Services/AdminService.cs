using Discord;
using System.Threading.Tasks;
using WhalesFargo.Helpers;

namespace WhalesFargo.Services
{
    /**
    * AdminService
    * Simple service for performing administrative functions. This is typically for admin users.
    * We place simple restrictions on this, but can be handled in any way.
    */
    public class AdminService : CustomService
    {
        // Private variables. 
        // TODO: Add any here.

        // Changes the prefix.
        public async Task ChangePrefix(char prefix)
        {
            Config.Instance.Prefix = prefix;
            Config.Instance.Write(); // Update local file
            Log($"Prefix has been changed to {prefix}", (int)E_LogOutput.Reply);
            await Task.Delay(0);
        }

        // Mutes the specific user.
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

        // Unmutes the specific user.
        public async Task UnmuteUser(IGuild guild, IUser user)
        {
            try
            {
                await (user as IGuildUser).ModifyAsync(x => x.Mute = false);
                Log($"{user.Mention} has been unmuted.", (int)E_LogOutput.Reply);
            }
            catch
            {
                Log($"Error while trying to unmute {user}.");
            }
        }

        // Kicks the specific user.
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

        // Bans the specific user.
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

        // From the list of roles, find a role by name.
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

        // Create a new role by name, given that it doesn't exist already.
        private async Task CreateRole(IGuild guild, string name)
        {
            // Let's see if the role exists.
            var role = FindRole(guild, name);
            if (role == null)
                role = await guild.CreateRoleAsync(name, GuildPermissions.All);
        }

        // Adds a role by name to the user's roles.
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

        // Removes a role by name from the user's roles.
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
