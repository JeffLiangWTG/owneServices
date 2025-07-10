using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.STLBillingCollector.Business
{
	public static class AppConfig
	{
		const string JsonConfigFileName = "CargoWise.RefDbRepo.STLBillingCollector.CmdLine.config.json";

		public static class Collectors
		{
			public static string OutputDirectory => Config["collectors:outputDirectory"];
			public static string OutputFileName => Config["collectors:outputFileName"];
			public static string TargetFramework => Config["collectors:targetFramework"];
		}

		public static class Nuget
		{
			public static string FeedUrl => Config["NugetUrl:FeedUrl"];
			public static string PackageUrl => Config["NugetUrl:PackageUrl"];
		}

		public static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					config = configurationBuilder.AddJsonFile(JsonConfigFileName).Build();
				}
				return config;
			}
		}
		static IConfiguration config;
	}
}
