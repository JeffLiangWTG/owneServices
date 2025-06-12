using System.Collections.Generic;

namespace Hawking.Elk.Common.Config
{
    public interface IIniFile
    {
        IDictionary<string, dynamic> KeyValueDictionary { get; }

        void Load(string filePath);
        void UpdateAndSave();

        dynamic GetValue(string key);
        void SetValue(string key, dynamic value);
    }
}
