using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public class AddressesFilterBusinessObject : FilterStripBusinessObject
	{
		public AddressesFilterBusinessObject() : base()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "Addresses";
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			AddOrganisationFilters(filters);
			AddCountryFilter(filters);
			AddValidationStatusFilter(filters);
			AddActiveStatusFilters(filters);
			AddAddressSourceAndJobNumberFilter(filters);
			return filters;
		}

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			AddOrgCodeFilter(filters);
			AddOrgNameFilter(filters);
			AddOrgMainCountryFilter(filters);
		}

		void AddOrgMainCountryFilter(ModuleFilterCollection filters)
		{
			var orgMainCountryFilter = new ModuleNkFilter(AddressFilterConstants.OrganisationMainCountry, GetOrgMainCountryQuery, ModuleIDs.RefCountry, Countries);
			orgMainCountryFilter.Category = FilterCategories.Organisations;
			orgMainCountryFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|OrganisationMainCountry", "Organization Main Country/Region");
			filters.AddFilter(orgMainCountryFilter);
		}

		ZQuery GetOrgMainCountryQuery(ZString countryCode)
		{
			var query = new ZDBOnlyQuery(typeof(MDMAdminPanelAddressView));
			var capabilityQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
			capabilityQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, 1);
			query.AddSubQuery(MDMAdminPanelAddressViewSchema.PK, capabilityQuery, JoinCondition.And);
			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressQuery.AddToFilter(OrgAddressSchema.OA_RN_NKCountryCode, countryCode);
			query.AddSubQuery(MDMAdminPanelAddressViewSchema.PK, addressQuery, JoinCondition.And);
			return query;
		}

		void AddOrgCodeFilter(ModuleFilterCollection filters)
		{
			var orgCodeFilter = new ModuleTextFilter(AddressFilterConstants.OrganisationCode, MDMAdminPanelAddressViewSchema.MDM_OrgCode);
			orgCodeFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|OrgCode", "Organization Code");
			orgCodeFilter.Category = FilterCategories.Organisations;
			filters.AddFilter(orgCodeFilter);
		}

		void AddOrgNameFilter(ModuleFilterCollection filters)
		{
			var orgNameFilter = new ModuleTextFilter(AddressFilterConstants.OrganisationName, MDMAdminPanelAddressViewSchema.MDM_OrgName);
			orgNameFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|OrgName", "Organization Name");
			orgNameFilter.Category = FilterCategories.Organisations;
			filters.AddFilter(orgNameFilter);
		}

		#region Validation Status Filter
		void AddValidationStatusFilter(ModuleFilterCollection filters)
		{
			var validationStatusFilter = new ModuleTextFilter(AddressFilterConstants.ValidationStatus, MDMAdminPanelAddressViewSchema.MDM_ValidationStatus, AddressValidationStatusList.AddressValidationStatuses);
			validationStatusFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|ValidationStatus", "Validation Status");
			filters.AddCustomFilter(validationStatusFilter);
			validationStatusFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			validationStatusFilter.DefaultProperty = AddressValidationStatus.Invalid;
			validationStatusFilter.Category = FilterCategories.StatusAndFlags;
			validationStatusFilter.Visibility = FilterVisibility.AlwaysVisible;
		}
		#endregion

		#region ActiveStatusFilter
		void AddActiveStatusFilters(ModuleFilterCollection filters)
		{
			AddActiveStatusFilter(filters, AddressFilterConstants.AddressActiveStatus, GetAddressActiveStatusQuery,
				ResString.GetMultilingualString("48225F09-7E99-40EB-958F-E9BD8056FF9B", "Address Active Status"));
			AddActiveStatusFilter(filters, AddressFilterConstants.OrganisationActiveStatus, GetOrganisationActiveStatusQuery,
				ResString.GetMultilingualString("9EA9279E-6A7D-41AF-9E7D-AF55CE36D405", "Organization Active Status"));
		}

		void AddActiveStatusFilter(ModuleFilterCollection filters, ZString description, GetTextQuery query, MultilingualString multilingualDescription)
		{
			var activeStatusFilter = filters.AddTextFilter(description, query, CancelledStatusList);
			activeStatusFilter.Property = StatusActive;
			activeStatusFilter.Category = FilterCategories.StatusAndFlags;
			activeStatusFilter.Visibility = FilterVisibility.AlwaysApplied;
			activeStatusFilter.DefaultProperty = StatusActive;
			activeStatusFilter.MultilingualDescription = multilingualDescription;
		}

		ZQuery GetAddressActiveStatusQuery(ZString status)
		{
			var query = new ZQuery();

			status = status.Trim();
			if (StatusInactive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_AddressActiveStatus, false);
				query.IgnoreActiveFilter = true;
			}
			else if (StatusActive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_AddressActiveStatus, true);
				query.IgnoreActiveFilter = false;
			}
			else if (StatusAll.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_AddressActiveStatus, true);
				query.AddToFilter(JoinCondition.Or, MDMAdminPanelAddressViewSchema.MDM_AddressActiveStatus, false);
				query.IgnoreActiveFilter = true;
			}

			return query;
		}

		ZQuery GetOrganisationActiveStatusQuery(ZString status)
		{
			var query = new ZQuery();

			status = status.Trim();
			if (StatusInactive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_HeaderActiveStatus, false);
				query.IgnoreActiveFilter = true;
			}
			else if (StatusActive.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_HeaderActiveStatus, true);
				query.IgnoreActiveFilter = false;
			}
			else if (StatusAll.EqualsUnresolvedOrLocalized(status, ignoreCase: true))
			{
				query.AddToFilter(MDMAdminPanelAddressViewSchema.MDM_HeaderActiveStatus, true);
				query.AddToFilter(JoinCondition.Or, MDMAdminPanelAddressViewSchema.MDM_HeaderActiveStatus, false);
				query.IgnoreActiveFilter = true;
			}

			return query;
		}
		#endregion

		#region AddressSourceAndJobNumberFilter

		void AddAddressSourceAndJobNumberFilter(ModuleFilterCollection filters)
		{
			var addressSourceAndJobNumberModuleFilter = new AddressSourceAndJobNumberModuleFilter(AddressFilterConstants.AddressSourceAndJobNumber);
			addressSourceAndJobNumberModuleFilter.Category = FilterCategories.TextSearch;
			addressSourceAndJobNumberModuleFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|AddressSourceAndJobNumber", AddressFilterConstants.AddressSourceAndJobNumber);
			filters.AddCustomFilter(addressSourceAndJobNumberModuleFilter);
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant string")]
		public static class AddressFilterConstants
		{
			public const string AdditionalAddress = "Additional Address Info";
			public const string Address1 = "Address1";
			public const string Address2 = "Address2";
			public const string AddressCode = "Address Code";
			public const string AddressType = "Address Type";
			public const string Country = "Country";
			public const string State = "State";
			public const string City = "City";
			public const string PostCode = "Post Code";
			public const string CompanyName = "Company Name";
			public const string Language = "Language";
			public const string ValidationStatus = "Validation Status";
			public const string OrganisationCode = "Organization Code";
			public const string OrganisationName = "Organization Name";
			public const string OrganisationMainCountry = "Organization Main Country";
			public const string OrganisationActiveStatus = "Organization Active Status";
			public const string AddressActiveStatus = "Address Active Status";
			public const string AddressSourceAndJobNumber = "Address Source / Job Number";
		}

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(AddressFilterConstants.Address1, MDMAdminPanelAddressViewSchema.MDM_Address1).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|Address1", "Address 1");
			filters.AddTextFilter(AddressFilterConstants.Address2, MDMAdminPanelAddressViewSchema.MDM_Address2).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|Address2", "Address 2");
			filters.AddTextFilter(AddressFilterConstants.AdditionalAddress, MDMAdminPanelAddressViewSchema.MDM_AdditionalAddressInformation).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|AdditionalAddress", "Additional Address Info");
			filters.AddTextFilter(AddressFilterConstants.AddressCode, MDMAdminPanelAddressViewSchema.MDM_AddressCode).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|AddressCode", "Address Code");
			filters.AddTextFilter(AddressFilterConstants.State, MDMAdminPanelAddressViewSchema.MDM_State).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|State", "State");
			filters.AddTextFilter(AddressFilterConstants.City, MDMAdminPanelAddressViewSchema.MDM_City).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|City", "City");
			filters.AddTextFilter(AddressFilterConstants.PostCode, MDMAdminPanelAddressViewSchema.MDM_PostCode).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|PostCode", "Post Code");
			filters.AddTextFilter(AddressFilterConstants.CompanyName, MDMAdminPanelAddressViewSchema.MDM_CompanyName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|Company", "Company");
			filters.AddTextFilter(AddressFilterConstants.Language, MDMAdminPanelAddressViewSchema.MDM_Language, () => new CodeDescriptionPairList(OLookUpEditType.Language)).MultilingualDescription = ResString.GetMultilingualString("27f65ee5-65a7-4674-8p9d-8bbba654bd11", "Language");
			ModuleTextFilter filterAddressType = filters.AddTextFilter(AddressFilterConstants.AddressType, MDMAdminPanelAddressViewSchema.MDM_AddressType, DocAddressTypes.GetAddressTypesCodeDescriptionPairList(Factory));
			filterAddressType.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|AddressType", "Address Type");
		}

		#endregion

		#region Country Filter

		void AddCountryFilter(ModuleFilterCollection filters)
		{
			ModuleNkFilter countryFilter = filters.AddNkFilter(AddressFilterConstants.Country, MDMAdminPanelAddressViewSchema.MDM_Country, ModuleIDs.RefCountry, Countries);
			countryFilter.Category = FilterCategories.TextSearch;
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|MDMAdminPanelAddressFilter|Country", "Country/Region");
		}

		#endregion

		#region Countries

		RefCountryCollection Countries => fCountries ?? (fCountries = new RefCountryCollection(Factory));

		RefCountryCollection fCountries;

		#endregion

		#endregion
	}
}
