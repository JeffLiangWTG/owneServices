using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface INomenclatureProducer
	{
		ICompositeKeyGeneratorResult Run(bool produceXml = true);
	}
}
