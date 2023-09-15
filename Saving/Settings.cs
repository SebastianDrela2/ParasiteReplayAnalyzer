using Newtonsoft.Json;

namespace ParasiteReplayAnalyzer.Saving
{
    internal class Settings
    {
        [JsonProperty]
        public string Sc2ReplayDirectoryPath { get; set; }

        public Settings()
        {

        }
        public Settings(string sc2ReplayDirectoryPath)
        {
            Sc2ReplayDirectoryPath = sc2ReplayDirectoryPath;
        }
    }
}
