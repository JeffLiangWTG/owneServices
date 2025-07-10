using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefAccessorialFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var codeFilter = filters.AddTextFilter("Code", RefAccessorialSchema.ASI_Code);
			codeFilter.MultilingualDescription = ResString.GetMultilingualString("RefAccessorialFilter|Code", "Code");

			var descriptionFilter = filters.AddTextFilter("Description", RefAccessorialSchema.ASI_Description);
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("RefAccessorialFilter|Description", "Description");
		}
	}
}
