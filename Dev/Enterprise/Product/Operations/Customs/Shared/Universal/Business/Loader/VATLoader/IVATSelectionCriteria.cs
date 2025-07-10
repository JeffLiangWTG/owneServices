using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public interface IVATSelectionCriteria
	{
		ZDateTime EffectiveDate { get; }
		ZString DataGrouping { get; }
		ISet<ZString> AdditionalCodes { get; }
		ISet<ZString> TradeGroups { get; }
		ZString TaxOrFeeCode { get; }
	}

	public static class VATSelectionCriteriaExtension
	{
		public static ZString FlatAdditionalCodes(this IVATSelectionCriteria criteria) => FlatStringSet(criteria.AdditionalCodes);
		public static ZString FlatTradeGroups(this IVATSelectionCriteria criteria) => FlatStringSet(criteria.TradeGroups);
		static ZString FlatStringSet(ISet<ZString> stringSet) => stringSet == null ? ZString.Empty : (ZString)string.Join(".", stringSet.Where(x => !x.IsEmpty).OrderBy(x => x));
	}
}
