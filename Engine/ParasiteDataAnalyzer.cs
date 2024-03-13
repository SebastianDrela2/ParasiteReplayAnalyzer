using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ParasiteReplayAnalyzer.Engine.ExtenstionMethods;
using s2protocol.NET;
using s2protocol.NET.Models;
using static IronPython.Modules.PythonIterTools;

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
        }

        public async Task LoadParasiteData()
        {
            var sc2Replay = await GetSc2Replay();

            if (sc2Replay != null)
            {
                ParasiteData = await Task.Run(() => GetParasiteData(sc2Replay));
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

        private ParasiteData GetParasiteData(Sc2Replay replay)
        {
            var parasiteMethodHelper = new ParasiteMethodHelper();            
            var upgradeEvents = parasiteMethodHelper.FilterUpgradeEvents(replay.TrackerEvents.SUpgradeEvents);
            var players = replay.Details!.Players.ToList();
            players!.ModifySecondToLastAndLastPlayers();
                       
            var gameMetaData = GetGameMetaData(replay, players, parasiteMethodHelper);
            var gameData = GetGameData(replay, players, parasiteMethodHelper, upgradeEvents);

            return new ParasiteData(gameMetaData, gameData, parasiteMethodHelper);
        }

        private GameData GetGameData(Sc2Replay replay, List<DetailsPlayer> players, ParasiteMethodHelper parasiteMethodHelper, ICollection<SUpgradeEvent> upgradeEvents)
        {
            var specialRoleTeams = parasiteMethodHelper.GetSpecialRoleTeams(players, upgradeEvents);
            var humanPlayerNames = parasiteMethodHelper.GetHumanPlayers(players, specialRoleTeams).Select(x => x.Name);           
            var playerKills = parasiteMethodHelper.GetPlayerKills(players, replay.TrackerEvents.SUnitBornEvents);
            var dictOfLifePercentages = parasiteMethodHelper.GetLifeTimePercentagesList(replay.TrackerEvents.SUnitBornEvents, players, replay.Metadata);
            var lastEvolution = parasiteMethodHelper.GetLastHostEvolution(replay.TrackerEvents.SUnitBornEvents);
            var spawns = parasiteMethodHelper.GetSpawns(upgradeEvents, players);
            var alivePlayers = parasiteMethodHelper.GetAlivePlayers(players, replay.TrackerEvents.SUnitBornEvents, spawns, specialRoleTeams[0]);

            return new GameData(humanPlayerNames, specialRoleTeams, playerKills, dictOfLifePercentages, lastEvolution, alivePlayers, spawns);
        }

        private GameMetaData GetGameMetaData(Sc2Replay replay, List<DetailsPlayer> players, ParasiteMethodHelper parasiteMethodHelper)
        {
            var replayName = Path.GetFileNameWithoutExtension(replay.FileName);           
            var gameLength = replay.Metadata?.Duration ?? 0;                     
            var playerHandles = parasiteMethodHelper.GetHandlesList(players);          
            var replayKey = GetReplayKey(players);

           return new GameMetaData(replayName, _parasiteReplayPath, replayKey, gameLength, players, playerHandles);
        }

        private string GetReplayKey(IEnumerable<DetailsPlayer> detailsPlayers)
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
