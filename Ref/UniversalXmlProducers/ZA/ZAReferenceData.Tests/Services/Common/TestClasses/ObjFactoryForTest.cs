using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Tariff.TestClasses;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common.TestClasses
{
	internal static class ObjFactoryForTest
	{
		public static ICountryCodeLoader GetCountryCodeLoader(ILogger logger)
		{
			var countryCodeLoader = new CountryCodeLoaderForTest(logger);

			return countryCodeLoader;
		}

		public static IMessageHandler GetMessageHandler(string inputFolder, string inputFile, TestLogger logger, SupportedMessageTypes messageType)
		{
			var messageHandler = new MessageHandlerForTest(inputFolder, inputFile, logger, messageType);

			return messageHandler;
		}

		public static ITariffHelper GetTariffHelper()
		{
			var helper = new TariffHelperForTest();

			return helper;
		}
	}
}
