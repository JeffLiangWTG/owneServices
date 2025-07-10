using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public interface ITariffAdditionalCodeSelectionCriteria
	{
		ZString Category { get; }
		ZDateTime EffectiveDate { get; }
		ZString TradeGroupCountry { get; }
		ZString DataGrouping { get; }
	}

	public static class TariffAdditionalCodeSelectionCriteriaExtension
	{
		public static ZDateTime ValidEffectiveDate(this ITariffAdditionalCodeSelectionCriteria criteria) => criteria.EffectiveDate.IsValid ? criteria.EffectiveDate : ZDateTime.Today;
	}
}


