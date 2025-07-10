using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer
{
	public sealed class DefaultRateGenerationStrategy : IRateGenerationStrategy
	{
		void IRateGenerationStrategy.GenerateRates(RefCusTariff targetTariff, IRate rate)
		{
			Argument.NotNull(targetTariff, nameof(targetTariff));
			Argument.NotNull(rate, nameof(rate));

			var refCusRate = RefCusRate.Create(rate);
			targetTariff.CusRates.Add(refCusRate);
		}
	}
}
