using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Module
{
	public class ZZRefCusRulingFilterStripBusinessObject : FilterStripBusinessObject
	{
		public ZZRefCusRulingFilterStripBusinessObject()
		{
		}

		public ZZRefCusRulingFilterLookups Lookups
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
		ZZRefCusRulingFilterLookups lookups;

		#region Implementation

		protected virtual ZZRefCusRulingFilterLookups GetNewLookups()
		{
			return new ZZRefCusRulingFilterLookups(this);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			var numberFilter = filters.AddTextFilter(Constants.ZZRefCusRulingFilters.RulingNumber, ZZRefCusRulingCombinedSchema.ZZX_RulingNumber);
			numberFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			numberFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank);
			numberFilter.Visibility = FilterVisibility.AlwaysVisible;
			numberFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusRulingFilter|RulingNumber", Constants.ZZRefCusRulingFilters.RulingNumber);

			var typeFilter = filters.AddTextFilter(Constants.ZZRefCusRulingFilters.RulingType, ZZRefCusRulingCombinedSchema.ZZX_RulingType, Lookups.RulingTypeList);
			typeFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			typeFilter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank);
			typeFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusRulingFilter|RulingType", Constants.ZZRefCusRulingFilters.RulingType);

			var effectiveDateFilter = filters.AddSingleDateFilter(Constants.ZZRefCusRulingFilters.EffectiveDate, delegate(ZDateTime date)
			{
				ZQuery dateQuery = new ZQuery();
				if (date.IsValid)
				{
					dateQuery.AddToFilter(ZZRefCusRulingCombinedSchema.ZZX_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date);
					dateQuery.AddToFilter(ZZRefCusRulingCombinedSchema.ZZX_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date);
				}
				return dateQuery;
			});
			effectiveDateFilter.Property1 = ZDateTime.Today;
			effectiveDateFilter.Category = FilterCategories.Dates;
			effectiveDateFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusRulingFilter|EffectiveDate", Constants.ZZRefCusRulingFilters.EffectiveDate);

			var appliesToFilter = filters.AddGuidFilter(Constants.ZZRefCusRulingFilters.AppliesTo, ModuleIDs.OrgAddresses, ZZRefCusRulingCombinedSchema.ZZX_OA_AppliesTo, Lookups.AppliesToAddressList);
			appliesToFilter.Category = FilterCategories.Locations;
			appliesToFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusRulingFilter|AppliesTo", Constants.ZZRefCusRulingFilters.AppliesTo);

			var appliesToOrganisation = filters.AddGuidFilter(Constants.ZZRefCusRulingFilters.AppliesToOrg, ModuleIDs.Organisation, GetOrgQuery, Lookups.AppliesToOrgList);
			appliesToOrganisation.Category = FilterCategories.Organisations;
			appliesToOrganisation.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusRulingFilter|AppliesToOrg", "Applies To Organization");

			return filters;
		}

		#endregion

		protected ZQuery GetOrgQuery(ZGuid orgPK)
		{
			var result = new ZDBOnlyQuery(typeof(ZZRefCusRulingCombined));
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);

			result.AddSubQuery(ZZRefCusRulingCombinedSchema.ZZX_OA_AppliesTo, orgAddressQuery, JoinCondition.And);
			result.AddToFilter(JoinCondition.Or, ZZRefCusRulingCombinedSchema.ZZX_OA_AppliesTo, DBNull.Value);
			return result;
		}
	}
}
