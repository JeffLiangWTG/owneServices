using CargoWise.RefDbRepo.Staging.ApplicationConfig;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Configuration;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Loader;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Common
{
	public static class ObjectFactoryHelper
	{
		public static ICountryCodeLoader GetCountryCodeLoader(ILogger logger)
		{
			var countryMatcher = new CountryMatcher();
			var countryCodeLoader = new CountryCodeLoader(countryMatcher, logger);

			return countryCodeLoader;
		}

		public static IMessageHandler GetMessageHandler(ILogger logger, SupportedMessageTypes messageType)
		{
			var stageRepo = StagingRepositoryFactory.GetStagingRepository();
			var messageHandler = new MessageHandler(stageRepo, ConfigurationProvider.InputFolder, ConfigurationProvider.InputFile, logger, messageType);

			return messageHandler;
		}

		public static ITariffHelper GetTariffHelper(ILogger logger)
		{
			var dataLoader = new RefDataLoader(ConfigurationProvider.RefDbServiceURI, ConfigurationProvider.IsRefDbServiceSecure);
			var helper = new TariffHelper(dataLoader, logger);

			return helper;
		}
	}
}
