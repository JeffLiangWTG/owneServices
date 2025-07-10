using CargoWise.RefDbRepo.CarrierMessagingBuss.Shared;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer
{
	public static class AppConfigurationProvider
	{
		public static AppConfiguration AppConfiguration
		{
			get
			{
				if (appConfiguration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					var configuration = configurationBuilder.AddJsonFile("CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer.config.json")
						.Build();
					appConfiguration = configuration.Get<AppConfiguration>();
				}
				return appConfiguration;
			}
		}
		static AppConfiguration appConfiguration;
	}
}
