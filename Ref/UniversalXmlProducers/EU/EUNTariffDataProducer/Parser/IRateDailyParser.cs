using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IRateDailyParser
	{
		IEnumerable<IRawRateDailyRecord> Parse(string filePath);
	}
}
