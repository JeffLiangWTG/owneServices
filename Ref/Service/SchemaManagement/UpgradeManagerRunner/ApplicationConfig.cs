using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	static class ApplicationConfig
	{
		static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					config = new ConfigurationBuilder().AddJsonFile("CargoWise.RefDbRepo.Service.UpgradeManagerRunner.config.json").Build();
				}
				return config;
			}
		}
		static IConfiguration config;

		public static string SafeConnectionString => Config["ConnectionStrings:safe"];
		public static string StagingDbName => Config[nameof(StagingDbName)];
	}
}
