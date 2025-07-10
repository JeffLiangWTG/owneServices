using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator
{
	class Program
	{
		static int Main(string[] args)
		{
			// Setup dataProvider
			var configProvider = new ConfigProvider();
			var httpHelper = new HttpClientHelper();
			var dataProvider = new MonthlyDataProvider(configProvider, httpHelper, configProvider.DownloadFolder_alternative);
			var importTariffs = dataProvider.GetImportTariffs();
			var publicationTime = dataProvider.GetPublicationTime();

			var writer = new ImportTariffWriter();
			writer.Write(importTariffs, publicationTime, configProvider.HasFilters, configProvider.ImportTariffUXmlFile);

			return (int)ProducerStatus.Success;
		}
	}
}
