using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using ParasiteReplayAnalyzer.Engine.Top500;
using s2protocol.NET.Models;

namespace ParasiteReplayAnalyzer.Engine.ExtenstionMethods
{
    public static class ExtenstionMethods
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
            var existingPlayerStats = listOfPlayerStats.First(x => x.Handles == kvp.Key);

            existingPlayerStats.AnotherPlayerKills += incomingPlayerStats.AnotherPlayerKills;
            existingPlayerStats.KillsByAnotherPlayer += incomingPlayerStats.KillsByAnotherPlayer;
            existingPlayerStats.HostGames += incomingPlayerStats.HostGames;
            existingPlayerStats.HostWins += incomingPlayerStats.HostWins;
            existingPlayerStats.HumanGames += incomingPlayerStats.HumanGames;
            existingPlayerStats.HumanWins += incomingPlayerStats.HumanWins;
            existingPlayerStats.SpawnedAmmount += incomingPlayerStats.SpawnedAmmount;
            existingPlayerStats.SurviveTimeHumanPercentages += incomingPlayerStats.SurviveTimeHumanPercentages;
            existingPlayerStats.SurvivedTimeAlienPercentages += incomingPlayerStats.SurvivedTimeAlienPercentages;

            var index = listOfPlayerStats.IndexOf(existingPlayerStats);
            listOfPlayerStats[index] = existingPlayerStats;
        }

        public static double RoundUpToSecondDigitAfterZero(this double number)
        {
            var multiplier = Math.Pow(10, 2);
            return Math.Ceiling(number * multiplier) / multiplier;
        }

        public static DetailsPlayer ChangeAlienIntoAlienAi(this DetailsPlayer? alienAi)
        {
            alienAi = new DetailsPlayer(alienAi.Color, alienAi.Control,alienAi.Handicap, alienAi.Hero, "Alien AI",alienAi.Observe,
                alienAi.Race, alienAi.Result, alienAi.TeamId, new Toon(1,"AlienAI",0,0), alienAi.WorkingSetSlotId);

            return alienAi;

        }

        public static DetailsPlayer ChangeStationSecurityIntoStationSecurityAi(this DetailsPlayer? stationSecurityAi)
        {
            stationSecurityAi = new DetailsPlayer(stationSecurityAi.Color, stationSecurityAi.Control, stationSecurityAi.Handicap, stationSecurityAi.Hero, "Station Security", stationSecurityAi.Observe,
                stationSecurityAi.Race, stationSecurityAi.Result, stationSecurityAi.TeamId, new Toon(2, "Station-Security", 0, 0), stationSecurityAi.WorkingSetSlotId);

            return stationSecurityAi;

        }

        public static void ModifySecondToLastAndLastPlayers(this List<DetailsPlayer?> players)
        {
            if (players.Count < 2)
            {
                return;
            }

            var stationSecurityAi = players.ElementAt(players.Count - 2);
            var stationAiIndex = players.IndexOf(stationSecurityAi);
            players[stationAiIndex] = stationSecurityAi.ChangeStationSecurityIntoStationSecurityAi();

            var alienAi = players.Last();
            var alienAiIndex = players.IndexOf(alienAi);
            players[alienAiIndex] = alienAi.ChangeAlienIntoAlienAi();
        }
    }
}
