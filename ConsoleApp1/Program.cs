using System;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Threading;
using System.ComponentModel;
using System.Timers;
using System.Linq;

namespace WhalesFargo
{
    public static class MyGlobals
    {

        public static int RTotal = 0; // can change because not const
        public static Boolean Debug = false;


    }

    class Program
    {
        private CommandService commands;
        private DiscordSocketClient client;
        private IServiceProvider services;


        // Bot Token. Do not share with other people
        string token = "MzM3MzI2MDYyODcyNTU5NjI2.DJumYQ.BR29W3nS1qV8HFnV_N_CBsUkfCw";


        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();


        /* The main task runner */

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


            // Interval of 5 minutes
            const double interval5Minutes = 4 * 30 * 1000; 
            // Creates a new system timer, and checks every 5 minutes for elapsed time.
            System.Timers.Timer checkForTime = new System.Timers.Timer(interval5Minutes);
            checkForTime.Elapsed += new ElapsedEventHandler(CheckForTime_ElapsedAsync);
            checkForTime.Enabled = true;


            // Doesn't end the program until the whole thing is done.
            await Task.Delay(-1);
        }

        /* This sets the bots status as default. Can easily be changed. */
        public async Task SetBotStatus()
        {
            await client.SetGameAsync("With Rogue Tonight ;D");
        }

        /* Checks time elapsed. Essential for the auto messasging of channels */
        public async void CheckForTime_ElapsedAsync(object sender, ElapsedEventArgs e)
        {

            string event_name = WhaleHelp.TimeIsReady();
            bool colo = String.Equals(event_name, "colo", StringComparison.Ordinal);
            bool gb = String.Equals(event_name, "gb", StringComparison.Ordinal);

            if (colo){
                SendColo();
            }
            else if (gb)
            {
                SendGb();
            }
        }


        /* Sends the Guild Battle Notification */
        public async Task SendGb()
        {
            // Gets the colo channel 
            var colochannel = client.GetChannel(223181247902515210) as SocketTextChannel;
            /* You can add references to any channel you wish */
            await colochannel.SendMessageAsync("@Team A, Guild Battle/Guild Raid will begin shortly.");
        }


        /* Sends the Colo Notification */
        public async Task SendColo()
        {
            // Gets the colo channel 
            var colochannel = client.GetChannel(357943551264555010) as SocketTextChannel;
            /* You can add references to any channel you wish */
            await colochannel.SendMessageAsync("@everyone, Coliseum will begin shortly.");
        }

        private async Task CheckSenpai(SocketMessage arg)
        {
            var user = arg.Author;
            var chnl = arg.Channel as SocketTextChannel;
            var message = arg as SocketUserMessage;
            string str_message = message.ToString();

            // Done 
            bool salt = str_message.IndexOf("salt", StringComparison.OrdinalIgnoreCase) >= 0;
            bool fart = str_message.IndexOf("swoosh", StringComparison.OrdinalIgnoreCase) >= 0;
            bool noob = str_message.IndexOf("noob", StringComparison.OrdinalIgnoreCase) >= 0;
            bool scam = str_message.IndexOf("scam", StringComparison.OrdinalIgnoreCase) >= 0;
            bool spawn = str_message.IndexOf("spawn", StringComparison.OrdinalIgnoreCase) >= 0;

            // Todo
            bool skumbag = str_message.IndexOf("skumbag", StringComparison.OrdinalIgnoreCase) >= 0;
            bool senpai = str_message.IndexOf("senpai", StringComparison.OrdinalIgnoreCase) >= 0;
            if (salt)
            {
                Console.WriteLine("Salt activated");
                await chnl.SendMessageAsync("https://imgur.com/1S9x2fH" );
            }
            else if (fart)
            {
                Console.WriteLine("fart activated");
                await chnl.SendMessageAsync("https://imgur.com/1hr7CfK");
                
            }
            else if (noob)
            {
                Console.WriteLine("noob activated");
                await chnl.SendMessageAsync("https://imgur.com/HxAkrS2");
                
            }
            else if (scam)
            {
                Console.WriteLine("scam activated");
                await chnl.SendMessageAsync("https://imgur.com/QnQCtoN");
                
            }
            else if (spawn)
            {
                Random rnd = new Random();
                int rannum = rnd.Next(1, 6);
                if (rannum == 1)
                {
                    await chnl.SendMessageAsync("https://imgur.com/XoXcx1X");
                    await chnl.SendMessageAsync("Are you sure you want to spawn??");

                }
                if (rannum == 2)
                {
                    await chnl.SendMessageAsync("https://vignette2.wikia.nocookie.net/unisonleague/images/5/59/Gear-Behemoth_Icon.png");
                    await chnl.SendMessageAsync("If you spawn, you could end up with a behemoth...");
                }
            }
            //await chnl.SendMessageAsync("Rogue, I think you're cute :D");

        }
        private async Task TrollRogue(SocketMessage arg)
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

      

        /* This message is sent once a user joins the server. */

        public async Task UserJoined(SocketGuildUser user)
        {
            var channel = client.GetChannel(223181247902515210) as SocketTextChannel;
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
                await TrollRogue(messageParam);
                await CheckSenpai(messageParam);
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
