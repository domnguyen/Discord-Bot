using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using WhalesFargo.Services;

namespace WhalesFargo.Modules
{
    /**
     * AdminModule
     * Perform administrative level commands.
     */
    [Name("Admin")]
    [Summary("Admin module to manage this discord server.")]
    public class AdminModule : CustomModule
    {
        // Private variables
        private readonly AdminService m_Service;

        // Dependencies are automatically injected via this constructor.
        // Remember to add an instance of the service.
        // to your IServiceCollection when you initialize your bot!
        public AdminModule(AdminService service)
        {
            m_Service = service;
            m_Service.SetParentModule(this); // Reference to this from the service.
        }

        [Command("mute")]
        [Remarks("mute [user]")]
        [Summary("This allows admins to mute users.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task MuteUser([Remainder] IGuildUser user)
        {
            await m_Service.MuteUser(Context.Guild, user);
        }

        [Command("unmute")]
        [Remarks("unmute [user]")]
        [Summary("This allows admins to unmute users.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task UnmuteUser([Remainder] IGuildUser user)
        {
            await m_Service.UnmuteUser(Context.Guild, user);
        }

        [Command("kick")]
        [Remarks("kick [user] [reason]")]
        [Summary("This allows admins to kick users.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task KickUser(IGuildUser user, [Remainder] string reason = null)
        {
            await m_Service.KickUser(Context.Guild, user, reason);
        }

        [Command("ban")]
        [Remarks("ban [user] [reason]")]
        [Summary("This allows admins to ban users.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanUser(IGuildUser user, [Remainder] string reason = null)
        {
            await m_Service.BanUser(Context.Guild, user, reason);
        }

        [Command("addrole")]
        [Remarks("addrole [user]")]
        [Summary("This allows admins to add specific roles to a user.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task AddRoleUser(IGuildUser user, [Remainder]string role)
        {
            await m_Service.AddRoleUser(Context.Guild, user, role);
        }

        [Command("removerole")]
        [Alias("removerole", "delrole")]
        [Remarks("delrole [user]")]
        [Summary("This allows admins to remove specific roles to a user.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task RemoveRoleUser(IGuildUser user, [Remainder]string role)
        {
            await m_Service.RemoveRoleUser(Context.Guild, user, role);
        }
    }
}
