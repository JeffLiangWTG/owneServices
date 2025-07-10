using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public interface ITariffViewFilterData
	{
		ZString RatesApplyToCountry { get; }

		ZDate? EffectiveDate { get; }
	}

	public interface ITariffViewFilterDataSupporter
	{
		ITariffViewFilterData TariffViewFilterData { get; set; }
	}

	public interface ITariffViewFilterDataProvider
	{
		ITariffViewFilterData TariffViewFilterData { get; }
	}
}
