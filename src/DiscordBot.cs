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
using WhalesFargo.Services;

namespace WhalesFargo
{
    /**
     * Main program to run the discord bot.
     */
    public class DiscordBot
    {
        // Static variables.
        public static string ConnectionStatus = "Disconnected";

        // Private variables.
        private DiscordSocketClient m_Client;       // Discord client.
        private CommandService m_Commands;          // Command service to link modules.
        private IServiceProvider m_Services;        // Service provider to add services to these modules.
        private string m_Token = "";                // Bot Token. Do not share if you plan to hardcode it here.
        private string m_TokenFile = "";            // If we have the token in a file, make sure it's the first line.
        private bool m_RetryConnection = true;      // Flag for retrying connection, for the first connection.
        private bool m_DesktopNotifications = true; // Flag for desktop notifications in minimized mode.

        /**
         * GetDesktopNotifications
         * Returns if we want to send desktop notifications in the UI, from the System Tray.
         */
        public bool GetDesktopNotifications() { return m_DesktopNotifications; }

        /**
         * RunAsync
         * 
         */
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
            m_Commands = new CommandService();
            m_Services = InstallServices(); // We install services by adding it to a service collection.
            m_RetryConnection = true;

            // The bot will automatically reconnect once the initial connection is established. 
            // To keep trying, we put it in a loop.
            while (true)
            {
                // Attempt to connect.
                try
                {
                    // Set the connecting status.
                    SetConnectionStatus("Connecting");

                    // Get the token from the application settings.
                    string token = GetBotToken(m_TokenFile);
                    if (!token.Equals("")) m_Token = token; // Overwrite if we find it.

                    // Login using the bot token.
                    await m_Client.LoginAsync(TokenType.Bot, m_Token);

                    // Startup the client.
                    await m_Client.StartAsync();

                    // Install commands once the client has logged in.
                    await InstallCommands();

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
                }
            }

            // Doesn't end the program until the whole thing is done.
            await Task.Delay(-1);
        }

        /**
         * Cancel
         * 
         */
        public async Task CancelAsync()
        {
            m_RetryConnection = false;
            await Task.Delay(0);
        }

        /**
         *  SetToken
         *  Sets the token to be from file or direct string.
         */
        public void SetBotToken(string token)
        {
            m_Token = "";
            m_TokenFile = "";
            if (System.IO.File.Exists(token))
                m_TokenFile = token;
            else
                m_Token = token;
        }

        /**
         * GetBotToken
         * Attempt to get the bot token from BotToken.txt
         * If it doesn't exist, we can't return anything.
         */
        private string GetBotToken(string filename)
        {
            string token = "";
            if (File.Exists(filename))
            {
                token = File.ReadLines(filename).First();
            }
            return token;
        }

        /**
         * SetConnectionStatus
         * Sets the connection status.
         * 
         */
        private void SetConnectionStatus(string s, Exception arg = null)
        {
            ConnectionStatus = s;
            if (arg != null) Console.WriteLine(arg);
            if (Program.UI != null)
            {
                Program.UI.SetConnectionStatus(s);
                if (ConnectionStatus.Equals("Connected")) Program.UI.DisableConnectionToken();
            }
        }

        /**
         * InstallServices
         * This is where you install all necessary services for our bot.
         * TODO: Make sure to add additional services here!!
         */
        private IServiceProvider InstallServices()
        {
            ServiceCollection services = new ServiceCollection();

            // Add all additional services here.
            services.AddSingleton<AdminService>(); // AdminModule : AdminService
            services.AddSingleton<AudioService>(); // AudioModule : AudioService
            services.AddSingleton<ChatService>(); // ChatModule : ChatService

            // Return the service provider.
            return services.BuildServiceProvider();
        }

        /**
         * InstallCommands
         * This is where you install all possible commands for our bot.
         * Essentially, it will take the Messages Received and send it into our Handler 
         */
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
            await m_Commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        /**
         * MessageReceived
         * Handles commands with prefixes '!' and mention prefix.
         * Others get handled differently.
         * @param messageParam   The command parsed as a SocketMessage.
         */
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
                // If it isn't a command, decide what to do with it here. TODO: Add others here.
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

        /**
         * Ready
         * This sets the bots status as default. Can easily be changed. 
         */
        private async Task Ready()
        {
            await m_Client.SetGameAsync("Type !help for help!");
        }

        /**
         * UserJoined
         * This message is sent once a user joins the server.
         * 
         * @param user - A single user.
         */
        private async Task UserJoined(SocketGuildUser user)
        {
            var channel = user.Guild.DefaultChannel;  // You can add references to any channel you wish
            await channel.SendMessageAsync("Welcome to the Discord server" + user.Mention + "! Feel free to ask around if you need help!");
        }

        /**
         * UserLeft
         * This message is sent once a user joins the server. 
         * 
         * @param user - A single user.
         */
        private async Task UserLeft(SocketGuildUser user)
        {
            var channel = user.Guild.DefaultChannel; // You can add references to any channel you wish
            await channel.SendMessageAsync(user.Mention + " has left the Discord server.");
        }

        /** 
        * Connected
        * Once fully connected, prints out here.
        */
        private Task Connected()
        {
            SetConnectionStatus("Connected");
            return Task.CompletedTask;
        }

        /** 
        * Disconnected
        * Handles if the bot is suddenly disconnected.
        * 
        * @param arg - Exception thrown if disconnected for any reason.
        */
        private Task Disconnected(Exception arg)
        {
            SetConnectionStatus("Disconnected", arg);
            return Task.CompletedTask;
        }

        /**
         * Log
         * Bot will log to Console 
         * 
         * @param msg - Message to write out to Console.
         */
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            if (Program.UI != null) Program.UI.SetConsoleText(msg.ToString());
            return Task.CompletedTask;
        }

    }
}
