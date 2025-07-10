using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IMeasureExclusionParser
	{
		IEnumerable<IRawMeasureExclusionRecord> Parse(string filePath);
	}
}
