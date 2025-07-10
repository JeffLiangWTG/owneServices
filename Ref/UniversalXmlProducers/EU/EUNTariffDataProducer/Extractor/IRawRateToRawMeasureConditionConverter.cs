using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IRawRateToRawMeasureConditionConverter
	{
		IEnumerable<IRawMeasureConditionRecord> Convert(IRawRateRecord rawRateRecord);
		IEnumerable<IRawRateRecord> Convert(IEnumerable<IRawMeasureConditionRecord> rawMeasureConditions);
	}
}
