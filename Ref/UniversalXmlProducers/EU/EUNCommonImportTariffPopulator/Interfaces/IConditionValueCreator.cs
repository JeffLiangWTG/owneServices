using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public interface IConditionValueCreator
	{
		IEnumerable<RefCusConditionValue> Get(IGrouping<string, measureCondition> conditions, string supplementaryUnit);
	}
}
