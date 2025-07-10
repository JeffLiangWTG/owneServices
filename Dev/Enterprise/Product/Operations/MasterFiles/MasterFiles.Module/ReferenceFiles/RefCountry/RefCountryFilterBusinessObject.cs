using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefCountryFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Economic Group", RefCountrySchema.RN_EconomicGrouping, new EconomicGroupList()).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCountryFilter|EconomicGroup", "Economic Group");
			filters.AddTextFilter("Code", RefCountrySchema.RN_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCountryFilter|Code", "Code");
			filters.AddFiltersForTranslatableText("Name", RefCountrySchema.RN_Desc, typeof(RefCountry), ResString.GetMultilingualString("MasterFiles|RefCountryFilter|Name", "Name"));
		}
	}
}
