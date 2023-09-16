﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using s2protocol.NET.Models;

namespace ParasiteReplayAnalyzer.Engine
{
    public class PlayerData
    {
        [JsonProperty]
        public string PlayerName;

        [JsonProperty] 
        public string Handle;

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
        public PlayerData(DetailsPlayer detailsPlayer, IEnumerable<DetailsPlayer> listOfAlivePlayers, IEnumerable<DetailsPlayer> spawns, DetailsPlayer host, int lifeDuration, ParasiteMethodHelper methodHelper)
        {
            PlayerName = detailsPlayer.Name;
            IsHost = host.Toon.Equals(detailsPlayer.Toon);
            IsSpawn = spawns.Any(x => x.Toon.Equals(detailsPlayer.Toon));
            IsAlive = listOfAlivePlayers.Any(x => x.Toon.Equals(detailsPlayer.Toon));
            LifeDuration = lifeDuration;
            PlayerColor = methodHelper.GetColorFromPlayer(detailsPlayer);
            Handle = methodHelper.GetHandles(detailsPlayer);
        }

    }
}
