using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Module
{
	public class WhsItemTransferHeaderFilterBusinessObject : WhsTransitFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			AddNumberFilters(result);
			return result;
		}

		protected override CargoWise.Schema.SchemaGuidColumn WarehouseFKSchemaColumn => WhsItemTransferHeaderSchema.WTH_WW_Warehouse;

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Schema.JobID, WhsItemTransferHeaderSchema.WTH_ReferenceNumber).MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsItemTransferHeaderFilterBusinessObject|TransferHeaderReference", "Transfer ID");
		}
	}
}
