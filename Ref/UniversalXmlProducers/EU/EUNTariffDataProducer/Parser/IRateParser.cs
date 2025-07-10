using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IRateParser
	{
		IEnumerable<IRawRateRecord> Parse(string filePath);
	}
}
