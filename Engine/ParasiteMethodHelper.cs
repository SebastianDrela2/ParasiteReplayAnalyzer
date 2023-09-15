using s2protocol.NET.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ParasiteReplayAnalyzer.Engine
{
    public class ParasiteMethodHelper
    {
        public static string? ConvertIdToString(int userId, ICollection<DetailsPlayer> players)
        {
            foreach (var player in players)
            {
                if (player.WorkingSetSlotId == userId)
                {
                    return player.Name;
                }
            }

            return null;
        }

        public List<string> GetSpawns(ICollection<SUpgradeEvent> upgradeEvents, ICollection<DetailsPlayer> detailsPlayers)
        {
            var spawnUpgradeSet = new HashSet<string>();
            var spawnList = new List<string>();

            foreach (var upgradeEvent in upgradeEvents)
            {
                if (upgradeEvent.UpgradeTypeName.Contains("isaspawn") && spawnUpgradeSet.Add(upgradeEvent.UpgradeTypeName))
                {
                    var color = upgradeEvent.UpgradeTypeName.Replace("isaspawn", "");
                    var player = detailsPlayers.FirstOrDefault(x => x.Color.Equals(GetPlayerColorFromColorName(color)));

                    if (player != null)
                    {
                        spawnList.Add(player.Name);
                    }
                }
            }

            return spawnList;
        }

        public ICollection<SUpgradeEvent> FilterUpgradeEvents(ICollection<SUpgradeEvent> upgradeEvents)
        {
            var filters = ReadResource("ParasiteReplayAnalyzer.Filters.UpgradeEventsFilters.txt");

            ICollection<SUpgradeEvent> newUpgradeEvents = new List<SUpgradeEvent>();

            foreach (var upgrade in upgradeEvents)
            {
                if (filters != null && filters.Contains(upgrade.UpgradeTypeName))
                {
                    continue;
                }

                newUpgradeEvents.Add(upgrade);
            }

            return newUpgradeEvents;
        }

        public string GetLastHostEvolution(ICollection<SUnitBornEvent> bornEvents)
        {
            var filters = ReadResource("ParasiteReplayAnalyzer.Filters.HostEvolutions.txt");
            return bornEvents.LastOrDefault(x => filters.Contains(x.UnitTypeName)).UnitTypeName;
        }

        public Dictionary<string, int> GetPlayerKills(ICollection<DetailsPlayer> detailsPlayers, ICollection<SUnitBornEvent> sUnitBornEvents)
        {
            var playersKillsDict = detailsPlayers.ToDictionary(player => player.Name, _ => 0);
            var filters = new HashSet<string>(ReadResource("ParasiteReplayAnalyzer.Filters.UnitsThatCountAsPlayerKills.txt"));

            foreach (var sUnitBornEvent in sUnitBornEvents)
            {
                if (sUnitBornEvent.SUnitDiedEvent?.KillerUnitBornEvent != null)
                {
                    var killerBornEvent = sUnitBornEvent.SUnitDiedEvent.KillerUnitBornEvent;
                    var player = ConvertIdToString(killerBornEvent.ControlPlayerId - 1, detailsPlayers);

                    if (player != null && filters.Contains(sUnitBornEvent.UnitTypeName))
                    {
                        if (IsAlienOrStationSecurity(player) || filters.Contains(killerBornEvent.UnitTypeName))
                        {
                            playersKillsDict[player]++;
                        }
                    }
                }
            }

            return playersKillsDict;
        }

        public List<string> GetAlivePlayers(ICollection<DetailsPlayer> detailsPlayers, ICollection<SUnitBornEvent> sUnitBornEvents, List<string> spawns, string? alien)
        {
            var filters = new HashSet<string>(ReadResource("ParasiteReplayAnalyzer.Filters.UnitsThatCountAsPlayerKills.txt"));
            var alienFilters = new HashSet<string>(ReadResource("ParasiteReplayAnalyzer.Filters.HostEvolutions.txt"));
            var spawnedList = new HashSet<string>();

            var alivePlayers = detailsPlayers.Select(player => player.Name).ToList();

            foreach (var sUnitBornEvent in sUnitBornEvents.Where(UnitCanBeConsideredKill))
            {
                var player = ConvertIdToString(sUnitBornEvent.ControlPlayerId - 1, detailsPlayers);

                if (player != null && filters.Contains(sUnitBornEvent.UnitTypeName))
                {
                    var isAlien = spawns.Contains(player) || alien == player;
                    var isNotAlien = !spawns.Contains(player) && alien != player;

                    if ((isAlien && alienFilters.Contains(sUnitBornEvent.UnitTypeName)) || spawnedList.Contains(player))
                    {
                        alivePlayers.Remove(player);
                    }
                    else if (isNotAlien && !alienFilters.Contains(sUnitBornEvent.UnitTypeName))
                    {
                        alivePlayers.Remove(player);
                    }

                    if (isAlien && !alienFilters.Contains(sUnitBornEvent.UnitTypeName))
                    {
                        spawnedList.Add(player);
                    }
                }
            }

            return alivePlayers;
        }

        public static string GetColorFromPlayer(DetailsPlayer detailsPlayer)
        {
            var color = detailsPlayer.Color;

            return color switch
            {
                { R:0 ,G:66 ,B:255} => "Blue",
                { R: 254, G: 138, B: 14 } => "Orange",
                { R: 22, G: 128, B: 0 } => "Green",
                { R: 16, G: 98, B: 70 } => "DarkGreen",
                { R: 180, G: 20, B: 30 } => "Red",
                { R: 28, G: 167, B: 234 } => "Teal",
                { R: 51, G: 26, B: 200 } => "DarkPurple",
                { R: 235, G: 225, B: 41 } => "Yellow",
                { R: 35, G: 35, B: 35 } => "Black",
                { R: 204, G: 166, B: 252 } => "LightPink",
                { R: 229, G: 91, B: 176 } => "Pink",
                { R: 82, G: 84, B: 148 } => "LightGrey",
                { R: 78, G: 42, B: 4 } => "Brown",
                _ => "unidentified"
            };
        }

        public PlayerColor GetPlayerColorFromColorName(string colorName)
        {
            var colorMap = new Dictionary<string, PlayerColor>
            {
                { "Blue", new PlayerColor { R = 0, G = 66, B = 255 } },
                { "Orange", new PlayerColor { R = 254, G = 138, B = 14 } },
                { "Green", new PlayerColor { R = 22, G = 128, B = 0 } },
                { "DarkGreen", new PlayerColor { R = 16, G = 98, B = 70 } },
                { "Red", new PlayerColor { R = 180, G = 20, B = 30 } },
                { "Teal", new PlayerColor { R = 28, G = 167, B = 234 } },
                { "DarkPurple", new PlayerColor { R = 51, G = 26, B = 200 } },
                { "Yellow", new PlayerColor { R = 235, G = 225, B = 41 } },
                { "Black", new PlayerColor { R = 35, G = 35, B = 35 } },
                { "LightPink", new PlayerColor { R = 204, G = 166, B = 252 } },
                { "Pink", new PlayerColor { R = 229, G = 91, B = 176 } },
                { "Brown", new PlayerColor { R = 78, G = 42, B = 4 } },
                { "LightGrey", new PlayerColor { R = 82, G = 84, B = 148 } }
            };

            if (colorMap.TryGetValue(colorName, out var playerColor))
            {
                return playerColor;
            }

            return new PlayerColor { R = 0, G = 0, B = 0 };
        }

        public Dictionary<string, string> GetHandlesList(ICollection<DetailsPlayer> detailsPlayers)
        {
            var playerDict = new Dictionary<string, string>();

            foreach (var player in detailsPlayers)
            {
                var handles = GetHandles(player);

                if (handles != "unidentified")
                {
                    playerDict.Add(handles, player.Name);
                }
            }

            return playerDict;
        }

        public Dictionary<string, int> GetLifeTimeList(ICollection<SUnitBornEvent> sUnitBornEvents, ICollection<DetailsPlayer> detailsPlayers, Metadata metadata)
        {
            var lifeTimeDict = new Dictionary<string, int>();
            var filters = new HashSet<string>(ReadResource("ParasiteReplayAnalyzer.Filters.UnitsThatCountAsPlayerKills.txt"));

            foreach (var player in detailsPlayers)
            {
                lifeTimeDict.Add(player.Name, metadata?.Duration ?? 0);
            }

            foreach (var sUnitBornEvent in sUnitBornEvents.Where(UnitCanBeConsideredKill))
            {
                var playerName = ConvertIdToString(sUnitBornEvent.ControlPlayerId - 1, detailsPlayers);

                if (playerName != null && filters.Contains(sUnitBornEvent.UnitTypeName))
                {
                    lifeTimeDict[playerName] = sUnitBornEvent.SUnitDiedEvent!.Gameloop / 16;
                }
            }

            return lifeTimeDict;
        }

        public List<string?> GetSpecialRoleTeams(ICollection<DetailsPlayer> players, ICollection<SUpgradeEvent> upgradeEvents)
        {
            return new List<string?>
            {
                GetPlayerByUpgrade("AlienIdentificationUpgrade2", upgradeEvents, players),
                GetPlayerByUpgrade("PlayerIsPsion",upgradeEvents, players),
                GetPlayerByUpgrade("PlayerisAndroid",upgradeEvents, players),
                "Station Security",
                "Alien"
            };
        }

        public  List<string> GetHumanPlayers(ICollection<DetailsPlayer> players, List<string?> specialRoleTeams)
        {
            return players.Select(player => player.Name).Where(player => specialRoleTeams.All(specialRole => specialRole != player)).ToList();
        }

        public string? GetPlayerByUpgrade(string upgradeType, ICollection<SUpgradeEvent> upgradeEvents, ICollection<DetailsPlayer> players)
        {
            var upgradeEvent = upgradeEvents.FirstOrDefault(x => x.UpgradeTypeName.Contains(upgradeType));

            if (upgradeEvent != null)
            {
                return ConvertIdToString(upgradeEvent.PlayerId - 1, players);
            }
            return string.Empty;
        }

        private string GetHandles(DetailsPlayer detailsPlayer)
        {
            var handles = $"{detailsPlayer.Toon.Region}-{detailsPlayer.Toon.ProgramId}-{detailsPlayer.Toon.Realm}-{detailsPlayer.Toon.Id}";
            handles = handles.Replace("\0", "");

            if (detailsPlayer.Toon.Id == 0)
            {
                return "unidentified";
            }

            return handles;
        }

        private bool UnitCanBeConsideredKill(SUnitBornEvent sUnitBornEvent)
        {
            return sUnitBornEvent.SUnitDiedEvent != null && sUnitBornEvent.SUnitDiedEvent.KillerUnitBornEvent != null &&
                   sUnitBornEvent.ControlPlayerId is > 0 and < 13;
        }

        private bool IsAlienOrStationSecurity(string playerName)
        {
            return playerName is "Alien" or "Station Security";
        }

        private List<string>? ReadResource(string resourceName)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                return null;
            }

            var returnList = new List<string>();

            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (line != null)
                {
                    returnList.Add(line);
                }
            }

            return returnList;

        }
    }
}
