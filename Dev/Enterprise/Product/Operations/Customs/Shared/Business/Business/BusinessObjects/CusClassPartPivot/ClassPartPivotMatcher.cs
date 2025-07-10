using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class ClassPartPivotMatcher
	{
		public ClassPartPivotMatcher(IEnumerable<BaseCusClassPartPivot> pivots, ZString htbCode, bool ignoreMatchOnOrgPK = false)
		{
			this.pivots = Argument.NotNull(pivots, nameof(pivots)).OrderBy(x => x.CI_DateStart.IsEmpty ? ZDateTime.MinSmallDateTimeValue : x.CI_DateStart).ToArray();
			this.htbCode = htbCode;
			this.ignoreMatchOnOrgPK = ignoreMatchOnOrgPK;
		}
		readonly BaseCusClassPartPivot[] pivots;
		readonly ZString htbCode;
		readonly bool ignoreMatchOnOrgPK;

		public BaseCusClassPartPivot[] GetMatchesIgnoringAttributes(string typeOfPivot, ZGuid importerPK, ZGuid supplierPK)
		{
			return GetMatchList(typeOfPivot, importerPK, supplierPK, true).ToArray();
		}

		public BaseCusClassPartPivot[] GetMatches(ZString typeOfPivot, ZGuid importerPK, ZGuid supplierPK, params CusAttributeFilter.AttributeValue[] attribs)
		{
			return GetMatchList(typeOfPivot, importerPK, supplierPK, false, attribs).ToArray();
		}

		public BaseCusClassPartPivot GetMatch(ZString typeOfPivot, ZGuid importerPK, ZGuid supplierPK, ZDate expiringDate, bool ignoreAttribs, params CusAttributeFilter.AttributeValue[] attribs)
		{
			var candidateMatches = GetMatchList(typeOfPivot, importerPK, supplierPK, ignoreAttribs, attribs);
			return GetMatchByExpiringDate(candidateMatches, expiringDate);
		}

		public BaseCusClassPartPivot GetMatch(ZString typeOfPivot, ZGuid importerPK, ZGuid supplierPK, bool ignoreAttribs, params CusAttributeFilter.AttributeValue[] attribs)
		{
			return GetMatchList(typeOfPivot, importerPK, supplierPK, ignoreAttribs, attribs).FirstOrDefault();
		}

		List<BaseCusClassPartPivot> GetMatchList(ZString typeOfPivot, ZGuid importerPK, ZGuid supplierPK, bool ignoreAttribs, params CusAttributeFilter.AttributeValue[] attribs)
		{
			return GetMatchListHelper(typeOfPivot, importerPK, supplierPK, ignoreAttribs, attribs, GetMatches);
		}

		public List<BaseCusClassPartPivot> GetMatchListSkipTypeFallback(ZString typeOfPivot, ZGuid importerPK, ZGuid supplierPK, bool ignoreAttribs, params CusAttributeFilter.AttributeValue[] attribs)
		{
			return GetMatchListHelper(typeOfPivot, importerPK, supplierPK, ignoreAttribs, attribs, GetMatchesSkipTypeFallback);
		}

		List<BaseCusClassPartPivot> GetMatchListHelper(ZString typeOfPivot, ZGuid importerPK, ZGuid supplierPK, bool ignoreAttribs, CusAttributeFilter.AttributeValue[] attribs, Func<ZString, ZString, ZGuid, bool, CusAttributeFilter.AttributeValue[], List<BaseCusClassPartPivot>> getMatchesFunc)
		{
			var result = new List<BaseCusClassPartPivot>();
			if (importerPK.IsValid)
			{
				result = GetMatchesPhase(importerPK);
			}
			if (result.Count == 0 && supplierPK.IsValid)
			{
				result = GetMatchesPhase(supplierPK);
			}
			if (result.Count == 0)
			{
				result = GetMatchesPhase(ZGuid.Empty);
			}
			return result;

			List<BaseCusClassPartPivot> GetMatchesPhase(ZGuid orgPK)
			{
				var result = getMatchesFunc(typeOfPivot, htbCode, orgPK, ignoreAttribs, attribs);
				if (result.Count == 0 && !ignoreAttribs)
				{
					result = getMatchesFunc(typeOfPivot, htbCode, orgPK, ignoreAttribs, []);
				}
				return result;
			}
		}

		BaseCusClassPartPivot GetMatchByExpiringDate(List<BaseCusClassPartPivot> candidateMatches, ZDate dateForSelection)
		{
			BaseCusClassPartPivot recentPivot = null;
			if (dateForSelection.IsValid)
			{
				int smallerSpanDays = 0;
				foreach (BaseCusClassPartPivot pivot in candidateMatches)
				{
					var startDate = pivot.CI_DateStart.IsEmpty ? ZDateTime.MinSmallDateTimeValue : pivot.CI_DateStart;
					var endDate = pivot.CI_DateEnd.IsEmpty ? ZDateTime.MaxSmallDateTimeValue : pivot.CI_DateEnd;

					if (startDate <= dateForSelection && endDate >= dateForSelection)
					{
						var spanDays = (dateForSelection - startDate).Days;
						if (recentPivot == null || (spanDays < smallerSpanDays || (spanDays == smallerSpanDays && (!pivot.CI_DateEnd.IsEmpty && (recentPivot.CI_DateEnd.IsEmpty || recentPivot.CI_DateEnd < endDate)))))
						{
							smallerSpanDays = spanDays;
							recentPivot = pivot;
						}
					}
				}
			}
			else
			{
				recentPivot = candidateMatches.FirstOrDefault();
			}
			return recentPivot;
		}

		List<BaseCusClassPartPivot> GetMatches(ZString typeOfPivot, ZString htb, ZGuid matchOrgPK, bool ignoreAttribs, params CusAttributeFilter.AttributeValue[] attribs)
		{
			var result = GetMatches(typeOfPivot, matchOrgPK, ignoreAttribs, attribs);
			if (result.Count == 0 && !htb.IsEmpty)
			{
				result = GetMatches(htb, matchOrgPK, ignoreAttribs, attribs);
			}
			return result;
		}

		List<BaseCusClassPartPivot> GetMatchesSkipTypeFallback(ZString typeOfPivot, ZString htb, ZGuid matchOrgPK, bool ignoreAttribs, params CusAttributeFilter.AttributeValue[] attribs)
		{
			var specificTypePivots = GetMatches(typeOfPivot, matchOrgPK, ignoreAttribs, attribs);

			return htb.IsEmpty ? specificTypePivots : specificTypePivots.Union(GetMatches(htb, matchOrgPK, ignoreAttribs, attribs)).ToList();
		}

		List<BaseCusClassPartPivot> GetMatches(ZString typeOfPivot, ZGuid matchOrgPK, bool ignoreAttribs, params CusAttributeFilter.AttributeValue[] attribs)
		{
			var result = GetMatches(typeOfPivot, matchOrgPK);
			if (!ignoreAttribs)
			{
				result = GetFilteredMatches(result, attribs);
			}
			return result;
		}

		List<BaseCusClassPartPivot> GetMatches(string typeOfPivot, ZGuid matchOrgPK)
		{
			return pivots.Where(x => !x.IsDeleted && x.CI_ChildType == typeOfPivot && (ignoreMatchOnOrgPK || matchOrgPK == x.CI_OH )).ToList();
		}

		List<BaseCusClassPartPivot> GetFilteredMatches(List<BaseCusClassPartPivot> list, params CusAttributeFilter.AttributeValue[] attribs)
		{
			var result = new List<BaseCusClassPartPivot>();
			var attrib1List = new List<ZString>();
			var attrib2List = new List<ZString>();
			var attrib3List = new List<ZString>();
			foreach (var attributeV in attribs)
			{
				if (!attributeV.Value.IsEmpty)
				{
					List<ZString> attribList = null;
					switch (attributeV.Name)
					{
						case CusAttributeFilter.AttributeFilterName.AT2:
							attribList = attrib2List;
							break;
						case CusAttributeFilter.AttributeFilterName.AT3:
							attribList = attrib3List;
							break;
						default:
							attribList = attrib1List;
							break;
					}
					attribList.Add(attributeV.Value);
				}
			}

			foreach (var pivot in list)
			{
				if (!pivot.IsDeleted && IsMatch(pivot, attrib1List, attrib2List, attrib3List))
				{
					result.Add(pivot);
				}
			}

			return result;
		}

		bool IsMatch(BaseCusClassPartPivot pivot, List<ZString> attrib1List, List<ZString> attrib2List, List<ZString> attrib3List)
		{
			return IsMatch(pivot.Attributes1, attrib1List)
				&& IsMatch(pivot.Attributes2, attrib2List)
				&& IsMatch(pivot.Attributes3, attrib3List);
		}

		bool IsMatch(CusAttributeFilterCollection attributes, List<ZString> attribListToMatch)
		{
			if (attribListToMatch.Count == 0)
			{
				if (attributes.Count != 0)
				{
					return false;
				}
			}
			else
			{//find the matching attrib
				foreach (var attribValue in attribListToMatch)
				{
					if (!attributes.HasValue1(attribValue))
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
