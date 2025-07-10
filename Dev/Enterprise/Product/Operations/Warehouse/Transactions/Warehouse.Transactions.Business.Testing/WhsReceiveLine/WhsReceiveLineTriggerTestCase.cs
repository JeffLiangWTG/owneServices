using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsReceiveLineTriggerTestCase : WhsDocketLineTriggerTestCase
	{
		protected override WhsDocketLine CreateUnfinalisedDocketLineOnFinalisedDocket(WhsDocketLine finalisedLine)
		{
			var line = base.CreateUnfinalisedDocketLineOnFinalisedDocket(finalisedLine);
			line.WE_OriginalInventoryStatus = InventoryStatus.Codes.Putaway;
			return line;
		}

		protected override WhsDocketLine GetNewDocketLineForNewNonFinalisedDocket(TestDataSimpleEnvironment data)
		{
			var docket = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			return GetNewDocketLineForDocket(docket, data.Part1);
		}

		protected override WhsDocketLine GetNewDocketLineForNewFinalisedDocket(WhsWarehouse whs, OrgHeader client, OrgSupplierPart product, ZDecimal qty)
		{
			var docket = Helper.CreateWhsReceive(client, whs);
			var line = GetNewDocketLineForDocket(docket, product);
			line.WE_TransactionQuantity = qty;
			docket.FinaliseDocketWithoutUserConfirmation();
			return line;
		}

		protected override void SetupDocketLine_CancelDocketWithUncancelledLineShouldBlowUp(WhsDocketLine line)
		{
			line.WE_WL = ZGuid.Empty; // DocketStatus PUT -> ENT
		}

		protected override WhsDocketLine GetNewDocketLineForDocket(WhsDocket docket, OrgSupplierPart product)
		{
			return Helper.CreateWhsReceiveInventoryLine((WhsReceive)docket, product, 1, docket.Warehouse.FindLocation("A-1")).InDocketLine;
		}

		protected override bool DoesCreateNewStock
		{
			get { return true; }
		}
	}
}
