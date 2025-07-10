using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsWorkOrderTriggerTest : WhsPickableDocketTriggerTest<WhsWorkOrder>
	{
		protected override WhsWorkOrder GetNewDocket(OrgHeader client, WhsWarehouse warehouse)
			=> Helper.CreateWhsWorkOrder(client, warehouse, "WO1", WorkOrderType.Codes.Disassemble);

		protected override WhsDocketLine GetNewDocketLine(WhsWorkOrder docket, OrgSupplierPart part, WhsLocation location)
		{
			var componentPart = Helper.CreateProduct(docket.Client, "PO2");
			Helper.CreateProductBOM(part, componentPart, 1m, componentPart.OP_StockKeepingUnit);

			return Helper.CreateWhsWorkOrderLine(docket, part, 10);
		}

		protected override WhsDocketLineCollection GetAllLines(WhsWorkOrder docket) => docket.AllLines;

		protected override string GetDocketSubTypeNotDefault() => WorkOrderType.Codes.Assemble;
	}
}
