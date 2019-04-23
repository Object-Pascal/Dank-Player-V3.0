using System.Collections.Generic;
using System.IO;

namespace Dank_Player_V3._0.Components
{
    public static class FileHelper
    {
        public static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            string[] searchPatterns = searchPattern.Split('|');

            List<string> files = new List<string>();
            for (int i = 0; i < searchPatterns.Length; i++)
                files.AddRange(Directory.GetFiles(path, searchPatterns[i], searchOption));

            files.Sort();
            return files.ToArray();
        }
    }
}
