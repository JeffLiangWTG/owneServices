using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IMeasureConditionParser
	{
		IEnumerable<IRawMeasureConditionRecord> Parse(string filePath);
	}
}
