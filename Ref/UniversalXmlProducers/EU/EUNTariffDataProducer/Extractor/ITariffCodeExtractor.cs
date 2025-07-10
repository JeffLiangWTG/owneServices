using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface ITariffCodeExtractor
	{
		IEnumerable<IWebTariffHeader> GetActualTariffHeaders(string tariffHeader, IEnumerable<IWebTariffHeader> source = null);
	}
}
