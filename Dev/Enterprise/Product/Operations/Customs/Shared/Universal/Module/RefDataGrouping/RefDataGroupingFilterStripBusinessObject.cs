using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using ModuleIDs = Enterprise.ZArchitecture.Modules.ModuleIDs;

namespace Enterprise.Customs.Universal.Module
{
	class RefDataGroupingFilterStripBusinessObject : FilterStripBusinessObject
	{
		public RefDataGroupingFilterStripBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			var groupFilter = filters.AddTextFilter("Group", RefDataGroupingSchema.ZZZ_DataGrouping);
			groupFilter.Visibility = FilterVisibility.AlwaysVisible;
			groupFilter.MultilingualDescription = ResString.GetMultilingualString("RefDataGroupingFilter|Group", "Group");

			var descriptionFilter = filters.AddTextFilter("Description", RefDataGroupingSchema.ZZZ_Description);
			descriptionFilter.Visibility = FilterVisibility.AlwaysVisible;
			descriptionFilter.MultilingualDescription = ResString.GetMultilingualString("RefDataGroupingFilter|Description", "Description");

			var p1arentGroupFilter = filters.AddGuidFilter("Parent Group", ModuleIDs.Customs.Universal.RefDataGrouping, RefDataGroupingSchema.ZZZ_ZZZ_Grouping, new RefDataGroupingCollection(Factory));
			p1arentGroupFilter.Visibility = FilterVisibility.AlwaysVisible;
			p1arentGroupFilter.MultilingualDescription = ResString.GetMultilingualString("RefDataGroupingFilter|ParentGroup", "Parent Group");
			return filters;
		}
	}
}
