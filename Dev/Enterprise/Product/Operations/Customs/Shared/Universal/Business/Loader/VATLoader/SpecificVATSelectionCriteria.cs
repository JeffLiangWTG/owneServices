using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class SpecificVATSelectionCriteria : IVATSelectionCriteria
	{
		public SpecificVATSelectionCriteria(ZString dataGrouping,
			ZDateTime effectiveDate,
			ZString taxOrFeeCode,
			ISet<ZString> additionalCodes,
			ISet<ZString> tradeGroups = null)
		{
			EffectiveDate = effectiveDate;
			DataGrouping = dataGrouping;
			TaxOrFeeCode = taxOrFeeCode;
			TradeGroups = tradeGroups;
			AdditionalCodes = additionalCodes;
		}

		public ZDateTime EffectiveDate { get; }

		public ZString DataGrouping { get; }

		public ZString TaxOrFeeCode { get; }

		public ISet<ZString> AdditionalCodes { get; }

		public ISet<ZString> TradeGroups { get; }
	}
}
