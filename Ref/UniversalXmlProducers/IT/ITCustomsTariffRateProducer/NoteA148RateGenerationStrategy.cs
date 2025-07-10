using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DTOModel;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.HtmlDataExtractor;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer
{
	public sealed class NoteA148RateGenerationStrategy : IRateGenerationStrategy
	{
		public NoteA148RateGenerationStrategy(IPreferenceDataLookup preferenceDataLookup)
		{
			_preferenceDataLookup = Argument.NotNull(preferenceDataLookup, nameof(preferenceDataLookup));
		}

		void IRateGenerationStrategy.GenerateRates(RefCusTariff targetTariff, IRate rate)
		{
			Argument.NotNull(targetTariff, nameof(targetTariff));
			Argument.NotNull(rate, nameof(rate));

			if (rate.RateType == UniversalReferenceConstants.RefCusRateType.Levies)
			{
				foreach (var preference in _preferenceDataLookup.Preferences)
				{
					var refCusRate = RefCusRate.CreateWithPreference(rate, preference.Code, preference.DataGrouping);
					targetTariff.CusRates.Add(refCusRate);
				}
			}
			else
			{
				var defaultStrategy = (IRateGenerationStrategy)new DefaultRateGenerationStrategy();
				defaultStrategy.GenerateRates(targetTariff, rate);
			}
		}

		readonly IPreferenceDataLookup _preferenceDataLookup;
	}
}
