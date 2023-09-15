using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ParasiteReplayAnalyzer.Engine.ExtenstionMethods;
using ParasiteReplayAnalyzer.Engine.ReplayComponents;

namespace ParasiteReplayAnalyzer.Engine.Top500
{
    public class MassAnalyzeLoader
    {
        public string ReplaysPath;

        public MassAnalyzeLoader()
        {
            ReplaysPath = ReplayFolderData.GetReplayResultsPath();
        }

        public List<ParasiteData> Load()
        {
            var parasiteDatas = new List<ParasiteData>();

            var analyzedReplays = Directory.GetFiles(ReplaysPath, "*.json", SearchOption.AllDirectories);

            foreach (var analyzedReplay in analyzedReplays)
            {
                var json = File.ReadAllText(analyzedReplay);

                var parasiteData = JsonConvert.DeserializeObject<ParasiteData>(json);

                if (parasiteData != null)
                {
                    parasiteDatas.Add(parasiteData);
                }
            }

            parasiteDatas.RemoveDuplicates();

            return parasiteDatas;
        }
    }
}
