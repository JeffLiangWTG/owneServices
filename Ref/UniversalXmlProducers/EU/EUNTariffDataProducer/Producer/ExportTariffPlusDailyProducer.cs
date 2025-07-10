using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ExportTariffPlusDailyProducer : ExportTariffProducer
	{
		public ExportTariffPlusDailyProducer(DateTime publishTime, IDailyTariffUpdatesFileProvider dailyTariffUpdatesFileProvider)
		{
			this.publishTime = publishTime;
			this.dailyTariffUpdatesFileProvider = Argument.NotNull(dailyTariffUpdatesFileProvider, nameof(dailyTariffUpdatesFileProvider));
		}

		protected override RawRecord GetNewRawRecord() => new ExportPlusDailyRawRecord(dailyTariffUpdatesFileProvider);

		public override bool DownloadFiles(IEnumerable<IWebFileInfo> webFileInfos)
		{
			var files = base.DownloadFiles(webFileInfos);

			RefreshPublishTime();

			return files;
		}

		void RefreshPublishTime()
		{
			PublishTime = publishTime;
			ApplicationConfig.SetPublishTime(publishTime);
		}

		readonly DateTime publishTime;
		readonly IDailyTariffUpdatesFileProvider dailyTariffUpdatesFileProvider;
	}
}
