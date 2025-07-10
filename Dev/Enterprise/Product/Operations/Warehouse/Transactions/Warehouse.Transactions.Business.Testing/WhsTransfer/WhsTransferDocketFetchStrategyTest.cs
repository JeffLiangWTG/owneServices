using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsTransferDocketFetchStrategyTest : WhsDocketFetchStrategyTest<WhsTransfer>
	{
		protected override WhsTransfer CreateDocketAndLine(OrgHeader org, WhsWarehouse warehouse, string docketId, OrgSupplierPart part, decimal numberOfUnits)
		{
			var location = warehouse.DefaultLocation;
			Helper.CreateWhsReceiveWithInventory(org, warehouse, "R" + docketId, part, numberOfUnits, location, "");

			var transfer = Helper.CreateWhsTransfer(org, warehouse, docketId);
			Helper.CreateWhsTransferLine(transfer, part, numberOfUnits, location.ToLocationString(), "");
			transfer.RunPreSaveValidation(); // to commit inventory

			return transfer;
		}
	}
}
