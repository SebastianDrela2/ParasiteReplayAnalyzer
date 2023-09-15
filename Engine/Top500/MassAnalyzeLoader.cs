using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ParasiteReplayAnalyzer.Engine.ExtenstionMethods;

namespace ParasiteReplayAnalyzer.Engine.Top500
{
    public class MassAnalyzeLoader
    {
        private string _replaysPath =>
            @"C:\Users\Seba\source\repos\ParasiteReplayAnalyzer\ParasiteReplayAnalyzer\ReplayResults";

        public MassAnalyzeLoader()
        {

        }

        public List<ParasiteData> Load()
        {
            var parasiteDatas = new List<ParasiteData>();

            var analyzedReplays = Directory.GetFiles(_replaysPath, "*.json", SearchOption.AllDirectories);

            foreach (var analyzedReplay in analyzedReplays)
            {
                var json = File.ReadAllText(analyzedReplay);

                var parasiteData = JsonConvert.DeserializeObject<ParasiteData>(json);

                if (parasiteData != null)
                {
                    parasiteDatas.Add(parasiteData);
                }
            }

            parasiteDatas.RemoveDuplicates();

            return parasiteDatas;
        }
    }
}
