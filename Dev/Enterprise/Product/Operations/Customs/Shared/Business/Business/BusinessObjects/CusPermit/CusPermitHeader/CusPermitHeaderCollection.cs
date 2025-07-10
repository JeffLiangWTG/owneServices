using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[ModuleID(ModuleId.Permits)]
	public class CusPermitHeaderCollection : ActiveBusinessObjectCollection<BaseCusPermitHeader>
	{
		public CusPermitHeaderCollection(BusinessObjectFactory factory, ZString countryCodeToLoad, ZString applicatonCode)
			: base(factory, SetCollectionFilter(countryCodeToLoad, applicatonCode))
		{
		}

		static ZQuery SetCollectionFilter(ZString countryCodeToLoad, ZString applicatonCode)
		{
			var query = new ZQuery();
			if (!countryCodeToLoad.IsEmpty)
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, countryCodeToLoad);
			}
			if (!applicatonCode.IsEmpty)
			{
				query.AddToFilter(CusPermitHeaderSchema.CPH_ApplicationCode, applicatonCode);
			}
			return query;
		}

		public CusPermitHeaderCollection(BusinessObjectFactory factory)
			: base(factory, new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Permit))
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Constant")]
		public static class FilterConstants
		{
			public const string PermitHolder = "Permit Holder";
			public const string PermitNumber = "Permit Number";
			public const string StartDate = "Start Date";
			public const string EndDate = "End Date";
			public const string PermitTypeSubType = "Permit Type/Sub Type";
			public const string PermitRule = "Permit Rule";
			public const string Country = "Country";
			public const string IsCurrent = "Is Current";
			public const string QtyValIndicator = "Qty/Value Indicator";
			public const string AppliesTo = "Applies To";
		}

		protected override bool MatchesFilterCore(BaseCusPermitHeader permitHeader, bool fetchOnlyFromLocalCache)
		{
			var isMatch = base.MatchesFilterCore(permitHeader, fetchOnlyFromLocalCache);
			if (isMatch && MatchesFilterDelegate != null)
			{
				isMatch = MatchesFilterDelegate(permitHeader);
			}
			return isMatch;
		}

		public Func<BaseCusPermitHeader, bool> MatchesFilterDelegate { get; set; }
	}
}
