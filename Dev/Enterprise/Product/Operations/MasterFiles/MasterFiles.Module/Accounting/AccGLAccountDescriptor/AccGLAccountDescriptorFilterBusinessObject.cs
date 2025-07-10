using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccGLAccountDescriptorFilterBusinessObject : FilterStripBusinessObject
	{
		public AccGLAccountDescriptorFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddRelatedFilters(filters);
			AddOtherFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Local Account Number", AccGLAccountDescriptorSchema.AJ_LocalAccountNumber).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLAccountDescriptorFilter|LocalAccountNumber", "Local Account Number");
			filters.AddTextFilter("Account Description", AccGLAccountDescriptorSchema.AJ_AccountDescription).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLAccountDescriptorFilter|AccountDescription", "Account Description");
		}

		#endregion

		#region Related

		void AddRelatedFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter("Parent Account", ModuleIDs.AccGLHeader, AccGLDescriptorPivotSchema.YJ_AG, GlobalAccounts);
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLAccountDescriptorFilter|ParentAccount", "Parent Account");
			filter.SubGroup = new AccGLDescriptorPivotSubGroup();
		}

		class AccGLDescriptorPivotSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccGLAccountDescriptor));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccGLDescriptorPivot), AccGLDescriptorPivotSchema.YJ_AJ);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);

				return query;
			}
		}

		#endregion

		#region Other

		void AddOtherFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter accountTypeFilter = filters.AddTextFilter("Account Type", GetReportCategoryFilter, AccountTypes);
			accountTypeFilter.Category = FilterCategories.Other;
			accountTypeFilter.DefaultProperty = "ALL";
			accountTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLAccountDescriptorFilter|AccountType", "Account Type");

			ModuleTextFilter languageFilter = filters.AddTextFilter("Language Code", GetLanguageFilter, Languages);
			languageFilter.Category = FilterCategories.Other;
			languageFilter.DefaultProperty = "ANY";
			languageFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLAccountDescriptorFilter|LanguageCode", "Language Code");

			ModuleTextFilter countryFilter = filters.AddTextFilter("Country", AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, Countries);
			countryFilter.Category = FilterCategories.Locations;
			countryFilter.DefaultProperty = "ALL";
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLAccountDescriptorFilter|Country", "Country/Region");
			countryFilter.SubGroup = new CountrySubGroup();

			ModuleTextFilter typeFilter = filters.AddTextFilter("Report Type", GetReportTypesFilter, ReportTypes);
			typeFilter.Category = FilterCategories.Other;
			typeFilter.DefaultProperty = "COA";
			typeFilter.IsActive = true;
			typeFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGLAccountDescriptorFilter|ReportTypesFilter", "Report Type");
		}

		ZQuery GetReportCategoryFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			if (!value.IsEmpty && value != "ALL")
			{
				query.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportCategory, value);
			}

			return query;
		}

		ZQuery GetLanguageFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value != "ANY")
			{
				query.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, value);
			}

			return query;
		}

		class CountrySubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(filter);

				return query;
			}
		}

		ZQuery GetReportTypesFilter(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value != "ANY")
			{
				query.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, value);
			}

			return query;
		}
		CodeDescriptionPairList ReportTypes
		{
			get
			{
				if (fReportTypes == null)
				{
					fReportTypes = new CodeDescriptionPairList();
					fReportTypes.Insert(0, new CodeDescriptionPair("COA", Res.GetString("Accounting|AccGLAccountDescriptorFilter|ChartOfAccount", "Chart Of Account")));
				}
				return fReportTypes;
			}
		}

		CodeDescriptionPairList fReportTypes;
		#endregion

		#endregion

		#region Lookups

		#region Languages List

		CodeDescriptionPairList Languages
		{
			get
			{
				if (fLanguages == null)
				{
					fLanguages = new CodeDescriptionPairList(OLookUpEditType.GLLanguage);
					fLanguages.Insert(0, new CodeDescriptionPair("ANY", Res.GetString("Accounting|AccGLAccountDescriptorFilter|AnyLanguage", "Any Language")));
				}

				return fLanguages;
			}
		}

		CodeDescriptionPairList fLanguages;

		#endregion

		#region Countries

		RefCountryCollection Countries
		{
			get
			{
				if (fCountries == null)
				{
					fCountries = new RefCountryCollection(Factory);
				}

				return fCountries;
			}
		}

		RefCountryCollection fCountries;

		#endregion

		#region AccountTypes List

		public virtual CodeDescriptionPairList AccountTypes
		{
			get
			{
				if (fAccountTypes == null)
				{
					fAccountTypes = new CodeDescriptionPairList(OLookUpEditType.GLAccountDescriptorType);
					fAccountTypes.Insert(0, new CodeDescriptionPair("ALL", Res.GetString("Accounting|AccGLAccountDescriptorFilter|All", "All")));
				}

				return fAccountTypes;
			}
		}

		CodeDescriptionPairList fAccountTypes;

		#endregion

		#region GlobalAccounts List

		AccGLHeaderCollection GlobalAccounts
		{
			get
			{
				if (fGlobalAccounts == null)
				{
					ZQuery fAGFilter = new ZQuery(AccGLHeaderSchema.AG_AccountType, AccountTypeComboBoxConstants.BalanceSheetAccount);
					fAGFilter.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, AccountTypeComboBoxConstants.ProfitAndLossAccount);
					fAGFilter.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, AccountTypeComboBoxConstants.Note);
					fGlobalAccounts = new AccGLHeaderCollection(Factory, fAGFilter);
				}

				return fGlobalAccounts;
			}
		}

		AccGLHeaderCollection fGlobalAccounts;

		#endregion

		#endregion
	}
}
