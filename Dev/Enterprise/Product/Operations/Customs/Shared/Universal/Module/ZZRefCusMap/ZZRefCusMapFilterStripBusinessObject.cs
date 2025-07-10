using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ModuleIDs = Enterprise.ZArchitecture.Modules.ModuleIDs;

namespace Enterprise.Customs.Universal.Module
{
	class ZZRefCusMapFilterStripBusinessObject : FilterStripBusinessObject
	{
		public ZZRefCusMapFilterStripBusinessObject()
		{
		}

		ModuleNkFilter countryOrGroupingFilter;
		ModuleTextFilter mapTypeFilter;
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var filter = filters.AddTextFilter(Constants.ZZRefCusMapFilters.CustomsValue, ZZRefCusMapCombinedSchema.ZZM_CustomsValue);
			filter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank);
			filter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusMapFilter|TransportMode", Constants.ZZRefCusMapFilters.CustomsValue);

			filter = filters.AddTextFilter(Constants.ZZRefCusMapFilters.CW1OrCommercialValue, ZZRefCusMapCombinedSchema.ZZM_CW1orCommercialValue);
			filter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank);
			filter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusMapFilter|CW1OrCommercialValue", Constants.ZZRefCusMapFilters.CW1OrCommercialValue);

			countryOrGroupingFilter = filters.AddNkFilter(Constants.RefCusTariffFilters.CountryOrGrouping, GetCountryOrGroupingQuery, ModuleIDs.Customs.Universal.RefDataGrouping, new RefDataGroupingCollection(Factory));
			countryOrGroupingFilter.ForeignCodeColumnOverride = RefDataGroupingSchema.ZZZ_DataGrouping;
			countryOrGroupingFilter.DefaultProperty = (ZString)Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			countryOrGroupingFilter.Visibility = FilterVisibility.AlwaysVisible;
			countryOrGroupingFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusMapFilter|CountryOrGrouping", Constants.ZZRefCusMapFilters.CountryOrGrouping);

			mapTypeFilter = filters.AddTextFilter(Constants.ZZRefCusMapFilters.MapType, GetMappingTypeQuery, RefCusMapTypeList.GetList(Factory));
			mapTypeFilter.Visibility = FilterVisibility.AlwaysVisible;
			mapTypeFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCusMapFilter|MapType", Constants.ZZRefCusMapFilters.MapType);

			return filters;
		}

		ZQuery GetCountryOrGroupingQuery(ZString nK)
		{
			return new ZQuery(ZZRefCusMapCombinedSchema.ZZM_ZZZ_NKDataGrouping, nK);
		}

		ZQuery GetMappingTypeQuery(ZString value)
		{
			return new ZQuery(ZZRefCusMapCombinedSchema.ZZM_ZZP_NKMapType, value);
		}

		#region IsSystemDefinedDefaultProperty

		protected override string IsSystemDefinedDefaultProperty => "";

		#endregion
	}
}
