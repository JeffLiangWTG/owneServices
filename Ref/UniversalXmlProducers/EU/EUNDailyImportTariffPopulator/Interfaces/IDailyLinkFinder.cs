using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	public interface IDailyLinkFinder
	{
		DateTime GetPublicationTime();
		IEnumerable<string> GetIncrementalObjectTraderExport_DeclarableGoodsNomenclatureLinkOrderByPublishDateAscending();
		IEnumerable<string> GetIncrementalObjectTraderExportLinkOrderByPublishDateAscending();
	}
}
