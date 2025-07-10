using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.T4Runner
{
	class ApplicationConfig
	{
		static readonly IConfigurationRoot Config = new ConfigurationBuilder().AddJsonFile(@"CargoWise.RefDbRepo.T4Runner.config.json").Build();

		public static string SharedPath = Config["SharedRefDataCommonPath"];
	}
}
