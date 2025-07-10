using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	sealed class DailyTariffUpdatesFileProviderMock : IDailyTariffUpdatesFileProvider
	{
		public DailyTariffUpdatesFileProviderMock(INomenclaturePlusDailyDataTestProvider dataTestProvider)
		{
			this.dataTestProvider = dataTestProvider;
		}

		public ICollection<IRawNomenclatureDailyRecord> NomenclatureDailyRawRecordCollection =>
			NomenclatureFileCollection
			.SelectMany(x => new NomenclatureDailyParser(x.FileName).Parse(x.DownloadPath))
			.ToList();

		public ICollection<IRawRateDailyRecord> RateDailyRawRecordCollection =>
			dataTestProvider.DailyRateFileCollection
			.Where(x => x.PublicationDate >= MinDate)
			.Select(x => new WebFileInfo("Measures", x.Path, x.PublicationDate))
			.SelectMany(x => new RateDailyParser(x.FileName).Parse(x.DownloadPath))
			.ToArray();

		public DateTime? LastDailyPublishTime =>
			NomenclatureFileCollection.OrderBy(x => x.LastModificationTime).LastOrDefault()?.LastModificationTime;

		public void CleanAll()
		{
		}

		public void DownloadAndExtractDailyUpdates(DateTime referenceDate, DateTime minDate)
		{
			MinDate = minDate;
		}

		public DateTime? MinDate { get; private set; }

		ICollection<IWebFileInfo> NomenclatureFileCollection =>
			dataTestProvider.DailyNomenclatureFileCollection
			.Where(x => x.PublicationDate >= MinDate)
			.Select(x => new WebFileInfo("Goods_Nomenclature", x.Path, x.PublicationDate))
			.ToArray();

		readonly INomenclaturePlusDailyDataTestProvider dataTestProvider;
	}
}
