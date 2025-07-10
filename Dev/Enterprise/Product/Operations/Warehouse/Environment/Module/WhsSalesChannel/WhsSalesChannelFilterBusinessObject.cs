using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	class WhsSalesChannelFilterBusinessObject : FilterStripBusinessObject
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
			filters.AddTextFilter(Schema.Code, WhsSalesChannelSchema.WSH_Code).MultilingualDescription = ResString.GetMultilingualString("WhsSalesChannelFilterBusinessObject|Code", "Code");
			filters.AddTextFilter(Schema.Description, WhsSalesChannelSchema.WSH_Description).MultilingualDescription = ResString.GetMultilingualString("WhsSalesChannelFilterBusinessObject|Description", "Description");
		}
	}
}
