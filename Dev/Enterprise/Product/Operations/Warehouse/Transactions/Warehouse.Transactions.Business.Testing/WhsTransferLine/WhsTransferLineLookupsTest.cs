using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsTransferLineLookupsTest : WhsDocketLineLookupsTest<WhsTransfer, WhsTransferLine>
	{
		#region TestInventoryStatuses

		protected override void TestInventoryStatusesCore()
		{
			var line1 = GetNewDocketLine();
			var available = new CodeDescriptionPair(InventoryStatus.Codes.Available, InventoryStatus.Descriptions.Available);
			var held = new CodeDescriptionPair(InventoryStatus.Codes.Held, InventoryStatus.Descriptions.Held);
			var inTransit = new CodeDescriptionPair(InventoryStatus.Codes.InTransit, InventoryStatus.Descriptions.InTransit);
			AssertContainsExactElementsInAnyOrder(new[] { available, held, inTransit }, line1.Lookups.InventoryStatuses);

			var received = new CodeDescriptionPair(InventoryStatus.Codes.Received, InventoryStatus.Descriptions.Received);
			var putaway = new CodeDescriptionPair(InventoryStatus.Codes.Putaway, InventoryStatus.Descriptions.Putaway);
			var puttingAway = new CodeDescriptionPair(InventoryStatus.Codes.PuttingAway, InventoryStatus.Descriptions.PuttingAway);
			var staged = new CodeDescriptionPair(InventoryStatus.Codes.Staged, InventoryStatus.Descriptions.Staged);
			var readyToPack = new CodeDescriptionPair(InventoryStatus.Codes.ReadyToPack, InventoryStatus.Descriptions.ReadyToPack);
			var transfer1 = GetNewWhsDocket();
			transfer1.WD_IsPutawayTransfer = true;
			line1.WE_WD = transfer1.PK;
			AssertContainsExactElementsInAnyOrder(new[] { available, held, inTransit, received, putaway, puttingAway }, line1.Lookups.InventoryStatuses);

			var pick = Factory.New<WhsPick>();
			var transfer2 = GetNewWhsDocket();
			transfer2.WD_WP_ParentPickForTransfer = pick.PK;
			var line2 = transfer2.Lines.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { available, held, inTransit, staged, readyToPack }, line2.Lookups.InventoryStatuses);

			var transfer3 = GetNewWhsDocket();
			transfer3.WD_DocketSubType = TransferType.Codes.Internal;
			transfer3.WD_WD_ParentDocket = transfer2.PK;
			var line3 = transfer3.Lines.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { available, held, inTransit, staged, readyToPack }, line3.Lookups.InventoryStatuses);
		}

		public void TestInventoryStatusCore_VasOrder()
		{
			var codes = new CodeDescriptionPairList();
			codes.AddPair(InventoryStatus.Codes.Available, InventoryStatus.Descriptions.Available);
			codes.AddPair(InventoryStatus.Codes.Held, InventoryStatus.Descriptions.Held);
			codes.AddPair(InventoryStatus.Codes.InTransit, InventoryStatus.Descriptions.InTransit);
			codes.AddPair(InventoryStatus.Codes.Staged, InventoryStatus.Descriptions.Staged);

			var notify = new TestNotificationBuffer();
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var recieve = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 2m, data.Whs1.FindLocation("A-1"), "");
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 1m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(notify);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(codes, initialTransfer.Lines[0].Lookups.InventoryStatuses);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			vasOrder.MarkVASOrderAsCompleted(notify);
			Factory.Save();

			WhsTransfer returnTransfer;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				returnTransfer = vasOrder.GetOrCreateReturnTransfer(notify);
				AssertNotNull("Precondition", returnTransfer);
			}
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(codes, returnTransfer.Lines[0].Lookups.InventoryStatuses);
		}

		#endregion

		#region TestPickedBy

		public void TestPickedBy()
		{
			var transferLine = GetNewDocketLine();
			AssertEquals(typeof(GlbStaffCollection), new WhsTransferLineLookups(transferLine).PickedBys.GetType());
		}

		#endregion

		#region TransferFromLocations

		public void TestTransferFromLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("Whs2", "Row1");
			var locationsWhs1 = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var locationsWhs2 = whs2.Rows.Single(r => r.WR_Name == "Row1").Locations;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, locationsWhs1[0], "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, whs2, "R2", data.Part1, 10m, locationsWhs2[0], "");

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine1 = transfer.Lines.AddNew();
			transferLine1.WE_OP = data.Part1.PK;
			transferLine1.WE_WL_TransferFrom = locationsWhs1[0].PK;
			transferLine1.WE_TransactionQuantity = 1m;
			transferLine1.RunPreSaveValidation();
			Factory.Save();

			AssertCollectionNotContains(locationsWhs2[0], transferLine1.Lookups.TransferFromLocations);
			AssertCollectionContains(locationsWhs1[0], transferLine1.Lookups.TransferFromLocations);
			AssertCollectionContains(locationsWhs1[1], transferLine1.Lookups.TransferFromLocations);

			transferLine1.Delete();
			transfer.WD_DocketSubType = TransferType.Codes.InterWhsDest;

			var transferLine2 = transfer.Lines.AddNew();
			transferLine2.WE_OP = data.Part1.PK;
			transferLine2.TransferFromWarehousePK = whs2.PK;
			transferLine2.WE_WL_TransferFrom = locationsWhs2[0].PK;
			transferLine2.WE_TransactionQuantity = 1m;
			transferLine2.RunPreSaveValidation();
			Factory.Save();

			AssertCollectionContains(locationsWhs2[0], transferLine2.Lookups.TransferFromLocations);
			AssertCollectionNotContains(locationsWhs1[0], transferLine2.Lookups.TransferFromLocations);
			AssertCollectionNotContains(locationsWhs1[1], transferLine2.Lookups.TransferFromLocations);
		}

		#endregion

		protected override bool NeedWE_WL_TransferFrom => true;
	}
}
