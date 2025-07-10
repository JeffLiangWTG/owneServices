using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	sealed class ImportTariffPlusDailyProducerForTest : ImportTariffPlusDailyProducer
	{
		public ImportTariffPlusDailyProducerForTest(DateTime publishTime,
			IDailyTariffUpdatesFileProvider dailyTariffUpdatesFileProvider,
			INomenclaturePlusDailyDataTestProvider dataTestProvider)
			: base(publishTime, dailyTariffUpdatesFileProvider)
		{
			this.dataTestProvider = dataTestProvider;
		}

		public override bool DownloadFiles(IEnumerable<IWebFileInfo> webFileInfos)
		{
			base.DownloadFiles(webFileInfos);

			if (webFileInfos is List<IWebFileInfo> result)
			{
				var dutiesImportPath = TestHelper.CopyToDownloadPath(dataTestProvider.DutiesImportFile.Path, "Duties Import.xlsx", dataTestProvider.DutiesImportFile.PublicationDate);
				result.Add(new WebFileInfo("Duties Import.xlsx", dutiesImportPath, dataTestProvider.DutiesImportFile.PublicationDate));

				var measureExclusionsPath = TestHelper.CopyToDownloadPath(dataTestProvider.MeasureExclusions.Path, "Measure exclusions.xlsx", dataTestProvider.MeasureExclusions.PublicationDate);
				result.Add(new WebFileInfo("Measure exclusions.xlsx", measureExclusionsPath, dataTestProvider.MeasureExclusions.PublicationDate));

				var measureConditionPath = TestHelper.CopyToDownloadPath(dataTestProvider.MeasureConditions.Path, "Measure conditions.xlsx", dataTestProvider.MeasureConditions.PublicationDate);
				result.Add(new WebFileInfo("Measure conditions.xlsx", measureConditionPath, dataTestProvider.MeasureConditions.PublicationDate));
			}
			return true;
		}

		public override IEnumerable<IWebFileInfo> LocateWebFiles()
			=> Enumerable.Empty<IWebFileInfo>();

		readonly INomenclaturePlusDailyDataTestProvider dataTestProvider;
	}
}
