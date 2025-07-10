using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.MasterFiles.Module
{
	public class GlbBranchFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddTextFilters(result);
			AddRelatedItemFilter(result);

			return result;
		}

		protected override SearchField ResolveSearchField(SearchField searchField)
		{
			var fieldNameUpper = searchField.FieldName.ToUpperInvariant();
			switch (fieldNameUpper)
			{
				case SearchFieldConstants.City:
				case SearchFieldConstants.State:
				case SearchFieldConstants.WebAddress:
				case SearchFieldConstants.HomePort:
					return new SearchFieldOverride(searchField, FilterCategories.Locations);
				case SearchFieldConstants.Company:
					return new SearchFieldOverride(searchField, FilterCategories.Other);
				case SearchFieldConstants.AccountingGroupCode:
					var typeFilter = new IndexSearchModuleTextFilter(searchField, BranchManagementCodeList);
					return new SearchFieldOverride(searchField, indexFilterOverride: typeFilter);
				default:
					return base.ResolveSearchField(searchField);
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersFromGlowCore()
		{
			var filters = base.GetModuleFiltersFromGlowCore();
			if (filters.FirstOrDefault(f => f.Description.EqualsIgnoringCase(SearchFieldConstants.Company)) is IndexSearchModuleGuidFilter companyFilter)
			{
				companyFilter.Property = GlbCompany.CurrentCompany.PK;
				companyFilter.DefaultProperty = GlbCompany.CurrentCompany.PK;
				companyFilter.Visibility = FilterVisibility.AlwaysVisible;
			}
			return filters;
		}

		static class SearchFieldConstants
		{
			public const string City = "CITY";
			public const string State = "STATE";
			public const string WebAddress = "WEBADDRESS";
			public const string HomePort = "HOMEPORT";
			public const string Company = "COMPANY";
			public const string AccountingGroupCode = "ACCOUNTINGGROUPCODE";
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", GlbBranchSchema.GB_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbBranchFilter|Code", "Code");
			filters.AddTextFilter("Name", GlbBranchSchema.GB_BranchName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbBranchFilter|Name", "Name");

			var branchManagementCodeFilter = filters.AddTextFilter("Branch Management Code", GlbBranchSchema.GB_AccountingGroupCode, BranchManagementCodeList);
			branchManagementCodeFilter.MultilingualDescription = ResString.GetMultilingualString("2400fe42-aad4-4708-809b-a72634c010ce", "Branch Management Code");

			ModuleFilter filter = filters.AddTextFilter("City", GlbBranchSchema.GB_City);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbBranchFilter|City", "City");

			filter = filters.AddTextFilter("State", GlbBranchSchema.GB_State);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbBranchFilter|State", "State");

			filter = filters.AddTextFilter("Web Address", GlbBranchSchema.GB_WebAddress);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbBranchFilter|WebAddress", "Web Address");
		}

		#endregion

		#region Related Items

		void AddRelatedItemFilter(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddNkFilter("Location/Port", GlbBranchSchema.GB_RL_NKHomePort, ModuleIDs.RefUNLOCO, UNLOCOs);
			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbBranchFilter|LocationPort", "Location/Port");

			ModuleGuidFilter companyFilter = filters.AddGuidFilter("Company", ModuleIDs.GlbCompany, GetCompanyQuery, Companies);
			companyFilter.Property = GlbCompany.CurrentCompany.PK;
			companyFilter.DefaultProperty = GlbCompany.CurrentCompany.PK;
			companyFilter.Visibility = CompanyFilterVisibility();
			companyFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbBranchFilter|Company", "Company");
		}

		protected virtual FilterVisibility CompanyFilterVisibility()
		{
			return FilterVisibility.AlwaysVisible;
		}

		ZQuery GetCompanyQuery(ZGuid companyPK)
		{
			ZQuery result = new ZQuery();
			if (companyPK.IsValid)
			{
				result.AddToFilter(GlbBranchSchema.GB_GC, companyPK);
			}
			return result;
		}

		#endregion

		#endregion

		#region Lookups

		#region Companies

		GlbCompanyCollection Companies
		{
			get { return fCompanies ?? (fCompanies = new GlbCompanyCollection(Factory)); }
		}

		GlbCompanyCollection fCompanies;

		#endregion

		#region Branch Management Codes

		CodeDescriptionPairList BranchManagementCodeList
		{
			get { return AccountingMasterFilesRegistry.Instance.BranchManagementCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList(); }
		}

		#endregion

		#region UNLOCOs

		RefUNLOCOCollection UNLOCOs
		{
			get { return fUNLOCOs ?? (fUNLOCOs = new RefUNLOCOCollection(Factory)); }
		}

		RefUNLOCOCollection fUNLOCOs;

		#endregion

		#endregion
	}
}
