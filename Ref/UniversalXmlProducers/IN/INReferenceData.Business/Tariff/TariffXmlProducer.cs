using CargoWise.RefDbRepo.INReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class TariffXmlProducer
	{
		public TariffXmlProducer(string dataSource)
		{
			this.dataSource = dataSource;
		}

		public void ProduceXml()
		{
			var locationProvider = new FolderLocationProvider();
			var dateTimeProvider = new DateTimeProvider();
			var logger = new Logger(dateTimeProvider);

			try
			{
				logger.Log(LogType.Info, $"{dataSource} Start");

				var downloader = new TariffPdfDownloader(new HttpClientHelper(), locationProvider, logger);
				var parser = new TariffPdfParser(locationProvider, logger);
				var builder = new RefTariffDataBuilder(logger);
				var lastProcessDateStorage = new LocalFileStorage("LastTariffProcessedDate");

				var stratagy = new TariffXmlProducingStratagy(
					dataSource,
					downloader,
					parser,
					builder,
					locationProvider,
					dateTimeProvider,
					lastProcessDateStorage,
					AppConfig.Tariff.DownloadPdfFiles,
					AppConfig.Tariff.ProcessPdfFiles,
					AppConfig.Tariff.GenerateRefDataXml,
					logger);
				stratagy.ProduceXml();

				logger.Log(LogType.Info, $"{dataSource} End");
			}
			catch (UnhandledApplicationException ex)
			{
				logger.Log(LogType.ReviewRequired, ex.Message);
			}
		}

		readonly string dataSource;
	}
}
