using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public interface IExcludeTradeGroupCreator
	{
		IEnumerable<RefCusExcludedTradeGroup> Get(measure measure);
	}
}
