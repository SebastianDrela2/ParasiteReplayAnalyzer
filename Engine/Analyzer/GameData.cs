using s2protocol.NET.Models;
using System.Collections.Generic;

namespace ParasiteReplayAnalyzer.Engine.Analyzer
{
    public struct GameData
    {
        public IEnumerable<string> HumanPlayerNames { get; }       
        public List<DetailsPlayer?> SpecialRoleTeams { get; set; }
        public Dictionary<string, int> PlayerKills { get; }
        public Dictionary<string, double> DictOfLifePercentages { get; }        
        public string LastHostEvolution { get; set; }
        public List<DetailsPlayer> AlivePlayers { get; }
        public List<DetailsPlayer> Spawns { get; }

        public GameData(IEnumerable<string> humanPlayerNames, List<DetailsPlayer?> specialRoleTeams, Dictionary<string, int> playerKills, Dictionary<string, double> dictOfLifePercentages, string lastEvolution, List<DetailsPlayer> alivePlayers, List<DetailsPlayer> spawns)
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
