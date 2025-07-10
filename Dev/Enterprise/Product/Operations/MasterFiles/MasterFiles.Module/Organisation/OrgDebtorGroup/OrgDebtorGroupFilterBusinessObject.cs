using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgDebtorGroupFilterBusinessObject : FilterStripBusinessObject
	{
		public OrgDebtorGroupFilterBusinessObject()
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
			filters.AddTextFilter("Code", OrgDebtorGroupSchema.OJ_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgDebtorGroupFilter|Code", "Code");
			filters.AddTextFilter("Description", OrgDebtorGroupSchema.OJ_Desc).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgDebtorGroupFilter|Description", "Description");
		}

		#endregion

		#endregion
	}
}
