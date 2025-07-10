using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	public interface IDailyDataProvider
	{
		IEnumerable<RefCusTariff> GetImportTariffs();
		IEnumerable<RefCusTariff> GetImportTariffs(IDailyLinkFinder linkFinder);
		DateTime GetPublicationTime();
	}
}
