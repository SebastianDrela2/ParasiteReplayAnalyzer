﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using IronPython.Modules;
using ParasiteReplayAnalyzer.Engine;
using ParasiteReplayAnalyzer.Engine.FileHelpers;
using ParasiteReplayAnalyzer.Engine.ReplayComponents;
using ParasiteReplayAnalyzer.Engine.Top500;
using ParasiteReplayAnalyzer.Saving;

namespace ParasiteReplayAnalyzer.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ParasiteMainUiWindow : Window
    {
        private MassAnalyzeLoader _massAnalyzeLoader = new();
        private SettingsManager _settingsManager;
        private Settings? _settings;
        private List<ReplayFolderData> _replayFolderDatas = new();
        private List<ParasiteData> _parasiteDatas;

        public ParasiteMainUiWindow()
        {
            _settingsManager = new SettingsManager();

            InitializeComponent();
            LoadSettings();
            LoadReplaysAsync();
            FillListBoxItems();
        }
        private void LoadSettings()
        {
            if (File.Exists(_settingsManager.SettingsPath))
            {
                _settings = _settingsManager.LoadSettings();
            }
            else
            {
                _settingsManager.SaveDefaultSettings();
                _settings = _settingsManager.LoadSettings();
            }
        }

        private async void LoadReplaysAsync()
        {
            _parasiteDatas = await Task.Run(() => _massAnalyzeLoader.Load());
        }

        private void FillListBoxItems()
        {
            var allFileNames = Directory.GetFiles(_settings.Sc2ReplayDirectoryPath, "*.Sc2Replay", SearchOption.AllDirectories);

            foreach (var path in allFileNames)
            {
                var file = Path.GetFileNameWithoutExtension(path);

                if (IsParasiteReplay(path))
                {
                    var replayFolderCode = FileHelperMethods.ExtractFirstCharacters(path);

                    if (!_replayFolderDatas.Any(x => x.ReplayFolderCode.Equals(replayFolderCode)))
                    {
                        _replayFolderDatas.Add(new ReplayFolderData(replayFolderCode, new List<ReplayData>()));
                    }

                    var folder = _replayFolderDatas.First(x => x.ReplayFolderCode == replayFolderCode);
                    folder.ReplaysData.Add(new ReplayData(file, path));

                    var index = _replayFolderDatas.IndexOf(folder);
                    _replayFolderDatas[index] = folder;

                    _listBoxReplays.Items.Add($"{replayFolderCode}/{file}");
                }
            }
        }

        private bool IsParasiteReplay(string replayName)
        {
            return replayName.Contains("P A R A S I T E - TEST");
        }

        private void OnAnalyzeClicked(object sender, RoutedEventArgs e)
        {
            if (_listBoxReplays.SelectedItem != null)
            {
                var selectedItem = _listBoxReplays.SelectedItem.ToString();

                if (selectedItem != null)
                {
                    Task.Run(() =>
                    {
                        var replayPath = FileHelperMethods.GetReplayPath(selectedItem, _replayFolderDatas);

                        var parasiteAnalyzer = new ParasiteDataAnalyzer(replayPath);

                        _settingsManager.SaveParasiteData(parasiteAnalyzer.ParasiteData);
                    }).ConfigureAwait(true);

                    Application.Current.Dispatcher.Invoke(() => { });
                }
            }
        }

        private void OnMassAnalyzeclicked(object sender, RoutedEventArgs e)
        {
            var files = Directory.GetFiles(_settingsManager.ReplayResultsPath, "*.json", SearchOption.AllDirectories);

            var maxConcurrentTasks = 15;
            var semaphore = new SemaphoreSlim(maxConcurrentTasks);

            var allReplays = _replayFolderDatas.SelectMany(y => y.ReplaysData).Select(x => x.ReplayPath);

            foreach (var replay in allReplays)
            {
                try
                {
                    if (files.Contains(replay))
                    {
                        continue;
                    }

                    Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();

                        try
                        {
                            var parasiteAnalyzer = new ParasiteDataAnalyzer(replay);

                            _settingsManager.SaveParasiteData(parasiteAnalyzer.ParasiteData);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    Console.WriteLine($"Launched Analyzer task of {Path.GetFileNameWithoutExtension(replay)} ");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }
            }
        }

        private void UpdateTextBoxResult(Action<MassAnalyzeCalculator, StringBuilder> action)
        {
            _textBoxResult.Text = "";

            var massAnalyzeCalculator = new MassAnalyzeCalculator(_parasiteDatas);
            var stringBuilder = new StringBuilder();

            action(massAnalyzeCalculator, stringBuilder);

            _textBoxResult.Text = stringBuilder.ToString();
        }

        private void OnWinRateClicked(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxResult((calculator, sb) =>
            {
                var humanWinRate = calculator.GetHumanWinrate();
                var aliensWinRate = calculator.GetAlienWinrate();
                sb.Append($"Human winrate: {humanWinRate}% \nAlien winrate: {aliensWinRate}%");
            });
        }

        private void OnBestHostsClicked(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxResult((calculator, sb) =>
            {
                var bestHosts = calculator.GetBestHosts();
                var ranking = 1;
                foreach (var host in bestHosts)
                {
                    sb.Append($"#{ranking} {host.PlayerName} Win: {host.HostWins / host.HostGames * 100}% Games: {host.HostGames}\n");
                    ranking++;
                }
            });
        }

        private void OnBestHumansClicked(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxResult((calculator, sb) =>
            {
                var bestHumans = calculator.GetBestHumans();
                var ranking = 1;
                foreach (var human in bestHumans)
                {
                    sb.Append($"#{ranking} {human.PlayerName} Win: {human.HumanWins / human.HumanGames * 100}% Games: {human.HumanGames}\n");
                    ranking++;
                }
            });
        }

        private void OnBestKillRatioClicked(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxResult((calculator, sb) =>
            {
                var bestKillers = calculator.GetBestKillers();
                var ranking = 1;
                foreach (var human in bestKillers)
                {
                    sb.Append($"#{ranking} {human.PlayerName} K/D: {human.AnotherPlayerKills / human.KillsByAnotherPlayer} Games: {human.HumanGames}\n");
                    ranking++;
                }
            });
        }

        private void OnBestSelferRatioClicked(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxResult((calculator, sb) =>
            {
                var bestSelfers = calculator.GetBestSelfers();
                var ranking = 1;
                foreach (var human in bestSelfers)
                {
                    sb.Append($"#{ranking} {human.PlayerName} Ratio: {human.SpawnedAmmount / human.HumanGames} Games: {human.HumanGames}\n");
                    ranking++;
                }
            });
        }

        private void OnBestAlienSurvivorsClicked(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxResult((calculator, sb) =>
            {
                var bestAlienSurvivors = calculator.GetBestAlienSurvivors();
                var ranking = 1;
                foreach (var human in bestAlienSurvivors)
                {
                    if (human.HostGames != 0)
                    {
                        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                            .AddSeconds(human.SurvivedTimeAlien / human.HostGames);

                        sb.Append($"#{ranking} {human.PlayerName} Average Time: {dateTime.Hour}:{dateTime.Minute}:{dateTime.Second} Games: {human.HostGames}\n");
                        ranking++;
                    }
                }
            });
        }

        private void OnBestHumanSurvivorsClicked(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxResult((calculator, sb) =>
            {
                var bestHumanSurvivors = calculator.GetBestHumanSurvivors();
                var ranking = 1;
                foreach (var human in bestHumanSurvivors)
                {
                    if (human.HumanGames != 0)
                    {
                        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                            .AddSeconds(human.SurvivedTimeHuman / human.HumanGames);

                        sb.Append($"#{ranking} {human.PlayerName} Average Time: {dateTime.Hour}:{dateTime.Minute}:{dateTime.Second} Games: {human.HumanGames}\n");
                        ranking++;
                    }
                }
            });
        }

        private void OnBestAlienFormsClicked(object sender, RoutedEventArgs e)
        {
            UpdateTextBoxResult((calulator, sb) =>
            {
                var bestForms = calulator.GetBestAlienForms();
                var ranking = 1;

                foreach (var form in bestForms)
                {
                    sb.Append($"#{ranking} {form.Name} WinRate: {form.WinPercentage}% Games: {form.Games}\n");
                    ranking++;
                }
            });
        }
    }
}