using System;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading;

namespace WhalesFargo
{
    public static class MyGlobals
    {
        
        public static int RTotal = 0; // can change because not const
        public static Boolean Debug = true;
    }

    class Program
    {
        private CommandService commands;
        private DiscordSocketClient client;
        private IServiceProvider services;

        

        // Bot Token. Do not share with other people
        string token = "MzM3MzI2MDYyODcyNTU5NjI2.DJumYQ.BR29W3nS1qV8HFnV_N_CBsUkfCw";
        
      



        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();
        
    
    
        public async Task MainAsync()
        {
            /* Start to make the connection to the server */
            client = new DiscordSocketClient();
            commands = new CommandService();
            services = new ServiceCollection().BuildServiceProvider();
           


            await InstallCommands();
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            
            // Send Messages, and userJoined to appropriate places
            client.Log += Log;
            client.UserJoined += UserJoined;
            client.Ready += SetBotStatus;

              // Start the Colo Reminders
              // Rerun the function every 5 minutes
              var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(1);

           
            
            
            // Doesn't end the program until the whole thing is done.
            await Task.Delay(-1);
        }
        public async Task SetBotStatus()
        {
            await client.SetGameAsync("With Rogue Tonight ;D");
        }

    private async Task CheckRouge(SocketMessage arg)
        {
            //Rogue
            ulong userID = 339836073716744194;

            //Dom
            //ulong userID = 319719246940602368;

            var user = arg.Author;
            var chnl = arg.Channel as SocketTextChannel;

            var message = arg as SocketUserMessage;

            //var channel = client.GetChannel(338430635775623180) as SocketTextChannel;

            if (MyGlobals.Debug)
            {
                Console.WriteLine("User:" + user);
                Console.WriteLine("Client User:" + client.GetUser(userID));
                Console.WriteLine("Current RogueVal: " + MyGlobals.RTotal);
            }
            if (user.Id == userID && MyGlobals.RTotal == 1 )
            {
                //Random
                Random rnd = new Random();
                int rannum = rnd.Next(1, 12);
                if (rannum == 1)
                {
                    await chnl.SendMessageAsync("Rogue, I think you're cute :D");
                }
                if (rannum == 2)
                {
                    await chnl.SendMessageAsync("Rogue's the cute one :wink:");
                }
                if (rannum == 3)
                {
                    await chnl.SendMessageAsync("Reon is a crayon");
                }
                if (rannum == 4)
                {
                    await chnl.SendMessageAsync("Cute sleepy rogue");
                }
                if (rannum == 5)
                {
                    await chnl.SendMessageAsync("Noob Lancer");
                }
                
            }

    
       

        }



        /* Welcome Message */

        public async Task UserJoined(SocketGuildUser user)
        {
            var channel = client.GetChannel(338430635775623180) as SocketTextChannel;
            /* You can add references to any channel you wish */
            await channel.SendMessageAsync("Welcome to the server" + user.Mention + "! Don't be a bitch and become a whale!");

        }

       

        /* This is where you install all possible commands for our bot.
         * Essentially, it will take the Messages Received and send it into our Handler */

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommand;
            // Discover all of the commands in this assembly and load them.
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)))
            {
                // If it isn't a command, check to see if rogue sent it.
                await CheckRouge(messageParam);
                return;
            }
            // Create a Command Context
            var context = new CommandContext(client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
        /* Bot will log to Console */





        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
