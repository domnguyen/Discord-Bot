#define DEBUG_VERBOSE // Use this to print out all log messages to console. Comment out to disable.

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WhalesFargo.Helpers;
using WhalesFargo.Services;

namespace WhalesFargo
{
    /**
     * DiscordBot
     * Main program to run the Discord Bot.
     * 
     */
    public class DiscordBot
    {
        // Static variables.
        public static string ConnectionStatus = Strings.Disconnected;

        // Private variables.
        private DiscordSocketClient m_Client;       // Discord client.
        private CommandService m_Commands;          // Command service to link modules.
        private IServiceProvider m_Services;        // Service provider to add services to these modules.
        
        private bool m_RetryConnection = true;      // Flag for retrying connection, for the first connection.
        private const int m_RetryInterval = 1000;   // Interval in milliseconds, for each connection attempt.
        private bool m_Running = false;             // Flag for checking if it's running.
        private const int m_RunningInterval = 1000; // Interval in milliseconds to check if running.
        private bool m_DesktopNotifications = true; // Flag for desktop notifications in minimized mode.

        // Sets the connection status.
        private void SetConnectionStatus(string s, Exception arg = null)
        {
            ConnectionStatus = s;
            if (arg != null) Console.WriteLine(arg);
            if (Program.UI != null) { Program.UI.SetConnectionStatus(s); }
        }

        // This function is called, when the client is fully connected.
        private Task Connected()
        {
            SetConnectionStatus("Connected");
            return Task.CompletedTask;
        }

        // This function is called, when the client suddenly disconnects.
        private Task Disconnected(Exception arg)
        {
            SetConnectionStatus("Disconnected", arg);
            return Task.CompletedTask;
        }

        // Returns if we want to send desktop notifications in the UI, from the System Tray.
        public bool GetDesktopNotifications() { return m_DesktopNotifications; }

        // Starts the async loop.
        public async Task RunAsync()
        {
            // Already running...
            if (m_Client != null)
            {
                if (m_Client.ConnectionState == ConnectionState.Connecting ||
                    m_Client.ConnectionState == ConnectionState.Connected)
                    return;
            }

            // Start to make the connection to the server
            m_Client = new DiscordSocketClient();
            m_Commands = new CommandService(); // Start the command service to add all our commands. See 'InstallCommands'
            m_Services = InstallServices(); // We install services by adding it to a service collection.
            m_RetryConnection = true; // Always set reconnect to true. Set this to false when we cancel the connection.
            m_Running = false; // Explicit.

            // The bot will automatically reconnect once the initial connection is established. 
            // To keep trying, keep it in a loop.
            while (true)
            {
                try // Attempt to connect.
                {
                    // Set the connecting status.
                    SetConnectionStatus("Connecting");

                    // Login using the bot token.
                    await m_Client.LoginAsync(TokenType.Bot, Credentials.DiscordToken);

                    // Startup the client.
                    await m_Client.StartAsync();

                    // Install commands once the client has logged in.
                    await InstallCommands();

                    // Successfully connected and running.
                    m_Running = true;

                    break;
                }
                catch
                {
                    await Log(new LogMessage(LogSeverity.Error, "RunAsync", "Failed to connect."));
                    if (m_RetryConnection == false)
                    {
                        SetConnectionStatus("Disconnected");
                        return;
                    }
                    await Task.Delay(m_RetryInterval); // Make sure we don't reconnect too fast.
                }
            }

            // Stays in this loop while running.
            while (m_Running) { await Task.Delay(m_RunningInterval); }

            // Doesn't end the program until the whole thing is done.
            if (m_Client.ConnectionState == ConnectionState.Connecting ||
                m_Client.ConnectionState == ConnectionState.Connected)
            {
                try { m_Client.StopAsync().Wait(); }
                catch { }
            }
        }

        // In the connection loop, cancels the request.
        public async Task CancelAsync()
        {
            m_RetryConnection = false;
            await Task.Delay(0);
        }
        
        // If connected, disconnect from the server.
        public async Task StopAsync()
        {
            if (m_Running) m_Running = false;
            await Task.Delay(0);
        }

        // This is where you install all necessary services for our bot.
        // TODO: Make sure to add any additional services you want here!!
        // In those services, if you have any commands, it will automatically 
        // discovered in 'InstallCommands'
        private IServiceProvider InstallServices()
        {
            ServiceCollection services = new ServiceCollection();

            // Add all additional services here.
            services.AddSingleton<AdminService>();      // AdminModule      : AdminService
            services.AddSingleton<AudioService>();      // AudioModule      : AudioService
            services.AddSingleton<ChatService>();       // ChatModule       : ChatService

            // Return the service provider.
            return services.BuildServiceProvider();
        }

        // This is where you install all possible commands for the Discord Client.
        // Essentially, it will take the Messages Received and send it into our Handler 
        // TODO: Add any necessary functions to receive or handle Discord Socket Events.
        private async Task InstallCommands()
        {
            // Before we install commands, we should check if everything was set up properly. Check if logged in.
            if (m_Client.LoginState != LoginState.LoggedIn) return;

            // Hook the MessageReceived Event into our Command Handler
            m_Client.MessageReceived += MessageReceived;

            // Add tasks to send Messages, and userJoined to appropriate places
            m_Client.Ready += Ready;
            m_Client.UserJoined += UserJoined;
            m_Client.UserLeft += UserLeft;
            m_Client.Connected += Connected;
            m_Client.Disconnected += Disconnected;
            m_Client.Log += Log;

            // Discover all of the commands in this assembly and load them.
           await m_Commands.AddModulesAsync(Assembly.GetEntryAssembly(), m_Services);
        }

        // Handles commands with prefixes '!' and mention prefix.
        // Others get handled differently.
        private async Task MessageReceived(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(m_Client.CurrentUser, ref argPos)))
            {
                // If it isn't a command, decide what to do with it here. 
                // TODO: Add any special handlers here.
                return;
            }

            // Create a Command Context
            var context = new CommandContext(m_Client, message);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await m_Commands.ExecuteAsync(context, argPos, m_Services);
            if (!result.IsSuccess) // If failed, write error to chat.
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }

        // This sets the bots status as default. Can easily be changed. 
        private async Task Ready()
        {
            await m_Client.SetGameAsync("Type !help for help!");
        }

        // This function is called once a user joins the server.
        private async Task UserJoined(SocketGuildUser user)
        {
            var channel = user.Guild.DefaultChannel;  // You can add references to any channel you wish
            await channel.SendMessageAsync("Welcome to the Discord server" + user.Mention + "! Feel free to ask around if you need help!");
        }

        // This function is called once a user joins the server. 
        private async Task UserLeft(SocketGuildUser user)
        {
            var channel = user.Guild.DefaultChannel; // You can add references to any channel you wish
            await channel.SendMessageAsync(user.Mention + " has left the Discord server.");
        }

        // This function is used for any client logging.
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            if (Program.UI != null) Program.UI.SetConsoleText(msg.ToString());
            return Task.CompletedTask;
        }

    }
}
