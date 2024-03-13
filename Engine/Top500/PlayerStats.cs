using System.Collections.Generic;
using System.Linq;
using ParasiteReplayAnalyzer.Engine.ExtenstionMethods;

namespace ParasiteReplayAnalyzer.Engine.Top500
{
    public class PlayerStats
    {
        public string Handles;

        public string PlayerName;

        public double AnotherPlayerKills;

        public double KillsByAnotherPlayer;

        public double SpawnedAmmount;

        public double HumanGames;

        public double HumanWins;

        public double HostGames;

        public double HostWins;

        public double SurvivedTimeAlienPercentages;

        public double SurviveTimeHumanPercentages;

        public PlayerStats(string handles, string playerName, double anotherPlayerKills, double killsByAnotherPlayer, 
            double spawnedAmmount, double hostGames, double hostWins, double humanGames, double humaneWins, double survivedTimeAlienPercentages, double surviveTimeHumanPercentages)
        {
            Handles = handles;
            PlayerName = playerName;
            AnotherPlayerKills = anotherPlayerKills;
            KillsByAnotherPlayer = killsByAnotherPlayer;
            SpawnedAmmount = spawnedAmmount;
            HostGames = hostGames;
            HostWins = hostWins;
            HumanGames = humanGames;
            HumanWins = humaneWins;
            SurvivedTimeAlienPercentages = survivedTimeAlienPercentages;
            SurviveTimeHumanPercentages = surviveTimeHumanPercentages;
        }

        public static List<PlayerStats> GetPlayerStats(List<ParasiteData> parasiteDatas)
        {
            var listOfPlayerStats = new List<PlayerStats>();

            var handlesExclusionList = new List<string>
            {
                "0--0-2",
                "0-Station-Security-0-2",
                "0--0-1",
                "0-AlienAI-0-1"
            };

            foreach (var parasiteData in parasiteDatas)
            {
                foreach (var handlesKvp in parasiteData.GameMetaData.PlayerHandles.Where(x => !handlesExclusionList.Contains(x.Key)))
                {
                    var playerStats = CreatePlayerStatsFromParasiteData(parasiteData, handlesKvp);

                    if (listOfPlayerStats.All(x => x.Handles != handlesKvp.Key))
                    {
                        listOfPlayerStats.Add(playerStats);
                    }
                    else
                    {
                        listOfPlayerStats.UpdatePlayerStats(playerStats, handlesKvp);
                    }
                }
            }

            return listOfPlayerStats;
        }

        private static PlayerStats CreatePlayerStatsFromParasiteData(ParasiteData parasiteData, KeyValuePair<string, string> kvp)
        {
            var playerData = parasiteData.PlayerDatas.FirstOrDefault(x => x.Handle == kvp.Key);

            double playerKills = parasiteData.PlayersKills.FirstOrDefault(x => x.Key == kvp.Key).Value;
            double killedByAnotherPlayerAmmount = playerData.IsAlive ? 1 : 0;

            double spawnedAmmount = playerData.IsSpawn ? 1 : 0;

            double hostAmmount = playerData.IsHost ? 1 : 0;
            double hostWins = playerData is { IsHost: true, IsAlive: true } ? 1 : 0;

            double humanAmmount = playerData is { IsHost: false } ? 1 : 0;
            double humanWins = playerData is { IsHost: false, IsSpawn: false, IsAlive: true } ? 1 : 0;

            var survivedTimeAlien = playerData.IsHost || playerData.IsSpawn ? playerData.LifeTimePercentage : 0;
            var survivedTimeHuman = playerData is { IsHost: false, IsSpawn: false } ? playerData.LifeTimePercentage : 0;

            return new PlayerStats(
                kvp.Key, kvp.Value, playerKills,
                killedByAnotherPlayerAmmount, spawnedAmmount, hostAmmount, hostWins, humanAmmount,
                humanWins, survivedTimeAlien, survivedTimeHuman);
        }
    }
}
