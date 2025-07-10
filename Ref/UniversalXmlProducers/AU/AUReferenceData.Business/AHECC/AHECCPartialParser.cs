using System;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public sealed class AHECCPartialParser : BaseAHECCParser
	{
		public AHECCPartialParser(IDateTimeProvider dateProvider)
			: base(dateProvider)
		{
		}

		protected override void ParseCore(DateTime runTime)
		{
			var todaysDate = runTime.Date;
			var dataManager = new ProcessingDataManager<ExportTariffProcessingData>("AUExportTariffPartialProcessingData.json");
			var processingData = dataManager.ProcessingData;

			if (DateTime.TryParse(processingData.PartialParserLastRun, out var lastRunDateTime) && lastRunDateTime.Date == todaysDate)
			{
				Console.WriteLine($"Partial update already ran today at {lastRunDateTime}. Skipping partial update.");
				return;
			}

			base.ParseCore(todaysDate);

			processingData.PartialParserLastRun = runTime.ToString("s");
			dataManager.SaveData();
		}

		protected override UpdateType UpdateType => UpdateType.Partial;

		protected override string DataSource => "AU Customs AHECC";

		protected override string OutputFileName => "AU Export Tariff (Partial).xml";

		protected override string DownloadDirectory => ApplicationConfig.AUReferenceFilesPartialDirectory;

		protected override string DownloadFileNamePrefix => ApplicationConfig.AHECCPartialFilePrefix;

		protected override string DataGrouping => Constants.DataGrouping;
	}
}
