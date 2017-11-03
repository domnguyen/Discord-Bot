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
using Discord.Audio;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace WhalesFargo
{
    public static class MyGlobals
    {

        public static int RTotal = 0; // can change because not const
        public static Boolean Debug = false; // Turn on for cmd printing
        public static int BotScan = 0;
        public static int volume = 15;
        public static IAudioClient BotAudioClient;
        public static ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();
        
        public static ConcurrentQueue<string> songQueue = new ConcurrentQueue<String>();
    }

    class Program
    {
        private CommandService commands;
  
        
        private DiscordSocketClient client;
        private IServiceProvider services;

      


        // Bot Token. Do not share with other people
        // Developemnt token
       // string token = "Mzc1MTgxMDM5OTAwOTUwNTI5.DNsGFw.lYU7hsurbo64wrB1qsHjs8eZjz4";
        
         // Live Token
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
            client.UserLeft += UserLeft;
            client.Ready += SetBotStatus;

            // Important for the publishing of GVG announcements!
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

            Console.WriteLine("Testing Elapsed section");
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
            await colochannel.SendMessageAsync("@everyone, Guild Battle/Guild Raid will begin shortly.");
        }


        /* Sends the Colo Notification */
        public async Task SendColo()
        {
            // Gets the colo channel 
            var colochannel = client.GetChannel(357943551264555010) as SocketTextChannel;
            /* You can add references to any channel you wish */
            await colochannel.SendMessageAsync("@everyone, Coliseum will begin shortly.");
        }


        /* Check contents of various messages sent */
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
            bool op = str_message.IndexOf("op", StringComparison.OrdinalIgnoreCase) >= 0;

            // If the bot scan is on
            if (MyGlobals.BotScan == 1)
            {
                if (salt)
                {

                    Console.WriteLine("Salt activated");
                    await chnl.SendMessageAsync("https://imgur.com/1S9x2fH");
                }
                else if (fart)
                {
                    Console.WriteLine("fart activated");
                    await chnl.SendMessageAsync("https://imgur.com/1hr7CfK");

                }
                else if (noob)
                {
                    Random rnd = new Random();
                    int rannum = rnd.Next(1, 10);
                    if (rannum % 2 == 0)
                    {
                        Console.WriteLine("noob activated");
                        await chnl.SendMessageAsync("https://imgur.com/HxAkrS2");
                    }

                }
                else if (scam)
                {
                    Random rnd = new Random();
                    int rannum = rnd.Next(1, 10);
                    if (rannum % 2 == 0)
                    {
                        Console.WriteLine("scam activated");
                        await chnl.SendMessageAsync("https://imgur.com/QnQCtoN");
                    }
                }
                else if (spawn)
                {
                    Random rnd = new Random();
                    int rannum = rnd.Next(1, 10);
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
        }
        private async Task TrollRogue(SocketMessage arg)
        { 
          

            if (MyGlobals.RTotal == 1)
            {
                //User to troll's ID
                ulong userID = 339836073716744194;
                var user = arg.Author;
                var chnl = arg.Channel as SocketTextChannel;
                var message = arg as SocketUserMessage;
                if (user.Id == userID)
                {
                    Random rnd = new Random();
                    if ((rnd.Next(1, 100) % 2) == 0)
                    {
                        string msg = WhaleHelp.getTrollUserMessage();
                        await chnl.SendMessageAsync(msg);
                    }
                }
            }
        }

     
        /* This message is sent once a user joins the server. */
        public async Task UserJoined(SocketGuildUser user)
        {
            var channel = user.Guild.DefaultChannel;
            /* You can add references to any channel you wish */
            await channel.SendMessageAsync("Welcome to the Discord server" + user.Mention + "! Feel free to ask around if you need help!");

        }

        /* This message is sent once a user joins the server. */
        public async Task UserLeft(SocketGuildUser user)
        {
            var channel = user.Guild.DefaultChannel;
            /* You can add references to any channel you wish */
            await channel.SendMessageAsync(user.Mention + " has left the Discord server.");

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





        public Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
