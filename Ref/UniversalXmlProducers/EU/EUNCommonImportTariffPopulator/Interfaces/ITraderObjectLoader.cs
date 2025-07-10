using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public interface ITraderObjectLoader
	{
		IEnumerable<T> Get<T>(IXmlFilter<T> filter, DateTime publicationDate);
	}
}
