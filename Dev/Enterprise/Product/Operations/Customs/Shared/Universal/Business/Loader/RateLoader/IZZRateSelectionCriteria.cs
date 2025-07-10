using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public interface IZZRateSelectionCriteria : IZZApplicabilitySelectionCriteria
	{
		ZString RateType { get; }
		ZString RateCode { get; }
		RateDirection Direction { get; }
	}

	public enum RateDirection
	{
		Both = 0,
		Import = 1,
		Export = 2,
	}

	public static class ZZRateSelectionCriteriaExtension
	{
		public static ZDateTime ValidEffectiveDate(this IZZApplicabilitySelectionCriteria criteria) => criteria.EffectiveDate.IsValid ? criteria.EffectiveDate : ZDateTime.Today;
		public static ZString FlatAdditionalCodes(this IZZApplicabilitySelectionCriteria criteria) => FlatStringSet(criteria.AdditionalCodes);
		public static ZString FlatSecondTradeGroups(this IZZApplicabilitySelectionCriteria criteria) => FlatStringSet(criteria.SecondTradeGroups);
		public static ZString XmlAdditionalCodes(this IZZRateSelectionCriteria criteria) => Utils.GetXmlForValueList(criteria.AdditionalCodes?.Select(x => x.ToString()).ToArray());
		static ZString FlatStringSet(ISet<ZString> stringSet) => stringSet == null ? ZString.Empty : (ZString)string.Join(".", stringSet.Where(x => !x.IsEmpty).OrderBy(x => x));
	}
}
