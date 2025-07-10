using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class SerilogConfigProvider
	{
		public static IEnumerable<KeyValuePair<string, string>> GetSerilogSettings(string logFileName)
		{
			var settings = new List<KeyValuePair<string, string>>();
			var serilogKeys = Config.AsEnumerable().Select(k => k.Key).Where(k => k.StartsWith(SettingPrefix, StringComparison.InvariantCultureIgnoreCase));
			foreach (var key in serilogKeys)
			{
				var appSettingValue = Config[key];
				if (appSettingValue != null)
				{
					var value = Environment.ExpandEnvironmentVariables(appSettingValue).Replace(LogFileNameProperty, logFileName);
					settings.Add(new KeyValuePair<string, string>(key.Substring(SettingPrefix.Length), value));
				}
			}
			return settings;
		}

		static IConfiguration Config
		{
			get
			{
				if (null == config)
				{
					LoadConfiguration();
				}
				return config;
			}
		}
		static IConfiguration config;
		static string jsonConfigFile;

		static void LoadConfiguration()
		{
			IConfigurationBuilder builder = new ConfigurationBuilder();
			var currentDirectory = Path.GetDirectoryName(MigrationHelper.GetExecutingAssemblyLocation());
			var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
			if (jsonConfigFile == null)
			{
				jsonConfigFile = Path.Combine(currentDirectory, $"{assemblyName}.config.json");
			}

			if (File.Exists(jsonConfigFile))
			{
				builder = builder.AddJsonFile(jsonConfigFile);
			}
			config = builder.Build();
		}

		const string SettingPrefix = "serilog:";
		const string LogFileNameProperty = "{RefLogFileName}";

#if DEBUG
		public static void SetConfigFileForTest(string path)
		{
			jsonConfigFile = path;
			config = null;
		}
#endif
	}
}
