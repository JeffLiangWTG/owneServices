using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Module
{
	class CusRefTariffVersionFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddTextFilter("Code", CusRefTariffVersionSchema.CRT_Version).MultilingualDescription = ResString.GetMultilingualString("Customs|RefCusTariffVersionFilter|CRT_Version", "Code");
			filters.AddTextFilter("Description", CusRefTariffVersionSchema.CRT_Description).MultilingualDescription = ResString.GetMultilingualString("Customs|RefCusTariffVersionFilter|CRT_Description", "Description");
			filters.AddDateFilter("EffectiveDate", CusRefTariffVersionSchema.CRT_EffectiveDate).MultilingualDescription = ResString.GetMultilingualString("Customs|RefCusTariffVersionFilter|CRT_EffectiveDate", "Effective Date");

			var countryFilter = filters.AddNkFilter("Country/Region", CusRefTariffVersionSchema.CRT_RN_NKCountryCode, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			countryFilter.Category = FilterCategories.Locations;
			countryFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|RefCusTariffVersionFilter|CRT_RN_NKCountryCode", "Country/Region");
			countryFilter.Visibility = FilterVisibility.AlwaysVisible;
			countryFilter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			countryFilter.ReadOnly = true;

			return filters;
		}
	}
}
