using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer
{
	public interface IRateGenerationStrategy
	{
		void GenerateRates(RefCusTariff targetTariff, IRate rate);
	}
}
