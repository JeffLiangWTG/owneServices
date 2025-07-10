using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDPickupHeaderFilterBusinessObject : CYDFilterBusinessObject
	{
		public override SchemaGuidColumn PKSchemaColumn => CYDPickupHeaderSchema.PK;

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => CYDPickupHeaderSchema.YPH_WW_Yard;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddJobNumberFilter(filters);
			AddBulkRunFilter(filters);

			return filters;
		}

		#region AddFilter

		void AddJobNumberFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddNumberFilter(Schema.JobNumber, CYDPickupHeaderSchema.YPH_JobNumber);
			filter.MultilingualDescription = ResString.GetMultilingualString("CYDPickupHeader|CYDPickupHeaderFilterBusinessObject|JobNumber", "Job Number");
		}

		void AddBulkRunFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddFlagFilter(
				"Is Bulk Run",
				"IsBulkRun",
				CYDPickupHeaderSchema.YPH_IsBulkRun,
				ModuleFilterSubGroup.Default);
			filter.MultilingualDescription = ResString.GetMultilingualString("CYDPickupHeader|CYDPickupHeaderFilterBusinessObject|BulkRun", "Bulk Run");
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			filter.Category = FilterCategories.Other;
			filter["IsBulkRun"] = true;
		}

		#endregion
	}
}
