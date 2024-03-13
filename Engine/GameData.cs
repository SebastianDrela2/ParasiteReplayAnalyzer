using s2protocol.NET.Models;
using System.Collections.Generic;

namespace ParasiteReplayAnalyzer.Engine
{
    public struct GameData
    {
        public IEnumerable<string> HumanPlayerNames { get; }
        public List<DetailsPlayer?> SpecialRoleTeams { get; }
        public Dictionary<DetailsPlayer, int> PlayerKills { get; }
        public Dictionary<DetailsPlayer, double> DictOfLifePercentages { get; }
        public string LastHostEvolution { get; }
        public List<DetailsPlayer> AlivePlayers { get; }
        public List<DetailsPlayer> Spawns { get; }

        public GameData(IEnumerable<string> humanPlayerNames, List<DetailsPlayer?> specialRoleTeams, Dictionary<DetailsPlayer, int> playerKills, Dictionary<DetailsPlayer, double> dictOfLifePercentages, string lastEvolution, List<DetailsPlayer> alivePlayers, List<DetailsPlayer> spawns)
        {
            HumanPlayerNames = humanPlayerNames;
            SpecialRoleTeams = specialRoleTeams;
            PlayerKills = playerKills;
            DictOfLifePercentages = dictOfLifePercentages;
            LastHostEvolution = lastEvolution;
            AlivePlayers = alivePlayers;
            Spawns = spawns;
        }       
    }
}
