using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Language {
    public static class Table {

        private static Dictionary<string, string> TextDictionary = null;

        private const char Delimiter = '=';

        private const string Directory = "Languages";
        private const string FileFormat = ".lang";

        public static string CurrentLanguage = "?";

        public static string[] GetLanguages () {

            string[] fullFiles = Utility.FileManager.GetFiles(Directory);
            if (fullFiles == null) return null;

            List<string> filtered = new List<string>();
            for (int i = 0; i < fullFiles.Length; i++) {
                if (fullFiles[i].EndsWith(FileFormat)) {
                    string name = fullFiles[i].Replace('\\', '/').Split('/').Last().Split('.').First();
                    filtered.Add(name);
                }
            }

            return filtered.ToArray();
        }

        public static void LoadFile (string langName) {
            if (TextDictionary == null) TextDictionary = new Dictionary<string, string>();
            TextDictionary.Clear();

            CurrentLanguage = langName;

            langName = Directory + "/" + langName + FileFormat;

            try {
                string[] lines = Utility.FileManager.ReadFile(langName);
                string[] parts;
                for (int i = 0; i < lines.Length; i++) {
                    parts = lines[i].Split(Delimiter);
                    TextDictionary.Add(parts[0], parts[1]);
                }
            }
            catch (Exception e) {
                Utility.ErrorLog.ReportError("Error reading language file: " + e.Message);
                TextDictionary = null;
            }
        }

        public static string Get (string key) {
            if (TextDictionary == null) return key;

            string retVal;
            if (TextDictionary.TryGetValue(key, out retVal)) return retVal;

            return key;
        }
    }
}
