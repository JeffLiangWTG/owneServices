using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class AccPOSChargeCodeGroupFilterBusinessObject : FilterStripBusinessObject
	{
		public AccPOSChargeCodeGroupFilterBusinessObject() : base()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();
			AddTextFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", AccPOSChargeCodeGroupViewSchema.GRO_Code).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPOSChargeCodeGroup|Code", "Code");
			filters.AddTextFilter("Description", AccPOSChargeCodeGroupViewSchema.GRO_Description).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccPOSChargeCodeGroup|Description", "Description");
		}

		#endregion

		#endregion
	}
}
