using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class CreatePutawayTransferHelperSharedTest : WhsTestCaseWithFactory
	{
		#region TestMultipleWarehouses

		public void TestMultipleWarehouses()
		{
			var whs1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "A", 2, 1);
			var client = Helper.CreateClient();
			var product = Helper.CreateProduct(client, "P1");
			var staff = Helper.CreateGlbStaff("S1", "S1");

			// create finalised receive with pallet "12345"
			Helper.CreateWhsReceiveWithInventory(client, whs1, "R1", product, 10m, whs1.DefaultLocation, "12345");
			// create not finalised receive in another warehouse with same palletID
			var receive2 = Helper.CreateWhsReceiveWithInventory(client, whs2, "R2", product, 7m, null, "12345", false, false);
			Helper.Factory.Save();

			CreateTransfer(whs2, ["12345"], staff.GS_Code);
			var transferLines = Helper.Factory.Load<WhsTransferLine>(new ZQuery(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Transfer));
			AssertEquals("Should create only one putaway transferLine", 1, transferLines.Length);
			AssertEquals("Should be for receive2", receive2.Inventory.Single().PK.ToGuid(), transferLines[0].PickLines[0].WZ_WE_InventoryLine);
		}

		#endregion

		#region CreateTransfer

		public void TestValidatePalletIDOnPutaway_CreatePutawayTransfers()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var nonDockDoorLocation1 = data.Whs1.FindLocation("A-1");
			var nonDockDoorLocation2 = data.Whs1.FindLocation("A-2");
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "12345", 20m);
			var inventory3 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "12345", 30m);
			var inventory4 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "", 40m);
			var inventory5 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "1234567", 50m);
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory3.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory4.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory5.WI_InventoryStatus);

			CreateTransfer(data.Whs1, ["12345"], staff1.GS_Code);
			Factory.Save();
			AssertInventoryStatusAndEventCount(inventory1, InventoryStatus.Codes.Received);
			AssertInventoryStatusAndEventCount(inventory2, InventoryStatus.Codes.Received);
			AssertInventoryStatusAndEventCount(inventory3, InventoryStatus.Codes.Received);
			AssertInventoryStatusAndEventCount(inventory4, InventoryStatus.Codes.Received);
			AssertInventoryStatusAndEventCount(inventory5, InventoryStatus.Codes.Received);

			var transfer = (WhsTransfer)inventory1.AllPickLines.First().DocketLine.Docket;
			AssertPutawayTransfer(transfer, data.Org1, data.Whs1, 3);

			var transferLineForPart1 = (WhsTransferLine)transfer.Lines.Single(l => l.WE_OP == data.Part1.PK);
			AssertPutawayTransferLine(transferLineForPart1, data.Part1, dockDoorLocation, null, "12345", 10m, staff1, ZDateTimeOffset.Now);

			var transferLineForPart2 = (WhsTransferLine)transfer.Lines.Single(l => l.WE_OP == data.Part2.PK && l.WE_TransactionQuantity == 20m);
			AssertPutawayTransferLine(transferLineForPart2, data.Part2, dockDoorLocation, null, "12345", 20m, staff1, ZDateTimeOffset.Now);

			var transferLineForPart3 = (WhsTransferLine)transfer.Lines.Single(l => l.WE_OP == data.Part2.PK && l.WE_TransactionQuantity == 30m);
			AssertPutawayTransferLine(transferLineForPart3, data.Part2, dockDoorLocation, null, "12345", 30m, staff1, ZDateTimeOffset.Now);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var transferInFactory2 = factory2.Load<WhsTransfer>(transfer.PK);
			AssertNotNull("Should have saved transfer to the database.", transferInFactory2);
		}

		public void TestPutawayHasBeenCompletedOnPallet()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var staff2 = Helper.CreateGlbStaff("S2", "S2");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation, "12345", 10m);
			Helper.Factory.Save();

			CreateTransfer(data.Whs1, ["12345"], staff1.GS_Code);
			inventory1.PutawayTransferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Putaway;

			var message = CreateTransfer(data.Whs1, ["12345"], staff2.GS_Code);
			AssertEquals("Putaway has been already completed for the Pallet ID 12345.", message);
		}

		public void TestValidatePalletIDOnPutaway_PutawayTransferExists()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "12345");
			inventory1.WI_ArrivalDate = ZDateTimeOffset.Now.AddDays(-2);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "12345");
			inventory2.WI_ArrivalDate = ZDateTimeOffset.Now.AddDays(-2);
			Factory.Save();

			CreateTransfer(data.Whs1, ["12345"], staff.GS_Code);
			var pickTime = ZDateTimeOffset.Now;
			var transferLineForPart1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory1);
			var transferLineForPart2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory2);
			transferLineForPart1.PickedTime = pickTime;
			transferLineForPart2.PickedTime = pickTime;

			AssertEquals("Precondition: inventory1 (receiveLine) status is RECEIVED.", true, inventory1.IsReceivedIntoDockDoor);
			AssertEquals("Precondition: inventory2 (receiveLine) status is RECEIVED.", true, inventory2.IsReceivedIntoDockDoor);
			AssertNotNull("Precondition: inventory1 has putaway transfer.", transferLineForPart1);
			AssertNotNull("Precondition: inventory2 has putaway transfer.", transferLineForPart2);
			AssertEquals("Precondition: transferLineForPart1 is picked.", true, transferLineForPart1.IsPicked);
			AssertEquals("Precondition: transferLineForPart2 is picked.", true, transferLineForPart2.IsPicked);

			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			AssertEquals("Precondition: there is 1 transfer.", 1, Factory.Load<WhsTransfer>(query).Length);

			CreateTransfer(data.Whs1, ["12345"], staff.GS_Code);
			AssertEquals("No new transfer is created.", 1, Helper.Factory.Load<WhsTransfer>(query).Length);
			AssertPutawayTransferLine(transferLineForPart1, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, null, "12345", 10m, staff, pickTime);
			AssertEquals("Receive line status is PFU.", DocketLineStatus.Codes.PickedForUnload, inventory1.InDocketLine.WE_DocketLineStatus);
			AssertEquals("Qty to move for transferLineForPart1 is correct.", 10m, transferLineForPart1.QtyToMoveIncludingMatchingLines);

			AssertEquals("transferLineForPart2 is picked.", true, transferLineForPart2.IsPicked);
			AssertPutawayTransferLine(transferLineForPart2, data.Part2, data.Whs1.DefaultOutboundDockDoorLocation, null, "12345", 20m, staff, pickTime);
			AssertEquals("Receive line status is PFU.", DocketLineStatus.Codes.PickedForUnload, inventory2.InDocketLine.WE_DocketLineStatus);
			AssertEquals("Qty to move for transferLineForPart2 is correct.", 20m, transferLineForPart2.QtyToMoveIncludingMatchingLines);
		}

		public void TestValidatePalletIDOnPutaway_PutawayTransferExistsAndAlreadyPicked()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var nonDockDoorLocation = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "12345");
			inventory1.WI_ArrivalDate = ZDateTimeOffset.Now.AddDays(-2);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "12345");
			inventory2.WI_ArrivalDate = ZDateTimeOffset.Now.AddDays(-2);
			Helper.Factory.Save();

			var pickedTime = ZDateTimeOffset.TruncateMilliseconds(ZDateTimeOffset.Now).AddDays(-1);
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPart1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultOutboundDockDoorLocation, nonDockDoorLocation, "12345", 10m);
			transferLineForPart1.PickedTime = pickedTime;
			var transferLineForPart2 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part2, data.Whs1.DefaultOutboundDockDoorLocation, nonDockDoorLocation, "12345", 20m);
			transferLineForPart2.PickedTime = pickedTime;
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			AssertEquals("Precondition: inventory1 status is RECEIVED.", true, inventory1.IsReceivedIntoDockDoor);
			AssertEquals("Precondition: inventory2 status is RECEIVED.", true, inventory2.IsReceivedIntoDockDoor);
			AssertNotNull("Precondition: inventory1 has putaway transfer.", ((WhsReceiveLine)inventory1.InDocketLine).PutawayTransfer);
			AssertNotNull("Precondition: inventory2 has putaway transfer.", ((WhsReceiveLine)inventory2.InDocketLine).PutawayTransfer);
			AssertEquals("Precondition: transferLineForPart1 is picked.", true, transferLineForPart1.IsPicked);
			AssertEquals("Precondition: transferLineForPart1.PickedTime is pickedTime.", pickedTime, transferLineForPart1.PickedTime);
			AssertEquals("Precondition: transferLineForPart2 is picked.", true, transferLineForPart2.IsPicked);
			AssertEquals("Precondition: transferLineForPart2.PickedTime is pickedTime.", pickedTime, transferLineForPart2.PickedTime);

			CreateTransfer(data.Whs1, ["12345"], staff1.GS_Code);
			AssertEquals("transferLineForPart1 is picked.", true, transferLineForPart1.IsPicked);
			AssertEquals("transferLineForPart1.PickedTime is still the same.", pickedTime, transferLineForPart1.PickedTime);
			AssertEquals("transferLineForPart2 is picked.", true, transferLineForPart2.IsPicked);
			AssertEquals("transferLineForPart2.PickedTime is still the same.", pickedTime, transferLineForPart2.PickedTime);
		}

		public void TestValidatePalletIDOnPutaway_CreatePutawayTransfers_InventoryWithNoQuantity()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "12345", 20m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, null, "12345");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, dockDoorLocation, "12345");
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Pending, inventory3.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory4.WI_InventoryStatus);

			var message = CreateTransfer(data.Whs1, ["12345"], staff1.GS_Code);
			AssertEquals("No error in the response.", null, message);

			var transfer = (WhsTransfer)inventory1.AllPickLines.First().DocketLine.Docket;
			AssertPutawayTransfer(transfer, data.Org1, data.Whs1, 2);
			var transferLine1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory1);
			AssertPutawayTransferLine(transferLine1, data.Part1, dockDoorLocation, null, "12345", 10m, staff1, ZDateTimeOffset.Now);
			var transferLine2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory2);
			AssertPutawayTransferLine(transferLine2, data.Part1, dockDoorLocation, null, "12345", 20m, staff1, ZDateTimeOffset.Now);

			AssertEquals(true, inventory1.HasPutawayTransfer);
			AssertEquals(true, inventory2.HasPutawayTransfer);
			AssertEquals(false, inventory3.HasPutawayTransfer);
			AssertEquals(false, inventory4.HasPutawayTransfer);
		}

		public void TestValidatePalletIDOnPutaway_CreatePutawayTransfers_NoQuantityToPutaway()
		{
			var staff1 = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, null, "12345");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, dockDoorLocation, "12345");
			Helper.Factory.Save();

			AssertEquals("Precondition", InventoryStatus.Codes.Pending, inventory1.WI_InventoryStatus);
			AssertEquals("Precondition", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);

			var message = CreateTransfer(data.Whs1, ["12345"], staff1.GS_Code);
			AssertEquals("There are no items to putaway.", message);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var staff = Helper.CreateGlbStaff("S1", "S1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory10UPart1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 10m);
			var inventory5UPart1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 5m);
			var inventory27UPart2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part2, dockDoorLocation, "PalletID1", 27m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part2, 16m);

			orderLine1.ReserveStockIfAbleTo(inventory10UPart1);
			orderLine2.ReserveStockIfAbleTo(inventory5UPart1);
			orderLine3.ReserveStockIfAbleTo(inventory27UPart2);
			AssertEquals("Inventory should have 1 reserved PickLines.", 1, inventory10UPart1.ReservedPickLines.Count);
			AssertEquals("Inventory should have 1 reserved PickLines.", 1, inventory5UPart1.ReservedPickLines.Count);
			AssertEquals("Inventory should have 1 reserved PickLines.", 1, inventory27UPart2.ReservedPickLines.Count);
			Helper.Factory.Save();

			CreateTransfer(data.Whs1, ["PalletID1"], staff.GS_Code);
			AssertEquals("Receive line's inventory has no more reserved pick lines.", 0, inventory10UPart1.ReservedPickLines.Count);
			AssertEquals("Receive line's inventory has no more reserved pick lines.", 0, inventory5UPart1.ReservedPickLines.Count);
			AssertEquals("Receive line's inventory has no more reserved pick lines.", 0, inventory27UPart2.ReservedPickLines.Count);

			var putawayTransferLine10UPart1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory10UPart1);
			AssertEquals("Part1 Putaway transfer line inventory has 1 reserved pick line.", 1, putawayTransferLine10UPart1.ReservedPickLines.Count);
			AssertEquals("Part1 Reserved Quantity is correct.", 3m, putawayTransferLine10UPart1.ReservedQuantity);

			var putawayTransferLine5UPart1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory5UPart1);
			AssertEquals("Part1 Putaway transfer line inventory has 1 reserved pick line.", 1, putawayTransferLine5UPart1.ReservedPickLines.Count);
			AssertEquals("Part1 Reserved Quantity is correct.", 5m, putawayTransferLine5UPart1.ReservedQuantity);

			var putawayTransferLine27UPart2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory27UPart2);
			AssertEquals("Part2 Putaway transfer line inventory has 1 reserved pick line.", 1, putawayTransferLine27UPart2.ReservedPickLines.Count);
			AssertEquals("Part2 Reserved Quantity is correct.", 16m, putawayTransferLine27UPart2.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var staff = Helper.CreateGlbStaff("S1", "S1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			orderLine1.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLines.", 1, inventory.ReservedPickLines.Count);
			AssertEquals("Precondition", 10m, inventory.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			inventory.WI_InDocketLineUnits = 6m;
			Helper.Factory.Save();

			var message = CreateTransfer(data.Whs1, ["PalletID1"], staff.GS_Code);
			AssertEquals("No error in the response.", null, message);

			var putawayTransferLine = PutawayHelper.GetPutawayTransferLineFromInventory(inventory);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 1, putawayTransferLine.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 6m, putawayTransferLine.ReservedQuantity);

			AssertEquals("Receive line got split.", 2, receive.Lines.Count);
			AssertEquals("Original inventory still has no reserved pick lines.", 0, inventory.ReservedPickLines.Count);
			AssertEquals("Original inventory still has no reserved pick lines.", 0m, inventory.InDocketLine.ReservedQuantity);

			var newInventory = receive.Lines.Single(line => line.WE_TransactionQuantity == 0);
			AssertEquals(4m, newInventory.WE_ClientOrderedUnits);
			AssertEquals("New inventory has reserved pick lines.", 1, newInventory.ReservedPickLines.Count);
			AssertEquals("New inventory has reserved pick lines.", 4m, newInventory.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit_MultipleReserveLines()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var staff = Helper.CreateGlbStaff("S1", "S1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 7m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);

			orderLine1.ReserveStockIfAbleTo(inventory);
			orderLine2.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLines.", 2, inventory.ReservedPickLines.Count);
			AssertEquals("Precondition", 10m, inventory.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			inventory.WI_InDocketLineUnits = 4m;
			Helper.Factory.Save();

			var message = CreateTransfer(data.Whs1, ["PalletID1"], staff.GS_Code);
			AssertEquals("No error in the response.", null, message);

			var putawayTransferLine = PutawayHelper.GetPutawayTransferLineFromInventory(inventory);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 2, putawayTransferLine.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 4m, putawayTransferLine.ReservedQuantity);

			AssertEquals("Receive line got split.", 2, receive.Lines.Count);
			AssertEquals("Original inventory has no reserved pick lines.", 0, inventory.ReservedPickLines.Count);
			AssertEquals("Original inventory has no reserved pick lines.", 0m, inventory.InDocketLine.ReservedQuantity);

			var newInventory = receive.Lines.Single(line => line.WE_TransactionQuantity == 0);
			AssertEquals(6m, newInventory.WE_ClientOrderedUnits);
			AssertEquals("New inventory has reserved pick lines.", 1, newInventory.ReservedPickLines.Count);
			AssertEquals("New inventory has reserved pick lines.", 6m, newInventory.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit_MultipleReserveLinesMovedToNewInventory()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;
			var staff = Helper.CreateGlbStaff("S1", "S1");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 10m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 7m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 3m);

			orderLine1.ReserveStockIfAbleTo(inventory);
			orderLine2.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLines.", 2, inventory.ReservedPickLines.Count);
			AssertEquals("Precondition", 10m, inventory.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			inventory.WI_InDocketLineUnits = 2m;
			Helper.Factory.Save();

			var message = CreateTransfer(data.Whs1, ["PalletID1"], staff.GS_Code);
			AssertEquals("No error in the response.", null, message);

			var putawayTransferLine = PutawayHelper.GetPutawayTransferLineFromInventory(inventory);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 1, putawayTransferLine.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 2m, putawayTransferLine.ReservedQuantity);

			AssertEquals("Receive line got split.", 2, receive.Lines.Count);
			AssertEquals("Original inventory has no reserved pick lines.", 0, inventory.ReservedPickLines.Count);
			AssertEquals("Original inventory has no reserved pick lines.", 0m, inventory.InDocketLine.ReservedQuantity);

			var newInventory = receive.Lines.Single(line => line.WE_TransactionQuantity == 0);
			AssertEquals(8m, newInventory.WE_ClientOrderedUnits);
			AssertEquals("New inventory has reserved pick lines.", 2, newInventory.ReservedPickLines.Count);
			AssertEquals("New inventory has reserved pick lines.", 8m, newInventory.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit_MultipleReserveLinesFromMultipleInventories()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 20m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 8m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 15m);

			orderLine1.ReserveStockIfAbleTo(inventory1);
			orderLine2.ReserveStockIfAbleTo(inventory2);
			AssertEquals("Precondition: Inventory1 should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition", 8m, inventory1.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition: Inventory2 should have 1 reserved PickLine.", 1, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition", 15m, inventory2.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			inventory1.WI_InDocketLineUnits = 6m;
			inventory2.WI_InDocketLineUnits = 12m;
			Helper.Factory.Save();

			var message = CreateTransfer(data.Whs1, ["PalletID1"], staff.GS_Code);
			AssertEquals("No error in the response.", null, message);

			var putawayTransferLine1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory1);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 1, putawayTransferLine1.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 6m, putawayTransferLine1.ReservedQuantity);

			var putawayTransferLine2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory2);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 1, putawayTransferLine2.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 12m, putawayTransferLine2.ReservedQuantity);

			AssertEquals("Receive lines got split.", 4, receive.Lines.Count);
			AssertEquals("Inventory1 has been split.", 6m, inventory1.WI_ExpectedReceiptQuantity);
			AssertEquals("Inventory1 has no reserved pick lines.", 0, inventory2.ReservedPickLines.Count);
			AssertEquals("Inventory1 has no reserved pick lines.", 0m, inventory2.InDocketLine.ReservedQuantity);

			var newInventory1 = receive.Lines.Single(line => line.WE_ClientOrderedUnits == 4m);
			AssertEquals("New inventory has reserved pick lines.", 1, newInventory1.ReservedPickLines.Count);
			AssertEquals("New inventory has reserved pick lines.", 2m, newInventory1.ReservedQuantity);

			AssertEquals("Inventory2 has been split.", 12m, inventory2.WI_ExpectedReceiptQuantity);
			AssertEquals("Inventory2 has no reserved pick lines.", 0, inventory2.ReservedPickLines.Count);
			AssertEquals("Inventory2 has no reserved pick lines.", 0m, inventory2.InDocketLine.ReservedQuantity);

			var newInventory2 = receive.Lines.Single(line => line.WE_ClientOrderedUnits == 8m);
			AssertEquals("New inventory has reserved pick lines.", 1, newInventory2.ReservedPickLines.Count);
			AssertEquals("New inventory has reserved pick lines.", 3m, newInventory2.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit_MultipleReserveLinesOnInventory()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultInboundDockDoorLocation;

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "INW1", ZDateTimeOffset.Empty);
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 10m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PalletID1", 40m);
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);
			var orderLine3 = Helper.CreateWhsOrderLine(order, data.Part1, 20m);

			orderLine1.ReserveStockIfAbleTo(inventory1);
			orderLine2.ReserveStockIfAbleTo(inventory2, 20m);
			orderLine3.ReserveStockIfAbleTo(inventory2, 20m);
			AssertEquals("Precondition: Inventory1 should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition", 10m, inventory1.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition: Inventory2 should have 2 reserved PickLine.", 2, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition", 40m, inventory2.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			var message = CreateTransfer(data.Whs1, ["PalletID1"], staff.GS_Code);
			AssertEquals("No error in the response.", null, message);

			var putawayTransferLine1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory1);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 1, putawayTransferLine1.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 10m, putawayTransferLine1.ReservedQuantity);

			var putawayTransferLine2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory2);
			AssertEquals("Putaway transfer line inventory has reserved pick lines.", 2, putawayTransferLine2.ReservedPickLines.Count);
			AssertEquals("Reserved Quantity is correct.", 40m, putawayTransferLine2.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit_SameOrderLine()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 70m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 70m);
			orderLine.ReserveStockIfAbleTo(inventory1, 40m);
			orderLine.ReserveStockIfAbleTo(inventory2, 30m);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition", 40m, inventory1.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 30m, inventory2.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			var message = CreateTransfer(data.Whs1, ["PalletID1"], staff.GS_Code);
			AssertEquals("No error in the response.", null, message);
			Helper.Factory.Save();

			var putawayTransferLine1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory1);
			AssertEquals("Reserve Lines should exist.", 1, putawayTransferLine1.ReservedPickLines.Count);
			AssertEquals("Should reserve 40 units.", 40m, putawayTransferLine1.ReservedQuantity);

			var putawayTransferLine2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory2);
			AssertEquals("Reserve Lines should exist.", 1, putawayTransferLine2.ReservedPickLines.Count);
			AssertEquals("Should reserve 30 units.", 30m, putawayTransferLine2.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesStayOnSimilarInventory()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 70m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 90m);
			orderLine.ReserveStockIfAbleTo(inventory1, 60m);
			orderLine.ReserveStockIfAbleTo(inventory2, 30m);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition", 60m, inventory1.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 30m, inventory2.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			var message = CreateTransfer(data.Whs1, ["PalletID1"], staff.GS_Code);
			AssertEquals("No error in the response.", null, message);
			Helper.Factory.Save();

			var allTransferLines = new List<WhsTransferLine>();
			var putawayTransferLine1 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory1);
			allTransferLines.Add(putawayTransferLine1);
			allTransferLines.AddRange(putawayTransferLine1.MatchingLines.Cast<WhsTransferLine>());
			var line1 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("Reserve Lines should exist.", 1, line1.ReservedPickLines.Count);
			AssertEquals("Should reserve 60 units.", 60m, line1.ReservedQuantity);

			var putawayTransferLine2 = PutawayHelper.GetPutawayTransferLineFromInventory(inventory2);
			allTransferLines.Add(putawayTransferLine2);
			allTransferLines.AddRange(putawayTransferLine2.MatchingLines.Cast<WhsTransferLine>());
			var line2 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			AssertEquals("Reserve Lines should exist.", 1, line2.ReservedPickLines.Count);
			AssertEquals("Should reserve 30 units.", 30m, line2.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_MultipleSplitPallets()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 70m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 30m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID2");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 25m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 60m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID2");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 90m);
			orderLine.ReserveStockIfAbleTo(inventory1, 15m);
			orderLine.ReserveStockIfAbleTo(inventory2, 30m);
			orderLine.ReserveStockIfAbleTo(inventory3, 5m);
			orderLine.ReserveStockIfAbleTo(inventory4, 40m);

			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory3.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory4.ReservedPickLines.Count);

			AssertEquals("Precondition", 15m, inventory1.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 30m, inventory2.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 5m, inventory3.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 40m, inventory4.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			var message1 = CreateTransfer(data.Whs1, ["PalletID1"], staff.GS_Code);
			AssertEquals("No error in the response.", null, message1);

			var message2 = CreateTransfer(data.Whs1, ["PalletID2"], staff.GS_Code);
			AssertEquals("No error in the response.", null, message2);

			var allTransferLines = new List<WhsTransferLine>();
			foreach (var inventory in new[] { inventory1, inventory2, inventory3, inventory4 })
			{
				var putawayTransferLine = PutawayHelper.GetPutawayTransferLineFromInventory(inventory);
				allTransferLines.Add(putawayTransferLine);
				allTransferLines.AddRange(putawayTransferLine.MatchingLines.Cast<WhsTransferLine>());
			}

			var lines1 = allTransferLines.Where(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals("Only 1 distinct transferLine should exist.", 1, lines1.Distinct().Count());
			AssertEquals("Reserve Lines should exist.", 1, lines1.First().ReservedPickLines.Count);
			AssertEquals("Should reserve 15 units.", 15m, lines1.First().ReservedQuantity);

			var line2 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			AssertEquals("Reserve Lines should exist.", 1, line2.ReservedPickLines.Count);
			AssertEquals("Should reserve 30 units.", 30m, line2.ReservedQuantity);

			var line3 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory3.WI_WE_InDocketLine);
			AssertEquals("Reserve Lines should exist.", 1, line3.ReservedPickLines.Count);
			AssertEquals("Should reserve 5 units.", 5m, line3.ReservedQuantity);

			var line4 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory4.WI_WE_InDocketLine);
			AssertEquals("Reserve Lines should exist.", 1, line4.ReservedPickLines.Count);
			AssertEquals("Should reserve 40 units.", 40m, line4.ReservedQuantity);
		}

		public void TestValidatePalletIDOnPutaway_ReserveLinesAreMovedToPutawayTransfer_ReserveLinesAreSplit_SameOrderLine_Mix()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-2");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK; // dock door location

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 70m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m, data.Whs1.DefaultOutboundDockDoorLocation, "PalletID1");
			Helper.Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 70m);
			orderLine.ReserveStockIfAbleTo(inventory1, 40m);
			orderLine.ReserveStockIfAbleTo(inventory3, 30m);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory have no reserved PickLine.", 0, inventory2.ReservedPickLines.Count);
			AssertEquals("Precondition: Inventory should have 1 reserved PickLine.", 1, inventory3.ReservedPickLines.Count);
			AssertEquals("Precondition", 40m, inventory1.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 0m, inventory2.InDocketLine.ReservedQuantity);
			AssertEquals("Precondition", 30m, inventory3.InDocketLine.ReservedQuantity);
			Helper.Factory.Save();

			var message = CreateTransfer(data.Whs1, ["PalletID1"], staff.GS_Code);
			AssertEquals("No error in the response.", null, message);
			Helper.Factory.Save();

			var allTransferLines = new List<WhsTransferLine>();
			allTransferLines.Add(PutawayHelper.GetPutawayTransferLineFromInventory(inventory1));
			allTransferLines.Add(PutawayHelper.GetPutawayTransferLineFromInventory(inventory2));
			allTransferLines.Add(PutawayHelper.GetPutawayTransferLineFromInventory(inventory3));
			var line1 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory1.WI_WE_InDocketLine);
			AssertEquals(1, line1.ReservedPickLines.Count);
			AssertEquals(40m, line1.ReservedQuantity);

			var line2 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory2.WI_WE_InDocketLine);
			AssertEquals(0, line2.ReservedPickLines.Count);
			AssertEquals(0m, line2.ReservedQuantity);

			var line3 = allTransferLines.Single(l => l.PickLines.Count == 1 && l.PickLines[0].WZ_WE_InventoryLine == inventory3.WI_WE_InDocketLine);
			AssertEquals(1, line3.ReservedPickLines.Count);
			AssertEquals(30m, line3.ReservedQuantity);
		}

		#endregion

		#region TestValidatePalletIDOnPutaway_OnePutawayTransfer

		public void TestValidatePalletIDOnPutaway_OnePutawayTransfer()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-2");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine1.HasPutawayTransfer);
			var message1 = CreateTransfer(data.Whs1, ["PLT-1"], staff.GS_Code);
			AssertNull("No errors from webservice call.", message1);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine1.HasPutawayTransfer);

			AssertEquals("Precondition", false, receiveLine2.HasPutawayTransfer);
			var message2 = CreateTransfer(data.Whs1, ["PLT-2"], staff.GS_Code);
			Factory.Save();
			AssertNull("No errors from webservice call.", message1);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine2.HasPutawayTransfer);

			AssertEquals("Only 1 putaway transfer was created.", receiveLine1.PutawayTransfer.PK, receiveLine2.PutawayTransfer.PK);
			var transferCountInDatabase = Helper.Factory.GetDatabaseCount(typeof(WhsDocket), new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertEquals("No extra transfers are created.", 1, transferCountInDatabase);
		}

		public void TestValidatePalletIDOnPutaway_OnePutawayTransfer_ExistingFinalisedTransfer()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine1.HasPutawayTransfer);
			var message1 = CreateTransfer(data.Whs1, ["PLT-1"], staff.GS_Code);
			AssertNull("No errors from webservice call.", message1);
			AssertEquals("Receive line has putaway transfer.", true, receiveLine1.HasPutawayTransfer);

			var putawayTransfer1 = receiveLine1.PutawayTransfer;
			var transferLine = putawayTransfer1.Lines[0];
			transferLine.WE_WL = data.Whs1.FindLocation("A-2").PK;

			putawayTransfer1.FinaliseDocketWithoutUserConfirmation();
			Helper.Factory.Save();
			AssertEquals("Putaway transfer is finalised.", true, putawayTransfer1.IsFinalised);
			AssertEquals("Receive is not finalised.", false, receive.IsFinalised);

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-2");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receiveLine2.HasPutawayTransfer);
			CreateTransfer(data.Whs1, ["PLT-2"], staff.GS_Code);
			Factory.Save();
			AssertEquals("Receive line has putaway transfer.", true, receiveLine2.HasPutawayTransfer);

			AssertNotEquals("Receive lines does not share the same putaway transfer.", receiveLine1.PutawayTransfer.PK, receiveLine2.PutawayTransfer.PK);

			var transferCountInDatabase = Helper.Factory.GetDatabaseCount(typeof(WhsDocket), new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertEquals("Number of transfers created is correct.", 2, transferCountInDatabase);
		}

		public void TestValidatePalletIDOnPutaway_OnePutawayTransfer_InventoryOnMultipleReceives()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive1Line = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receive2Line = Helper.CreateWhsReceiveLine(receive2, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receive1Line.HasPutawayTransfer);
			AssertEquals("Precondition", false, receive2Line.HasPutawayTransfer);
			var message = CreateTransfer(data.Whs1, ["PLT-1"], staff.GS_Code);
			Factory.Save();
			AssertNull("No errors from webservice call.", message);
			AssertEquals("Receive line has putaway transfer.", true, receive1Line.HasPutawayTransfer);
			AssertEquals("Receive line has putaway transfer.", true, receive2Line.HasPutawayTransfer);

			AssertEquals("Only 1 putaway transfer was created.", receive1Line.PutawayTransfer.PK, receive2Line.PutawayTransfer.PK);

			var transferCountInDatabase = Helper.Factory.GetDatabaseCount(typeof(WhsDocket), new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertEquals("No extra transfers are created.", 1, transferCountInDatabase);
		}

		public void TestValidatePalletIDOnPutaway_OnePutawayTransfer_BothReceivesWithPutawayTransfer()
		{
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receive1Line1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-1");
			var receive1Line2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 30m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-SAME");

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receive2Line1 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 20m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-2");
			var receive2Line2 = Helper.CreateWhsReceiveLine(receive2, data.Part1, 40m, data.Whs1.DefaultOutboundDockDoorLocation, "PLT-SAME");
			Helper.Factory.Save();

			AssertEquals("Precondition", false, receive1Line1.HasPutawayTransfer);
			var message1 = CreateTransfer(data.Whs1, ["PLT-1"], staff.GS_Code);
			AssertNull("No errors from webservice call.", message1);
			AssertEquals("Receive line has putaway transfer.", true, receive1Line1.HasPutawayTransfer);

			AssertEquals("Precondition", false, receive2Line1.HasPutawayTransfer);
			var message2 = CreateTransfer(data.Whs1, ["PLT-2"], staff.GS_Code);
			Factory.Save();
			AssertNull("No errors from webservice call.", message2);
			AssertEquals("Receive line has putaway transfer.", true, receive2Line1.HasPutawayTransfer);
			AssertNotEquals("Receive lines does not share the same putaway transfer.", receive1Line1.PutawayTransfer.PK, receive2Line1.PutawayTransfer.PK);

			var message3 = CreateTransfer(data.Whs1, ["PLT-SAME"], staff.GS_Code);
			AssertNull("No errors from webservice call.", message3);
			AssertEquals("Receive line has putaway transfer.", true, receive1Line2.HasPutawayTransfer);
			AssertEquals("Receive line has putaway transfer.", true, receive2Line2.HasPutawayTransfer);

			AssertEquals("Receive lines share the same putaway transfer.", receive1Line2.PutawayTransfer.PK, receive2Line2.PutawayTransfer.PK);
			AssertEquals("Putaway transfer with earlier docket id was selected.", receive1Line1.PutawayTransfer.PK, receive1Line2.PutawayTransfer.PK);

			var transferCountInDatabase = Helper.Factory.GetDatabaseCount(typeof(WhsDocket), new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer));
			AssertEquals("No extra transfers are created.", 2, transferCountInDatabase);
		}
		
		protected void TestValidatePalletIDOnPutaway_OnePutawayTransfer_DBHits(int expectedDockets, int expectedInvs, Func<Dictionary<string, int>> expectedDBHits)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S1", "S1");
			var samePalletId = "PLT-SAME";

			for (var i = 0; i < 10; i++)
			{
				var palletCount = i.ToString();

				var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, palletCount);
				Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, palletCount);
				var line2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, samePalletId);
				line2.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
				Factory.Save();

				CreateTransfer(data.Whs1, [palletCount], staff.GS_Code);
			}

			var receive11 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R11");
			Helper.CreateWhsReceiveLine(receive11, data.Part1, 10m, data.Whs1.DefaultOutboundDockDoorLocation, samePalletId);
			Factory.Save();

			AssertEquals("Precondition: Correct number of dockets.", expectedDockets, Factory.GetDatabaseCount(typeof(WhsDocket)));
			AssertEquals("Precondition: Correct number of inventories.", expectedInvs, Factory.GetDatabaseCount(typeof(WhsInventoryView)));

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			using (AssertDbHitsWithUsefulQueryInformation(expectedDBHits(), otherFactory))
			using (RowFactory.SetCachedTables())
			{
				var inventories = otherFactory.Load<WhsInventoryView>(PutawayHelper.FindInventoryQuery([samePalletId], data.Whs1.PK));
				var helper = new CreatePutawayTransferHelper();
				var errorMessage = helper.CreateTransfer(otherFactory, data.Whs1, inventories, false, staff.GS_Code);
				AssertNull("No errors from webservice call.", errorMessage);
			}
		}

		#endregion

		#region Implementation

		protected abstract bool IsMultiPalletPutaway { get; }

		protected string CreateTransfer(WhsWarehouse warehouse, string[] palletIds, string staffCode)
		{
			var inventories = Factory.Load<WhsInventoryView>(PutawayHelper.FindInventoryQuery(palletIds, warehouse.PK));
			var helper = new CreatePutawayTransferHelper();
			return helper.CreateTransfer(Factory, warehouse, inventories, IsMultiPalletPutaway, staffCode);
		}

		protected void AssertPutawayTransferLine(
			WhsTransferLine line,
			OrgSupplierPart product,
			WhsLocation sourceLocation,
			WhsLocation destinationLocation,
			string palletID,
			decimal quantity,
			GlbStaff pickedBy,
			ZDateTimeOffset pickedTime)
		{
			CombineAssertions(() =>
			{
				AssertEquals(product.PK, line.WE_OP);
				AssertEquals(sourceLocation.PK, line.WE_WL_TransferFrom);
				AssertEquals(destinationLocation?.PK ?? ZGuid.Empty, line.WE_WL);
				AssertEquals(palletID, line.WE_TransferFromPalletId);
				AssertEquals(palletID, line.WE_PalletID);
				AssertEquals(InventoryStatus.Codes.PuttingAway, line.WE_OriginalInventoryStatus);
				AssertEquals(InventoryStatus.Codes.PuttingAway, line.WE_CurrentInventoryStatus);
				AssertEquals(quantity, line.QtyCommittedIncludingMatchingLines);
				AssertZDatesWithin5Minutes("PickedTime", pickedTime, line.PickedTime);

				if (IsMultiPalletPutaway)
				{
					AssertEquals(pickedBy, line.PutawayBy);
				}
			});
		}

		protected void AssertPutawayTransfer(WhsTransfer putawayTransfer, OrgHeader client, WhsWarehouse warehouse, int expectednumberOfLines)
		{
			AssertEquals(true, putawayTransfer.WD_IsPutawayTransfer);
			AssertEquals(client.PK, putawayTransfer.WD_OH_Client);
			AssertEquals(warehouse.PK, putawayTransfer.WD_WW_Whs);
			AssertEquals(expectednumberOfLines, putawayTransfer.Lines.Count);
		}

		protected static void AssertInventoryStatusAndEventCount(WhsInventoryView inventory1, string inventoryStatus)
		{
			AssertEquals(inventoryStatus, inventory1.WI_InventoryStatus);
			AssertEquals(0, inventory1.InDocketLine.Logs.Find(l => l.SL_SE_NKEvent == Events.WarehouseReceiptConfirmedPutaway.Code).Count());
		}

		#endregion
	}
}
