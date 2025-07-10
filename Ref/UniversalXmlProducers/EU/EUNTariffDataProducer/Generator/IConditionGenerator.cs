using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IConditionGenerator
	{
		RefCusCondition Convert(IGroupedMeasureConditionKey groupedMeasureConditionKey, IEnumerable<IRawMeasureConditionRecord> measureConditionRecords, IEnumerable<string> excludedTradeGroups, IEnumerable<IRawRateRecord> uomRecords);
	}
}
