using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Hawking.Elk.Common.Config
{
    public class IniFile : IIniFile
    {
        const string KeyValuePattern = @"(?<key>[a-zA-Z0-9_]+)\s*=(?<value>.*)";

        public string FilePath { get; private set; }
        public IDictionary<string, dynamic> KeyValueDictionary { get; private set; }

        public void Load(string filePath)
        {
            FilePath = filePath;
            KeyValueDictionary = new Dictionary<string, dynamic>();

            if (File.Exists(FilePath))
            {
                foreach (var line in File.ReadAllLines(FilePath))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var match = Regex.Match(line, KeyValuePattern);
                        if (match.Success)
                        {
                            KeyValueDictionary.Add(match.Groups["key"].Value, GetDynamicValue(match.Groups["value"].Value));
                        }
                    }
                }
            }
        }

        static dynamic GetDynamicValue(string strValue)
        {
            if (string.IsNullOrEmpty(strValue) || string.IsNullOrWhiteSpace(strValue))
            {
                return string.Empty;
            }

            var trimmedValue = strValue.Trim();

            if (int.TryParse(trimmedValue, out var integerValue))
            {
                return integerValue;
            }

            if (double.TryParse(trimmedValue, out var doubleValue))
            {
                return doubleValue;
            }

            if (DateTime.TryParse(trimmedValue, out var datetimeValue))
            {
                return datetimeValue;
            }

            if (bool.TryParse(trimmedValue, out var booValue))
            {
                return booValue;
            }

            return trimmedValue;
        }

        public dynamic GetValue(string key)
        {
            return KeyValueDictionary.ContainsKey(key) ? KeyValueDictionary[key] : string.Empty;
        }

        public void SetValue(string key, dynamic value)
        {
            KeyValueDictionary[key] = value;
        }

        public void UpdateAndSave()
        {
            var strBuilder = new StringBuilder();
            foreach (var keyValuePair in KeyValueDictionary)
            {
                strBuilder.AppendLine(string.Format("{0}={1}", keyValuePair.Key, keyValuePair.Value));
            }

            File.WriteAllText(FilePath, strBuilder.ToString());
        }
    }
}