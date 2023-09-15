using System.Collections.Generic;

namespace ParasiteReplayAnalyzer.Engine.ReplayComponents
{
    public class ReplayFolderData
    {
        public string ReplayFolderCode;

        public List<ReplayData> ReplaysData;

        public ReplayFolderData(string replayFolderCode, List<ReplayData> replaysData)
        {
            ReplayFolderCode = replayFolderCode;
            ReplaysData = replaysData;
        }
    }
}
