using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgCreditorGroupFilterBusinessObject : FilterStripBusinessObject
	{
		public OrgCreditorGroupFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", OrgCreditorGroupSchema.OG_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgCreditorGroupFilter|Code", "Code");
			filters.AddTextFilter("Description", OrgCreditorGroupSchema.OG_Desc).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgCreditorGroupFilter|Description", "Description");
		}

		#endregion

		#endregion
	}
}
