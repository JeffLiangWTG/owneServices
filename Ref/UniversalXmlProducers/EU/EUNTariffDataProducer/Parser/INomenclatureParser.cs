using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface INomenclatureParser
	{
		IEnumerable<IRawNomenclatureRecord> Parse(string filePath);
	}
}
