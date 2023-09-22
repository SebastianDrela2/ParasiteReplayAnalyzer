using Newtonsoft.Json;

namespace ParasiteReplayAnalyzer.Saving
{
    public class Settings
    {
        [JsonProperty]
        public string Sc2ReplayDirectoryPath { get; set; }

        [JsonProperty]
        public int MaxConcurrentAnalyzeTasks { get; set; } = 10;

        public Settings()
        {

        }
        public Settings(string sc2ReplayDirectoryPath, int maxConcurrentAnalyzeTasks)
        {
            Sc2ReplayDirectoryPath = sc2ReplayDirectoryPath;
        }
    }
}
