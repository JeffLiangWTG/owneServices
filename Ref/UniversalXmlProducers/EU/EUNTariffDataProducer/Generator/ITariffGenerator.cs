using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface ITariffGenerator
	{
		RefCusTariff GenerateRawTariff(string tariffHeader, IEnumerable<IRawRateRecord> rateRecords, IEnumerable<IRawMeasureExclusionRecord> measureExclusionRecords, IEnumerable<IRawMeasureConditionRecord> measureConditionRecords, IEnumerable<IRawRateRecord> uomRecords);
		RefCusTariff GenerateRawTariffWithonlyMeasureConditions(string tariffHeader, IEnumerable<IRawMeasureConditionRecord> measureConditionRecords, IEnumerable<IRawMeasureExclusionRecord> measureExclusionRecords, IEnumerable<IRawRateRecord> uomRecords);
	}
}
