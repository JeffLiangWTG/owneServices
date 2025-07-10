using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsAdjustmentTriggerTest : WhsDocketTriggerTest<WhsAdjustment>
	{
		protected override WhsAdjustment GetNewDocket(OrgHeader client, WhsWarehouse warehouse)
			=> Helper.CreateWhsAdjustment(client, warehouse);

		protected override WhsDocketLine GetNewDocketLine(WhsAdjustment docket, OrgSupplierPart part, WhsLocation location)
			=> Helper.CreateWhsAdjustmentLine(docket, part, 10, location);

		protected override string GetDocketSubTypeNotDefault() => AdjustmentType.Codes.Customs;
	}
}
