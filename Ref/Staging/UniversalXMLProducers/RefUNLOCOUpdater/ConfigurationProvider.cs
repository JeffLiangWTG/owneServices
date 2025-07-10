using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater
{
	public static class ConfigurationProvider
	{
		public static string JsonConfigFile
		{
			get
			{
				return jsonConfigFile ?? (jsonConfigFile = "CargoWise.RefDbRepo.UniversalXMLProducers.RefUNLOCOUpdater.config.json");
			}
			set
			{
				jsonConfigFile = value;
			}
		}
		static string jsonConfigFile;

		public static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile(JsonConfigFile)
						.Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		public const string BaseAddress = @"http://www.unece.org";
		public static string UNECELastUpdatedTimeRegex => Configuration["UNECELastUpdatedTimeRegex"];
		public static string UNECELastUpdatedTimeFormat => Configuration["UNECELastUpdatedTimeFormat"];
		public static string UNECEMdbZipFileRegex => Configuration["UNECEMdbZipFileRegex"];
		public static string OutputFilePath => Configuration["OutputFilePath"];
		public static string SafeDataUpdateUri => Configuration["SafeDataUpdateUri"];
		public static string UNECESourceUri => Configuration["UNECESourceUri"];
		public static string IATASourceUri => Configuration["IATASourceUri"];
		public static string IATASourceUsername => Configuration["IATASourceUsername"];
		public static string IATASourcePassword => Configuration["IATASourcePassword"];
		public static string DownloadMDBTempPath => Configuration["DownloadMDBTempPath"];
		public static string StateCodeListFile => Configuration["StateCodeListFile"];
		public static string ProxyService => Configuration["ProxyService"];
		public static string ProxyUsername => Configuration["ProxyUsername"];
		public static string ProxyPassword => Configuration["ProxyPassword"];

		public static DateTime UNECESpecificFilePublicationDate
		{
			get
			{
				var configValue = Configuration["UNECESpecificFilePublicationDate"];
				return string.IsNullOrEmpty(configValue) ? DateTime.MinValue.Date : DateTime.Parse(configValue, CultureInfo.InvariantCulture).Date;
			}
		}

		public static DateTime UNECESpecificFileEnableUntil
		{
			get
			{
				var configValue = Configuration["UNECESpecificFileEnableUntil"];
				return string.IsNullOrEmpty(configValue) ? DateTime.MinValue.Date : DateTime.Parse(configValue, CultureInfo.InvariantCulture).Date;
			}
		}

		public static string ProgramSpecificConfigurationsUNECEFilePath
		{
			get
			{
				var configValue = Configuration["UNECESpecificFile"];
				return string.IsNullOrEmpty(configValue) ? "" : Path.Combine(binFolder, configValue);
			}
		}
		public static string ProgramSpecificConfigurationsIATAFilePath
		{
			get
			{
				var configValue = Configuration["IATAStationsSpecificFile"];
				return string.IsNullOrEmpty(configValue) ? "" : Path.Combine(binFolder, configValue);
			}
		}
		public static string ProgramSpecificConfigurationsAmendmentDataFilePath
		{
			get
			{
				var configValue = Configuration["UNECEAmendmentDataFile"];
				return string.IsNullOrEmpty(configValue) ? "" : Path.Combine(binFolder, configValue);
			}
		}

		readonly static string binFolder = AppDomain.CurrentDomain.BaseDirectory;

#if DEBUG
		public static void SetConfigFileForTest(string path)
		{
			jsonConfigFile = path;
			configuration = null;
		}
#endif

	}
}
