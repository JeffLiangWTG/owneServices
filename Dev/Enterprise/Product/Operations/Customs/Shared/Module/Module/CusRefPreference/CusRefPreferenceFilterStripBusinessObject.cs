using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.Universal.CusRefPreferenceCollection.FilterConstants;

namespace Enterprise.Customs.Module
{
	public class CusRefPreferenceFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var countryFilter = filters.AddNkFilter(Constants.CountryCode, CusRefPreferenceSchema.CR8_RN_NKCountryCode, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			countryFilter.Visibility = FilterVisibility.AlwaysVisible;
			countryFilter.DefaultProperty = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("360D0A3B-AC14-4438-B085-10B0F3D5B74E", Constants.CountryCode);
			countryFilter.ReadOnly = true;

			var preferenceFilter = filters.AddTextFilter(Constants.Preference, CusRefPreferenceSchema.CR8_Preference);
			preferenceFilter.Visibility = FilterVisibility.AlwaysVisible;
			preferenceFilter.MultilingualDescription = ResString.GetMultilingualString("6FE36665-C1AC-42B0-9907-8E41D32586C9", Constants.Preference);

			var descriptionFilter = filters.AddTextFilter(Constants.Description, CusRefPreferenceSchema.CR8_Description);
			descriptionFilter.Visibility = FilterVisibility.AlwaysVisible;
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("8753272B-2E3A-43E4-91ED-03952A2B88EA", Constants.Description);

			return filters;
		}
	}
}
