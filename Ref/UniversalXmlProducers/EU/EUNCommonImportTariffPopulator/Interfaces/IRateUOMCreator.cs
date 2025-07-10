using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public interface IRateUOMCreator
	{
		IEnumerable<RefCusRateUOM> Get(IEnumerable<measureComponent> components);
		IEnumerable<RefCusRateUOM> Get(IEnumerable<measureCondition> conditions);
	}
}
