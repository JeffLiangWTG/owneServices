using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public static class IEnumerablePivotsExtension
	{
		public static BaseCusClassPartPivot[] GetMatchesIgnoringAttributes(this IEnumerable<BaseCusClassPartPivot> pivots, ZString country, string typeOfPivot, ZGuid importerPK, ZGuid supplierPK)
		{
			return GetMatcher(pivots, country, false).GetMatchesIgnoringAttributes(typeOfPivot, importerPK, supplierPK);
		}

		public static BaseCusClassPartPivot GetImportMatch(this IEnumerable<BaseCusClassPartPivot> pivots, ZString country, ZGuid importerPK, ZGuid supplierPK, params CusAttributeFilter.AttributeValue[] attribs)
		{
			return GetMatcher(pivots, country, false).GetMatch(HTICode(country), importerPK, supplierPK, false, attribs);
		}

		public static BaseCusClassPartPivot GetImportMatch(this IEnumerable<BaseCusClassPartPivot> pivots, ZString country, ZGuid importerPK, ZGuid supplierPK, ZBool ignoreMatchOnOrgPK, params CusAttributeFilter.AttributeValue[] attribs)
		{
			return GetMatcher(pivots, country, ignoreMatchOnOrgPK).GetMatch(HTICode(country), importerPK, supplierPK, false, attribs);
		}

		public static BaseCusClassPartPivot GetImportMatch(this IEnumerable<BaseCusClassPartPivot> pivots, ZString country, ZGuid importerPK, ZGuid supplierPK, ZDate expiringDate, params CusAttributeFilter.AttributeValue[] attribs)
		{
			return GetMatcher(pivots, country, false).GetMatch(HTICode(country), importerPK, supplierPK, expiringDate, false, attribs);
		}

		public static BaseCusClassPartPivot GetExportMatch(this IEnumerable<BaseCusClassPartPivot> pivots, ZString country, bool isSchedB, ZGuid importerPK, ZGuid supplierPK)
		{
			var typeOfPivot = isSchedB ? SHBCode(country) : HTECode(country);
			return GetMatcher(pivots, country, false).GetMatch(typeOfPivot, importerPK, supplierPK, true);
		}

		public static BaseCusClassPartPivot GetExportMatch(this IEnumerable<BaseCusClassPartPivot> pivots, ZString country, bool isSchedB, ZGuid importerPK, ZGuid supplierPK, ZBool ignoreMatchOnOrgPK)
		{
			var typeOfPivot = isSchedB ? SHBCode(country) : HTECode(country);
			return GetMatcher(pivots, country, ignoreMatchOnOrgPK).GetMatch(typeOfPivot, importerPK, supplierPK, true);
		}

		public static BaseCusClassPartPivot GetExportMatch(this IEnumerable<BaseCusClassPartPivot> pivots, ZString country, bool isSchedB, ZGuid importerPK, ZGuid supplierPK, ZDate expiringDate)
		{
			var typeOfPivot = isSchedB ? SHBCode(country) : HTECode(country);
			return GetMatcher(pivots, country, false).GetMatch(typeOfPivot, importerPK, supplierPK, expiringDate, true);
		}

		public static BaseCusClassPartPivot[] GetNonDeletedPivots(this IEnumerable<BaseCusClassPartPivot> pivots) => pivots.Where(p => !p.IsDeleted).ToArray();

		static ClassPartPivotMatcher GetMatcher(this IEnumerable<BaseCusClassPartPivot> pivots, ZString country, bool ignoreMatchOnOrgPK) => new ClassPartPivotMatcher(pivots.GetNonDeletedPivots(), HTBCode(country), ignoreMatchOnOrgPK);

		static ZString HTBCode(ZString country) => ClassificationTypeProvider.GetProviderFor(country).HTBCode;
		static ZString HTICode(ZString country) => ClassificationTypeProvider.GetProviderFor(country).HTICode;
		static ZString SHBCode(ZString country) => ClassificationTypeProvider.GetProviderFor(country).SHBCode;
		static ZString HTECode(ZString country) => ClassificationTypeProvider.GetProviderFor(country).HTECode;
	}
}
