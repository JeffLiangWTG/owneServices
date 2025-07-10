using System.Collections.Generic;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface ICompositeKeyTreeGenerator
	{
		ICompositeKeyNode GenerateTree(IEnumerable<INomenclatureRecord> nomenclatureRecords);
	}
}
