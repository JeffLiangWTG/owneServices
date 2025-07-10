using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.PLReferenceData.Services
{
	public class ApplicationConfig
	{
		public const string JsonConfigFile = "CargoWise.RefDbRepo.PLReferenceData.CmdLine.config.json";

		public static ApplicationConfig Instance => instance.Value;
		static readonly Lazy<ApplicationConfig> instance = new(() => new ApplicationConfig());
		public ISettingsIndexer Settings { get; }

		public string OutputDirectory { get; }
		public string PuescXmlDictionariesUrl { get; }
		public int MaxRetry { get; }
		public string TestPuescXmlDictionariesUrl { get; }
		public string DownloadsPLPath { get; }

		ApplicationConfig()
		{
			var configRoot = new ConfigurationBuilder()
				.AddJsonFile(JsonConfigFile, optional: true, reloadOnChange: true)
				.Build();

			Settings = new ConfigurationSettingsIndexer(configRoot);

			OutputDirectory = configRoot["OutputPath"];
			PuescXmlDictionariesUrl = configRoot["PuescXmlDictionariesUrl"];
			TestPuescXmlDictionariesUrl = configRoot["TestPuescXmlDictionariesUrl"];
			MaxRetry = Convert.ToInt32(configRoot["MaxRetry"], CultureInfo.CurrentCulture);
			DownloadsPLPath = configRoot["DownloadsPLPath"];
		}

		class ConfigurationSettingsIndexer : ISettingsIndexer
		{
			readonly IConfigurationRoot root;

			public ConfigurationSettingsIndexer(IConfigurationRoot root)
			{
				this.root = root;
			}

			public string this[string index] => root[index];
		}
	}
}
