using CargoWise.RefDbRepo.INReferenceData.Services;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public sealed class ErrorCodesXmlProducer
	{
		public ErrorCodesXmlProducer(string dataSource)
		{
			this.dataSource = dataSource;
		}

		public void ProduceXml()
		{
			var dateTimeProvider = new DateTimeProvider();
			var logger = new Logger(dateTimeProvider);
			var downloadContextProvider = new ErrorCodeDownloadContextProvider();
			var outputFolder = AppConfig.Shared.OutputDirectory;

			try
			{
				logger.Log(LogType.Info, $"{dataSource} Start");

				using (var httpClient = new HttpClientWithCookieSupport())
				{
					var downloader = new ErrorCodesDownloader(
											httpClient,
											downloadContextProvider,
											AppConfig.ErrorCodes.SleepInterval,
											AppConfig.ErrorCodes.MaxRetry,
											AppConfig.ErrorCodes.CodeSearchStart,
											AppConfig.ErrorCodes.CodeSearchEnd,
											logger);

					var stratagy = new ErrorCodesXmlProducingStratagy(dataSource, downloader, dateTimeProvider, outputFolder, logger);
					stratagy.ProduceXml();
				}

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
