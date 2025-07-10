using System.Globalization;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.DataPurgingProcessor
{
	public static class Application
	{
		const string JsonConfigFile = "CargoWise.RefDbRepo.Staging.DataPurgingProcessor.config.json";
		static IConfiguration configuration;

		public static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configurationBuilder.AddJsonFile(JsonConfigFile);
					configuration = configurationBuilder.Build();
				}
				return configuration;
			}
		}

		public static string RefDbRepoStaging => DbConnectionStringManager.StagingConnectionString;

		public static int PurgeDataMonthsAgo => int.Parse(Configuration[nameof(PurgeDataMonthsAgo)], CultureInfo.InvariantCulture);

		public static bool ShouldPurgeOrphanData => bool.Parse(Configuration[nameof(ShouldPurgeOrphanData)]);

		public static string NeedPurgeTableNames => Configuration[nameof(NeedPurgeTableNames)];
	}
}
