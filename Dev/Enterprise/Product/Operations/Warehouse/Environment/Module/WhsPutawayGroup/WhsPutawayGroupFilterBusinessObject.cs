using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public class WhsPutawayGroupFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Code = "Code"; // Filter description
			public const string Description = "Description"; // Filter description
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddTextFilters(result);
			return result;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(Schema.Code, WhsPutawayGroupSchema.WPG_Code).MultilingualDescription = ResString.GetMultilingualString("WhsPutawayGroupFilterBusinessObject|Code", "Code");
			filters.AddTextFilter(Schema.Description, WhsPutawayGroupSchema.WPG_Description).MultilingualDescription = ResString.GetMultilingualString("WhsPutawayGroupFilterBusinessObject|Description", "Description");
		}
	}
}
