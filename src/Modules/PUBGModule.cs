using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using WhalesFargo.Services;
using Pubg.Net;
using System;

namespace WhalesFargo.Modules
{

    /**
     * ChatModule
     * Class that handles the Chat response portion of the program.
     * A chat module is created here with commands that interact with the ChatService.
     */
    [Name("PUBG")]
    [Summary("Module Used to interact with the PUBG API")]
    public class PUBGModule : CustomModule
    {
        // Private variables
        private readonly PUBGService m_Service;

        // Remember to add an instance of the AudioService
        // to your IServiceCollection when you initialize your bot!
        public PUBGModule(PUBGService service)
        {
            m_Service = service;
            m_Service.SetParentModule(this); // Reference to this from the service.
        }

        [Command("seasons")]
        [Alias("seasons")]
        [Remarks("!seasons")]
        [Summary("The bot will pull the seasons")]
        public async Task Seasons([Remainder] string userName = "")
        {
           var pubgSeason = new PubgSeasonService();

            /*
           var totalSeasons =  pubgSeason.GetSeasonsPC(PubgPlatform.Steam,Credentials.ApiKey);
            string toReturn = "";

            foreach (PubgSeason ps in totalSeasons)
            {
                toReturn = ps.Id + " | Is Current Season : " + ps.IsCurrentSeason;
                m_Service.SayMessage(toReturn);
                await Task.Delay(0);
            }*/
            // We'll cache this instead of repeatedly calling it.
            m_Service.SayMessage(@"division.bro.official.2017-beta | Is Current Season : False
division.bro.official.2017-pre2 | Is Current Season : False
division.bro.official.2017-pre1 | Is Current Season : False
division.bro.official.2018-05 | Is Current Season : False
division.bro.official.2018-09 | Is Current Season : False
division.bro.official.pc-2018-01 | Is Current Season : False
division.bro.official.2018-03 | Is Current Season : False
division.bro.official.2017-pre8 | Is Current Season : False
division.bro.official.2018-02 | Is Current Season : False
division.bro.official.pc-2018-03 | Is Current Season : False
division.bro.official.2017-pre3 | Is Current Season : False
division.bro.official.2017-pre9 | Is Current Season : False
division.bro.official.2018-08 | Is Current Season : False
division.bro.official.2017-pre4 | Is Current Season : False
division.bro.official.2018-06 | Is Current Season : False
division.bro.official.2017-pre5 | Is Current Season : False
division.bro.official.2018-01 | Is Current Season : False
division.bro.official.2017-pre7 | Is Current Season : False
division.bro.official.2017-pre6 | Is Current Season : False
division.bro.official.pc-2018-02 | Is Current Season : False
division.bro.official.pc-2018-04 | Is Current Season : True
division.bro.official.2018-04 | Is Current Season : False
division.bro.official.2018-07 | Is Current Season : False");

            await Task.Delay(0);

        }





        [Command("kdr")]
        [Alias("kdr")]
        [Remarks("!kdr [username]")]
        [Summary("The bot will pull the KDR for the user for the last season")]
        public async Task KDR([Remainder] string userName = "")
        {

            string toReturn = "";

            var playerService = new PubgPlayerService();
            var request = new GetPubgPlayersRequest
            {
                ApiKey = Credentials.ApiKey,
                PlayerNames = new string[] { userName }
            };


            var p_players = playerService.GetPlayers(PubgPlatform.Steam, request);
            foreach (PubgPlayer p in p_players) {
                if(p.Name.ToLower().Equals(userName.ToLower()))
                {

                  

                    PubgPlayerSeason stats = playerService.GetPlayerSeason(PubgPlatform.Steam,p.Id, "division.bro.official.pc-2018-04", Credentials.ApiKey);
                    toReturn += "Wins: " + stats.GameModeStats.SquadFPP.Wins;
                    toReturn += "Total Games: " + stats.GameModeStats.SquadFPP.RoundsPlayed;
                    toReturn += "K/D: " + Convert.ToDecimal(stats.GameModeStats.SquadFPP.Kills * (1.00) / stats.GameModeStats.SquadFPP.RoundsPlayed * (1.00));
                    toReturn += "Top 10's: " + stats.GameModeStats.SquadFPP.Top10s;


                }
            }

            



            m_Service.SayMessage(toReturn);
            await Task.Delay(0);
        }

        

    }
}


       