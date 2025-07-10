using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	static class ApplicationConfig
	{
		static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					config = new ConfigurationBuilder().AddJsonFile("CargoWise.RefDbRepo.Staging.DbUpgrader.config.json").Build();
				}
				return config;
			}
		}
		static IConfiguration config;

		public static string StagingConnectionString => Config["ConnectionStrings:staging"];
		public static string SafeDBName => Config[nameof(SafeDBName)];
	}
}
