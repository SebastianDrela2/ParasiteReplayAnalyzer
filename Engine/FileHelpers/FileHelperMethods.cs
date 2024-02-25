using ParasiteReplayAnalyzer.Engine.ReplayComponents;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ParasiteReplayAnalyzer.Engine.FileHelpers
{
    public class FileHelperMethods
    {
        public static string ExtractFirstCharacters(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            var parts = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var result = "";

            foreach (string part in parts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    result += part[0];
                }
            }

            return result;
        }

        public static string ExtractCodeFromSelectedItem(string selectedItem)
        {
            var result = string.Empty;

            foreach (var c in selectedItem)
            {
                if (c != '/')
                {
                    result += c;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        public static string ExtractReplayPathFromSelectedItem(string selectedItem)
        {
            var result = string.Empty;

            var passedSlash = false;

            foreach (var c in selectedItem)
            {
                if (c == '/')
                {
                    passedSlash = true;
                }
                else if (passedSlash)
                {
                    result += c;
                }
            }

            return result;
        }

        public static string GetReplayPath(string selectedItem, List<ReplayFolderData> replayFolderDatas)
        {
            var code = ExtractCodeFromSelectedItem(selectedItem);
            var replay = ExtractReplayPathFromSelectedItem(selectedItem);

            var folder = replayFolderDatas.First(x => x.ReplayFolderCode.Equals(code));
            var replayPath = folder.ReplaysData.First(x => x.ReplayName.Equals(replay)).ReplayPath;

            return replayPath;
        }

        public static string GetParentDirectoryNameWithFile(string path)
        {
            var replayCode = Directory.GetParent(path).Name;

            var file = Path.GetFileNameWithoutExtension(path);

            var result = $@"{replayCode}\{file}";

            return result;
        }

        public static string GetReplayCodeFromPathWithFile(string path)
        {
            var replayCode = ExtractFirstCharacters(path);
            var file = Path.GetFileNameWithoutExtension(path);

            var result = $@"{replayCode}\{file}";

            return result;
        }
    }
}
