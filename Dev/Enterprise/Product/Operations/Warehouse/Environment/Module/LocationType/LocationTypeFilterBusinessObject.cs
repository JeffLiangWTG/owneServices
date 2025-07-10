using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public class LocationTypeFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Code = "Code"; // Filter description
			public const string Description = "Description"; // Filter description
			public const string LocationClass = "Location Class"; // Filter Location Class
			public const string DefaultGranularity = "Default Cycle Count Granularity"; // Filter Location Class
		}

		#region GetModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			return filters;
		}

		#endregion

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(Schema.Code, WhsLocationTypeSchema.WLT_Code).MultilingualDescription = ResString.GetMultilingualString("LocationTypeFilterBusinessObject|Code", "Code");
			filters.AddTextFilter(Schema.Description, WhsLocationTypeSchema.WLT_Description).MultilingualDescription = ResString.GetMultilingualString("LocationTypeFilterBusinessObject|Description", "Description");
			AddLocationClassFilter(filters);
			AddDefaultCycleCountGranularityFilter(filters);
		}

		void AddLocationClassFilter(ModuleFilterCollection filters)
		{
			var locationClassFilter = filters.AddTextFilter(Schema.LocationClass, WhsLocationTypeSchema.WLT_LocationClass, () => new CodeLists.LocationClasses());
			locationClassFilter.MultilingualDescription = ResString.GetMultilingualString("LocationTypeFilterBusinessObject|LocationClass", "Location Class");

			locationClassFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			locationClassFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			locationClassFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			locationClassFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			locationClassFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			locationClassFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
		}

		void AddDefaultCycleCountGranularityFilter(ModuleFilterCollection filters)
		{
			var defaultGranularityFilter = filters.AddTextFilter(Schema.DefaultGranularity, WhsLocationTypeSchema.WLT_DefaultCycleCountGranularity, new CodeLists.CycleCountGranularities());
			defaultGranularityFilter.MultilingualDescription = ResString.GetMultilingualString("LocationTypeFilterBusinessObject|DefaultGranularity", "Default Cycle Count Granularity");

			defaultGranularityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			defaultGranularityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			defaultGranularityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			defaultGranularityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			defaultGranularityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			defaultGranularityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
		}

		#endregion
	}
}
