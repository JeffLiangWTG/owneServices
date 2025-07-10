using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class BrLogUtils
	{
		static BrLogUtils _instance = new BrLogUtils();
		Dictionary<string, object> logs;
		public string LogFilePath { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
			Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location) + "_RefDataRepo_BR.log");

		BrLogUtils() { }

		public static BrLogUtils Instance => _instance;

		public void AddLog(string key, object value)
		{
			LoadLogs();
			if (logs.ContainsKey(key))
			{
				logs[key] = value;
			}
			else
			{
				logs.Add(key, value);
			}
			SaveLogs();
		}

		public string GetKeyValueAsString(string key)
		{
			LoadLogs();
			return logs.ContainsKey(key) ? Convert.ToString(logs[key], CultureInfo.CurrentCulture) : null;
		}

		public void ClearLogs()
		{
			logs = null;
		}

		void LoadLogs()
		{
			if (logs != null)
			{
				return;
			}

			if (!File.Exists(LogFilePath))
			{
				logs = new Dictionary<string, object>();
			}
			else
			{
				string logData = File.ReadAllText(LogFilePath);
				logs = string.IsNullOrEmpty(logData) ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(logData);
			}
		}

		void SaveLogs()
		{
			var json = JsonConvert.SerializeObject(logs);

			File.WriteAllText(LogFilePath, json);
		}
	}
}
