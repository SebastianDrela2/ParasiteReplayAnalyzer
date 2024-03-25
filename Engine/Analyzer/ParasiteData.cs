using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using s2protocol.NET;
using s2protocol.NET.Models;

namespace ParasiteReplayAnalyzer.Engine.Analyzer
{
    public class ParasiteData
    {
        [JsonProperty]
        public GameMetaData GameMetaData;

        [JsonProperty]
        public GameData GameData;

        [JsonProperty]
        public Dictionary<string, int> PlayersKills;

        [JsonProperty]
        public List<PlayerData> PlayerDatas = new();

        [JsonProperty]
        public string VictoryStatus;

        public ParasiteData()
        {

        }
        public ParasiteData(GameMetaData gameMetaData, GameData gameData, ParasiteMethodHelper methodHelper)
        {
            GameMetaData = gameMetaData;
            GameData = gameData;

            PlayersKills = gameData.PlayerKills.Select(x => new KeyValuePair<string, int>(methodHelper.GetHandles(x.Key), x.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            AddPlayerDatas(gameData, gameMetaData.Players, methodHelper);

            VictoryStatus = GetMatchStatus();
        }

        private void AddPlayerDatas(GameData gameData, IEnumerable<DetailsPlayer> players, ParasiteMethodHelper methodHelper)
        {
            foreach (var player in players.Where(player => player.Name is not ("Alien AI" or "Station Security")))
            {
                gameData.DictOfLifePercentages.TryGetValue(player, out var lifeDurationPercentage);
                var playerData = new PlayerData(gameData, player, methodHelper, lifeDurationPercentage);

                PlayerDatas.Add(playerData);
            }
        }

        private string GetMatchStatus()
        {
            var anyAlienIsAlive = PlayerDatas.Any(x => x is { IsHost: true, IsAlive: true } or { IsSpawn: true, IsAlive: true });
            var anyHumanIsAlive = PlayerDatas.Any(x => x is { IsHost: false, IsSpawn: false, IsAlive: true });

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
