using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccGroupsFilterBusinessObject : FilterStripBusinessObject
	{
		public AccGroupsFilterBusinessObject()
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
			filters.AddTextFilter("Code", AccGroupsSchema.AR_Code).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccGroupsFilter|Code", "Code");
			filters.AddFiltersForTranslatableText("Description", AccGroupsSchema.AR_Desc, typeof(AccGroups), ResString.GetMultilingualString("AccGroups|AccGroupsFilter|Description", "Description"));
		}

		#endregion

		#endregion
	}
}
