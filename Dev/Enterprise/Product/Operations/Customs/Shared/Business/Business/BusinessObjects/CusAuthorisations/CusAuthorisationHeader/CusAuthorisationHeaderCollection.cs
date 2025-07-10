using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Business.CusAuthorisationHeader;

namespace Enterprise.Customs.Business
{
	public class CusAuthorisationHeaderCollection : ActiveBusinessObjectCollection<CusAuthorisationHeader>
	{
		public CusAuthorisationHeaderCollection(BusinessObjectFactory factory) : base(factory, GetFilter())
		{
		}

		public CusAuthorisationHeaderCollection(BusinessObjectFactory factory, ZString type, ZGuid permitHolder, ZDate transactionDate)
			: this(factory, type.IsEmpty ? Array.Empty<ZString>() : new[] { type }, permitHolder.IsValid ? new[] { permitHolder } : Array.Empty<ZGuid>(), transactionDate, ZString.Empty, ZString.Empty)
		{
		}

		public CusAuthorisationHeaderCollection(BusinessObjectFactory factory, ZString[] types, ZGuid[] permitHolders, ZDate transactionDate, ZString ruleCode, ZString ruleCodeValue)
			: base(factory, GetFilter(types, permitHolders, transactionDate, ruleCode, ruleCodeValue))
		{
		}

		static ZQuery GetFilter()
		{
			var filter = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Authorisation);
			filter.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			return filter;
		}

		static ZQuery GetFilter(ZString[] types, ZGuid[] permitHolders, ZDate transactionDate, ZString ruleCode, ZString ruleCodeValue)
		{
			var filter = new ZQuery();

			if (!types.Any() || !permitHolders.Any() || !transactionDate.IsValid)
			{
				filter.IsNoResultQuery = true;
			}
			else
			{
				var query = GetCusAuthorisationHeaderQueryBuilder(GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
				.AddTypeFilter(types)
				.AddPermitHoldersFilter(permitHolders)
				.AddTransactionDateFilter(transactionDate);

				filter = ruleCode.IsEmpty ? query.Build() : query.AddRuleFilter(query.Build(), ruleCode, ruleCodeValue);
			}

			return filter;
		}

		static CusAuthorisationHeaderQueryBuilder GetCusAuthorisationHeaderQueryBuilder(ZString countryCode) => new CusAuthorisationHeaderQueryBuilder(countryCode);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "FilterConstants")]
		public static class FilterConstants
		{
			public const string AuthorisationHolder = "Authorization Holder";
			public const string StartDate = "Start Date";
			public const string EndDate = "End Date";
			public const string AuthorisationNumber = "Authorization Number";
			public const string AuthorisationType = "Authorization Type";
			public const string IsCurrent = "Is Current";
			public const string AuthorisationDescription = "Description";
			public const string AuthorisationAddress = "Authorization Address";
			public const string RuleDetails = "Rule Details";
			public const string Country = "Country/Region";
			public const string AdHoc = "Ad Hoc";
		}
	}
}
