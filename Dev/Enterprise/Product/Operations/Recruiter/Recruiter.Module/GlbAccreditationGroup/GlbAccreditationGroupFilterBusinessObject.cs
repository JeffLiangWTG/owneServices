using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Module
{
	public class GlbAccreditationGroupFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Description", GlbAccreditationGroupSchema.HAG_Description).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbAccreditationGroupFilter|Description", "Description");
		}
	}
}
