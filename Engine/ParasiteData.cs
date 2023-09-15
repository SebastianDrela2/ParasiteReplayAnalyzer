using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Modules;
using Newtonsoft.Json;
using s2protocol.NET;

namespace ParasiteReplayAnalyzer.Engine
{
    public class ParasiteData
    {
        private Sc2Replay _replay;

        [JsonProperty] 
        public int GameLength;

        [JsonProperty]
        public string ReplayUniqueKey;

        [JsonProperty] 
        public string FullPath;

        [JsonProperty] 
        public string ReplayName;

        [JsonProperty] 
        public Dictionary<string, string> PlayerHandles;

        [JsonProperty]
        public List<string> HumanPlayers;

        [JsonProperty]
        public string? AlienPlayer;

        [JsonProperty]
        public string? DroidPlayer;

        [JsonProperty]
        public string? PsionPlayer;

        [JsonProperty]
        public List<string> AlienSpawns;

        [JsonProperty]
        public Dictionary<string, int> PlayersKills;

        [JsonProperty]
        public List<PlayerData> PlayerDatas = new();

        [JsonProperty]
        public string VictoryStatus;

        [JsonProperty]
        public string LastHostEvolution;

        public ParasiteData()
        {

        }
        public ParasiteData(string replayName, string replayUniqueKey, int gameLength, List<string> humanPlayers ,string? droidPlayer, string? psionPlayer,
            string? alienPlayer, List<string> alienSpawns, Dictionary<string, string> playerHandles, Dictionary<string, int> playersKills, Dictionary<string,int> lifeDurationLists, string lastHostEvolution, List<string> listOfAlivePlayers
                , List<string> spawns, Sc2Replay replay, string fullPath)
        {
            _replay = replay;
            
            ReplayName = replayName;
            ReplayUniqueKey = replayUniqueKey;
            GameLength = gameLength;
            HumanPlayers = humanPlayers;
            DroidPlayer = droidPlayer;
            PsionPlayer = psionPlayer;
            AlienPlayer = alienPlayer;
            AlienSpawns = alienSpawns;
            PlayerHandles = playerHandles;
            PlayersKills = playersKills;
            LastHostEvolution = lastHostEvolution;
            FullPath = fullPath;

            AddPlayerDatas(listOfAlivePlayers, lifeDurationLists, spawns, alienPlayer);

            VictoryStatus = GetMatchStatus();
        }

        private void AddPlayerDatas(List<string> listOfAlivePlayers, Dictionary<string, int> lifeDurationLists, List<string> spawns, string? host)
        {
            foreach (var player in _replay.Details.Players)
            {
                if (player.Name is "Alien" or "Station Security")
                {
                    continue;
                }

                lifeDurationLists.TryGetValue(player.Name, out var lifeDuration);
                PlayerDatas.Add( new PlayerData(player, listOfAlivePlayers, spawns, host, lifeDuration));
            }
        }

        private string GetMatchStatus()
        {
            var anyAlienIsAlive =
                PlayerDatas.Any(x => x is { IsHost: true, IsAlive: true } or { IsSpawn: true, IsAlive: true });

            var anyHumanIsAlive = PlayerDatas.Any(x => x is {IsHost: false, IsSpawn:false, IsAlive:true});

            if (anyAlienIsAlive && anyHumanIsAlive || !anyAlienIsAlive && !anyHumanIsAlive)
            {
                return "Undecided";
            }

            if (anyAlienIsAlive && !anyHumanIsAlive)
            {
                return "Alien Win";
            }

            return "Human Win";
        }
    }
}
