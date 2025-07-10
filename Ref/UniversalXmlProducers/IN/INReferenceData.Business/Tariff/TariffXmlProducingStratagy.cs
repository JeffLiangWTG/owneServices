using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.INReferenceData.Services;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public sealed class TariffXmlProducingStratagy
	{
		public TariffXmlProducingStratagy(
			string dataSource,
			ITariffPdfDownloader downloader,
			ITariffPdfParser parser,
			IRefTariffDataBuilder builder,
			IFolderLocationProvider locationProvider,
			IDateTimeProvider dateTimeProvider,
			ILocalFileStorage lastProcessDateStorage,
			bool downloadPdfFiles,
			bool processPdfFiles,
			bool generateRefDataXml,
			ILogger logger)
		{
			this.dataSource = dataSource;
			this.downloader = downloader;
			this.parser = parser;
			this.builder = builder;
			this.locationProvider = locationProvider;
			this.dateTimeProvider = dateTimeProvider;
			this.lastProcessDateStorage = lastProcessDateStorage;
			this.downloadPdfFiles = downloadPdfFiles;
			this.processPdfFiles = processPdfFiles;
			this.generateRefDataXml = generateRefDataXml;
			this.logger = logger;
		}

		readonly string dataSource;
		readonly ITariffPdfDownloader downloader;
		readonly ITariffPdfParser parser;
		readonly IRefTariffDataBuilder builder;
		readonly IFolderLocationProvider locationProvider;
		readonly IDateTimeProvider dateTimeProvider;
		readonly ILocalFileStorage lastProcessDateStorage;
		readonly bool downloadPdfFiles;
		readonly bool processPdfFiles;
		readonly bool generateRefDataXml;
		readonly ILogger logger;

		public void ProduceXml()
		{
			var lastTariffDate = lastProcessDateStorage.Load<DateTime?>();
			logger.Log(LogType.Info, "Last processed date", lastTariffDate);

			DateTime? currentTariffDate;
			string pdfDownloadsFolder;
			if (downloadPdfFiles)
			{
				currentTariffDate = downloader.GetLatestTariffDate(lastTariffDate);

				if (currentTariffDate == null || lastTariffDate == currentTariffDate)
				{
					logger.Log(LogType.ReviewRequired, "No new data found and last Tariff date is on", lastTariffDate);
					return;
				}

				pdfDownloadsFolder = downloader.DownloadPdfFiles(currentTariffDate.Value);
				if (string.IsNullOrEmpty(pdfDownloadsFolder))
				{
					logger.Log(LogType.ReviewRequired, "No data downloaded for date", currentTariffDate);
					return;
				}
				logger.Log(LogType.Info, "Downloaded data to folder", pdfDownloadsFolder);
			}
			else
			{
				currentTariffDate = lastTariffDate;
				pdfDownloadsFolder = locationProvider.GetPdfDownloadFolder(currentTariffDate);
				logger.Log(LogType.Info, "Pdf download skipped as per cofiguration");
			}

			string jsonFilesCreationFolder;
			if (processPdfFiles)
			{
				jsonFilesCreationFolder = parser.ParseFilesAsJson(pdfDownloadsFolder, currentTariffDate);
				if (string.IsNullOrEmpty(jsonFilesCreationFolder))
				{
					logger.Log(LogType.ReviewRequired, "No data parsed for date", currentTariffDate);
					return;
				}
				logger.Log(LogType.Info, "Parsed json data to folder", jsonFilesCreationFolder);
			}
			else
			{
				currentTariffDate = lastTariffDate;
				jsonFilesCreationFolder = locationProvider.GetJsonCreationFolder(currentTariffDate);
				logger.Log(LogType.Info, "PDF parsing skipped as per configuration");
			}

			if (generateRefDataXml)
			{
				var tariffsDictionary = builder.GetRefData(jsonFilesCreationFolder);
				var xmlWriterConfig = XMLWriterHelper.GetRefTariffWriterConfiguration(currentTariffDate.Value);
				foreach (var (chapter, refTariffs) in tariffsDictionary)
				{
					var dataSourceOfChapter = $"{dataSource} chapter {chapter}";
					XMLWriterHelper.ExportToXMLFile(xmlWriterConfig, refTariffs, dataSourceOfChapter, currentTariffDate.Value, UpdateType.Full, locationProvider.GetOutputTariffXmlFilePath(chapter, dateTimeProvider.GetIndiaToday()));
				}
			}

			lastProcessDateStorage.Save(currentTariffDate);
		}
	}
}
