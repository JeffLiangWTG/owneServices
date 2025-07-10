using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface ITradeGroupParser
	{
		IEnumerable<IRawTradeGroupRecord> Parse(string filePath);
	}
}
