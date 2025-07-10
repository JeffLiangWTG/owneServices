using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class ConfigurationProvider
	{
		public static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configurationBuilder.AddJsonFile("CargoWise.RefDbRepo.BRReferenceData.CmdLine.config.json");
					configuration = configurationBuilder.Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;
	}
}
