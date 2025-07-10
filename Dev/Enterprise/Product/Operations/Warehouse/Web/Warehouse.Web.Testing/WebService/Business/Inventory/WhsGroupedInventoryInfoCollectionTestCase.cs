using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	public class WhsGroupedInventoryInfoCollectionTestCase : DataObjectInfoCollectionTestCase<WhsGroupedInventoryInfo>
	{
		#region TestConstructor

		public void TestConstructor_InventoryIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>("Should blow up", @"inventory is required.
Parameter name: inventory", () => new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level1, null, Helper.CreateWarehouse("WHS"), 500));
		}

		public void TestConstructor_FirstInventoryCountNotGreaterThanZero()
		{
			AssertExceptionThrown<ArgumentException>("Should blow up", @"firstInventoriesCount must greater than 0.
Parameter name: firstInventoriesCount", () => new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level1, Array.Empty<WhsInventoryView>(), Helper.CreateWarehouse("WHS1"), 0));
			AssertExceptionThrown<ArgumentException>("Should blow up", @"firstInventoriesCount must greater than 0.
Parameter name: firstInventoriesCount", () => new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level1, Array.Empty<WhsInventoryView>(), Helper.CreateWarehouse("WHS2"), -1));
		}

		public void TestConstructor_GroupByProductAndClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Helper.CreateProductClientRelationShip(client2, data.Part2);

			var receive_Client1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive_Client1, data.Part1, 10m, data.Whs1.DefaultLocation, "P1");
			Helper.CreateWhsReceiveInventoryLine(receive_Client1, data.Part1, 10m, data.Whs1.DefaultLocation, "");
			Helper.CreateWhsReceiveInventoryLine(receive_Client1, data.Part2, 10m, data.Whs1.DefaultLocation, "P3");
			receive_Client1.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive_Client1);

			var receive_Client2 = Helper.CreateWhsReceive(client2, data.Whs1, "R2");
			Helper.CreateWhsReceiveInventoryLine(receive_Client2, data.Part1, 10m, data.Whs1.DefaultLocation, "P4");
			Helper.CreateWhsReceiveInventoryLine(receive_Client2, data.Part1, 10m, data.Whs1.DefaultLocation, "P5");
			Helper.CreateWhsReceiveInventoryLine(receive_Client2, data.Part2, 10m, data.Whs1.DefaultLocation, "P6");
			receive_Client2.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive_Client2);

			Factory.Save();

			var order_Client1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Helper.CreateWhsOrderLine(order_Client1, data.Part1, 5m);
			Helper.CreateWhsOrderLine(order_Client1, data.Part2, 5m);

			var order_Client2 = Helper.CreateWhsOrder(client2, data.Whs1);
			Helper.CreateWhsOrderLine(order_Client2, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order_Client2, data.Part2, 9m);

			var pick = Helper.CreatePickNew(order_Client1, order_Client2);
			pick.AutoAllocateItemsWithMock();

			Factory.Save();

			var inventory = receive_Client1.Inventory.Cast<WhsInventoryView>().Concat(receive_Client2.Inventory.Cast<WhsInventoryView>());
			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level1, inventory.ToArray(), data.Whs1, 100);

			AssertEquals(4, groupedInventoryInfoCollection.Count);
			AssertEquals(4, groupedInventoryInfoCollection.TotalCount);

			var groupedInventory_Product1AndClient1 = groupedInventoryInfoCollection.Single(g => g.ProductPK == data.Part1.PK && g.ClientPK == data.Org1.PK);
			AssertInventoryInfoLine(groupedInventory_Product1AndClient1, 20m, 15m, 1, expectedQtyUQ: "UNT");

			var groupedInventory_Product2AndClient1 = groupedInventoryInfoCollection.Single(g => g.ProductPK == data.Part2.PK && g.ClientPK == data.Org1.PK);
			AssertInventoryInfoLine(groupedInventory_Product2AndClient1, 10m, 5m, 1, expectedQtyUQ: "UNT");

			var groupedInventory_Product1AndClient2 = groupedInventoryInfoCollection.Single(g => g.ProductPK == data.Part1.PK && g.ClientPK == client2.PK);
			AssertInventoryInfoLine(groupedInventory_Product1AndClient2, 20m, 19m, 2, expectedQtyUQ: "UNT");

			var groupedInventory_Product2AndClient2 = groupedInventoryInfoCollection.Single(g => g.ProductPK == data.Part2.PK && g.ClientPK == client2.PK);
			AssertInventoryInfoLine(groupedInventory_Product2AndClient2, 10, 1m, 1, expectedQtyUQ: "UNT");
		}

		public void TestConstructor_GroupByProductAndClient_ReceivedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PLT1", 15m);
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PLT2", 10m);
			Factory.Save();

			AssertEquals($"Precondition: Inventory Status of inventory should be {InventoryStatus.Codes.Received}", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals($"Precondition: Inventory Status of inventory should be {InventoryStatus.Codes.Received}", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);

			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level1, new[] { inventory1, inventory2 }, data.Whs1, 100);
			AssertEquals(1, groupedInventoryInfoCollection.Count);
			AssertEquals(1, groupedInventoryInfoCollection.TotalCount);

			AssertInventoryInfoLine(groupedInventoryInfoCollection.Single(), 25m, 0m, 2, expectedQtyUQ: "UNT");
		}

		public void TestConstructor_GroupByProductAndClient_InTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "P1");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "P1");
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 20m, "A-1", "P1", "", "");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is In-Transit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			Factory.Save();

			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level1, new[] { inventory1, inventory2 }, data.Whs1, 100);
			AssertEquals(1, groupedInventoryInfoCollection.Count);
			AssertEquals(1, groupedInventoryInfoCollection.TotalCount);

			AssertInventoryInfoLine(groupedInventoryInfoCollection.Single(), 0m, 0m, 1, expectedQtyUQ: "UNT");
		}

		public void TestConstructor_GroupedByAttributes()
		{
			var tomorrow = ZDate.Today.AddDays(1);
			var tomorrrowAsDateTime = tomorrow.ToDateTime();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part1);
			Helper.CreateProductClientRelationShip(client2, data.Part2);

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false, useSerialNumber: false);

			Helper.SetClientAllAttributeType(client2, true);
			Helper.SetProductAllAttributeUse(client2, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(client2, data.Part2, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var receive_Client1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1_Client1 = Helper.CreateWhsReceiveInventoryLine(receive_Client1, data.Part1, 10m, data.Whs1.DefaultLocation, "P1", tomorrow, tomorrow, "A1", "A1", "A3", "");
			var receiveLine2_Client1 = Helper.CreateWhsReceiveInventoryLine(receive_Client1, data.Part1, 10m, data.Whs1.DefaultLocation, "P2", tomorrow, tomorrow, "A1", "A1", "A3", "");
			var receiveLine3_Client1 = Helper.CreateWhsReceiveInventoryLine(receive_Client1, data.Part1, 10m, data.Whs1.DefaultLocation, "", tomorrow, tomorrow, "A1", "A2", "A3", "");
			var receiveLine4_Client1 = Helper.CreateWhsReceiveInventoryLine(receive_Client1, data.Part2, 10m, data.Whs1.DefaultLocation, "P3", tomorrow, tomorrow, "A1", "A1", "A3", "");
			receive_Client1.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive_Client1);

			var receive_Client2 = Helper.CreateWhsReceive(client2, data.Whs1, "R2");
			var newDate = tomorrow.AddDays(2);
			Helper.CreateWhsReceiveInventoryLine(receive_Client2, data.Part1, 10m, data.Whs1.DefaultLocation, "P4", tomorrow, tomorrow, "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveInventoryLine(receive_Client2, data.Part1, 10m, data.Whs1.DefaultLocation, "P5", tomorrow, tomorrow, "A1", "A2", "A3", "");

			// Grouped by each Attribute
			Helper.CreateWhsReceiveInventoryLine(receive_Client2, data.Part2, 10m, data.Whs1.DefaultLocation, "P6", tomorrow, tomorrow, "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveInventoryLine(receive_Client2, data.Part2, 10m, data.Whs1.DefaultLocation, "P6", tomorrow, tomorrow, "A1", "B2", "A3", "");
			Helper.CreateWhsReceiveInventoryLine(receive_Client2, data.Part2, 10m, data.Whs1.DefaultLocation, "P6", tomorrow, tomorrow, "A1", "A2", "B3", "");
			Helper.CreateWhsReceiveInventoryLine(receive_Client2, data.Part2, 10m, data.Whs1.DefaultLocation, "P6", newDate, tomorrow, "A1", "A2", "A3", "");
			Helper.CreateWhsReceiveInventoryLine(receive_Client2, data.Part2, 10m, data.Whs1.DefaultLocation, "P6", tomorrow, newDate, "A1", "A2", "A3", "");
			receive_Client2.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive_Client2);

			Factory.Save();

			var order_Client1 = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1_Client1 = Helper.CreateWhsOrderLine(order_Client1, data.Part1, 5m);
			orderLine1_Client1.ReserveStockIfAbleTo(receiveLine1_Client1, 5m);
			var orderLine2_Client1 = Helper.CreateWhsOrderLine(order_Client1, data.Part2, 5m);
			orderLine2_Client1.ReserveStockIfAbleTo(receiveLine4_Client1, 5m);

			var order_Client2 = Helper.CreateWhsOrder(client2, data.Whs1);
			Helper.CreateWhsOrderLine(order_Client2, data.Part1, 1m);
			Helper.CreateWhsOrderLine(order_Client2, data.Part2, 9m);

			var pick = Helper.CreatePickNew(order_Client1, order_Client2);
			pick.AutoAllocateItemsWithMock();

			Factory.Save();

			var inventory = receive_Client1.Inventory.Cast<WhsInventoryView>().Concat(receive_Client2.Inventory.Cast<WhsInventoryView>());
			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level2, inventory.ToArray(), data.Whs1, 100);

			AssertEquals(9, groupedInventoryInfoCollection.Count);
			AssertEquals(9, groupedInventoryInfoCollection.TotalCount);

			var groupedInventory_Product1AndClient1AndAttributeA = groupedInventoryInfoCollection.Single(g => g.ProductPK == data.Part1.PK && g.ClientPK == data.Org1.PK && g.Attribute2 == "A1");
			AssertInventoryInfoLine(groupedInventory_Product1AndClient1AndAttributeA,
				20m, 15m, 2,
				"A1", "A1", "A3",
				tomorrrowAsDateTime, tomorrrowAsDateTime, expectedQtyUQ: "UNT");

			var groupedInventory_Product1AndClient1AndAttributeB = groupedInventoryInfoCollection.Single(g => g.ProductPK == data.Part1.PK && g.ClientPK == data.Org1.PK && g.Attribute2 == "A2");
			AssertInventoryInfoLine(groupedInventory_Product1AndClient1AndAttributeB,
				 10m, 10m, 0,
				 "A1", "A2", "A3",
				 tomorrrowAsDateTime, tomorrrowAsDateTime, expectedQtyUQ: "UNT");

			var groupedInventory_Product2AndClient1 = groupedInventoryInfoCollection.Single(g => g.ProductPK == data.Part2.PK && g.ClientPK == data.Org1.PK);
			AssertInventoryInfoLine(groupedInventory_Product2AndClient1,
				 10m, 5m, 1,
				 "A1", "A1", "A3",
				 tomorrrowAsDateTime, tomorrrowAsDateTime, expectedQtyUQ: "UNT");

			var groupedInventory_Product1AndClient2 = groupedInventoryInfoCollection.Single(g => g.ProductPK == data.Part1.PK && g.ClientPK == client2.PK);
			AssertInventoryInfoLine(groupedInventory_Product1AndClient2,
				 20m, 19m, 2,
				 "A1", "A2", "A3",
				 tomorrrowAsDateTime, tomorrrowAsDateTime, expectedQtyUQ: "UNT");

			var groupedInventory_Product2AndClient2 = groupedInventoryInfoCollection
				.Single(g => g.ProductPK == data.Part2.PK && g.ClientPK == client2.PK
				&& g.Attribute1 == "A1" && g.Attribute2 == "A2" && g.Attribute3 == "A3" && g.ExpiryDate == tomorrrowAsDateTime && g.PackingDate == tomorrrowAsDateTime);
			AssertInventoryInfoLine(groupedInventory_Product2AndClient2,
				 10m, 1m, 1,
				 "A1", "A2", "A3",
				 tomorrrowAsDateTime, tomorrrowAsDateTime, expectedQtyUQ: "UNT");

			var groupedInventory_Product2AndClient2AndAttribute2 = groupedInventoryInfoCollection
				.Single(g => g.ProductPK == data.Part2.PK && g.ClientPK == client2.PK && g.Attribute2 == "B2");
			AssertInventoryInfoLine(groupedInventory_Product2AndClient2AndAttribute2,
				 10m, 10m, 1,
				 "A1", "B2", "A3",
				 tomorrrowAsDateTime, tomorrrowAsDateTime, expectedQtyUQ: "UNT");

			var groupedInventory_Product2AndClient2AndAttribute3 = groupedInventoryInfoCollection
				.Single(g => g.ProductPK == data.Part2.PK && g.ClientPK == client2.PK && g.Attribute3 == "B3");
			AssertInventoryInfoLine(groupedInventory_Product2AndClient2AndAttribute3,
				 10m, 10m, 1,
				 "A1", "A2", "B3",
				 tomorrrowAsDateTime, tomorrrowAsDateTime, expectedQtyUQ: "UNT");

			var groupedInventory_Product2AndClient2AndExpiryDate = groupedInventoryInfoCollection.Single(g => g.ProductPK == data.Part2.PK && g.ClientPK == client2.PK && g.ExpiryDate == newDate.ToDateTime());
			AssertInventoryInfoLine(groupedInventory_Product2AndClient2AndExpiryDate,
				 10m, 10m, 1,
				 "A1", "A2", "A3",
				 newDate.ToDateTime(), tomorrrowAsDateTime, expectedQtyUQ: "UNT");

			var groupedInventory_Product2AndClient2AndPackingDate = groupedInventoryInfoCollection.Single(g => g.ProductPK == data.Part2.PK && g.ClientPK == client2.PK && g.PackingDate == newDate.ToDateTime());
			AssertInventoryInfoLine(groupedInventory_Product2AndClient2AndPackingDate,
				 10m, 10m, 1,
				 "A1", "A2", "A3",
				 tomorrrowAsDateTime, newDate.ToDateTime(), expectedQtyUQ: "UNT");
		}

		public void TestConstructor_GroupedByAttributes_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.BatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "P1", ZDate.Empty, ZDate.Empty, "A1", "", "", "SN1", "");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "P1", ZDate.Empty, ZDate.Empty, "A1", "", "", "SN2", "");

			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 2m);
			var pick = Helper.CreatePickNew(order);
			pick.AutoAllocateItemsWithMock();
			Factory.Save();

			var inventory = receive.Inventory.Cast<WhsInventoryView>();
			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level2, inventory.ToArray(), data.Whs1, 100);

			AssertEquals(2, groupedInventoryInfoCollection.Count);
			AssertEquals(2, groupedInventoryInfoCollection.TotalCount);

			var groupedInventory_SerialNumber1 = groupedInventoryInfoCollection.Single(g => g.SerialNumber == "SN1");
			AssertInventoryInfoLine(groupedInventory_SerialNumber1,
				1m, 0m, 1,
				"A1", "", "",
				default, default, expectedQtyUQ: "UNT", expectedSerialNumber: "SN1");

			var groupedInventory_SerialNumber2 = groupedInventoryInfoCollection.Single(g => g.SerialNumber == "SN2");
			AssertInventoryInfoLine(groupedInventory_SerialNumber2,
				1m, 0m, 1,
				"A1", "", "",
				default, default, expectedQtyUQ: "UNT", expectedSerialNumber: "SN2");
		}

		public void TestConstructor_GroupedByAttributes_ReceivedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var today = ZDate.Today;
			var todayAsDateTime = today.ToDateTime();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PLT1", 15m);
			inventory1.WI_PartAttrib1 = "A1";
			inventory1.WI_PartAttrib2 = "A2";
			inventory1.WI_PartAttrib3 = "A3";
			inventory1.WI_ExpiryDate = today;
			inventory1.WI_PackingDate = today;
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PLT1", 10m);
			inventory2.WI_PartAttrib1 = "B1";
			inventory2.WI_PartAttrib2 = "A2";
			inventory2.WI_PartAttrib3 = "A3";
			inventory2.WI_ExpiryDate = today;
			inventory2.WI_PackingDate = today;
			Factory.Save();

			AssertEquals($"Precondition: Inventory Status of inventory should be {InventoryStatus.Codes.Received}", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals($"Precondition: Inventory Status of inventory should be {InventoryStatus.Codes.Received}", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);

			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level2, new[] { inventory1, inventory2 }, data.Whs1, 100);

			AssertEquals(2, groupedInventoryInfoCollection.Count);
			AssertEquals(2, groupedInventoryInfoCollection.TotalCount);

			var groupedInventory_A1 = groupedInventoryInfoCollection.Single(g => g.Attribute1 == "A1");
			AssertInventoryInfoLine(groupedInventory_A1,
				 15m, 0m, 1,
				 "A1", "A2", "A3",
				 todayAsDateTime, todayAsDateTime, expectedQtyUQ: "UNT");

			var groupedInventory_B1 = groupedInventoryInfoCollection.Single(g => g.Attribute1 == "B1");
			AssertInventoryInfoLine(groupedInventory_B1,
				 10m, 0m, 1,
				 "B1", "A2", "A3",
				 todayAsDateTime, todayAsDateTime, expectedQtyUQ: "UNT");
		}

		public void TestConstructor_GroupedByAttributes_ReceivedInventory_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.BatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PLT1", 1m);
			inventory1.WI_PartAttrib1 = "A1";
			inventory1.WI_SerialNumber = "SN1";
			var inventory2 = Helper.CreateInventoryForDockDoorLocation(receive, data.Part1, dockDoorLocation, "PLT1", 1m);
			inventory2.WI_PartAttrib1 = "A1";
			inventory2.WI_SerialNumber = "SN2";
			Factory.Save();

			AssertEquals($"Precondition: Inventory Status of inventory should be {InventoryStatus.Codes.Received}", InventoryStatus.Codes.Received, inventory1.WI_InventoryStatus);
			AssertEquals($"Precondition: Inventory Status of inventory should be {InventoryStatus.Codes.Received}", InventoryStatus.Codes.Received, inventory2.WI_InventoryStatus);

			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level2, new[] { inventory1, inventory2 }, data.Whs1, 100);

			AssertEquals(2, groupedInventoryInfoCollection.Count);
			AssertEquals(2, groupedInventoryInfoCollection.TotalCount);

			var groupedInventory_SN1 = groupedInventoryInfoCollection.Single(g => g.SerialNumber == "SN1");
			AssertInventoryInfoLine(groupedInventory_SN1,
				 1m, 0m, 1,
				 "A1", "", "",
				 default, default, expectedQtyUQ: "UNT", expectedSerialNumber: "SN1");

			var groupedInventory_SN2 = groupedInventoryInfoCollection.Single(g => g.SerialNumber == "SN2");
			AssertInventoryInfoLine(groupedInventory_SN2,
				 1m, 0m, 1,
				 "A1", "", "",
				 default, default, expectedQtyUQ: "UNT", expectedSerialNumber: "SN2");
		}

		public void TestConstructor_GroupedByAttributes_InTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var location = data.Whs1.FindLocation("A-1");
			var today = ZDate.Today;
			var todayAsDateTime = today.ToDateTime();
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "P1", today, today, "A1", "A2", "A3", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 10m, "A-1", "P1", ZGuid.Empty, "", "", new ZDateTimeOffset(today), today, today, "A1", "A2", "A3");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is In-Transit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			Factory.Save();

			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level2, new[] { inventory }, data.Whs1, 100);
			AssertEquals(1, groupedInventoryInfoCollection.Count);
			AssertEquals(1, groupedInventoryInfoCollection.TotalCount);

			var groupedInventory = groupedInventoryInfoCollection.Single();
			AssertInventoryInfoLine(groupedInventory,
				 0m, 0m, 1,
				 "A1", "A2", "A3",
				 todayAsDateTime, todayAsDateTime, expectedQtyUQ: "UNT");
		}

		public void TestConstructor_GroupedByAttributes_InTransitInventory_SerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.BatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, location.PK, "P1", ZDate.Empty, ZDate.Empty, "A1", "", "", "SN1", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, location.PK, "P1", ZDate.Empty, ZDate.Empty, "A1", "", "", "SN2", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 1m, "A-1", "P1", ZGuid.Empty, "", "", ZDateTimeOffset.Today, ZDate.Empty, ZDate.Empty, "A1", "", "");
			transferLine1.WE_SerialNumber = "SN1";
			transferLine1.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is In-Transit", InventoryStatus.Codes.InTransit, transferLine1.WE_CurrentInventoryStatus);
			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 1m, "A-1", "P1", ZGuid.Empty, "", "", ZDateTimeOffset.Today, ZDate.Empty, ZDate.Empty, "A1", "", "");
			transferLine2.WE_SerialNumber = "SN2";
			transferLine2.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is In-Transit", InventoryStatus.Codes.InTransit, transferLine2.WE_CurrentInventoryStatus);

			Factory.Save();

			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level2, new[] { inventory1, inventory2 }, data.Whs1, 100);
			AssertEquals(2, groupedInventoryInfoCollection.Count);
			AssertEquals(2, groupedInventoryInfoCollection.TotalCount);

			var groupedInventory1 = groupedInventoryInfoCollection.Single(g => g.SerialNumber == "SN1");
			AssertInventoryInfoLine(groupedInventory1,
				 0m, 0m, 1,
				 "A1", "", "",
				 default, default, expectedQtyUQ: "UNT", expectedSerialNumber: "SN1");

			var groupedInventory2 = groupedInventoryInfoCollection.Single(g => g.SerialNumber == "SN2");
			AssertInventoryInfoLine(groupedInventory2,
				 0m, 0m, 1,
				 "A1", "", "",
				 default, default, expectedQtyUQ: "UNT", expectedSerialNumber: "SN2");
		}

		public void TestConstructor_DetailInventory()
		{
			var today = ZDate.Today;
			var todayAsDateTime = today.ToDateTime();
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "P1", today, today, "A1", "A1", "A3", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "P2", today, today, "A1", "A2", "A3", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var inventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level3, receive.Inventory.Cast<WhsInventoryView>().ToArray(), data.Whs1, 100);

			AssertEquals(2, inventoryInfoCollection.Count);
			AssertEquals(2, inventoryInfoCollection.TotalCount);

			var inventoryInfo1 = inventoryInfoCollection.Single(g => g.ProductPK == data.Part1.PK);
			AssertInventoryInfoLine(inventoryInfo1,
				0m, 0m, 0,
				"A1", "A1", "A3",
				todayAsDateTime, todayAsDateTime,
				data.Whs1.DefaultLocation.ToLocationString(), "P1",
				10m, "UNT");

			var inventoryInfo2 = inventoryInfoCollection.Single(g => g.ProductPK == data.Part2.PK);
			AssertInventoryInfoLine(inventoryInfo2,
				0m, 0m, 0,
				"A1", "A2", "A3",
				todayAsDateTime, todayAsDateTime,
				data.Whs1.DefaultLocation.ToLocationString(), "P2",
				10m, "UNT");
		}

		public void TestConstructor_DetailInventory_ReceivedInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var today = ZDate.Today;
			var todayAsDateTime = today.ToDateTime();
			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			var dockDoorLocation = data.Whs1.FindLocation("A-1");
			dockDoorLocation.WLV_WLT_LocationType = dockDoorLocationType.PK;
			var nonDockDoorLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 15m, dockDoorLocation, "PLT1");
			inventory.WI_PartAttrib1 = "A1";
			inventory.WI_PartAttrib2 = "A2";
			inventory.WI_PartAttrib3 = "A3";
			inventory.WI_ExpiryDate = today;
			inventory.WI_PackingDate = today;
			Factory.Save();

			AssertEquals($"Precondition: Inventory Status of inventory should be {InventoryStatus.Codes.Received}", InventoryStatus.Codes.Received, inventory.WI_InventoryStatus);

			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level3, new[] { inventory }, data.Whs1, 100);
			AssertEquals(1, groupedInventoryInfoCollection.Count);
			AssertEquals(1, groupedInventoryInfoCollection.TotalCount);

			AssertInventoryInfoLine(groupedInventoryInfoCollection.Single(),
				0m, 0m, 0,
				"A1", "A2", "A3",
				todayAsDateTime, todayAsDateTime,
				dockDoorLocation.ToLocationString(), "PLT1",
				15m, "UNT");

			inventory.WI_WL = nonDockDoorLocation.PK;
			var groupedInventoryInfoCollection2 = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level3, new[] { inventory }, data.Whs1, 100);
			AssertEquals(1, groupedInventoryInfoCollection2.Count);
			AssertEquals(1, groupedInventoryInfoCollection2.TotalCount);

			AssertInventoryInfoLine(groupedInventoryInfoCollection2.Single(),
				0m, 0m, 0,
				"A1", "A2", "A3",
				todayAsDateTime, todayAsDateTime,
				nonDockDoorLocation.ToLocationString(), "PLT1",
				15m, "UNT");
		}

		public void TestConstructor_DetailInventory_InTransitInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false, useSerialNumber: false);

			var location = data.Whs1.FindLocation("A-1");
			var today = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location, "P1", today, today, "A1", "A2", "A3", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1.PK, 10m, "A-1", "P1", ZGuid.Empty, "", "", new ZDateTimeOffset(today), today, today, "A1", "A2", "A3");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition: Transfer Line is In-Transit", InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);

			Factory.Save();

			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level3, new[] { inventory }, data.Whs1, 100);
			AssertEquals(1, groupedInventoryInfoCollection.Count);
			AssertEquals(1, groupedInventoryInfoCollection.TotalCount);

			AssertInventoryInfoLine(groupedInventoryInfoCollection.Single(),
				0m, 0m, 0,
				"A1", "A2", "A3",
				today.ToDateTime(), today.ToDateTime(),
				location.ToLocationString(), "P1",
				0m, "UNT");
		}

		void AssertInventoryInfoLine(WhsGroupedInventoryInfo groupedInventory,
			decimal expectedTotalUnitsOnHand, decimal expectedTotalUnitsOnAvailable, int expectedTotalPalletIDs,
			string expectedAttribute1 = "", string expectedAttribute2 = "", string expectedAttribute3 = "",
			DateTime expectedExpiryDate = new DateTime(), DateTime expectedPackingDate = new DateTime(),
			string expectedLocation = "", string expectedPalletID = "",
			decimal expectedQty = 0m, string expectedQtyUQ = "", string expectedSerialNumber = "")
		{
			AssertEquals("Attribute1", expectedAttribute1, groupedInventory.Attribute1);
			AssertEquals("Attribute2", expectedAttribute2, groupedInventory.Attribute2);
			AssertEquals("Attribute3", expectedAttribute3, groupedInventory.Attribute3);
			AssertEquals("SerialNumber", expectedSerialNumber, groupedInventory.SerialNumber);
			AssertEquals("ExpiryDate", expectedExpiryDate, groupedInventory.ExpiryDate);
			AssertEquals("PackingDate", expectedPackingDate, groupedInventory.PackingDate);
			AssertEquals("TotalUnitsOnHand", expectedTotalUnitsOnHand, groupedInventory.TotalUnitsOnHand);
			AssertEquals("TotalUnitsOnAvailable", expectedTotalUnitsOnAvailable, groupedInventory.TotalUnitsOnAvailable);
			AssertEquals("TotalPalletIDs", expectedTotalPalletIDs, groupedInventory.TotalPalletIDs);

			AssertEquals("Location", expectedLocation, groupedInventory.Location);
			AssertEquals("PalletID", expectedPalletID, groupedInventory.PalletID);
			AssertEquals("Qty", expectedQty, groupedInventory.Qty);
			AssertEquals("QtyUQ", expectedQtyUQ, groupedInventory.QtyUQ);
		}

		public void TestConstructor_UsesWarehouseCountryForFormatString()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, useSerialNumber: false);

			var today = ZDate.Today;
			var todayAsDateTime = today.ToDateTime();
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "P1", today, today, "A1", "A2", "A3", "");
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "P1", today, today, "A1", "A2", "A3", "");
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure receive is finalised", true, receive.IsFinalised);

			Factory.Save();

			var groupedInventoryInfoCollection1 = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level1, new[] { inventory1 }, data.Whs1, 100);

			AssertEquals("ddMMyy", groupedInventoryInfoCollection1.InventoryLineInfoCollection.ProductPartAttributesInfos[0].ExpiryDateFormatString); // Date format for testing
			AssertEquals("ddMMyy", groupedInventoryInfoCollection1.InventoryLineInfoCollection.ProductPartAttributesInfos[0].PackingDateFormatString); // Date format for testing

			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "CN";

			var groupedInventoryInfoCollection2 = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level2, new[] { inventory2 }, data.Whs1, 100);

			AssertEquals("yyMMdd", groupedInventoryInfoCollection2.InventoryLineInfoCollection.ProductPartAttributesInfos[0].ExpiryDateFormatString); // Date format for testing
			AssertEquals("yyMMdd", groupedInventoryInfoCollection2.InventoryLineInfoCollection.ProductPartAttributesInfos[0].PackingDateFormatString); // Date format for testing
		}

		#endregion

		#region TestInventoryLineInfoCollection

		public void TestInventoryLineInfoCollection_Level1()
		{
			TestInventoryLineInfoCollectionCore(WhsInventoryLevel.Level1);
		}

		public void TestInventoryLineInfoCollection_Level2()
		{
			TestInventoryLineInfoCollectionCore(WhsInventoryLevel.Level2);
		}

		public void TestInventoryLineInfoCollection_Level3()
		{
			TestInventoryLineInfoCollectionCore(WhsInventoryLevel.Level3);
		}

		void TestInventoryLineInfoCollectionCore(WhsInventoryLevel level)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var receive_Client1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive_Client1, data.Part1, 10m, data.Whs1.DefaultLocation, "P1");
			Helper.CreateWhsReceiveInventoryLine(receive_Client1, data.Part1, 10m, data.Whs1.DefaultLocation, "");
			Helper.CreateWhsReceiveInventoryLine(receive_Client1, data.Part2, 10m, data.Whs1.DefaultLocation, "P3");
			receive_Client1.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive_Client1);

			Factory.Save();

			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(level, receive_Client1.Inventory.Cast<WhsInventoryView>().ToArray(), data.Whs1, 100);

			var expectCount = level == WhsInventoryLevel.Level3 ? 3 : 2;
			AssertEquals(expectCount, groupedInventoryInfoCollection.Count);
			AssertEquals(expectCount, groupedInventoryInfoCollection.TotalCount);

			var inventoryLineInfoCollection = groupedInventoryInfoCollection.InventoryLineInfoCollection;
			AssertNotNull(inventoryLineInfoCollection);
			AssertEquals(2, inventoryLineInfoCollection.ProductInfos.Count);
			AssertEquals(2, inventoryLineInfoCollection.ProductPartAttributesInfos.Count);

			var expectedLineInfoCount = level == WhsInventoryLevel.Level3 ? 3 : 2;
			AssertEquals(expectedLineInfoCount, inventoryLineInfoCollection.InventoryLineInfos.Count);
		}

		public void TestInventoryLineInfoCollection_WithSerialNumber_Level1()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.BatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "P1", ZDate.Empty, ZDate.Empty, "A1", "", "", "SN1", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "P1", ZDate.Empty, ZDate.Empty, "A1", "", "", "SN2", "");

			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level1, receive.Inventory.Cast<WhsInventoryView>().ToArray(), data.Whs1, 100);

			AssertEquals(1, groupedInventoryInfoCollection.Count);
			AssertEquals(1, groupedInventoryInfoCollection.TotalCount);

			AssertInventoryInfoLine(groupedInventoryInfoCollection.Single(), 2m, 2m, 1, expectedQtyUQ: "UNT");
		}

		public void TestInventoryLineInfoCollection_WithSerialNumber_Level3()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.BatchNumber);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "P1", ZDate.Empty, ZDate.Empty, "A1", "", "", "SN1", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1.PK, 1m, data.Whs1.DefaultLocation.PK, "P1", ZDate.Empty, ZDate.Empty, "A1", "", "", "SN2", "");

			receive.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(WhsInventoryLevel.Level3, receive.Inventory.Cast<WhsInventoryView>().ToArray(), data.Whs1, 1);

			AssertEquals(1, groupedInventoryInfoCollection.Count);
			AssertEquals(2, groupedInventoryInfoCollection.TotalCount);

			AssertInventoryInfoLine(groupedInventoryInfoCollection.Single(), 0m, 0m, 0, expectedAttribute1: "A1", expectedSerialNumber: "SN1",
				expectedLocation: data.Whs1.DefaultLocation.ToLocationString(), expectedPalletID: "P1",
				expectedQty: 1m, expectedQtyUQ: "UNT");
		}

		#endregion

		#region TestTotalCount

		public void TestTotalCount_Level1()
		{
			TestTotalCountCore(WhsInventoryLevel.Level1, 1, 4);
		}

		public void TestTotalCount_Level2()
		{
			TestTotalCountCore(WhsInventoryLevel.Level2, 1, 4);
		}

		public void TestTotalCount_Level3()
		{
			TestTotalCountCore(WhsInventoryLevel.Level3, 1, 6);
		}

		void TestTotalCountCore(WhsInventoryLevel level, int expectedCount, int expectedTotalCount)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var org2 = Helper.CreateClient("O2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Helper.CreateProductClientRelationShip(org2, data.Part2);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "P1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "P2");
			var receiveLine3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation, "P2");
			var receiveLine4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, data.Whs1.DefaultLocation, "P1");
			receive.FinaliseDocketWithoutUserConfirmation();

			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R2");
			var receiveLine21 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, data.Whs1.DefaultLocation, "P1");
			var receiveLine22 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, data.Whs1.DefaultLocation, "P2");
			receive2.FinaliseDocketWithoutUserConfirmation();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			var inventory = receive.Inventory.Cast<WhsInventoryView>().Concat(receive2.Inventory.Cast<WhsInventoryView>());
			var groupedInventoryInfoCollection = new WhsGroupedInventoryInfoCollection(level, inventory.ToArray(), data.Whs1, 1);

			AssertEquals(expectedCount, groupedInventoryInfoCollection.Count);
			AssertEquals(expectedCount, groupedInventoryInfoCollection.InventoryLineInfoCollection.InventoryLineInfos.Count);
			AssertEquals(expectedTotalCount, groupedInventoryInfoCollection.TotalCount);
		}

		#endregion

		#region Implementation

		protected override Type GetExpectedCollectionType()
		{
			return typeof(WhsGroupedInventoryInfoCollection);
		}

		protected override Type GetExpectedObjectInfoType()
		{
			return typeof(WhsGroupedInventoryInfo);
		}

		protected override WhsGroupedInventoryInfo GetNewObjectInfo()
		{
			return new WhsGroupedInventoryInfo();
		}

		protected override DataObjectInfoCollection<WhsGroupedInventoryInfo> GetNewObjectInfoCollection()
		{
			return new WhsGroupedInventoryInfoCollection();
		}

		#endregion
	}
}
