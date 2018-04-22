using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using WhalesFargo.Services;
using Discord.WebSocket;

namespace WhalesFargo.Modules
{
    public class AdminModule : CustomModule
    {
        // Private variables
        private readonly AdminService m_Service;

        // Dependencies are automatically injected via this constructor.
        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot!
        public AdminModule(AdminService service)
        {
            m_Service = service;
            m_Service.SetParentModule(this); // Reference to this from the service.
        }

        [Command("mute")]
        [Remarks("!mute [user]")]
        [Summary("This allows admins to mute users.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task MuteUser([Remainder] IGuildUser user = null)
        {
            await m_Service.MuteUser(Context.Guild, Context.User);
        }

        [Command("unmute")]
        [Remarks("!unmute [user]")]
        [Summary("This allows admins to unmute users.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task UnmuteUser([Remainder] IGuildUser user = null)
        {
            await m_Service.UnmuteUser(Context.Guild, Context.User);
        }

        [Command("kick")]
        [Remarks("!kick [user]")]
        [Summary("This allows admins to kick.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task KickUser(IGuildUser user = null, [Remainder] string reason = null)
        {
            await m_Service.KickUser(Context.Guild, user, reason);
        }

        [Command("addrole")]
        [Remarks("!addrole [user]")]
        [Summary("This allows admins to add specific roles to a user.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddRoleUser(IGuildUser user = null, [Remainder]string role = null)
        {
            await m_Service.AddRoleUser(Context.Guild, user, role);
        }

        [Command("removerole")]
        [Alias("removerole", "delrole")]
        [Remarks("!removerole [user]")]
        [Summary("This allows admins to remove specific roles to a user.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveRoleUser(IGuildUser user = null, [Remainder]string role = null)
        {
            await m_Service.RemoveRoleUser(Context.Guild, user, role);
        }


    }
}
