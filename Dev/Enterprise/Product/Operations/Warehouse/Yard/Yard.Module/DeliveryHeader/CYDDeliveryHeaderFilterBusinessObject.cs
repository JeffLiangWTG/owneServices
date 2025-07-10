using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDDeliveryHeaderFilterBusinessObject : CYDFilterBusinessObject
	{
		public override SchemaGuidColumn PKSchemaColumn => CYDDeliveryHeaderSchema.PK;

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => CYDDeliveryHeaderSchema.YDH_WW_Yard;

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
			var filter = filters.AddNumberFilter(Schema.JobNumber, CYDDeliveryHeaderSchema.YDH_JobNumber);
			filter.MultilingualDescription = ResString.GetMultilingualString("CYDDeliveryHeader|CYDDeliveryHeaderFilterBusinessObject|JobNumber", "Job Number");
		}
		void AddBulkRunFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddFlagFilter(
				"Is Bulk Run",
				"IsBulkRun",
				CYDDeliveryHeaderSchema.YDH_IsBulkRun,
				ModuleFilterSubGroup.Default);
			filter.MultilingualDescription = ResString.GetMultilingualString("CYDDeliveryHeader|CYDDeliveryHeaderFilterBusinessObject|BulkRun", "Bulk Run");
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			filter.Category = FilterCategories.Other;
			filter["IsBulkRun"] = true;
		}
		#endregion

	}
}
