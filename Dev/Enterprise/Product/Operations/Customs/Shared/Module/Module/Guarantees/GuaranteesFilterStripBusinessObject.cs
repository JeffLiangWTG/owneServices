using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	public class GuaranteesFilterStripBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "FilterConstants")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CA1052:Static holder types should be Static or NotInheritable", Justification = "Class is inherited and cannot be static")]
		public class FilterConstants
		{
			public const string Reference = "Reference";
			public const string TransactionDate = "Transaction Date";
			public const string EndDate = CusGuaranteeHeaderCollection.FilterConstants.EndDate;
			public const string GuaranteeNumber = CusGuaranteeHeaderCollection.FilterConstants.GuaranteeNumber;
			public const string GuaranteeHolder = CusGuaranteeHeaderCollection.FilterConstants.GuaranteeHolder;
			public const string GuaranteeHolders = CusGuaranteeHeaderCollection.FilterConstants.GuaranteeHolders;
			public const string GuaranteeSubType = CusGuaranteeHeaderCollection.FilterConstants.GuaranteeSubType;
			public const string GuaranteeType = CusGuaranteeHeaderCollection.FilterConstants.GuaranteeType;
			public const string CreationCountry = "Creation Country";
			public const string OrganisationCountry = "Organization Country";
		}

		public GuaranteesFilterLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = GetNewLookups();
				}
				return lookups;
			}
		}
		GuaranteesFilterLookups lookups;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddReferenceFilter(filters);
			AddTransactionDateFilter(filters);
			AddGuaranteeNumberFilter(filters);
			AddGuaranteeHolderFilter(filters);
			AddGuaranteeHoldersFilter(filters);
			AddGuaranteeSubTypeFilter(filters);
			AddGuaranteeTypeFilter(filters);
			AddGuaranteeEndDateFilter(filters);
			AddCreationCountryFilter(filters);
			AddOrganisationCountryFilter(filters);
			return filters;
		}

		void AddReferenceFilter(ModuleFilterCollection filters)
		{
			var referenceFilter = new GuaranteesModuleTextFilter(FilterConstants.Reference, this);
			referenceFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|GuaranteesFilter|Reference", FilterConstants.Reference);
			referenceFilter.MaxLength = CusPermitLineTransactionSchema.CPL_Reference.MaxLength;
			filters.AddFilter(referenceFilter);
		}

		void AddTransactionDateFilter(ModuleFilterCollection filters)
		{
			var transactionDateFilter = filters.AddDateFilter(FilterConstants.TransactionDate, GetTransactionDateQuery);
			transactionDateFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|GuaranteesFilter|TransactionDate", FilterConstants.TransactionDate);
			transactionDateFilter.Category = FilterCategories.Dates;
		}

		void AddGuaranteeEndDateFilter(ModuleFilterCollection filters)
		{
			var endDateFilter = filters.AddDateFilter(FilterConstants.EndDate, GetEndDateQuery);
			endDateFilter.Category = FilterCategories.Dates;
			endDateFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|GuaranteesFilter|GuaranteeEndDate", FilterConstants.EndDate);
			endDateFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
		}

		void AddGuaranteeNumberFilter(ModuleFilterCollection filters)
		{
			var guaranteeNumberFilter = filters.AddNumberFilter(FilterConstants.GuaranteeNumber, CusPermitHeaderSchema.CPH_Number);
			guaranteeNumberFilter.MultilingualDescription = ResString.GetMultilingualString("GuaranteesFilter|GuaranteeNumber", FilterConstants.GuaranteeNumber);
			guaranteeNumberFilter.Category = FilterCategories.NumbersAndReferences;
		}

		void AddGuaranteeHolderFilter(ModuleFilterCollection filters)
		{
			var guaranteeHolderFilter = filters.AddGuidFilter(FilterConstants.GuaranteeHolder, ModuleIDs.Organisation, GetGuaranteeHolderQuery, OrganisationList);
			guaranteeHolderFilter.Category = FilterCategories.Organisations;
			guaranteeHolderFilter.MultilingualDescription = ResString.GetMultilingualString("CusPermitFilter|GuaranteeHolder", FilterConstants.GuaranteeHolder);
		}

		ZQuery GetTransactionDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var cusPermitHeaderQuery = new ZDBOnlyQuery(typeof(BaseCusGuaranteeHeader));

			var cusPermitLineReferenceQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitLineTransaction), CusPermitLineTransactionSchema.CPL_CPH_PermitHeader);
			AddDateTimeRange(cusPermitLineReferenceQuery, comparisonOperator, JoinCondition.And, CusPermitLineTransactionSchema.CPL_TransactionDate, fromDate, toDate, false);

			cusPermitHeaderQuery.AddSubQuery(cusPermitLineReferenceQuery, JoinCondition.And);
			return cusPermitHeaderQuery;
		}

		ZQuery GetEndDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZQuery();
			AddDateTimeRange(result, comparisonOperator, JoinCondition.And, CusPermitHeaderSchema.CPH_EndDate, value1, value2);
			if (comparisonOperator == DateComparisonOperator.HasDateInRange && !value1.IsEmpty && value2.IsEmpty)
			{
				result.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, null);
			}
			return result;
		}

		ZQuery GetGuaranteeHolderQuery(ZGuid value)
		{
			return new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, value);
		}

		void AddGuaranteeHoldersFilter(ModuleFilterCollection filters)
		{
			var guaranteeHoldersFilter = new ModuleGuidsFilter(FilterConstants.GuaranteeHolders, ModuleIDs.Organisation, GetGuaranteeHoldersQuery, OrganisationList, OrganisationList);
			guaranteeHoldersFilter.Category = FilterCategories.Organisations;
			guaranteeHoldersFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|GuaranteesFilter|GuaranteeHolders", FilterConstants.GuaranteeHolders);
			filters.AddFilter(guaranteeHoldersFilter);
		}

		ZQuery GetGuaranteeHoldersQuery(ZGuid value1, ZGuid value2)
		{
			return new ZQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, new[] { value1, value2 });
		}

		void AddGuaranteeTypeFilter(ModuleFilterCollection filters)
		{
			ModuleFilter guaranteeSubTypeFilter = filters.AddTextFilter(FilterConstants.GuaranteeType, CusPermitHeaderSchema.CPH_Type, Lookups.GuaranteeTypeList);
			guaranteeSubTypeFilter.Category = FilterCategories.ModesAndTypes;
			guaranteeSubTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|GuaranteesFilter|GuaranteeType", FilterConstants.GuaranteeType);
		}

		void AddGuaranteeSubTypeFilter(ModuleFilterCollection filters)
		{
			ModuleFilter guaranteeSubTypeFilter = filters.AddTextFilter(FilterConstants.GuaranteeSubType, CusPermitHeaderSchema.CPH_SubType);
			guaranteeSubTypeFilter.Category = FilterCategories.ModesAndTypes;
			guaranteeSubTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|GuaranteesFilter|GuaranteeSubType", FilterConstants.GuaranteeSubType);
		}

		void AddCreationCountryFilter(ModuleFilterCollection filters)
		{
			var creationCountryFilter = filters.AddNkFilter(FilterConstants.CreationCountry, CusPermitHeaderSchema.CPH_RN_NKCountryCode, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			creationCountryFilter.Category = FilterCategories.Organisations;
			creationCountryFilter.MultilingualDescription = ResString.GetMultilingualString("5435C646-DDE1-4110-B860-D6DE028D4BB5", FilterConstants.CreationCountry);
			creationCountryFilter.Visibility = FilterVisibility.AlwaysVisible;
			creationCountryFilter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			creationCountryFilter.MaxLength = CusPermitHeaderSchema.CPH_RN_NKCountryCode.MaxLength;
		}

		void AddOrganisationCountryFilter(ModuleFilterCollection filters)
		{
			var organisationCountry = filters.AddNkFilter(FilterConstants.OrganisationCountry, GetOrganisationCountryQuery, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			organisationCountry.Category = FilterCategories.Organisations;
			organisationCountry.MultilingualDescription = ResString.GetMultilingualString("{239855EC-43C2-483F-9B27-2136555EB1E5}", FilterConstants.OrganisationCountry);
			organisationCountry.MaxLength = CusPermitHeaderSchema.CPH_RN_NKCountryCode.MaxLength;
		}

		ZQuery GetOrganisationCountryQuery(ZString countryCode)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, countryCode);

			var guaranteeHeaderQuery = new ZDBOnlyQuery(typeof(BaseCusGuaranteeHeader));
			guaranteeHeaderQuery.AddSubQuery(CusPermitHeaderSchema.CPH_OH_PermitHolder, orgHeaderSubQuery, JoinCondition.And);
			return guaranteeHeaderQuery;
		}

		public OrganisationsFindBoxCollection OrganisationList
		{
			get
			{
				return Factory.GetCachedValue("OrganisationList", delegate
				{
					return new OrganisationsFindBoxCollection(Factory);
				});
			}
		}

		protected virtual GuaranteesFilterLookups GetNewLookups()
		{
			return new GuaranteesFilterLookups(this);
		}
	}
}
