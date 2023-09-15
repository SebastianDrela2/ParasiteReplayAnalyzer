using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using s2protocol.NET;
using s2protocol.NET.Models;

namespace ParasiteReplayAnalyzer.Engine
{
    public class ParasiteDataAnalyzer
    {
        private string _parasiteReplayPath { get; set; }

        private static readonly string? _assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static ReplayDecoder _decoder = new ReplayDecoder(_assemblyPath);

        public ParasiteData? ParasiteData { get; set; }

        public ParasiteDataAnalyzer(string parasiteReplayPath)
        {
            _parasiteReplayPath = parasiteReplayPath;
            LoadParasiteData();
        }

        public void LoadParasiteData()
        {
            var sc2Replay = GetSc2Replay().Result;

            if (sc2Replay != null)
            {
                ParasiteData = GetParasiteData(sc2Replay, _parasiteReplayPath);
            }
        }

        private async Task<Sc2Replay?> GetSc2Replay()
        {
            if (_assemblyPath != null)
            {
                var replay = await _decoder.DecodeAsync(_parasiteReplayPath);

                return replay;
            }

            return null;
        }

        private ParasiteData GetParasiteData(Sc2Replay replay, string replayPath)
        {
            var parasiteMethodHelper = new ParasiteMethodHelper();
            var replayName = Path.GetFileNameWithoutExtension(replay.FileName);
            var upgradeEvents = parasiteMethodHelper.FilterUpgradeEvents(replay.TrackerEvents.SUpgradeEvents);

            var players = replay.Details.Players;
            var playerHandles = parasiteMethodHelper.GetHandlesList(players);

            var replayKey = GetReplayKey(players);

            var spawns = parasiteMethodHelper.GetSpawns(upgradeEvents, players);
            var specialRoleTeams = parasiteMethodHelper.GetSpecialRoleTeams(players, upgradeEvents);
            var humanPlayers = parasiteMethodHelper.GetHumanPlayers(players, specialRoleTeams);

            var lastEvolution = parasiteMethodHelper.GetLastHostEvolution(replay.TrackerEvents.SUnitBornEvents);
            var playerKills = parasiteMethodHelper.GetPlayerKills(players, replay.TrackerEvents.SUnitBornEvents);

            var alivePlayers = parasiteMethodHelper.GetAlivePlayers(players, replay.TrackerEvents.SUnitBornEvents, spawns, specialRoleTeams[0]);
            var dictionaryOfLifes = parasiteMethodHelper.GetLifeTimeList(replay.TrackerEvents.SUnitBornEvents, players, replay.Metadata);


            var gameLength = replay.Metadata?.Duration ?? 0;

            return new ParasiteData(replayName, replayKey, gameLength, humanPlayers, specialRoleTeams[2], specialRoleTeams[1],
                specialRoleTeams[0], spawns, playerHandles, playerKills, dictionaryOfLifes, lastEvolution, alivePlayers, spawns, replay, replayPath);
        }

        private string GetReplayKey(ICollection<DetailsPlayer> detailsPlayers)
        {
            var key = string.Empty;

            foreach (var player in detailsPlayers)
            {
                if (player.Name is not "Alien" && player.Name is not "Station Security")
                {
                    key += $"{player.Toon.Id}{player.Color.R}";
                }
            }

            return key;
        }

    }
}
