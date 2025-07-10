using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher.Configuration
{
	public static class AppConfig
	{
		public const string JsonConfigFile = "CargoWise.RefDbRepo.UniversalXmlFileWatcher.config.json";

		static IConfiguration? configuration;
		static string? refDbStagingConnString;

		public static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					configuration = new ConfigurationBuilder().AddJsonFile(JsonConfigFile).Build();
				}
				return configuration;
			}
		}

		public static string RefDbRepoStagingConnString => refDbStagingConnString ?? (refDbStagingConnString = DbConnectionStringManager.StagingConnectionString);
	}
}
