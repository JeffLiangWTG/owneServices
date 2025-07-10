using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	class Program
	{
		static int Main(string[] args)
		{
			var monthlyIntegrationFactory = new MonthlyIntegrationFactory();
			var httpClientHelper = new HttpClientHelper();
			var dailyConfigProvider = new DailyConfigProvider();

			var dailyIntegrationHelper = new DailyIntegrationHelper(monthlyIntegrationFactory, dailyConfigProvider, httpClientHelper);
			var dailyHelper = new DailyHelper();
			var declarableLoader = new DeclarableLoader();
			var dailyDataProvider = new DailyDataProvider(dailyIntegrationHelper, dailyConfigProvider, httpClientHelper, dailyHelper, declarableLoader);
			var importTariffs = dailyDataProvider.GetImportTariffs();
			var writer = new ImportTariffWriter();
			writer.Write(importTariffs, dailyDataProvider.GetPublicationTime(), dailyConfigProvider.DailyImportTariffUXmlFile);

			return (int)ProducerStatus.Success;
		}
	}
}
