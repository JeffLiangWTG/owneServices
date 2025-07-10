using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProductionRules.Integration;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class CrossDockManagerTest : WhsTestCaseWithFactory
	{
		public void TestAllocateCrossDockedLine_Receive_Null()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
				new CrossDockManager().AllocateCrossDockedLines(null, Enumerable.Empty<WhsReceiveLine>()));
		}

		public void TestAllocateCrossDockedLine_Inventories_Null()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				allocateLocations: false, finalise: false);
			AssertExceptionThrown<ArgumentNullException>(() =>
				new CrossDockManager().AllocateCrossDockedLines(receive.Factory, null));
		}

		public void TestAllocateCrossDockedLine_NotCrossDocked()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				allocateLocations: false, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			AssertEquals("Precondition: Empty location.", ZGuid.Empty, inventory.WI_WL);

			var crossDockManager = new CrossDockManager();
			crossDockManager.AllocateCrossDockedLines(receive.Factory, receive.Lines.Cast<WhsReceiveLine>());
			AssertNoRowWarnings(inventory);
			AssertNoErrors(inventory.WI_InDocketLineUnitsInfo);
			AssertEquals("Should be 1 inventory line.", 1, receive.Inventory.Count);
			AssertEquals("Should be 1 receive line.", 1, receive.Lines.Count);
			AssertEquals("Should *not* have set the location.", ZGuid.Empty, inventory.WI_WL);
			AssertEquals("Should be no Reserved Pick Lines.", 0, inventory.ReservedPickLines.Count);
		}

		public void TestAllocateCrossDockedLine()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var dockLocationA = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKA", 1, 1).Locations.Single();
			dockLocationA.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				allocateLocations: false, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			order.WD_WL_CrossDock = dockLocationA.PK;
			AssertEquals("Precondition: Order is reserved.", 10m,
				orderLine.ReserveStockIfAbleTo(inventory).ReservedQuantity);
			Factory.Save();

			AssertEquals("Precondition: Empty location.", ZGuid.Empty, inventory.WI_WL);

			var crossDockManager = new CrossDockManager();
			crossDockManager.AllocateCrossDockedLines(receive.Factory, receive.Lines.Cast<WhsReceiveLine>());
			AssertNoRowWarnings(inventory);
			AssertNoErrors(inventory.WI_InDocketLineUnitsInfo);
			AssertEquals("Should be 1 inventory line.", 1, receive.Inventory.Count);
			AssertEquals("Should be 1 receive line.", 1, receive.Lines.Count);
			AssertEquals("Should have set the location.", dockLocationA.PK, inventory.WI_WL);

			AssertEquals("Reserved Pick Line should remain.", 1, inventory.ReservedPickLines.Count);
			AssertEquals("Reserved Pick Line should remain.", 10m,
				inventory.ReservedPickLines.Single().ReservedQuantity);
			AssertEquals("Reserved Pick Line should remain.", orderLine.PK,
				inventory.ReservedPickLines.Single().DocketLine.PK);

			AssertNoExceptionThrown("Ensure Save is allowed.", () => Factory.Save());
		}

		public void TestAllocateCrossDockedLine_WithPalletAllocatedToMultipleCrossDockLocations()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "INW1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "PL1");

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 6m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 4m);
			var crossDockLocation1 = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK1").Locations.Single();
			var crossDockLocation2 = Helper.CreateRowAndGenerateLocations(data.Whs1, "XDOCK2").Locations.Single();
			order1.WD_WL_CrossDock = crossDockLocation1.PK;
			order2.WD_WL_CrossDock = crossDockLocation2.PK;
			var reservedLine1 = order1.Lines[0].ReserveStockIfAbleTo(inventory);
			var reservedLine2 = order2.Lines[0].ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is reserved.", 6m, reservedLine1.ReservedQuantity);
			AssertEquals("Precondition: Stock is reserved.", 4m, reservedLine2.ReservedQuantity);
			Helper.Factory.Save();

			AssertNull("Precondition: location should be empty.", inventory.Location);

			var crossDockManager = new CrossDockManager();

			AssertExceptionThrown(typeof(FactLoadingException), "Cannot Cross Dock a Single Pallet to multiple Cross Dock Locations.",
				() => crossDockManager.AllocateCrossDockedLines(receive.Factory, receive.Lines.Cast<WhsReceiveLine>()));
		}

		public void TestAllocateCrossDockedLine_WithPutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockLocationA = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKA", 1, 1).Locations.Single();
			dockLocationA.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, null, "P1", allocateLocations: false, finalise: false);
			var receiveLine = receive.Lines[0];
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			order.WD_WL_CrossDock = dockLocationA.PK;
			var reserveLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Order is reserved.", 10m, reserveLine.ReservedQuantity);
			Factory.Save();

			AssertEquals("Precondition: Empty location.", ZGuid.Empty, inventory.WI_WL);

			receiveLine.WE_WL = data.Whs1.WW_DefaultInboundDockDoor;
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "P1", 10m);
			transferLine.PickedTime = ZDateTimeOffset.Now;
			reserveLine.WZ_WE_InventoryLine = transferLine.PK;

			var crossDockManager = new CrossDockManager();
			crossDockManager.AllocateCrossDockedLines(receive.Factory, receive.Lines.Cast<WhsReceiveLine>());
			AssertNoRowWarnings(inventory);
			AssertNoErrors(inventory.WI_InDocketLineUnitsInfo);
			AssertEquals("Should be 1 inventory line.", 1, receive.Inventory.Count);
			AssertEquals("Should be 1 receive line.", 1, receive.Lines.Count);
			AssertEquals("Should not have changed Dock Door Location.", data.Whs1.WW_DefaultInboundDockDoor, inventory.WI_WL);
			AssertEquals("Should have set the Cross Dock Location.", dockLocationA.PK, transferLine.WE_WL);

			AssertEquals("Reserved Pick Line should remain on Putaway Tranfer Line.", 1, transferLine.ReservedPickLines.Count);
			AssertEquals("Reserved Pick Line should remain on Putaway Tranfer Line.", 10m, transferLine.ReservedPickLines.Single().ReservedQuantity);
			AssertEquals("Reserved Pick Line should remain on Putaway Tranfer Line.", orderLine.PK, transferLine.ReservedPickLines.Single().DocketLine.PK);

			AssertNoExceptionThrown("Ensure Save is allowed.", () => Factory.Save());
		}

		public void TestAllocateCrossDockedLine_WithPutawayTransfer_MultiplePallets()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockLocationA = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKA", 1, 1).Locations.Single();
			var dockLocationB = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKB", 1, 1).Locations.Single();
			dockLocationA.WLV_WLT_LocationType = dockDoorLocationType.PK;
			dockLocationB.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, null, "P1", allocateLocations: false, finalise: false);
			var inventory1 = receive.Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, null, "P2");
			var receiveLine1 = receive.Lines[0];
			var receiveLine2 = inventory2.InDocketLine;
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 10m);
			var orderLine1 = order1.Lines[0];
			var orderLine2 = order2.Lines[0];
			order1.WD_WL_CrossDock = dockLocationA.PK;
			order2.WD_WL_CrossDock = dockLocationB.PK;
			var reserveLine1 = orderLine1.ReserveStockIfAbleTo(inventory1);
			var reserveLine2 = orderLine2.ReserveStockIfAbleTo(inventory2);
			AssertEquals("Precondition: Order is reserved.", 10m, reserveLine1.ReservedQuantity);
			AssertEquals("Precondition: Order is reserved.", 10m, reserveLine2.ReservedQuantity);
			Factory.Save();

			AssertEquals("Precondition: Empty location.", ZGuid.Empty, inventory1.WI_WL);
			AssertEquals("Precondition: Empty location.", ZGuid.Empty, inventory2.WI_WL);

			receiveLine1.WE_WL = data.Whs1.WW_DefaultInboundDockDoor;
			receiveLine2.WE_WL = data.Whs1.WW_DefaultInboundDockDoor;

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "P1", 10m);
			var transferLine2 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, data.Whs1.DefaultInboundDockDoorLocation, null, "P2", 10m);
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			reserveLine1.WZ_WE_InventoryLine = transferLine1.PK;
			reserveLine2.WZ_WE_InventoryLine = transferLine2.PK;

			var crossDockManager = new CrossDockManager();
			crossDockManager.AllocateCrossDockedLines(receive.Factory, receive.Lines.Cast<WhsReceiveLine>());
			AssertNoRowWarnings(inventory1);
			AssertNoRowWarnings(inventory2);
			AssertNoErrors(inventory1.WI_InDocketLineUnitsInfo);
			AssertNoErrors(inventory2.WI_InDocketLineUnitsInfo);
			AssertEquals("Should be 1 inventory line.", 2, receive.Inventory.Count);
			AssertEquals("Should be 1 receive line.", 2, receive.Lines.Count);
			AssertEquals("Should not have changed Dock Door Location.", data.Whs1.WW_DefaultInboundDockDoor, inventory1.WI_WL);
			AssertEquals("Should not have changed Dock Door Location.", data.Whs1.WW_DefaultInboundDockDoor, inventory2.WI_WL);
			AssertEquals("Should have set the Cross Dock Location.", dockLocationA.PK, transferLine1.WE_WL);
			AssertEquals("Should have set the Cross Dock Location.", dockLocationB.PK, transferLine2.WE_WL);

			AssertEquals("Reserved Pick Line should remain on Putaway Tranfer Line.", 1, transferLine1.ReservedPickLines.Count);
			AssertEquals("Reserved Pick Line should remain on Putaway Tranfer Line.", 10m, transferLine1.ReservedPickLines.Single().ReservedQuantity);
			AssertEquals("Reserved Pick Line should remain on Putaway Tranfer Line.", orderLine1.PK, transferLine1.ReservedPickLines.Single().DocketLine.PK);

			AssertEquals("Reserved Pick Line should remain on Putaway Tranfer Line.", 1, transferLine2.ReservedPickLines.Count);
			AssertEquals("Reserved Pick Line should remain on Putaway Tranfer Line.", 10m, transferLine2.ReservedPickLines.Single().ReservedQuantity);
			AssertEquals("Reserved Pick Line should remain on Putaway Tranfer Line.", orderLine2.PK, transferLine2.ReservedPickLines.Single().DocketLine.PK);

			AssertNoExceptionThrown("Ensure Save is allowed.", () => Factory.Save());
		}

		public void TestAllocateCrossDockedLine_MultipleLines()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var dockLocationA = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKA", 1, 1).Locations.Single();
			dockLocationA.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Inventory[0];
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var orderLine = order.Lines[0];
			order.WD_WL_CrossDock = dockLocationA.PK;
			AssertEquals("Precondition: Order is reserved.", 10m,
				orderLine.ReserveStockIfAbleTo(inventory1).ReservedQuantity);
			AssertEquals("Precondition: Order is reserved.", 10m,
				orderLine.ReserveStockIfAbleTo(inventory2).ReservedQuantity);

			AssertEquals("Precondition: Empty location.", ZGuid.Empty, inventory1.WI_WL);

			var crossDockManager = new CrossDockManager();
			crossDockManager.AllocateCrossDockedLines(receive.Factory, receive.Lines.Cast<WhsReceiveLine>());
			AssertNoRowWarnings(inventory1);
			AssertNoErrors(inventory1.WI_InDocketLineUnitsInfo);
			AssertEquals("Should be 2 inventory lines.", 2, receive.Inventory.Count);
			AssertEquals("Should be 2 receive lines.", 2, receive.Lines.Count);

			AssertEquals("Should have set the location.", dockLocationA.PK, inventory1.WI_WL);
			AssertEquals("Reserved Pick Line should remain.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Reserved Pick Line should remain.", 10m,
				inventory1.ReservedPickLines.Single().ReservedQuantity);
			AssertEquals("Reserved Pick Line should remain.", orderLine.PK,
				inventory1.ReservedPickLines.Single().DocketLine.PK);

			AssertEquals("Should have set the location.", dockLocationA.PK, inventory2.WI_WL);
			AssertEquals("Reserved Pick Line should remain.", 1, inventory2.ReservedPickLines.Count);
			AssertEquals("Reserved Pick Line should remain.", 10m,
				inventory2.ReservedPickLines.Single().ReservedQuantity);
			AssertEquals("Reserved Pick Line should remain.", orderLine.PK,
				inventory2.ReservedPickLines.Single().DocketLine.PK);

			AssertNoExceptionThrown("Ensure Save is allowed.", () => Factory.Save());
		}

		public void TestAllocateCrossDockedLine_NotAllLinesPassedIn()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var dockLocationA = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKA", 1, 1).Locations.Single();
			dockLocationA.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				allocateLocations: false, finalise: false);
			var inventory1 = receive.Lines[0];
			var inventory2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 20m);
			var orderLine = order.Lines[0];
			order.WD_WL_CrossDock = dockLocationA.PK;
			AssertEquals("Precondition: Order is reserved.", 10m,
				orderLine.ReserveStockIfAbleTo(inventory1.Inventory[0]).ReservedQuantity);
			AssertEquals("Precondition: Order is reserved.", 10m,
				orderLine.ReserveStockIfAbleTo(inventory2.Inventory[0]).ReservedQuantity);

			AssertEquals("Precondition: Empty location.", ZGuid.Empty, inventory1.WE_WL);
			AssertEquals("Precondition: Empty location.", ZGuid.Empty, inventory2.WE_WL);

			var crossDockManager = new CrossDockManager();
			crossDockManager.AllocateCrossDockedLines(receive.Factory, new[] { inventory1 });
			AssertNoRowWarnings(inventory1);
			AssertNoErrors(inventory1.Inventory[0].WI_InDocketLineUnitsInfo);
			AssertEquals("Should be 2 inventory lines.", 2, receive.Inventory.Count);
			AssertEquals("Should be 2 receive lines.", 2, receive.Lines.Count);

			AssertEquals("Should have set the location.", dockLocationA.PK, inventory1.WE_WL);
			AssertEquals("Reserved Pick Line should remain.", 1, inventory1.ReservedPickLines.Count);
			AssertEquals("Reserved Pick Line should remain.", 10m,
				inventory1.ReservedPickLines.Single().ReservedQuantity);
			AssertEquals("Reserved Pick Line should remain.", orderLine.PK,
				inventory1.ReservedPickLines.Single().DocketLine.PK);

			AssertEquals("Should *not* have set the location.", ZGuid.Empty, inventory2.WE_WL);

			AssertNoExceptionThrown("Ensure Save is allowed.", () => Factory.Save());
		}

		public void TestAllocateCrossDockedLine_SplitsIfPartiallyReserved()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var dockLocationA = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKA", 1, 1).Locations.Single();
			dockLocationA.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				allocateLocations: false, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			var orderLine = order.Lines[0];
			order.WD_WL_CrossDock = dockLocationA.PK;
			AssertEquals("Precondition: Order is reserved.", 4m,
				orderLine.ReserveStockIfAbleTo(inventory).ReservedQuantity);
			Factory.Save();

			AssertEquals("Precondition: Empty location.", ZGuid.Empty, inventory.WI_WL);

			var crossDockManager = new CrossDockManager();
			crossDockManager.AllocateCrossDockedLines(receive.Factory, receive.Lines.Cast<WhsReceiveLine>());
			AssertEquals("Should be 2 inventory lines.", 2, receive.Inventory.Count);
			AssertEquals("Should be 2 receive lines.", 2, receive.Lines.Count);

			var reservedLine = receive.Lines.Cast<WhsReceiveLine>()
				.SingleOrDefault(i => i.ReservedPickLines.Count == 1);
			var notReservedLine = receive.Lines.Cast<WhsReceiveLine>()
				.SingleOrDefault(i => i.ReservedPickLines.Count == 0);
			AssertNoErrors(reservedLine.Inventory[0].WI_InDocketLineUnitsInfo);
			AssertNoErrors(notReservedLine.Inventory[0].WI_InDocketLineUnitsInfo);
			AssertNoRowWarnings(reservedLine);
			AssertNoRowWarnings(notReservedLine);
			AssertNotNull("Should have split the inventory.", reservedLine);
			AssertNotNull("Should have split the inventory.", notReservedLine);
			AssertEquals("Should have split the inventory.", 4m, reservedLine.WE_StockOnHand);
			AssertEquals("Should have split the inventory.", 6m, notReservedLine.WE_StockOnHand);

			AssertEquals("Should have set the location.", dockLocationA.PK, reservedLine.WE_WL);
			AssertEquals("Should *not* have set the location.", ZGuid.Empty, notReservedLine.WE_WL);

			AssertEquals("Reserved Pick Line should remain.", 1, reservedLine.ReservedPickLines.Count);
			AssertEquals("Reserved Pick Line should remain.", 4m,
				reservedLine.ReservedPickLines.Single().ReservedQuantity);
			AssertEquals("Reserved Pick Line should remain.", orderLine.PK,
				reservedLine.ReservedPickLines.Single().DocketLine.PK);

			AssertNoExceptionThrown("Ensure Save is allowed.", () => Factory.Save());
		}

		public void TestAllocateCrossDockedLine_CrossDockLocationNotAssigned()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m,
				allocateLocations: false, finalise: false);
			var inventory = receive.Inventory[0];
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var orderLine = order.Lines[0];
			AssertEquals("Precondition: Order is reserved.", 10m,
				orderLine.ReserveStockIfAbleTo(inventory).ReservedQuantity);
			Factory.Save();

			AssertEquals("Precondition: Empty location.", ZGuid.Empty, inventory.WI_WL);

			var crossDockManager = new CrossDockManager();
			crossDockManager.AllocateCrossDockedLines(receive.Factory, receive.Lines.Cast<WhsReceiveLine>());
			AssertHasRowWarning(inventory.InDocketLine,
				"Order Cross Dock Location has not been assigned for allocated inventory lines");
			AssertEquals("Should be 1 inventory line.", 1, receive.Inventory.Count);
			AssertEquals("Should be 1 receive line.", 1, receive.Lines.Count);
			AssertEquals("Should *not* have set the location.", ZGuid.Empty, inventory.WI_WL);

			AssertEquals("Reserved Pick Line should remain.", 1, inventory.ReservedPickLines.Count);
			AssertEquals("Reserved Pick Line should remain.", 10m,
				inventory.ReservedPickLines.Single().ReservedQuantity);
			AssertEquals("Reserved Pick Line should remain.", orderLine.PK,
				inventory.ReservedPickLines.Single().DocketLine.PK);
		}

		public void TestAllocateCrossDockedLine_MultiOrders_SplitsIfUsingDifferentLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var dockLocationA = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKA", 1, 1).Locations.Single();
			var dockLocationB = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKB", 1, 1).Locations.Single();
			var dockLocationC = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKC", 1, 1).Locations.Single();
			dockLocationA.WLV_WLT_LocationType = dockDoorLocationType.PK;
			dockLocationB.WLV_WLT_LocationType = dockDoorLocationType.PK;
			dockLocationC.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var orderLine3 = Helper.CreateWhsOrderLine(order3, data.Part1, 10m);
			order1.WD_WL_CrossDock = dockLocationA.PK;
			order2.WD_WL_CrossDock = dockLocationB.PK;
			order3.WD_WL_CrossDock = dockLocationC.PK;
			AssertEquals("Precondition: Order 1 is reserved.", 10m,
				orderLine1.ReserveStockIfAbleTo(inventory).ReservedQuantity);
			AssertEquals("Precondition: Order 2 is reserved.", 10m,
				orderLine2.ReserveStockIfAbleTo(inventory).ReservedQuantity);
			AssertEquals("Precondition: Order 3 is reserved.", 10m,
				orderLine3.ReserveStockIfAbleTo(inventory).ReservedQuantity);

			Factory.Save();

			var crossDockManager = new CrossDockManager();
			crossDockManager.AllocateCrossDockedLines(receive.Factory, receive.Lines.Cast<WhsReceiveLine>());
			AssertNoErrors(inventory.WI_InDocketLineUnitsInfo);
			AssertNoRowWarnings(inventory);
			AssertEquals("Inventory should be successfully split.", 3, receive.Inventory.Count);
			AssertEquals("Receive Lines should be added for Reserved Pick Lines.", 3, receive.Lines.Count);
			AssertEquals("Reserved Pick Lines should be re-assigned correctly across split Inventory.", 3,
				receive.Lines.Cast<WhsReceiveLine>().Count(i =>
					i.ReservedPickLines.Count == 1 && i.ReservedPickLines.Single().ReservedQuantity == 10m));
			AssertContainsExactElementsInAnyOrder(new[] { orderLine1, orderLine2, orderLine3 },
				receive.Lines.Cast<WhsReceiveLine>().Select(i => i.ReservedPickLines.Single().DocketLine));
			AssertContainsExactElementsInAnyOrder(new[] { dockLocationA, dockLocationB, dockLocationC },
				receive.Lines.Cast<WhsReceiveLine>().Select(i => i.Location));
			AssertNoExceptionThrown("Ensure Save is allowed.", () => Factory.Save());
		}

		public void TestAllocateCrossDockedLine_MultiOrders_SameLocation_DoesNotSplit()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var dockLocationA = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKA", 1, 1).Locations.Single();
			dockLocationA.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 30m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m);
			var orderLine3 = Helper.CreateWhsOrderLine(order3, data.Part1, 10m);
			order1.WD_WL_CrossDock = dockLocationA.PK;
			order2.WD_WL_CrossDock = dockLocationA.PK;
			order3.WD_WL_CrossDock = dockLocationA.PK;
			AssertEquals("Precondition: Order 1 is reserved.", 10m,
				orderLine1.ReserveStockIfAbleTo(inventory).ReservedQuantity);
			AssertEquals("Precondition: Order 2 is reserved.", 10m,
				orderLine2.ReserveStockIfAbleTo(inventory).ReservedQuantity);
			AssertEquals("Precondition: Order 3 is reserved.", 10m,
				orderLine3.ReserveStockIfAbleTo(inventory).ReservedQuantity);

			Factory.Save();

			var crossDockManager = new CrossDockManager();
			crossDockManager.AllocateCrossDockedLines(receive.Factory, receive.Lines.Cast<WhsReceiveLine>());
			AssertNoErrors(inventory.WI_InDocketLineUnitsInfo);
			AssertNoRowWarnings(inventory);

			AssertEquals("Should be 1 inventory line.", 1, receive.Inventory.Count);
			AssertEquals("Should be 1 receive line.", 1, receive.Lines.Count);
			AssertEquals("Should have set the location.", dockLocationA.PK, inventory.WI_WL);

			AssertEquals("Reserved Pick Line should remain.", 3, inventory.ReservedPickLines.Count);
			AssertEquals("Reserved Pick Line should remain.", 30m, inventory.ReservedPickLines.Sum(rl => rl.WZ_Units));
			AssertContainsExactElementsInAnyOrder(new[] { orderLine1, orderLine2, orderLine3 },
				inventory.ReservedPickLines.Select(rl => rl.DocketLine));
			AssertNoExceptionThrown("Ensure Save is allowed.", () => Factory.Save());
		}

		[StressTest]
		public void TestAllocateCrossDockedLine_DBHits()
		{
			const int NumberOfInventoriesToCreate = 25; // Inventory Lines to create
			var data = new TestDataSimpleEnvironment(Factory, 5, 5, saveFactory_doNotUseForNewTests: false);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);

			var inventories = new List<WhsInventoryView>(NumberOfInventoriesToCreate);
			for (int i = 0; i < NumberOfInventoriesToCreate; i++)
			{
				inventories.Add(Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m));
			}

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test",
				false, 0, LocationClasses.Codes.DDL);
			var dockLocationA = Helper.CreateRowAndGenerateLocations(data.Whs1, "DOCKA", 1, 1).Locations.Single();
			dockLocationA.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			for (int i = 0; i < NumberOfInventoriesToCreate; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i, data.Part1, 1m);
				order.WD_WL_CrossDock = dockLocationA.PK;

				var orderLine = order.Lines[0];
				AssertEquals("Precondition: Order is reserved.", 1m,
					orderLine.ReserveStockIfAbleTo(inventories[i]).ReservedQuantity);
			}

			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 2 },
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ WhsRowSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 },
				{ WhsDocketLineSchema.Constants.TableName, 2 },
				{ WhsInventoryViewSchema.Constants.TableName, 1 },
				{ WhsLocationTypeSchema.Constants.TableName, 1 },
				{ WhsLocationViewSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsProductParamsByWhsAndClientSchema.Constants.TableName, 1 }
			};

			var otherFactory = new BusinessObjectFactory();
			var receiveInOtherFactory = otherFactory.Load<WhsReceive>(receive.PK);

			var crossDockManager = new CrossDockManager();
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, otherFactory))
			{
				// simulate fetch hints from PutawayManager which always adds this WhsInventoryView Fetch Hint
				var pokeForFetchHints = receiveInOtherFactory.Inventory;

				crossDockManager.AllocateCrossDockedLines(otherFactory,
					receiveInOtherFactory.Lines.Cast<WhsReceiveLine>());
			}

			AssertEquals("Ensure operation worked.", true, receiveInOtherFactory.Lines.All(l => !l.WE_WL.IsEmpty));
		}
	}
}
