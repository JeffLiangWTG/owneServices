using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PutawayExistingPalletManagerTest : WhsTestCaseWithFactory
	{
		public void TestAllocateExistingPalletLocations_Factory_Null()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			AssertExceptionThrown<ArgumentNullException>(() =>
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					null,
					new[] { receive },
					Enumerable.Empty<WhsReceiveLine>(),
					data.Whs1));
		}

		public void TestAllocateExistingPalletLocations_Receive_Null()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			AssertExceptionThrown<ArgumentNullException>(() =>
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					null,
					Enumerable.Empty<WhsReceiveLine>(),
					data.Whs1));
		}

		public void TestAllocateExistingPalletLocations_Inventories_Null()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			AssertExceptionThrown<ArgumentNullException>(() =>
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { receive },
					null,
					data.Whs1));
		}

		public void TestAllocateExistingPalletLocations_Warehouse_Null()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			AssertExceptionThrown<ArgumentNullException>(() =>
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { receive },
					Enumerable.Empty<WhsReceiveLine>(),
					null));
		}

		public void TestAllocateExistingPalletLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var pallet1 = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, loc1, "PLT-1");
			var pallet2 = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, loc2, "PLT-2");
			Factory.Save();

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet1 = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m, null, "PLT-1");
			var newPallet2 = Helper.CreateWhsReceiveLine(newReceive, data.Part2, 10m, null, "PLT-2");

			AssertEquals("Precondition.", ZGuid.Empty, newPallet1.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, newPallet2.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should have set location.", loc1.PK, newPallet1.WE_WL);
			AssertEquals("Should have set location.", loc2.PK, newPallet2.WE_WL);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}

		public void TestAllocateExistingPalletLocations_PutawayTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var dockDoorLocation = data.Whs1.FindLocation("A-3");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var pallet1 = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, loc1, "PLT-1");
			var pallet2 = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, loc2, "PLT-2");
			Factory.Save();

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet1 =
				Helper.CreateInventoryForDockDoorLocation(newReceive, data.Part1, dockDoorLocation, "PLT-1", 10m);
			var newPallet2 =
				Helper.CreateInventoryForDockDoorLocation(newReceive, data.Part1, dockDoorLocation, "PLT-2", 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine1 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, null, "PLT-1", 10m);
			var transferLine2 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, null, "PLT-2", 10m);
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("Precondition.", ZGuid.Empty, transferLine1.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, transferLine2.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should have set location.", loc1.PK, transferLine1.WE_WL);
			AssertEquals("Should have set location.", loc2.PK, transferLine2.WE_WL);
			AssertEquals("Should *not* have changed dock door location.", dockDoorLocation.PK, newPallet1.WI_WL);
			AssertEquals("Should *not* have changed dock door location.", dockDoorLocation.PK, newPallet2.WI_WL);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}

		public void TestAllocateExistingPalletLocations_PutawayTransferLine_Received()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var dockDoorLocation = data.Whs1.FindLocation("A-3");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var pallet1 = Helper.CreateWhsReceiveInventoryLine(oldReceive, data.Part1, 10m, loc1, "PLT-1");
			var pallet2 = Helper.CreateWhsReceiveInventoryLine(oldReceive, data.Part1, 10m, loc2, "PLT-2");
			Factory.Save();

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet1 =
				Helper.CreateInventoryForDockDoorLocation(newReceive, data.Part1, dockDoorLocation, "PLT-1", 10m);
			var newPallet2 =
				Helper.CreateInventoryForDockDoorLocation(newReceive, data.Part1, dockDoorLocation, "PLT-2", 10m);
			Factory.Save();

			AssertEquals("Precondition: Received.", InventoryStatus.Codes.Received, newPallet1.WI_InventoryStatus);
			AssertEquals("Precondition: Received.", InventoryStatus.Codes.Received, newPallet2.WI_InventoryStatus);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should *not* have changed dock door location.", dockDoorLocation.PK, newPallet1.WI_WL);
			AssertEquals("Should *not* have changed dock door location.", dockDoorLocation.PK, newPallet2.WI_WL);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}

		public void TestAllocateExistingPalletLocations_NotAllInventoryPassedIn()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var pallet1 = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, loc1, "PLT-1");
			var pallet2 = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, loc2, "PLT-2");
			Factory.Save();

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet1 = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m, null, "PLT-1");
			var newPallet2 = Helper.CreateWhsReceiveLine(newReceive, data.Part2, 10m, null, "PLT-2");

			AssertEquals("Precondition.", ZGuid.Empty, newPallet1.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, newPallet2.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					new[] { newPallet1 },
					data.Whs1);
			AssertEquals("Should have set location.", loc1.PK, newPallet1.WE_WL);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet2.WE_WL);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}

		public void TestAllocateExistingPalletLocations_ReceiveZeroQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var location = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var line1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 0m, location, "PLT-1");
			var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location, "PLT-1");
			Factory.Save();

			line2.WE_WL = ZGuid.Empty;
			new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { receive },
					new[] { line1, line2 },
					data.Whs1);

			AssertEquals("Should *not* have set location.", ZGuid.Empty, line2.WE_WL);
		}

		public void TestAllocateExistingPalletLocations_DifferentWarehouse()
		{
			var whs2 = Helper.CreateWarehouse("Wh2", "A", 1, 1);
			var data = new TestDataSimpleEnvironment(Factory);

			var oldReceive = Helper.CreateWhsReceive(data.Org1, whs2, "R1");
			Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, whs2.FindLocation("A"), "PLT-1");
			Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, null, "PLT-1");
			Factory.Save();

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m, null, "PLT-1");

			AssertEquals("Precondition.", ZGuid.Empty, newPallet.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should *not* have set the location.", ZGuid.Empty, newPallet.WE_WL);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}

		public void TestAllocateExistingPalletLocations_NoStockOnHand()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var pallet = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, data.Whs1.FindLocation("A"), "PLT-1");
			oldReceive.FinaliseDocket();
			AssertIsFinalisedPrecondition(oldReceive);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();

			AssertEquals("Precondition: Picked.", 0m, pallet.WE_StockOnHand);

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m, null, "PLT-1");

			AssertEquals("Precondition.", ZGuid.Empty, newPallet.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should *not* have set the location.", ZGuid.Empty, newPallet.WE_WL);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}

		public void TestAllocateExistingPalletLocations_CaseInsensitive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var loc = data.Whs1.FindLocation("A");

			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var pallet = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, loc, "PLT-1");
			Factory.Save();

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m, null, "pLt-1");

			AssertEquals("Precondition.", ZGuid.Empty, newPallet.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should have set the location.", loc.PK, newPallet.WE_WL);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}

		public void TestAllocateExistingPalletLocations_PalletAlreadyPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var pallet = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, loc1, "PLT-1");
			Factory.Save();

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m, loc2, "PLT-1");

			AssertEquals("Precondition.", loc2.PK, newPallet.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should *not* have overriden the location.", loc2.PK, newPallet.WE_WL);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}

		public void TestAllocateExistingPalletLocations_NotAPallet()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var loc1 = data.Whs1.FindLocation("A");

			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var pallet = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, loc1, "PLT-1");
			Factory.Save();

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var inventory = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m);

			AssertEquals("Precondition.", ZGuid.Empty, inventory.WE_WL);

			var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
			rowFactory.ResetDatabaseLoadCount();

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should *not* have set the location.", ZGuid.Empty, inventory.WE_WL);
			AssertEquals("Should *not* have gone to the database.", 0, rowFactory.DatabaseLoadCount);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}

		public void TestAllocateExistingPalletLocations_OnThisReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var loc = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var pallet1_1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc, "PLT-1");
			var pallet1_2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "PLT-1");
			AssertEquals("Precondition.", ZGuid.Empty, pallet1_2.WE_WL);

			_ = ((ILineToPutaway)pallet1_1).LocationPK; // Pre-run dock door transfer queries
			_ = ((ILineToPutaway)pallet1_2).LocationPK; // Pre-run dock door transfer queries

			var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
			rowFactory.ResetDatabaseLoadCount();

			using
				(receive
					.GetValidationSuspender()) // Avoid hit for validation, fetch hints should be added at higher level
			{
				var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { receive },
					receive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
				AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
					otherReceivesInventory.Count());
			}

			AssertEquals("Should have set the location.", loc.PK, pallet1_2.WE_WL);
			AssertEquals("Should *not* have gone to the database.", 0, rowFactory.DatabaseLoadCount);
		}

		public void TestAllocateExistingPalletLocations_OnThisReceive_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var loc1 = data.Whs1.FindLocation("A-1");
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var newPallet1 =
				Helper.CreateInventoryForDockDoorLocation(newReceive, data.Part1, dockDoorLocation, "PLT-1", 10m);
			var newPallet2 =
				Helper.CreateInventoryForDockDoorLocation(newReceive, data.Part1, dockDoorLocation, "PLT-1", 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine1 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, loc1, "PLT-1", 10m);
			var transferLine2 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, null, "PLT-1", 10m);
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("Precondition.", ZGuid.Empty, transferLine2.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should have set location.", loc1.PK, transferLine2.WE_WL);
			AssertEquals("Should *not* have changed dock door location.", dockDoorLocation.PK, newPallet2.WI_WL);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}

		public void TestAllocateExistingPalletLocations_OnThisReceive_NotAllInventoryPassedIn()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var loc = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var pallet1_1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc, "PLT-1");
			var pallet1_2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "PLT-1");
			AssertEquals("Precondition.", ZGuid.Empty, pallet1_2.WE_WL);

			_ = ((ILineToPutaway)pallet1_1).LocationPK; // Pre-run dock door transfer queries
			_ = ((ILineToPutaway)pallet1_2).LocationPK; // Pre-run dock door transfer queries

			var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
			rowFactory.ResetDatabaseLoadCount();

			using
				(receive
					.GetValidationSuspender()) // Avoid hit for validation, fetch hints should be added at higher level
			{
				var otherReceivesInventory =
					new PutawayExistingPalletManager().AllocateExistingPalletLocations(
						Factory,
						new[] { receive },
						new[] { pallet1_2 },
						data.Whs1);
				AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
					otherReceivesInventory.Count());
			}

			AssertEquals("Should have set the location.", loc.PK, pallet1_2.WE_WL);
			AssertEquals("Should *not* have gone to the database.", 0, rowFactory.DatabaseLoadCount);
		}

		public void TestAllocateExistingPalletLocations_OnThisReceive_MultipleLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var pallet1_1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc1, "PLT-1");
			var pallet1_2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc2, "PLT-1");
			var pallet1_3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "PLT-1");

			AssertEquals("Precondition.", ZGuid.Empty, pallet1_3.WE_WL);

			_ = ((ILineToPutaway)pallet1_1).LocationPK; // Pre-run dock door transfer queries
			_ = ((ILineToPutaway)pallet1_2).LocationPK; // Pre-run dock door transfer queries
			_ = ((ILineToPutaway)pallet1_3).LocationPK; // Pre-run dock door transfer queries

			var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
			rowFactory.ResetDatabaseLoadCount();

			using
				(receive
					.GetValidationSuspender()) // Avoid hit for validation, fetch hints should be added at higher level
			{
				var otherReceivesInventory =
					new PutawayExistingPalletManager().AllocateExistingPalletLocations(
						Factory,
						new[] { receive },
						receive.Lines.Cast<WhsReceiveLine>(),
						data.Whs1);
				AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
					otherReceivesInventory.Count());
			}

			AssertNotEquals("Should have set the location to something, not blown up.", ZGuid.Empty, pallet1_2.WE_WL);
			AssertEquals("Should *not* have gone to the database.", 0, rowFactory.DatabaseLoadCount);
		}

		public void TestAllocateExistingPalletLocations_OnThisReceive_CaseInsensitive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var loc = data.Whs1.FindLocation("A");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var pallet1_1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc, "PLT-1");
			var pallet1_2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, null, "pLt-1");

			AssertEquals("Precondition.", ZGuid.Empty, pallet1_2.WE_WL);

			_ = ((ILineToPutaway)pallet1_1).LocationPK; // Pre-run dock door transfer queries
			_ = ((ILineToPutaway)pallet1_2).LocationPK; // Pre-run dock door transfer queries

			var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
			rowFactory.ResetDatabaseLoadCount();

			using
				(receive
					.GetValidationSuspender()) // Avoid hit for validation, fetch hints should be added at higher level
			{
				var otherReceivesInventory =
					new PutawayExistingPalletManager().AllocateExistingPalletLocations(
						Factory,
						new[] { receive },
						receive.Lines.Cast<WhsReceiveLine>(),
						data.Whs1);
				AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
					otherReceivesInventory.Count());
			}

			AssertEquals("Should have set the location.", loc.PK, pallet1_2.WE_WL);
			AssertEquals("Should *not* have gone to the database.", 0, rowFactory.DatabaseLoadCount);
		}

		public void TestAllocateExistingPalletLocations_OnThisReceive_PalletAlreadyPutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var pallet1_1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc1, "PLT-1");
			var pallet1_2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, loc2, "PLT-1");

			_ = ((ILineToPutaway)pallet1_1).LocationPK; // Pre-run dock door transfer queries
			_ = ((ILineToPutaway)pallet1_2).LocationPK; // Pre-run dock door transfer queries

			var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
			rowFactory.ResetDatabaseLoadCount();

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { receive },
					receive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should *not* have set the location.", loc1.PK, pallet1_1.WE_WL);
			AssertEquals("Should *not* have set the location.", loc2.PK, pallet1_2.WE_WL);
			AssertEquals("Should *not* have gone to the database.", 0, rowFactory.DatabaseLoadCount);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}

		public void TestAllocateExistingPalletLocations_NotPutawayStockOnAnotherReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var nonPalletisedStock = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m);
			var pallet1 = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, null, "PLT-1");
			var pallet2 = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, null, "PLT-2");
			Factory.Save();

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet1_1 = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m, null, "PLT-1");
			var newPallet1_2 = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m, null, "PLT-1");
			var newPallet2 = Helper.CreateWhsReceiveLine(newReceive, data.Part2, 10m, null, "PLT-2");

			AssertEquals("Precondition.", ZGuid.Empty, newPallet1_1.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, newPallet1_2.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, newPallet2.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet1_1.WE_WL);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet1_2.WE_WL);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet2.WE_WL);
			AssertContainsExactElementsInAnyOrder("Should find stock on other receives needing to be putaway.",
				new[] { pallet1.Inventory[0], pallet2.Inventory[0] }, otherReceivesInventory);
		}

		public void TestAllocateExistingPalletLocations_NotPutawayStockOnAnotherReceive_MultipleReceives()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var oldReceive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var nonPalletisedStock1 = Helper.CreateWhsReceiveLine(oldReceive1, data.Part1, 10m);
			var pallet1 = Helper.CreateWhsReceiveLine(oldReceive1, data.Part1, 10m, null, "PLT-1");
			var pallet2 = Helper.CreateWhsReceiveLine(oldReceive1, data.Part1, 10m, null, "PLT-2");

			var oldReceive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var nonPalletisedStock2 = Helper.CreateWhsReceiveLine(oldReceive2, data.Part1, 10m);
			var pallet3 = Helper.CreateWhsReceiveLine(oldReceive2, data.Part1, 10m, null, "PLT-3");
			var pallet4 = Helper.CreateWhsReceiveLine(oldReceive2, data.Part1, 10m, null, "PLT-4");

			Factory.Save();

			var newReceive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var newPallet1_1 = Helper.CreateWhsReceiveLine(newReceive1, data.Part1, 10m, null, "PLT-1");
			var newPallet1_2 = Helper.CreateWhsReceiveLine(newReceive1, data.Part1, 10m, null, "PLT-1");

			var newReceive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R4");
			var newPallet2 = Helper.CreateWhsReceiveLine(newReceive2, data.Part2, 10m, null, "PLT-2");
			var newPallet3_1 = Helper.CreateWhsReceiveLine(newReceive2, data.Part1, 10m, null, "PLT-3");
			var newPallet3_2 = Helper.CreateWhsReceiveLine(newReceive2, data.Part1, 10m, null, "PLT-3");
			var newPallet4 = Helper.CreateWhsReceiveLine(newReceive2, data.Part2, 10m, null, "PLT-4");

			AssertEquals("Precondition.", ZGuid.Empty, newPallet1_1.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, newPallet1_2.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, newPallet2.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, newPallet3_1.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, newPallet3_2.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, newPallet4.WE_WL);

			var lines = newReceive1.Lines.Cast<WhsReceiveLine>();
			lines = lines.Concat(newReceive2.Lines.Cast<WhsReceiveLine>());

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive1, newReceive2 },
					lines,
					data.Whs1);

			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet1_1.WE_WL);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet1_2.WE_WL);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet2.WE_WL);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet3_1.WE_WL);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet3_2.WE_WL);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet4.WE_WL);

			AssertContainsExactElementsInAnyOrder("Should find stock on other receives needing to be putaway.",
				new[] { pallet1.Inventory[0], pallet2.Inventory[0], pallet3.Inventory[0], pallet4.Inventory[0] }, otherReceivesInventory);
		}

		public void TestAllocateExistingPalletLocations_NotPutawayStockOnAnotherReceive_CaseInsensitive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");

			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var nonPalletisedStock = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m);
			var pallet1 = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, null, "pLT-1");
			var pallet2 = Helper.CreateWhsReceiveLine(oldReceive, data.Part1, 10m, null, "Plt-2");
			Factory.Save();

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet1_1 = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m, null, "PLT-1");
			var newPallet1_2 = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m, null, "PLT-1");
			var newPallet2 = Helper.CreateWhsReceiveLine(newReceive, data.Part2, 10m, null, "PLT-2");

			AssertEquals("Precondition.", ZGuid.Empty, newPallet1_1.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, newPallet1_2.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, newPallet2.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet1_1.WE_WL);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet1_2.WE_WL);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet2.WE_WL);
			AssertContainsExactElementsInAnyOrder("Should find stock on other receives needing to be putaway.",
				new[] { pallet1.Inventory[0], pallet2.Inventory[0] }, otherReceivesInventory);
		}

		public void TestAllocateExistingPalletLocations_NotPutawayStockOnAnotherReceive_WithOneReceivePutaway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc = data.Whs1.FindLocation("A-1");

			var oldReceive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var oldReceive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(oldReceive1, data.Part1, 10m, loc, "PLT-1");
			Helper.CreateWhsReceiveLine(oldReceive2, data.Part1, 10m, null, "PLT-1");
			Factory.Save();

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m, null, "PLT-1");

			AssertEquals("Precondition.", ZGuid.Empty, newPallet.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should have set location.", loc.PK, newPallet.WE_WL);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}

		public void
			TestAllocateExistingPalletLocations_NotPutawayStockOnAnotherReceive_WithOneReceivePutaway_CaseInsensitive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var loc = data.Whs1.FindLocation("A-1");

			var oldReceive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var oldReceive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			Helper.CreateWhsReceiveLine(oldReceive1, data.Part1, 10m, loc, "pLt-1");
			Helper.CreateWhsReceiveLine(oldReceive2, data.Part1, 10m, null, "PLT-1");
			Factory.Save();

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m, null, "PLT-1");

			AssertEquals("Precondition.", ZGuid.Empty, newPallet.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should have set location.", loc.PK, newPallet.WE_WL);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}

		public void TestAllocateExistingPalletLocations_NotPutawayStockOnAnotherReceive_PutawayTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var dockDoorLocation = data.Whs1.FindLocation("A-3");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var oldPallet1 =
				Helper.CreateInventoryForDockDoorLocation(oldReceive, data.Part1, dockDoorLocation, "PLT-1", 10m);
			var oldPallet2 =
				Helper.CreateInventoryForDockDoorLocation(oldReceive, data.Part1, dockDoorLocation, "PLT-2", 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine1 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, null, "PLT-1", 10m);
			var transferLine2 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, null, "PLT-2", 10m);
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			Factory.Save();

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet1 = Helper.CreateWhsReceiveLine(newReceive, data.Part1, 10m, null, "PLT-1");
			var newPallet2 = Helper.CreateWhsReceiveLine(newReceive, data.Part2, 10m, null, "PLT-2");

			AssertEquals("Precondition.", ZGuid.Empty, newPallet1.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, newPallet2.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet1.WE_WL);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet2.WE_WL);
			AssertContainsExactElementsInAnyOrder("Should find stock on other receives needing to be putaway.",
				new[] { transferLine1.Inventory[0], transferLine2.Inventory[0] }, otherReceivesInventory);
		}

		public void TestAllocateExistingPalletLocations_NotPutawayStockOnAnotherReceive_PutawayTransferLine_Received()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var dockDoorLocation = data.Whs1.FindLocation("A-3");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var oldReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var oldPallet1 =
				Helper.CreateInventoryForDockDoorLocation(oldReceive, data.Part1, dockDoorLocation, "PLT-1", 10m);
			var oldPallet2 =
				Helper.CreateInventoryForDockDoorLocation(oldReceive, data.Part1, dockDoorLocation, "PLT-2", 10m);
			Factory.Save();

			AssertEquals("Precondition: Received.", InventoryStatus.Codes.Received, oldPallet1.WI_InventoryStatus);
			AssertEquals("Precondition: Received.", InventoryStatus.Codes.Received, oldPallet2.WI_InventoryStatus);

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet1 = Helper.CreateWhsReceiveInventoryLine(newReceive, data.Part1, 10m, null, "PLT-1");
			var newPallet2 = Helper.CreateWhsReceiveInventoryLine(newReceive, data.Part2, 10m, null, "PLT-2");

			AssertEquals("Precondition.", ZGuid.Empty, newPallet1.WI_WL);
			AssertEquals("Precondition.", ZGuid.Empty, newPallet2.WI_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet1.WI_WL);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, newPallet2.WI_WL);
			AssertContainsExactElementsInAnyOrder("Should find stock on other receives needing to be putaway.",
				new[] { oldPallet1, oldPallet2 }, otherReceivesInventory);
		}

		public void
			TestAllocateExistingPalletLocations_NotPutawayStockOnAnotherReceive_DoesNotReturnOwnPutawayTransferLine()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var loc1 = data.Whs1.FindLocation("A-1");
			var loc2 = data.Whs1.FindLocation("A-2");
			var dockDoorLocation = data.Whs1.FindLocation("A-3");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var newReceive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var newPallet1 =
				Helper.CreateInventoryForDockDoorLocation(newReceive, data.Part1, dockDoorLocation, "PLT-1", 10m);
			var newPallet2 =
				Helper.CreateInventoryForDockDoorLocation(newReceive, data.Part1, dockDoorLocation, "PLT-2", 10m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine1 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, null, "PLT-1", 10m);
			var transferLine2 =
				Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockDoorLocation, null, "PLT-2", 10m);
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			transfer.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("Precondition.", ZGuid.Empty, transferLine1.WE_WL);
			AssertEquals("Precondition.", ZGuid.Empty, transferLine2.WE_WL);

			var otherReceivesInventory =
				new PutawayExistingPalletManager().AllocateExistingPalletLocations(
					Factory,
					new[] { newReceive },
					newReceive.Lines.Cast<WhsReceiveLine>(),
					data.Whs1);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, transferLine1.WE_WL);
			AssertEquals("Should *not* have set location.", ZGuid.Empty, transferLine2.WE_WL);
			AssertEquals("Should *not* have changed dock door location.", dockDoorLocation.PK, newPallet1.WI_WL);
			AssertEquals("Should *not* have changed dock door location.", dockDoorLocation.PK, newPallet2.WI_WL);
			AssertEquals("Should find no stock on other receives needing to be putaway.", 0,
				otherReceivesInventory.Count());
		}
	}
}
