using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXmlProcessor.Configuration
{
	public static class ApplicationConfig
	{
		public static int BulkInsertSize => Convert.ToInt32(Config["BulkInsertSize"], CultureInfo.InvariantCulture);
		public static string RefDbRepoStagingConnString => DbConnectionStringManager.StagingConnectionString;
		static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					config = new ConfigurationBuilder().AddJsonFile(jsonConfigFile).Build();
				}
				return config;
			}
		}

		static string jsonConfigFile = "CargoWise.RefDbRepo.UniversalXmlProcessor.config.json";
		static IConfiguration config;

#if DEBUG
		public static void SetConfigFileForTest(string path)
		{
			jsonConfigFile = path;
			config = null;
		}
#endif
	}
}
