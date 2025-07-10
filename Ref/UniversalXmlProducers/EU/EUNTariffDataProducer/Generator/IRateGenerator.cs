using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IRateGenerator
	{
		IEnumerable<RefCusRate> Convert(IGroupedRateRecord rateRecord, IEnumerable<string> applicableTradeGroups, IEnumerable<IRawMeasureExclusionRecord> measureExclusionRecords, IEnumerable<IRawMeasureConditionRecord> measureConditionRecords);
	}
}
