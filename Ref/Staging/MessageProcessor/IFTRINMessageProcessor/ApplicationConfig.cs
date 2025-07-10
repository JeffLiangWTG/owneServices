using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor
{
	public static class ApplicationConfig
	{
		static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile(JsonConfigFileName)
						.Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		const string JsonConfigFileName = "CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor.config.json";

		public static string OutputPath => Configuration[nameof(OutputPath)];

		public static string ConnectionStrings => DbConnectionStringManager.StagingConnectionString;

	}
}
