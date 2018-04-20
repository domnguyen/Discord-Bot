using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WhalesFargo
{
    /**
     * HelpModule
     * Class that handles the help functionality of the program.
     * This searches for all modules and prints out it's 'summary' tag.
     * If you search by module, it will print out all it's commands by it's 'summary' tag.
     * If you want it to show usage, use the "Remarks".
     */
    public class HelpModule : ModuleBase
    {
        private readonly CommandService m_Commands;
        private readonly IServiceProvider m_Provider;
        private bool m_UseRemarks = true; // Set this to true to use Remarks instead.

        public HelpModule(CommandService commands, IServiceProvider provider)
        {
            m_Commands = commands;
            m_Provider = provider;
        }

        [Command("help", RunMode = RunMode.Async)]
        [Alias("help", "h")]
        [Summary("Finds all the modules and prints out it's summary tag.")]
        public async Task HelpAsync()
        {
            // Get all the modules.
            var modules = m_Commands.Modules.Where(x => !string.IsNullOrWhiteSpace(x.Summary));

            // Create an embed builder.
            var emb = new EmbedBuilder();
            emb.WithTitle("Here is the list of modules.");

            // For each module...
            foreach (var module in modules)
            {
                bool success = false;
                foreach (var command in module.Commands)
                {
                    var result = await command.CheckPreconditionsAsync(Context, m_Provider);
                    if (result.IsSuccess)
                    {
                        success = true;
                        break;
                    }
                }
                if (!success)
                    continue;

                emb.AddField(module.Name, module.Summary);
            }

            if (emb.Fields.Count <= 0) // Added error checking in case we don't have summary tags yet.
                await ReplyAsync("Module information cannot be found, please try again later.");
            else
                await ReplyAsync("", false, emb);
        }

        [Command("help", RunMode = RunMode.Async)] // TODO: Change this once all summaries are added.
        [Alias("help", "h")]
        [Summary("Finds all the commands from a specific module and prints out it's summary tag.")]
        public async Task HelpAsync(string moduleName)
        {
            // Get the module in question.
            var module = m_Commands.Modules.FirstOrDefault(x => x.Name.ToLower() == moduleName.ToLower());

            // If null, we chose a bad module.
            if (module == null)
            {
                await ReplyAsync($"The module `{moduleName}` does not exist. Are you sure you typed the right module?");
                await HelpAsync(); // Show the list of modules again.
                return;
            }

            // Find all it's commands.
            var commands = module.Commands.Where(x => !string.IsNullOrWhiteSpace(x.Summary)).GroupBy(x => x.Name).Select(x => x.First());

            // If none of them have summaries or don't exist, return.
            if (commands.Count() == 0)
            {
                await ReplyAsync($"The module `{module.Name}` has no available commands.");
                return;
            }

            // Create an embed builder.
            var emb = new EmbedBuilder();

            // For each command...
            foreach (var command in commands)
            {
                var result = await command.CheckPreconditionsAsync(Context, m_Provider);
                if (result.IsSuccess)
                {
                    if (m_UseRemarks)
                        emb.AddField(command.Remarks, command.Summary);
                    else
                        emb.AddField(command.Aliases.First(), command.Summary);
                }
            }

            if (emb.Fields.Count <= 0) // Added error checking in case we don't have summary tags yet.
                await ReplyAsync("Command information cannot be found, please try again later.");
            else
                await ReplyAsync("", false, emb);
        }

    }
}
