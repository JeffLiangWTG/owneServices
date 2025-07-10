using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Module
{
	public class ZZRefCarrierFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			filters.AddTextFilter(Constants.ZZRefCarrierFilters.Code, ZZRefCarrierCombinedSchema.ZZ4_Code).MultilingualDescription = ResString.GetMultilingualString("ZZRefCarrierFilter|Code", Constants.ZZRefCarrierFilters.Code);
			filters.AddTextFilter(Constants.ZZRefCarrierFilters.Description, ZZRefCarrierCombinedSchema.ZZ4_Description).MultilingualDescription = ResString.GetMultilingualString("ZZRefCarrierFilter|Description", Constants.ZZRefCarrierFilters.Description);
			filters.AddTextFilter(Constants.ZZRefCarrierFilters.Country, ZZRefCarrierCombinedSchema.ZZ4_CountryOrGrouping).MultilingualDescription = ResString.GetMultilingualString("ZZRefCarrierFilter|Country", Constants.ZZRefCarrierFilters.Country);

			var textFilter = filters.AddTextFilter(Constants.ZZRefCarrierFilters.CarrierType, ZZRefCarrierCombinedCollection.GetCarrierTypeFilter, new RefCarrierTypeList());
			textFilter.SubGroup = new HeaderFilterSubGroup();
			textFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCarrierFilter|CarrierType", Constants.ZZRefCarrierFilters.CarrierType);

			var tmtextFilter = filters.AddTextFilter(Constants.ZZRefCarrierFilters.TransportMode, ZZRefCarrierCombinedCollection.GetTransportModeFilter, new RefCarrierTransportModeList());
			tmtextFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCarrierFilter|TransportMode", Constants.ZZRefCarrierFilters.TransportMode);

			var countryOrGroupingFilter = filters.AddNkFilter(Constants.ZZRefCarrierFilters.CountryOrGrouping, GetCountryOrGroupingQuery, ModuleIDs.Customs.Universal.RefDataGrouping, new RefDataGroupingCollection(Factory));
			countryOrGroupingFilter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			countryOrGroupingFilter.Visibility = FilterVisibility.AlwaysVisible;
			countryOrGroupingFilter.MultilingualDescription = ResString.GetMultilingualString("ZZRefCarrierFilter|CountryOrGrouping", Constants.ZZRefCarrierFilters.CountryOrGrouping);

			return filters;
		}

		ZQuery GetCountryOrGroupingQuery(ZString nK)
		{
			return new ZQuery(ZZRefCarrierCombinedSchema.ZZ4_CountryOrGrouping, nK);
		}
	}

	class HeaderFilterSubGroup : ModuleFilterSubGroup
	{
		public override ZQuery GetSubQuery(ZQuery filter)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(ZZRefCarrierCombined));
			var carrierTypeFilter = new ZDBOnlySubQuery(typeof(ZZRefCarrierAttributeCombined), ZZRefCarrierAttributeCombinedSchema.ZZG_ZZ4_CarrierCode);
			carrierTypeFilter.AddToFilter(filter);
			result.AddSubQuery(carrierTypeFilter, JoinCondition.And);
			return result;
		}
	}
}
