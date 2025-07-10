using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class TariffAdditionalCodeSelectionCriteria : ITariffAdditionalCodeSelectionCriteria
	{
		public TariffAdditionalCodeSelectionCriteria(ZString category, ZDateTime effectiveDate, ZString tradeGroupCountry, ZString dataGrouping)
		{
			Category = category;
			TradeGroupCountry = tradeGroupCountry;
			EffectiveDate = effectiveDate;
			DataGrouping = dataGrouping;
		}

		public ZString Category { get; }

		public ZDateTime EffectiveDate { get; }

		public ZString TradeGroupCountry { get; }

		public ZString DataGrouping { get; }
	}
}
