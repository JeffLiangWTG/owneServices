using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Module
{
	public class JobMawbFilterBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string MasterbillNumber = "Masterbill Number";
			public const string ServiceLevel = "Service Level";
			public const string Airline = "Airline";
			public const string SerialNo = "Serial No.";
			public const string PortOfLoading = "Port of Loading";
			public const string BorrowedInFrom = "Borrowed 'in' From";
			public const string BorrowedOutTo = "Borrowed 'out' To";
			public const string Printed = "Printed";
			public const string Company = "Company";
			public const string Branch = "Branch";
			#endregion
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddTextFilters(result);
			AddFlagsFilters(result);
			AddRelatedItemFilters(result);

			AccountingFilterStrip.AddJobManagementFilters(result, Env.Security.JobMAWBJobInvoicing);

			return result;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(Descriptions.ServiceLevel, JobMawbSchema.JM_ServiceLevel, GetNeutralAirWaybillServiceLevel).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobMawbFilter|ServiceLevel", "Service Level");

			var airlineFilter = filters.AddTextFilter(Descriptions.Airline, JobMawbSchema.JM_Airline3DigitPrefix);
			airlineFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobMawbFilter|Airline", "Airline");
			airlineFilter.Visibility = FilterVisibility.AlwaysVisible;

			filters.AddTextFilter(Descriptions.SerialNo, JobMawbSchema.JM_MAWB).MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobMawbFilter|SerialNo", "Serial No.");

			var masterbillFilter = filters.AddTextFilter(Descriptions.MasterbillNumber, GetMasterbillNumberQuery);
			masterbillFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobMawbFilter|MasterbillNumber", "Master bill Number");
			masterbillFilter.MaxLength = JobMawbSchema.JM_MAWB.MaxLength + JobMawbSchema.JM_Airline3DigitPrefix.MaxLength + 1;
		}

		ZQuery GetMasterbillNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			var airwayPrefix = TryExtractAirlinePrefix(value);
			var serialNumber = value.Contains('-') ? value.SubstringSafe(value.IndexOf('-') + 1) : value.SubstringSafe(Math.Min(3, value.Length));
			serialNumber = serialNumber.Left(JobMawb.Schema.JM_MAWBMaxLength);

			if (!serialNumber.IsEmpty)
			{
				query.AddToFilter(JobMawbSchema.JM_MAWB, comparisonOperator, serialNumber);

				if (!airwayPrefix.IsEmpty)
				{
					query.AddToFilter(JoinCondition.And, JobMawbSchema.JM_Airline3DigitPrefix, comparisonOperator, airwayPrefix);
				}
			}
			else
			{
				if (!airwayPrefix.IsEmpty)
				{
					query.AddToFilter(JobMawbSchema.JM_MAWB, comparisonOperator, airwayPrefix);
					query.AddToFilter(JoinCondition.Or, JobMawbSchema.JM_Airline3DigitPrefix, comparisonOperator, airwayPrefix);
				}
			}

			if (value.Length > 3 && !value.Contains('-'))
			{
				query.AddToFilter(JoinCondition.Or, JobMawbSchema.JM_MAWB, comparisonOperator, value.Left(JobMawb.Schema.JM_MAWBMaxLength));
			}

			return query;
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			ModuleNkFilter closestPortFilter = filters.AddNkFilter(Descriptions.PortOfLoading, GetPortOfLoadingQuery, ModuleIDs.RefUNLOCO, PortOfLoadings);
			closestPortFilter.Category = FilterCategories.Other;
			closestPortFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobMawbFilter|PortOfLoading", "Port of Loading");

			var companyFilter = filters.AddGuidFilter(Descriptions.Company, ModuleIDs.GlbCompany, JobMawbSchema.JM_GC_Company, new GlbCompanyCollection(Factory));
			companyFilter.Category = FilterCategories.Other;
			companyFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobMawbFilter|Company", "Company");
			companyFilter.Visibility = FilterVisibility.AlwaysVisible;

			var branchFilter = filters.AddGuidFilter(Descriptions.Branch, ModuleIDs.GlbBranchNotCurrentCompanyRelated, JobMawbSchema.JM_GB, new GlbBranchCollection(Factory));
			branchFilter.Category = FilterCategories.Other;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobMawbFilter|Branch", "Branch");
			branchFilter.Visibility = FilterVisibility.AlwaysVisible;

			var borrowedFromFilter = new ModuleGuidFilterForOrg(Descriptions.BorrowedInFrom, ModuleIDs.Organisation,
				GetOrgAddressColumnQueryWithOperatorDelegate(JobMawbSchema.JM_OA_From), BindingLists.Organisations);
			borrowedFromFilter.Category = FilterCategories.Other;
			borrowedFromFilter.SupportsBlankComparisonOperators = true;
			borrowedFromFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobMawbFilter|BorrowedinFrom", "Borrowed 'in' From");
			filters.AddFilter(borrowedFromFilter);

			ModuleGuidFilter borrowedToFilter = filters.AddGuidFilter(Descriptions.BorrowedOutTo, ModuleIDs.Organisation, JobMawbSchema.JM_OH_AllocatedTo, AllocatedTos);
			borrowedToFilter.Category = FilterCategories.Other;
			borrowedToFilter.MultilingualDescription = ResString.GetMultilingualString("BForwarding|JobMawbFilter|orrowedoutTo", "Borrowed 'out' To");
		}

		ZQuery GetPortOfLoadingQuery(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobMawb));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(GlbBranch), JobMawbSchema.JM_GB);
			subQuery.AddToFilter(GlbBranchSchema.GB_RL_NKHomePort, value);
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		static GetGuidQueryWithOperator GetOrgAddressColumnQueryWithOperatorDelegate(SchemaColumn orgAddressColumn)
		{
			return (SQLComparisonOperator comparisonOperator, object pK) => GetOrgAddressColumnQueryWithOperator(pK, orgAddressColumn, comparisonOperator);
		}

		static ZQuery GetOrgAddressColumnQueryWithOperator(object pK, SchemaColumn orgAddressColumn, SQLComparisonOperator comparisonOperator)
		{
			var result = new ZDBOnlyQuery(typeof(JobMawb));
			var notIn = comparisonOperator == SQLComparisonOperator.IsBlank;
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), orgAddressColumn, notIn);

			if (comparisonOperator == SQLComparisonOperator.IsBlank || comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(orgAddressColumn, comparisonOperator == SQLComparisonOperator.IsBlank ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, (ZGuid)pK);
				result.AddSubQuery(orgAddressQuery, JoinCondition.And);
			}
			return result;
		}

		#endregion

		#region Flags

		void AddFlagsFilters(ModuleFilterCollection filters)
		{
			string showCurrentBranchOnly = Res.GetString("Forwarding|JobMawbFilter|ShowCurrentBranchOnly", "Show Current Branch Only");

			ModuleTextFilter printedFilter = filters.AddTextFilter(Descriptions.Printed, GetPrintedFilter, PrintedList);
			printedFilter.Category = FilterCategories.StatusAndFlags;
			printedFilter.DefaultProperty = PrintedCodes.Printed;
			printedFilter.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobMawbFilter|Printed", "Printed");

			ModuleFlagsFilter otherFlagFilters = filters.AddFlagsFilter("Allocated to Consol / Show Branches",
				new string[] { Res.GetString("Forwarding|JobMawbFilter|AllocatedToConsol", "Allocated to Consol"), showCurrentBranchOnly },
				new GetFlagsQuery[] { GetAllocatedToConsolFilter, GetShowAllBranchesFilter });

			otherFlagFilters.Category = FilterCategories.StatusAndFlags;
			otherFlagFilters.MultilingualDescription = ResString.GetMultilingualString("Forwarding|JobMawbFilter|AllocatedToConsolShowBranches", "Allocated to Consol / Show Branches");

			ModuleFlagsFilterDefaults otherFlagFiltersDefaults = new ModuleFlagsFilterDefaults(otherFlagFilters);
			otherFlagFilters.DefaultProperties[showCurrentBranchOnly] = otherFlagFiltersDefaults[showCurrentBranchOnly].IsDefault;
			otherFlagFilters.Visibility = FilterVisibility.AlwaysVisible;
		}

		ZQuery GetPrintedFilter(ZString value)
		{
			var query = new ZQuery();

			if (value == PrintedCodes.Printed)
			{
				query.AddToFilter(JobMawbSchema.JM_IsPrinted, ZBool.True);
			}
			else if (value == PrintedCodes.NotPrinted)
			{
				query.AddToFilter(JobMawbSchema.JM_IsPrinted, ZBool.False);
			}

			return query;
		}

		ZQuery GetAllocatedToConsolFilter(ZBool value)
		{
			if (value)
			{
				var consolQuery = new ZQuery(JobMawbSchema.JM_ParentTableCode, JobConsolSchema.Constants.Prefix);
				return consolQuery.AddToFilter(JoinCondition.And, JobMawbSchema.JM_ParentID, SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				var consolQuery = new ZQuery(JobMawbSchema.JM_ParentTableCode, string.Empty);
				return consolQuery.AddToFilter(JoinCondition.Or, JobMawbSchema.JM_ParentID, SQLComparisonOperator.Equal, null);
			}
		}

		ZQuery GetShowAllBranchesFilter(ZBool value)
		{
			var query = new ZQuery();

			if (value)
			{
				query.AddToFilter(JobMawbSchema.JM_GB, GlbBranch.CurrentBranch.PK);
			}

			return query;
		}

		#endregion

		#endregion

		#region Lookups

		#region Printed Lookup

		CodeDescriptionPairList PrintedList
		{
			get
			{
				if (JMPrintedList == null)
				{
					JMPrintedList = new CodeDescriptionPairList();
					JMPrintedList.AddPair(PrintedCodes.All, Res.GetString("Forwarding|JobMawbFilter|All", "All"));
					JMPrintedList.AddPair(PrintedCodes.Printed, Res.GetString("Forwarding|JobMawbFilter|Printed", "Printed"));
					JMPrintedList.AddPair(PrintedCodes.NotPrinted, Res.GetString("Forwarding|JobMawbFilter|NotPrinted", "Not Printed"));
				}
				return JMPrintedList;
			}
		}

		CodeDescriptionPairList JMPrintedList;

		static class PrintedCodes
		{
			public static string All
			{
				get { return Res.GetString("fe7ee54f-9286-4e66-b3ec-2dfb869cb5f1", "All"); }
			}

			public static string Printed
			{
				get { return Res.GetString("bcf20ff1-84f0-45c3-8b40-514ac72e8d67", "Printed"); }
			}

			public static string NotPrinted
			{
				get { return Res.GetString("bcbaa478-1fd4-49ec-8d2b-efea6cb0899b", "Not Printed"); }
			}
		}

		#endregion

		#region PortOfLoadings Guid LookUp

		public RefUNLOCOCollection PortOfLoadings
		{
			get
			{
				if (fPortOfLoadings == null)
				{
					fPortOfLoadings = new RefUNLOCOCollection(Factory);
				}
				return fPortOfLoadings;
			}
		}

		RefUNLOCOCollection fPortOfLoadings;

		#endregion

		#region Froms Guid Lookup

		public OrgHeaderCollection Froms
		{
			get
			{
				if (fFroms == null)
				{
					fFroms = new OrgHeaderCollection(Factory);
				}
				return fFroms;
			}
		}

		OrgHeaderCollection fFroms;

		#endregion

		#region AllocatedTos Guid Lookup

		public OrgHeaderCollection AllocatedTos
		{
			get
			{
				if (fAllocatedTos == null)
				{
					fAllocatedTos = new OrgHeaderCollection(Factory);
				}
				return fAllocatedTos;
			}
		}

		OrgHeaderCollection fAllocatedTos;

		#endregion

		#region NeutralAirWaybillServiceLevel Lookup

		public CodeDescriptionPairList GetNeutralAirWaybillServiceLevel()
		{
			var result = new CodeDescriptionPairList();

			var defaultServiceLevelList = new OrgCarrierServiceLevelCollection(Factory);
			defaultServiceLevelList.Load();

			foreach (OrgCarrierServiceLevel defaultServiceLevel in defaultServiceLevelList.OfType<OrgCarrierServiceLevel>())
			{
				result.AddPair(defaultServiceLevel.PL_Code, defaultServiceLevel.PL_CarrierServiceLevelDescriptionMultilingual);
			}

			foreach (ModuleTextFilter textFilter in this.OfType<ModuleTextFilter>())
			{
				if (textFilter.Description.Contains(Descriptions.MasterbillNumber, StringComparison.OrdinalIgnoreCase))
				{
					var airlinePrefix = TryExtractAirlinePrefix(textFilter.Property);
					if (!airlinePrefix.IsEmpty)
					{
						var refAirlineQuery = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK);
						refAirlineQuery.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, airlinePrefix);
						refAirlineQuery.AddToFilter(RefAirlineSchema.RM_IsActive, true);
						var miscServQuery = new ZDBOnlyQuery(typeof(OrgMiscServ));
						miscServQuery.AddSubQuery(OrgMiscServSchema.OM_RM_Airline, refAirlineQuery, JoinCondition.And);

						var miscServ = Factory.LoadTop1<OrgMiscServ>(miscServQuery);
						if (miscServ != null)
						{
							foreach (OrgCarrierServiceLevel serviceLevel in miscServ.CarrierServiceLevels.OfType<OrgCarrierServiceLevel>())
							{
								if (!result.ContainsCode(serviceLevel.PL_Code))
								{
									result.AddPair(serviceLevel.PL_Code, serviceLevel.PL_CarrierServiceLevelDescriptionMultilingual);
								}
							}
						}
					}
				}
			}

			return result;
		}

		ZString TryExtractAirlinePrefix(ZString mawbNumber)
		{
			return mawbNumber.Contains('-')
				? mawbNumber.Left(mawbNumber.IndexOf('-')).Left(3)
				: mawbNumber.Left(3);
		}

		#endregion

		#endregion

		#region IAccountingFilterStripHolder Members

		IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (AccountingFilterStrip_innerValue == null)
				{
					AccountingFilterStrip_innerValue = (IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this);
					AccountingFilterStrip_innerValue.Initialize(addProfitLossReasonFilters: true);
				}

				return AccountingFilterStrip_innerValue;
			}
		}
		IAccountingFilterStrip AccountingFilterStrip_innerValue;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobMawb));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => null;

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories
		{
			get { return null; }
		}

		#endregion
	}
}
