using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class StmFeatureTestFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Template Name", StmFeatureTestSchema.SFT_FeatureName).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|StmFeatureTestFilter|FeatureName", "Feature Name");
		}
	}
}
