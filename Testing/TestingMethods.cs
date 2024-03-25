using System.Collections.Generic;
using System.IO;
using ParasiteReplayAnalyzer.Engine.Analyzer;
using s2protocol.NET;
using s2protocol.NET.Models;

namespace ParasiteReplayAnalyzer.Testing
{
    internal class TestingMethods
    {
        private static Sc2Replay _replay;
        public TestingMethods(Sc2Replay replay)
        {
            _replay = replay;
        }

        public void TestOutcome(Sc2Replay? replay, ParasiteMethodHelper parasiteMethodHelper)
        {
            var upgradeEvents = replay.TrackerEvents.SUpgradeEvents;

            upgradeEvents = parasiteMethodHelper.FilterUpgradeEvents(upgradeEvents);

            WriteUpgrades(upgradeEvents);
            WriteMarinesKilled(replay.TrackerEvents.SUnitBornEvents);
            WriteInitEvents(replay.TrackerEvents.SUnitInitEvents);
            WriteDetailsPlayers(replay.Details.Players);
            WriteBornUnits(replay.TrackerEvents.SUnitBornEvents);
            WriteTypeChangesUnits(replay.TrackerEvents.SUnitTypeChangeEvents);
        }

        private void WriteUpgrades(ICollection<SUpgradeEvent> upgradeEvents)
        {
            var writelist3 = new List<string>();

            foreach (var e in upgradeEvents)
            {
                writelist3.Add($"{e.Gameloop} {e.UpgradeTypeName} {e.PlayerId}");
            }

            File.WriteAllLines("upgrades.txt", writelist3);
        }

        private void WriteMarinesKilled(ICollection<SUnitBornEvent> sUnitBornEvents)
        {
            var writeList = new List<string>();

            foreach (var e in sUnitBornEvents)
            {
                if (e.SUnitDiedEvent != null)
                {
                    if (e.SUnitDiedEvent.KillerUnitBornEvent != null && e.ControlPlayerId is > 0 and < 9 && e.UnitTypeName.Contains("Marine"))
                    {
                        writeList.Add(
                            $"{e.Gameloop} {e.UnitTypeName} {ParasiteMethodHelper.ConvertIdToPlayer(e.ControlPlayerId -1, _replay.Details.Players)} {e.CreatorAbilityName} {e.SUnitDiedEvent.KillerUnitBornEvent.UnitTypeName} {ParasiteMethodHelper.ConvertIdToPlayer(e.SUnitDiedEvent.KillerUnitBornEvent.ControlPlayerId -1, _replay.Details.Players)}");
                    }
                }
            }

            File.WriteAllLines("killed.txt", writeList);
        }

        private void WriteInitEvents(ICollection<SUnitInitEvent> initEvents)
        {
            var writeList2 = new List<string>();

            foreach (var e in initEvents)
            {
                writeList2.Add($"{e.Gameloop} {e.UnitTypeName} {e.PlayerId}");
            }

            File.WriteAllLines("initEvents.txt", writeList2);
        }

        private void WriteDetailsPlayers(ICollection<DetailsPlayer> detailsPlayers)
        {
            var writeList = new List<string>();

            foreach (var e in detailsPlayers)
            {
                writeList.Add($"{e.Name} {e.Race} {e.WorkingSetSlotId} {e.Color}");
            }

            File.WriteAllLines("detailPlayers.txt", writeList);
        }

        private void WriteBornUnits(ICollection<SUnitBornEvent> bornEvents)
        {
            var writeList = new List<string>();

            foreach (var e in bornEvents)
            {
                writeList.Add($"{e.Gameloop} {ParasiteMethodHelper.ConvertIdToPlayer(e.ControlPlayerId -1 , _replay.Details.Players)} {e.UnitTypeName}");
            }

            File.WriteAllLines("bornUnits.txt", writeList);
        }

        private void WriteTypeChangesUnits(ICollection<SUnitTypeChangeEvent> typeChanges)
        {
            var writeList = new List<string>();

            foreach (var e in typeChanges)
            {
                writeList.Add($"{e.Gameloop} {ParasiteMethodHelper.ConvertIdToPlayer(e.PlayerId - 1, _replay.Details.Players)} {e.UnitTypeName}");
            }

            File.WriteAllLines("typeChanges.txt", writeList);
        }
    }
}
