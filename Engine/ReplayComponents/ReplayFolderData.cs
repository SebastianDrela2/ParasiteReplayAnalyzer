using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public static string GetCsprojDirectory(string currentDirectory)
        {
            while (currentDirectory != null)
            {
                var solutionFiles = Directory.GetFiles(currentDirectory, "*.csproj");

                if (solutionFiles.Any())
                {
                    return currentDirectory;
                }

                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            }

            return null;
        }
    }
}
