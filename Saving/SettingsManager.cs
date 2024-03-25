using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ParasiteReplayAnalyzer.Engine.Analyzer;
using ParasiteReplayAnalyzer.Engine.FileHelpers;
using ParasiteReplayAnalyzer.Engine.ReplayComponents;

namespace ParasiteReplayAnalyzer.Saving
{
    public class SettingsManager
    {
        public Settings? Settings => LoadSettings();
        public string ReplayResultsPath => ReplayFolderData.GetReplayResultsPath();

        public string SettingsPath => GetDefaultSettingsPath();

        public Settings? LoadSettings()
        {
            if (!File.Exists(SettingsPath))
            {
                return new Settings(GetDefaultReplaysPath(), 10);
            }

            var json = File.ReadAllText(SettingsPath);
            return JsonConvert.DeserializeObject<Settings>(json);
        }

        public void SaveSettings(string replaysPath, int maxConcurrentTasks = 10)
        {
            var settings =
                new Settings(replaysPath, maxConcurrentTasks);

            var parentDirectory = Directory.GetParent(SettingsPath)!.FullName;

            if (!Directory.Exists(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            var json = JsonConvert.SerializeObject(settings);

            File.WriteAllText(SettingsPath , json);
        }

        public async Task SaveParasiteDataAsync(ParasiteData data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);

            var directory = FileHelperMethods.ExtractFirstCharacters(data.GameMetaData.ReplayPath);
            var directoryPath = Path.Combine(ReplayResultsPath, directory);

            Directory.CreateDirectory(directoryPath);

            var filePath = Path.Combine(directoryPath, $"{data.GameMetaData.ReplayName}.json");

            await using var writer = new StreamWriter(filePath);
            await writer.WriteAsync(json);
        }

        public string GetDefaultReplaysPath()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var accountsPath = $@"{documentsPath}\StarCraft II\Accounts";

            var idFolders = Directory.GetDirectories(accountsPath);
            var firstIdFolder = Path.GetFileName(Directory.GetDirectories(accountsPath).First());
            var firstHandleFolder = Path.GetFileName(Directory.GetDirectories(idFolders.First()).First());

            var defaultReplaysPath =
                $@"{documentsPath}\StarCraft II\Accounts\{firstIdFolder}\{firstHandleFolder}\Replays\Multiplayer";

            return defaultReplaysPath;
        }

        public string GetDefaultSettingsPath()
        {
            var settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ParasiteReplayAnalyzer", "Settings");
            var settingsFileName = "settings.json";
            var fullSettingsFilePath = Path.Combine(settingsFilePath, settingsFileName);

            return fullSettingsFilePath;
        }
    }

}
