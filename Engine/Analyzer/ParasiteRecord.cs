using System.Linq;
using System.Text.RegularExpressions;

namespace ParasiteReplayAnalyzer.Engine.Analyzer
{
    internal class ParasiteRecord
    {
        private const string _numberRegexPattern = @"\d+";

        private string _displayName;

        public string ReplayName;
        public int ReplayNumber;

        public ParasiteRecord(string replayName)
        {
            var regex = new Regex(_numberRegexPattern);
            var match = regex.Matches(replayName).FirstOrDefault()?.Value;

            if (match is null)
            {
                ReplayNumber = 1;
            }
            else
            {
                ReplayNumber = int.Parse(match);
            }

            ReplayName = replayName;
        }

        public void SetDisplayName(string displayName)
        {
            _displayName = displayName;
        }

        public string GetDisplayName()
        {
            return _displayName;
        }
    }
}
