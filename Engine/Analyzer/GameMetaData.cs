using s2protocol.NET.Models;
using System.Collections.Generic;

namespace ParasiteReplayAnalyzer.Engine.Analyzer
{
    public struct GameMetaData
    {
        public string ReplayName { get; }
        public string ReplayPath { get; }
        public string ReplayKey { get; }
        public int GameLength { get; }
        public List<DetailsPlayer> Players { get; }
        public Dictionary<string, string> PlayerHandles { get; }

        public GameMetaData(string replayName, string replayPath, string replayKey, int gameLength, List<DetailsPlayer> players, Dictionary<string, string> playerHandles)
        {
            ReplayName = replayName;
            ReplayPath = replayPath;
            ReplayKey = replayKey;
            GameLength = gameLength;
            Players = players;
            PlayerHandles = playerHandles;
        }
    }
}
