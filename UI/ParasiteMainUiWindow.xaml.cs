using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using ParasiteReplayAnalyzer.Engine;
using ParasiteReplayAnalyzer.Engine.ExtenstionMethods;
using ParasiteReplayAnalyzer.Engine.FileHelpers;
using ParasiteReplayAnalyzer.Engine.ReplayComponents;
using ParasiteReplayAnalyzer.Engine.Top500;
using ParasiteReplayAnalyzer.Saving;
using Path = System.IO.Path;

namespace ParasiteReplayAnalyzer.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ParasiteMainUiWindow : Window
    {
        private MassAnalyzeLoader _massAnalyzeLoader = new();
        private SettingsManager _settingsManager;
        private List<ReplayFolderData> _replayFolderDatas = new();
        private List<ParasiteData> _parasiteDatas;        

        public ParasiteMainUiWindow()
        {
            _settingsManager = new SettingsManager();

            InitializeComponent();
            LoadSettings();
            LoadReplays();
            FillListBoxItems();
        }
        private void LoadSettings()
        {
            if (!File.Exists(_settingsManager.SettingsPath))
            {
                _settingsManager.SaveSettings(_settingsManager.Settings.Sc2ReplayDirectoryPath, _settingsManager.Settings.MaxConcurrentAnalyzeTasks);
            }

            _settingsManager.LoadSettings();
        }

        public void LoadReplays()
        {
            _parasiteDatas = _massAnalyzeLoader.Load();
        }

        public void FillListBoxItems()
        {
            _listBoxReplays.Items.Clear();
            _replayFolderDatas.Clear();

            if (!Directory.Exists(_settingsManager.Settings.Sc2ReplayDirectoryPath))
            {
                return;
            }

            var allFileNames = Directory.GetFiles(_settingsManager.Settings.Sc2ReplayDirectoryPath, "*.Sc2Replay",
                    SearchOption.AllDirectories);

            foreach (var path in allFileNames)
            {
                if (!IsParasiteReplay(path))
                {
                    continue;
                }

                var fileName = Path.GetFileNameWithoutExtension(path);               
                var replayFolderCode = FileHelperMethods.ExtractFirstCharacters(path);
                var replayDisplay = $"{replayFolderCode}/{fileName}";

                SetReplayFolderData(fileName, path, replayFolderCode);

                _listBoxReplays.Items.Add(replayDisplay);
            }
        }

        private void SetReplayFolderData(string fileName, string path, string replayFolderCode)
        {
            if (!_replayFolderDatas.Any(x => x.ReplayFolderCode.Equals(replayFolderCode)))
            {
                _replayFolderDatas.Add(new ReplayFolderData(replayFolderCode, new List<ReplayData>()));
            }

            var folder = _replayFolderDatas.First(x => x.ReplayFolderCode == replayFolderCode);
            folder.ReplaysData.Add(new ReplayData(fileName, path));

            var index = _replayFolderDatas.IndexOf(folder);
            _replayFolderDatas[index] = folder;
        }

        private bool IsParasiteReplay(string replayName)
        {
            return replayName.Contains("P A R A S I T E - TEST");
        }

        private async void OnAnalyzeClickedAsync(object sender, RoutedEventArgs e)
        {
            if (_listBoxReplays.SelectedItem != null)
            {
                var selectedItem = _listBoxReplays.SelectedItem.ToString();

                if (selectedItem != null)
                {
                    await Task.Run(() => AnalyzeReplayAsync(selectedItem)).ConfigureAwait(true);
                }
            }
        }

        private async Task AnalyzeReplayAsync(string selectedItem)
        {
            var watch = new Stopwatch();
            watch.Start();

            var replayPath = FileHelperMethods.GetReplayPath(selectedItem, _replayFolderDatas);

            var parasiteAnalyzer = new ParasiteDataAnalyzer(replayPath);
            await parasiteAnalyzer.LoadParasiteData();

            await _settingsManager.SaveParasiteDataAsync(parasiteAnalyzer.ParasiteData);
            watch.Stop();

            Application.Current.Dispatcher.Invoke(() =>
            {
                _textBoxResult.Text += $"Analyzed: {parasiteAnalyzer.ParasiteData.GameMetaData.ReplayName} in {watch.ElapsedMilliseconds / 1000} seconds\n";
            });
        }

        private async void OnMassAnalyzeClickedAsync(object sender, RoutedEventArgs e)
        {
            _textBoxResult.Text = "Started mass replay analysis...\n";

            if (!Directory.Exists(_settingsManager.ReplayResultsPath))
            {
                Directory.CreateDirectory(_settingsManager.ReplayResultsPath);
            }

            var files = Directory.GetFiles(_settingsManager.ReplayResultsPath, "*.json", SearchOption.AllDirectories)
                .Select(FileHelperMethods.GetParentDirectoryNameWithFile).ToHashSet();
            
            var allReplays = _replayFolderDatas.SelectMany(y => y.ReplaysData).Select(x => x.ReplayPath);                      
            var cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() => AnalyzeReplaysAsync(allReplays, files, cancellationTokenSource), cancellationTokenSource.Token);
        }

        private async Task AnalyzeReplaysAsync(IEnumerable<string> allReplays, HashSet<string> files, CancellationTokenSource cancellationTokenSource)
        {
            var replayTasks = new List<Task>();
            var maxConcurrentTasks = 10;
            var semaphore = new SemaphoreSlim(maxConcurrentTasks);
            var completedReplays = 0;
            var watch = new Stopwatch();

            try
            {
                foreach (var replay in allReplays)
                {
                    var analyzedReplayCodePath = FileHelperMethods.GetReplayCodeFromPathWithFile(replay);

                    if (files.Contains(analyzedReplayCodePath))
                    {
                        continue;
                    }

                    replayTasks.Add(AnalyzeReplayAsync(replay, semaphore, cancellationTokenSource.Token)
                        .ContinueWith(task =>
                        {
                            completedReplays++;
                            watch = UpdateUiProgress(watch, replayTasks.Count, completedReplays,
                                    cancellationTokenSource.Token);
                        }, cancellationTokenSource.Token));
                }

                await Task.WhenAll(replayTasks);
                watch = UpdateUiProgress(watch, replayTasks.Count, completedReplays, cancellationTokenSource.Token);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    _textBoxResult.Text = $"Finished mass replay analysis... Analyzed {replayTasks.Count} Replays";
                });
            }
            catch (OperationCanceledException)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _textBoxResult.Text = "Mass replay analysis canceled.";
                });
            }
        }

        private async Task AnalyzeReplayAsync(string replay, SemaphoreSlim semaphore, CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var parasiteAnalyzer = new ParasiteDataAnalyzer(replay);
                await parasiteAnalyzer.LoadParasiteData();

                await _settingsManager.SaveParasiteDataAsync(parasiteAnalyzer.ParasiteData);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private Stopwatch UpdateUiProgress(Stopwatch watch, int ammountOfTasks, int completedReplays, CancellationToken cancellationToken)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (watch.IsRunning)
                {
                    watch.Stop();
                }

                var leftReplaysToAnalyze = ammountOfTasks - completedReplays;
                var estimatedTimeLeft = GetEstimatedTimeLeft(leftReplaysToAnalyze, watch.ElapsedMilliseconds);

                if (estimatedTimeLeft is not { Hours: 0, Minutes: 0, Seconds: 0 })
                {
                    _textBoxResult.Text = $"Analyzed {completedReplays}/{ammountOfTasks}\n" +
                                          $"Estimated time left: {estimatedTimeLeft}";
                }

                watch = new Stopwatch();
                watch.Start();
            });

            return watch;
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
                var undecidedWinrate = calculator.GetUndecidedWinrate();

                sb.Append($"Human winrate: {humanWinRate}% Games: {_parasiteDatas.Count(x => x.VictoryStatus.Equals("Human Win"))} \n" +
                          $"Alien winrate: {aliensWinRate}% Games: {_parasiteDatas.Count(x => x.VictoryStatus.Equals("Alien Win"))} \n" +
                          $"Undecided winrate: {undecidedWinrate}% Games: {_parasiteDatas.Count(x => x.VictoryStatus.Equals("Undecided"))} ");
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
                    sb.Append($"#{ranking} {host.PlayerName} Win: {(host.HostWins / host.HostGames * 100).RoundUpToSecondDigitAfterZero()}% Games: {host.HostGames}\n");
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
                    sb.Append($"#{ranking} {human.PlayerName} Win: {(human.HumanWins / human.HumanGames * 100).RoundUpToSecondDigitAfterZero()}% Games: {human.HumanGames}\n");
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
                    sb.Append($"#{ranking} {human.PlayerName} K/D: {(human.AnotherPlayerKills / human.KillsByAnotherPlayer).RoundUpToSecondDigitAfterZero()} Games: {human.HumanGames}\n");
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
                    sb.Append($"#{ranking} {human.PlayerName} Ratio: {(human.SpawnedAmmount / human.HumanGames).RoundUpToSecondDigitAfterZero()} Games: {human.HumanGames}\n");
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
                        sb.Append($"#{ranking} ({human.Handles}) {human.PlayerName} Rate: {human.SurvivedTimeAlienPercentages / human.HostGames}% Games: {human.HostGames}\n");
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
                        sb.Append($"#{ranking} ({human.Handles}) {human.PlayerName} Rate: {human.SurviveTimeHumanPercentages / human.HumanGames}% Games: {human.HumanGames}\n");
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
                    sb.Append($"#{ranking} {form.Name} WinRate: {form.WinPercentage.RoundUpToSecondDigitAfterZero()}% Games: {form.Games}\n");
                    ranking++;
                }
            });
        }

        private void OnMenuItemOptionsClicked(object sender, RoutedEventArgs e)
        {
            _ = new SettingsUI(_settingsManager, this);
        }

        private async void OnMenuAnalyzeClicked(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                var replayPath = openFileDialog.FileName;
                var parasiteAnalyzer = new ParasiteDataAnalyzer(replayPath);

                await parasiteAnalyzer.LoadParasiteData();
                await _settingsManager.SaveParasiteDataAsync(parasiteAnalyzer.ParasiteData!);
            }

        }

        private TimeSpan GetEstimatedTimeLeft(int leftReplays, long currentReplayAnalysisTimeInMilliseconds)
        {
            var totalExpectedTimeInSeconds = leftReplays * (currentReplayAnalysisTimeInMilliseconds/1000);

            var timeSpan = TimeSpan.FromSeconds(totalExpectedTimeInSeconds);

            return timeSpan;
        }
    }
}
