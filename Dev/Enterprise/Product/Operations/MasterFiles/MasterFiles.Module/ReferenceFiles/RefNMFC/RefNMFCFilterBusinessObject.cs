using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefNMFCFilterBusinessObject : FilterStripBusinessObject
	{
		public RefNMFCFilterBusinessObject()
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
			filters.AddTextFilter("Class", RefNMFCSchema.FN_Class).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefNMFCFilter|Class", "Class");
			filters.AddTextFilter("Description", RefNMFCSchema.FN_Description).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefNMFCFilter|Description", "Description");
			filters.AddTextFilter("Item No", RefNMFCSchema.FN_ItemNo).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefNMFCFilter|ItemNo", "Item No");
		}

		#endregion

		#endregion
	}
}
