using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface INomenclatureDailyParser
	{
		IEnumerable<IRawNomenclatureDailyRecord> Parse(string filePath);
	}
}
