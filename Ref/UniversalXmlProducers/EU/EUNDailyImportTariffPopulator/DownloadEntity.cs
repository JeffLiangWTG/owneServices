using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	internal class DownloadEntity : IDownloadEntity
	{
		public DownloadEntity(DateTime publishDate)
		{
			PublishDate = publishDate;
		}

		public DateTime PublishDate { get; set; }
		public string IncrementalObjectTraderExportDeclarableGoodsNomenclatureLink { get; set; }
		public string IncrementalObjectTraderExportLink { get; set; }
	}
}
