using System;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.ITReferenceData.Business;
using CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures;

namespace CargoWise.RefDbRepo.ITReferenceData.CmdLine.ExportMeasures
{
	public static class ExportMeasuresProgram
	{
		public static async Task RunAsync(string outputPath)
		{
			var logger = new ConsoleErrorLogger();
			using var httpClient = new HttpClient();
			var dateTimeProvider = new DateTimeProvider();
			var httpClientWrapper = new HttpClientWrapper(httpClient);

			var publicationTimeLoader = new PublicationTimeLoader(dateTimeProvider, httpClientWrapper);
			var loader = new ExportMeasuresLoader(logger, dateTimeProvider, httpClient);
			var mapper = new ExportMeasureMapper();
			var tariffLoader = new CusTariffLoader(logger, httpClient, CusTariffLoader.TariffType.Export);
			var tradeGroupLookup = new TradeGroupLookup(logger, httpClient);
			var additionalCodeLookup = new AdditionalCodeLookup(logger, httpClient);

			var producer = new ExportMeasuresProducer(logger, loader, mapper, tariffLoader, tradeGroupLookup, additionalCodeLookup, publicationTimeLoader);
			var xmlProducerOption = new XmlProducerOption(dateTimeProvider, publicationTimeLoader);
			var xmlProducer = new ExportMeasuresXmlProducer(xmlProducerOption);

			var tariffs = await producer.ProduceEntitiesAsync();

			if (tariffs.Count == 0)
			{
				throw new InvalidOperationException("Failed to produce XML data: no tariffs found");
			}

			xmlProducer.ExportToXml(tariffs, outputPath);
		}
	}
}
