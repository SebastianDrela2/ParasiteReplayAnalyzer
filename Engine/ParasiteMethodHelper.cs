using s2protocol.NET.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using IronPython.Modules;
using static System.Net.Mime.MediaTypeNames;

namespace ParasiteReplayAnalyzer.Engine
{
    public class ParasiteMethodHelper
    {
        public static DetailsPlayer? ConvertIdToPlayer(int userId, ICollection<DetailsPlayer> players)
        {
            foreach (var player in players)
            {
                if (player.WorkingSetSlotId == userId)
                {
                    return player;
                }
            }

            return null;
        }

        public List<DetailsPlayer> GetSpawns(ICollection<SUpgradeEvent> upgradeEvents, ICollection<DetailsPlayer> detailsPlayers)
        {
            var spawnList = new List<DetailsPlayer>();

            foreach (var upgradeEvent in upgradeEvents)
            {
                if (upgradeEvent.UpgradeTypeName.Contains("isaspawn"))
                {
                    var colorName = upgradeEvent.UpgradeTypeName.Replace("isaspawn", "");
                    var colorRgb = GetPlayerColorFromColorName(colorName);

                    var player = detailsPlayers.FirstOrDefault(x => x.Color.R == colorRgb.R && x.Color.G == colorRgb.G && x.Color.B == colorRgb.B);

                    if (player != null)
                    {
                        spawnList.Add(player);
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

        public Dictionary<DetailsPlayer, int> GetPlayerKills(ICollection<DetailsPlayer> detailsPlayers, ICollection<SUnitBornEvent> sUnitBornEvents)
        {
            var playersKillsDict = detailsPlayers.ToDictionary(player => player, _ => 0);
            var unitsThatCountAsPlayerKills = new HashSet<string>(ReadResource("ParasiteReplayAnalyzer.Filters.UnitsThatCountAsPlayerKills.txt"));

            foreach (var sUnitBornEvent in sUnitBornEvents)
            {
                var killerBornEvent = sUnitBornEvent.SUnitDiedEvent?.KillerUnitBornEvent;

                if (killerBornEvent != null)
                {
                    var player = ConvertIdToPlayer(killerBornEvent.ControlPlayerId - 1, detailsPlayers);

                    if (player != null && unitsThatCountAsPlayerKills.Contains(sUnitBornEvent.UnitTypeName))
                    {
                        if (IsAlienOrStationSecurity(player.Name) || unitsThatCountAsPlayerKills.Contains(killerBornEvent.UnitTypeName))
                        {
                            playersKillsDict[player]++;
                        }
                    }
                }
            }

            return playersKillsDict;
        }

        public List<DetailsPlayer> GetAlivePlayers(ICollection<DetailsPlayer> detailsPlayers, ICollection<SUnitBornEvent> sUnitBornEvents, List<DetailsPlayer> spawns, DetailsPlayer? alien)
        {
            var filters = new HashSet<string>(ReadResource("ParasiteReplayAnalyzer.Filters.UnitsThatCountAsPlayerKills.txt"));
            var alienFilters = new HashSet<string>(ReadResource("ParasiteReplayAnalyzer.Filters.HostEvolutions.txt"));
            var spawnedList = new HashSet<DetailsPlayer>();

            var alivePlayers = detailsPlayers.Select(player => player).ToList();

            foreach (var sUnitBornEvent in sUnitBornEvents.Where(UnitCanBeConsideredKill))
            {
                var player = ConvertIdToPlayer(sUnitBornEvent.ControlPlayerId - 1, detailsPlayers);

                if (player != null && filters.Contains(sUnitBornEvent.UnitTypeName))
                {
                    var isAlien = spawns.Any(x => x.Toon.Equals(player.Toon)) || alien == player;
                    var isNotAlien = !spawns.Contains(player) && alien != player;

                    if ((isAlien && alienFilters.Contains(sUnitBornEvent.UnitTypeName)) || spawnedList.Any(x => x.Toon.Equals(x.Toon)))
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

        public string GetColorFromPlayer(DetailsPlayer detailsPlayer)
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

        public Dictionary<DetailsPlayer, int> GetLifeTimeList(ICollection<SUnitBornEvent> sUnitBornEvents, ICollection<DetailsPlayer> detailsPlayers, Metadata metadata)
        {
            var lifeTimeDict = new Dictionary<DetailsPlayer, int>();
            var filters = new HashSet<string>(ReadResource("ParasiteReplayAnalyzer.Filters.UnitsThatCountAsPlayerKills.txt"));

            foreach (var player in detailsPlayers)
            {
                lifeTimeDict.Add(player, metadata?.Duration ?? 0);
            }

            foreach (var sUnitBornEvent in sUnitBornEvents.Where(UnitCanBeConsideredKill))
            {
                var player = ConvertIdToPlayer(sUnitBornEvent.ControlPlayerId - 1, detailsPlayers);

                if (player != null && filters.Contains(sUnitBornEvent.UnitTypeName))
                {
                    lifeTimeDict[player] = sUnitBornEvent.SUnitDiedEvent!.Gameloop / 16;
                }
            }

            return lifeTimeDict;
        }

        public List<DetailsPlayer?> GetSpecialRoleTeams(ICollection<DetailsPlayer> players, ICollection<SUpgradeEvent> upgradeEvents)
        {
            return new List<DetailsPlayer?>
            {
                GetPlayerByUpgrade("AlienIdentificationUpgrade2",upgradeEvents, players),
                GetPlayerByUpgrade("PlayerIsPsion", upgradeEvents, players),
                GetPlayerByUpgrade("PlayerisAndroid", upgradeEvents, players),
                new DetailsPlayer(new PlayerColor(999,999,999,999),13,0,"","Alien AI",0,"AI",0,0,new Toon(1,"",0,0), 13),
                new DetailsPlayer(new PlayerColor(999,999,999,999),14,0,"","Station Security",0,"AI",0,0,new Toon(2,"",0,0), 14)
            };
        }

        public  List<DetailsPlayer> GetHumanPlayers(ICollection<DetailsPlayer> players, List<DetailsPlayer?> specialRoleTeams)
        {
            return players.Where(player => specialRoleTeams.All(specialRole => specialRole?.Toon != player.Toon)).ToList();
        }

        public DetailsPlayer? GetPlayerByUpgrade(string upgradeType, ICollection<SUpgradeEvent> upgradeEvents, ICollection<DetailsPlayer> players)
        {
            var upgradeEvent = upgradeEvents.FirstOrDefault(x => x.UpgradeTypeName.Contains(upgradeType));

            if (upgradeEvent != null)
            {
                return ConvertIdToPlayer(upgradeEvent.PlayerId - 1, players);
            }
            return null;
        }

        public string GetHandles(DetailsPlayer detailsPlayer)
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
            return playerName is "Alien AI" or "Station Security";
        }

        private List<string> ReadResource(string resourceName)
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
