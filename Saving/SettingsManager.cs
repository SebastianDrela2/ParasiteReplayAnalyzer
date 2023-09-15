using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ParasiteReplayAnalyzer.Engine;
using ParasiteReplayAnalyzer.Engine.FileHelpers;

namespace ParasiteReplayAnalyzer.Saving
{
    internal class SettingsManager
    {
        public string ReplayResultsPath =>
            @"C:\Users\Seba\source\repos\ParasiteReplayAnalyzer\ParasiteReplayAnalyzer\ReplayResults";

        public string SettingsPath => GetDefaultSettingsPath();

        public Settings? LoadSettings()
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonConvert.DeserializeObject<Settings>(json);
        }

        public void SaveDefaultSettings()
        {
            var defaultReplaysPath = GetDefaultReplaysPath();

            var settings =
                new Settings(defaultReplaysPath);

            var parentDirectory = Directory.GetParent(SettingsPath).FullName;

            if (!Directory.Exists(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            var json = JsonConvert.SerializeObject(settings);

            File.WriteAllText(SettingsPath , json);
        }

        public void SaveParasiteData(ParasiteData data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);

            var directory = FileHelperMethods.ExtractFirstCharacters(data.FullPath);

            if (!Directory.Exists($@"{ReplayResultsPath}\{directory}"))
            {
                Directory.CreateDirectory($@"{ReplayResultsPath}\{directory}");
            }

            var path =
                $@"{ReplayResultsPath}\{directory}\{data.ReplayName}.json";
            File.WriteAllText(path, json);

            Console.WriteLine($"Analyzed: {data.ReplayName}");
        }

        private string GetDefaultReplaysPath()
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

        private string GetDefaultSettingsPath()
        {
            var settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ParasiteReplayAnalyzer", "Settings");
            var settingsFileName = "settings.json";
            var fullSettingsFilePath = Path.Combine(settingsFilePath, settingsFileName);

            return fullSettingsFilePath;
        }
    }

}
