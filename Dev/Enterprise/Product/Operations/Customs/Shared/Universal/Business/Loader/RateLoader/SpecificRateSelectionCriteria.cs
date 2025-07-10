using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class SpecificRateSelectionCriteria : IZZRateSelectionCriteria
	{
		public SpecificRateSelectionCriteria(ZString countryOfOrigin,
			ZString dataGrouping,
			ZString primaryPreference,
			ZString concessionOrder,
			ISet<ZString> additionalCodes,
			ZDateTime effectiveDate,
			ZString rateType,
			ZString rateCode,
			ISet<ZString> secondTradeGroups = null,
			RateDirection direction = RateDirection.Both)
		{
			EffectiveDate = effectiveDate;
			TradeGroupCountry = countryOfOrigin;
			SecondTradeGroups = secondTradeGroups;
			DataGrouping = dataGrouping;
			PrimaryPreference = primaryPreference;
			AdditionalCodes = additionalCodes;
			ConcessionOrder = concessionOrder;
			RateType = rateType;
			RateCode = rateCode;
			Direction = direction;
		}

		public ZDateTime EffectiveDate { get; }

		public ZString TradeGroupCountry { get; }

		public ISet<ZString> SecondTradeGroups { get; }

		public ZString DataGrouping { get; }

		public ZString PrimaryPreference { get; }

		public ISet<ZString> AdditionalCodes { get; }

		public ZString ConcessionOrder { get; }

		public ZString RateType { get; }

		public ZString RateCode { get; }

		public RateDirection Direction { get; }
	}
}
