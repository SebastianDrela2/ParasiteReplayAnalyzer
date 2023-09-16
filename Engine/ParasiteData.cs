using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using s2protocol.NET;
using s2protocol.NET.Models;

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
        public ParasiteData(string replayName, string replayUniqueKey, int gameLength, IEnumerable<string> humanPlayers ,DetailsPlayer? droidPlayer, DetailsPlayer? psionPlayer,
            DetailsPlayer? alienPlayer, Dictionary<string, string> playerHandles, Dictionary<DetailsPlayer, int> playersKills, Dictionary<DetailsPlayer,int> lifeDurationLists, string lastHostEvolution, List<DetailsPlayer> listOfAlivePlayers
                , List<DetailsPlayer> spawns, Sc2Replay replay, string fullPath, List<DetailsPlayer> players, ParasiteMethodHelper methodHelper)
        {
            _replay = replay;
            
            ReplayName = replayName;
            ReplayUniqueKey = replayUniqueKey;
            GameLength = gameLength;
            HumanPlayers = humanPlayers.ToList();
            DroidPlayer = droidPlayer?.Name;
            PsionPlayer = psionPlayer?.Name;
            AlienPlayer = alienPlayer?.Name;
            AlienSpawns = spawns.Select(x => x.Name).ToList();
            PlayerHandles = playerHandles;
            PlayersKills = playersKills.Select(x=> new KeyValuePair<string,int>(methodHelper.GetHandles(x.Key), x.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            LastHostEvolution = lastHostEvolution;
            FullPath = fullPath;

            AddPlayerDatas(listOfAlivePlayers, lifeDurationLists, spawns, alienPlayer, players, methodHelper);

            VictoryStatus = GetMatchStatus();
        }

        private void AddPlayerDatas(List<DetailsPlayer> listOfAlivePlayers, Dictionary<DetailsPlayer, int> lifeDurationLists, List<DetailsPlayer> spawns, DetailsPlayer? host, List<DetailsPlayer> players, ParasiteMethodHelper methodHelper)
        {
            foreach (var player in players)
            {
                if (player.Name is "Alien AI" or "Station Security")
                {
                    continue;
                }

                lifeDurationLists.TryGetValue(player, out var lifeDuration);
                PlayerDatas.Add( new PlayerData(player, listOfAlivePlayers, spawns, host, lifeDuration, methodHelper));
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
