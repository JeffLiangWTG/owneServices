using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator
{
	public interface IDataProvider
	{
		IEnumerable<RefCusTariff> GetImportTariffs();
		DateTime GetPublicationTime();
	}
}
