using System.Collections.Generic;
using System.Linq;
using ParasiteReplayAnalyzer.Engine.ExtenstionMethods;

namespace ParasiteReplayAnalyzer.Engine.Top500
{
    public class MassAnalyzeCalculator
    {
        private List<PlayerStats>? _playerStats;
        private readonly List<ParasiteData>? _parasiteDatas;

        public MassAnalyzeCalculator(List<ParasiteData> parasiteDatas)
        {
            if (_parasiteDatas != null || _playerStats != null)
            {
                return;
            }

            _parasiteDatas = parasiteDatas;
            _playerStats = PlayerStats.GetPlayerStats(parasiteDatas);
        }

        public double GetUndecidedWinrate()
        {
            double undecidedWins = _parasiteDatas.Count(x => x.VictoryStatus.Equals("Undecided"));
            double totalGames = _parasiteDatas.Count;

            return (undecidedWins / totalGames * 100).RoundUpToSecondDigitAfterZero();
        }
        public double GetAlienWinrate()
        {
            double alienWins = _parasiteDatas.Count(x => x.VictoryStatus.Equals("Alien Win"));
            double totalGames = _parasiteDatas.Count;

            return (alienWins / totalGames * 100).RoundUpToSecondDigitAfterZero();
        }

        public double GetHumanWinrate()
        {
            double humanWins = _parasiteDatas.Count(x => x.VictoryStatus.Equals("Human Win"));
            double totalGames = _parasiteDatas.Count;

            return (humanWins / totalGames * 100).RoundUpToSecondDigitAfterZero();
        }

        public List<PlayerStats> GetBestHosts()
        {
            var bestHosts = _playerStats.Where(x => x.HostGames > 15)
                .OrderByDescending(x => x.HostGames == 0 || x.HostWins == 0 ? 0.0 : (x.HostWins / x.HostGames).RoundUpToSecondDigitAfterZero()).ToList();

            return bestHosts;
        }

        public List<PlayerStats> GetBestHumans()
        {
            var bestHumans = _playerStats.Where(x => x.HumanGames > 25)
                .OrderByDescending(x => x.HumanWins == 0 || x.HumanGames == 0 ? 0.0 : (x.HumanWins / x.HumanGames).RoundUpToSecondDigitAfterZero()).ToList();

            return bestHumans;
        }

        public List<PlayerStats> GetBestKillers()
        {
            var bestKillers = _playerStats.Where(x => x.HumanGames > 15)
                .OrderByDescending(x => x.AnotherPlayerKills == 0 || x.KillsByAnotherPlayer == 0 ? 0.0 : (x.AnotherPlayerKills / x.KillsByAnotherPlayer)).ToList();

            return bestKillers;
        }

        public List<PlayerStats> GetBestSelfers()
        {

            var bestSelfers = _playerStats.Where(x => x.HumanGames > 20)
                .OrderByDescending(x =>
                    x.SpawnedAmmount == 0 || x.HumanGames == 0 ? 0.0 : (x.SpawnedAmmount / x.HumanGames).RoundUpToSecondDigitAfterZero()).ToList();

            return bestSelfers;
        }

        public List<PlayerStats> GetBestAlienSurvivors()
        {
            var bestAlienSurvivors = _playerStats.Where(x => x.HostGames > 10)
                .OrderByDescending(x =>
                    x.SurvivedTimeAlien == 0 || x.HumanGames == 0 ? 0.0 : (x.SurvivedTimeAlien / x.HumanGames).RoundUpToSecondDigitAfterZero()).ToList();

            return bestAlienSurvivors;
        }

        public List<PlayerStats> GetBestHumanSurvivors()
        {
            var bestHumanSurvivors = _playerStats.Where(x => x.HumanGames > 20)
                .OrderByDescending(x =>
                    x.SurvivedTimeHuman == 0 || x.HumanGames == 0 ? 0.0 : (x.SurvivedTimeHuman / x.HumanGames).RoundUpToSecondDigitAfterZero()).ToList();

            return bestHumanSurvivors;
        }

        public List<AlienForm> GetBestAlienForms()
        {
            var orderedEvolutions = _parasiteDatas
                .GroupBy(x => x.LastHostEvolution)
                .Select(group =>
                {
                    var totalGames = group.Count();
                    var alienWins = group.Count(x => x.VictoryStatus.Equals("Alien Win"));
                    var winPercentage = (double)alienWins / totalGames * 100;

                    return new AlienForm
                    {
                        Name = group.Key,
                        Games = alienWins,
                        WinPercentage = winPercentage
                    };

                })
                .OrderByDescending(group => group.WinPercentage)
                .ToList();

            return orderedEvolutions;
        }
    }
}
