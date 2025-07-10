using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class TariffViewFilterData : ITariffViewFilterData
	{
		public TariffViewFilterData(ZString ratesApplyToCountry, ZDate? effectiveDate)
		{
			RatesApplyToCountry = ratesApplyToCountry;
			EffectiveDate = effectiveDate;
		}

		public ZString RatesApplyToCountry { get; }

		public ZDate? EffectiveDate { get; }
	}
}
