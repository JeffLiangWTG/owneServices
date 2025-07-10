using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Common.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsTransferLineFromInventoryTestCase : DocketLineFromInventoryHelperTest<WhsTransfer, WhsTransferLine>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TransferLineFromInventoryHelper(new TestNotificationBuffer(), null));
		}

		#endregion

		protected override WhsTransfer GetNewDocket(OrgHeader client, WhsWarehouse warehouse)
		{
			return Helper.CreateWhsTransfer(client, warehouse);
		}

		protected override WhsTransferLine GetNewDocketLine(WhsTransfer docket, OrgSupplierPart part, WhsLocation location)
		{
			var transferLine = Helper.CreateWhsTransferLine(docket, part, 10m, location, location);
			transferLine.RunPreSaveValidation();

			return transferLine;
		}

		protected override void AssertLocations(WhsDocketLine line, WhsInventoryView inventory)
		{
			AssertEquals("TransferFromWarehousePK", inventory.InDocketLine.WarehousePK, ((WhsTransferLine)line).TransferFromWarehousePK);
			if (line.Docket.WD_IsPutawayTransfer)
			{
				AssertEquals("WE_PalletID", inventory.WI_PalletID, line.WE_PalletID);
				AssertEquals("WE_OriginalInventoryStatus", InventoryStatus.Codes.Received, line.WE_OriginalInventoryStatus);
			}

			AssertEquals("WE_WL_TransferFrom", inventory.WI_WL, line.WE_WL_TransferFrom);
			AssertEquals("WE_TransferFromPalletId", inventory.WI_PalletID, line.WE_TransferFromPalletId);
		}

		protected override DocketLineFromInventoryHelper<WhsTransferLine> GetInventoryHelper(WhsDocket docket)
		{
			return new TransferLineFromInventoryHelper(docket?.NotificationSubscriber, (WhsTransfer)docket);
		}
	}
}
