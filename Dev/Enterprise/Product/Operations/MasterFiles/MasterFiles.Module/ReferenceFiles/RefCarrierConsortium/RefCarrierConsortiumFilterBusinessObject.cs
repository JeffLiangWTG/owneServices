using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefCarrierConsortiumFilterBusinessObject : FilterStripBusinessObject
	{
		public RefCarrierConsortiumFilterBusinessObject()
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
			filters.AddTextFilter("Code", RefCarrierConsortiumSchema.RG_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCarrierConsortiumFilter|Code", "Code");
		}

		#endregion

		#endregion
	}
}
