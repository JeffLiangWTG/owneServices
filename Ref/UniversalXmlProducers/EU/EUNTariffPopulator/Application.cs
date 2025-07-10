using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator
{
	public static class Application
	{
		const string JsonConfigFile = "CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffPopulator.config.json";
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

		public static string OutputFileFolderPath => Configuration["OutputFileFolderPath"];
		public static string SeleniumServerURL => Configuration["SeleniumServerURL"];
	}
}
