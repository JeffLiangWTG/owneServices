using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.Universal.CusRefRateCodeCollection.FilterConstants;

namespace Enterprise.Customs.Module
{
	public class CusRefRateCodeFilterSrtipBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var countryFilter = filters.AddNkFilter(Constants.CountryCode, CusRefRateCodeSchema.CR7_RN_NKCountryCode, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			countryFilter.Visibility = FilterVisibility.AlwaysVisible;
			countryFilter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			countryFilter.Category = FilterCategories.Locations;
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("F8B76B84-67D6-4602-AD5E-1B2DB99D4F11", Constants.CountryCode);
			countryFilter.ReadOnly = true;

			var rateCodeFilter = filters.AddTextFilter(Constants.RateCode, CusRefRateCodeSchema.CR7_RateCode);
			rateCodeFilter.Visibility = FilterVisibility.AlwaysVisible;
			rateCodeFilter.MultilingualDescription = ResString.GetMultilingualString("F4821195-F44C-4CC3-B9A0-E4797D794F42", Constants.RateCode);

			var descriptionFilter = filters.AddTextFilter(Constants.Description, CusRefRateCodeSchema.CR7_Description);
			descriptionFilter.Visibility = FilterVisibility.AlwaysVisible;
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("4D95046F-23F3-41AE-9A5D-751ACC35E2EE", Constants.Description);

			var rateTypeFilter = filters.AddTextFilter(Constants.RateType, CusRefRateCodeSchema.CR7_RateType, () => Factory.GetCachedValue<RefCusRateTypeCustomizableList>());
			rateTypeFilter.Visibility = FilterVisibility.AlwaysVisible;
			rateTypeFilter.MultilingualDescription = ResString.GetMultilingualString("A9DFD4AE-F6D2-424B-AC9C-7D39DEBDC220", Constants.RateType);

			return filters;
		}
	}
}
