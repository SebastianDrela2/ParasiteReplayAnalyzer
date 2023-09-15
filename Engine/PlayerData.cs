using System.Collections.Generic;
using Newtonsoft.Json;
using s2protocol.NET.Models;

namespace ParasiteReplayAnalyzer.Engine
{
    public class PlayerData
    {
        [JsonProperty]
        public string PlayerName;

        [JsonProperty]
        public string PlayerColor;

        [JsonProperty]
        public bool IsSpawn;

        [JsonProperty]
        public bool IsHost;

        [JsonProperty]
        public bool IsAlive;

        [JsonProperty] 
        public int LifeDuration;

        public PlayerData()
        {

        }
        public PlayerData(DetailsPlayer detailsPlayer, List<string> listOfAlivePlayers, List<string> spawns, string? host, int lifeDuration)
        {
            PlayerName = detailsPlayer.Name;
            IsHost = host.Equals(detailsPlayer.Name);
            IsSpawn = spawns.Contains(detailsPlayer.Name);
            IsAlive = listOfAlivePlayers.Contains(detailsPlayer.Name);
            LifeDuration = lifeDuration;
            PlayerColor = ParasiteMethodHelper.GetColorFromPlayer(detailsPlayer);
        }

    }
}
