using System.Collections.Generic;
using System.Linq;
using ParasiteReplayAnalyzer.Engine.Top500;

namespace ParasiteReplayAnalyzer.Engine.ExtenstionMethods
{
    public static class ParasiteExtenstionMethods
    {
        public static void RemoveDuplicates(this List<ParasiteData> parasiteDatas)
        {
            var duplicateData = parasiteDatas
                .GroupBy(group => group.ReplayUniqueKey)
                .Where(x => x.Count() > 1)
                .SelectMany(x => x)
                .ToList();

            var bestGameData = duplicateData
                .GroupBy(x => x.ReplayUniqueKey)
                .Select(group => group.OrderByDescending(item => item.GameLength).First())
                .ToList();

            parasiteDatas.RemoveAll(duplicateData.Contains);

            parasiteDatas.AddRange(bestGameData);
        }

        public static void UpdatePlayerStats(this List<PlayerStats> listOfPlayerStats, PlayerStats incomingPlayerStats, KeyValuePair<string, string> kvp)
        {
            var existingPlayerStats = listOfPlayerStats.FirstOrDefault(x => x.Handles == kvp.Key);

            if (existingPlayerStats != null)
            {
                existingPlayerStats.AnotherPlayerKills += incomingPlayerStats.AnotherPlayerKills;
                existingPlayerStats.KillsByAnotherPlayer += incomingPlayerStats.KillsByAnotherPlayer;
                existingPlayerStats.HostGames += incomingPlayerStats.HostGames;
                existingPlayerStats.HostWins += incomingPlayerStats.HostWins;
                existingPlayerStats.HumanGames += incomingPlayerStats.HumanGames;
                existingPlayerStats.HumanWins += incomingPlayerStats.HumanWins;
                existingPlayerStats.SpawnedAmmount += incomingPlayerStats.SpawnedAmmount;
                existingPlayerStats.SurvivedTimeHuman += incomingPlayerStats.SurvivedTimeHuman;
                existingPlayerStats.SurvivedTimeAlien += incomingPlayerStats.SurvivedTimeAlien;

                var index = listOfPlayerStats.IndexOf(existingPlayerStats);
                listOfPlayerStats[index] = existingPlayerStats;
            }
        }
    }
}
