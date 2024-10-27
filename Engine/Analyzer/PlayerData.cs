using System.Linq;
using Newtonsoft.Json;
using s2protocol.NET.Models;

namespace ParasiteReplayAnalyzer.Engine.Analyzer;

public class PlayerData
{
    [JsonProperty] public string Handle;

    [JsonProperty] public bool IsAlive;

    [JsonProperty] public bool IsHost;

    [JsonProperty] public bool IsSpawn;

    [JsonProperty] public double LifeTimePercentage;

    [JsonProperty] public string PlayerColor;

    [JsonProperty] public string PlayerName;

    public PlayerData()
    {
    }

    public PlayerData(GameData gameData, DetailsPlayer detailsPlayer, ParasiteMethodHelper methodHelper,
        double lifeTimePercentage)
    {
        PlayerName = detailsPlayer.Name;
        IsHost = gameData.SpecialRoleTeams[2]!.Toon.Equals(detailsPlayer.Toon);
        IsSpawn = gameData.Spawns.Any(x => x.Toon.Equals(detailsPlayer.Toon));
        IsAlive = gameData.AlivePlayers.Any(x => x.Toon.Equals(detailsPlayer.Toon));
        LifeTimePercentage = lifeTimePercentage;
        PlayerColor = methodHelper.GetColorFromPlayer(detailsPlayer);
        Handle = methodHelper.GetHandles(detailsPlayer);
    }
}