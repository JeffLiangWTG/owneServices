using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	internal interface IDownloadEntity
	{
		DateTime PublishDate { get; set; }
		string IncrementalObjectTraderExportDeclarableGoodsNomenclatureLink { get; set; }
		string IncrementalObjectTraderExportLink { get; set; }
	}
}
