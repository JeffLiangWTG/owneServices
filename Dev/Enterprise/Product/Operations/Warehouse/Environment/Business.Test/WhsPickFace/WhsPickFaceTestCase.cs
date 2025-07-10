using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsPickFace))]
	class WhsPickFaceTestCase : WhsEnvBusinessObjectTestCase
	{
		#region TestILocationConsumer

		public void TestILocationConsumer()
		{
			var whs = Helper.CreateWarehouse("WH1");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2);
			Factory.Save();

			var pickFace = Factory.New<WhsPickFace>();
			var locationPK = row.Locations[0].PK;
			pickFace.WF_WL = locationPK;
			var locationConsumer = (ILocationConsumer)pickFace;

			AssertEquals(locationConsumer.LocationTypeForMessages, "Pickface");
			AssertEquals(locationConsumer.LocationPK, locationPK);

			locationConsumer.LocationTitle = "Test";
			AssertEquals(locationConsumer.LocationTitle, "Test");
		}

		#endregion

		#region TestSaveAndDeleteBusinessObject

		[ExpectNoExceptions]
		public override void TestSaveAndDeleteBusinessObject()
		{
			var whs = Helper.CreateWarehouse("1");
			var row = Helper.CreateRow(whs, "R");

			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var pickFace = Helper.CreateProductPickFace(part, client, row.Locations[0]);
			Factory.Save();

			pickFace.Delete();
			Factory.Save();
		}

		#endregion

		#region TestSetDefaultValues

		public void TestSetDefaultValues()
		{
			var pickFace = Factory.New<WhsPickFace>();
			AssertEquals("Should default Replenish Maximum to 1.", 1m, pickFace.WF_ReplenishMaximum);
			AssertEquals("Should default Replenish Multiple to 1.", 1m, pickFace.WF_ReplenishmentMultiple);
		}

		#endregion

		#region TestLocationWhsGuid

		public void TestLocationWhsGuid()
		{
			var whs1 = Helper.CreateWarehouse("WHS1", "A", 2, 2);
			var whs2 = Helper.CreateWarehouse("WHS2", "A", 2, 2);
			Factory.Save();

			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "P1");

			var pickFace = Helper.CreateProductPickFace(part, client, whs1, "A-1-1");
			AssertEquals("Precondition: ", whs1.PK, pickFace.LocationWhsGuid);
			AssertEquals("Precondition: ", "A-1-1", pickFace.LocationString);

			// When warehouse is changed Location should be cleared.
			pickFace.LocationWhsGuid = whs2.PK;
			AssertEquals(whs2.PK, pickFace.LocationWhsGuid);
			AssertEquals("", pickFace.LocationString);
		}

		#endregion

		#region TestLocation

		public void TestLocation()
		{
			// create environment
			var whs = Helper.CreateWarehouse("AAAA");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2);
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "AAAA");
			var locationType = Helper.CreateLocationType("PFT", "PFT Test", false, 1, LocationClasses.Codes.FIX);
			Array.ForEach(row.Locations.ToArray(), l => l.WLV_WLT_LocationType = locationType.PK);

			Factory.Save();

			// create pickface
			var parent = Factory.New<WhsPickFace>();
			parent.LocationWhsGuid = whs.PK;
			parent.WF_OP = part.PK;

			// test setting components
			parent.LocationString = "A-2-1";
			AssertEquals("LocationString should not have errors", false, parent.LocationStringInfo.HasErrors());
			AssertEquals("FK must be valid after setting location string", row.Locations[2].PK, parent.WF_WL);

			parent.LocationString = "";
			AssertEquals("LocationString should have error", true, parent.LocationStringInfo.HasErrors());
			AssertEquals("FK should be empty", ZGuid.Empty, parent.WF_WL);

			parent.LocationString = "A-1-1";
			AssertEquals("LocationString should not have errors", false, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString should = 'A-1-1'", "A-1-1", parent.LocationString);
			AssertEquals("FK must be valid after setting location string", row.Locations[0].PK, parent.WF_WL);

			parent.LocationString = "A-50-50";
			AssertEquals("LocationString should have errors", true, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString should = 'A-50-50'", "A-50-50", parent.LocationString);
			AssertEquals("FK should be empty", ZGuid.Empty, parent.WF_WL);

			parent.LocationString = "SHEEP";
			AssertEquals("LocationString should have errors", true, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString should = 'SHEEP'", "SHEEP", parent.LocationString);
			AssertEquals("FK should be empty", ZGuid.Empty, parent.WF_WL);
		}

		public void TestLocation_FixedWidthLocation()
		{
			// create environment
			var whs = Helper.CreateFixedWidthLocationWarehouse("AAA", 2, 2, 2);
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "ZZ");
			var locationType = Helper.CreateLocationType("PFT", "PFT Test", false, 1, LocationClasses.Codes.FIX);
			Array.ForEach(row.Locations.ToArray(), l => l.WLV_WLT_LocationType = locationType.PK);

			Factory.Save();

			// create pickface
			var parent = Factory.New<WhsPickFace>();
			parent.LocationWhsGuid = whs.PK;
			parent.WF_OP = part.PK;

			var location = whs.FindLocation("A020201");
			// test setting components
			parent.LocationString = "A020201";
			AssertEquals("LocationString should not have errors", false, parent.LocationStringInfo.HasErrors());
			AssertEquals("FK must be valid after setting location string", location.PK, parent.WF_WL);
			AssertEquals("LocationString should = 'A-02-02-01'", "A-02-02-01", parent.LocationString);

			parent.LocationString = "";
			AssertEquals("LocationString should have error", true, parent.LocationStringInfo.HasErrors());
			AssertEquals("FK should be empty", ZGuid.Empty, parent.WF_WL);

			parent.LocationString = "A-02-02-01";
			AssertEquals("LocationString should not have errors", false, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString should = 'A-02-02-01'", "A-02-02-01", parent.LocationString);
			AssertEquals("FK must be valid after setting location string", location.PK, parent.WF_WL);

			parent.LocationString = "A505050";
			AssertEquals("LocationString should have errors", true, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString should = 'A505050'", "A505050", parent.LocationString);
			AssertEquals("FK should be empty", ZGuid.Empty, parent.WF_WL);

			parent.LocationString = "A0202";
			AssertEquals("LocationString should not have errors", false, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString should = 'A-02-02-01'", "A-02-02-01", parent.LocationString);
			AssertEquals("FK must be valid after setting location string", location.PK, parent.WF_WL);

			parent.LocationString = "SHEEP";
			AssertEquals("LocationString should have errors", true, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString should = 'SHEEP'", "SHEEP", parent.LocationString);
			AssertEquals("FK should be empty", ZGuid.Empty, parent.WF_WL);

			parent.LocationString = "A-02-02";
			AssertEquals("LocationString should not have errors", false, parent.LocationStringInfo.HasErrors());
			AssertEquals("LocationString should = 'A-02-02-01'", "A-02-02-01", parent.LocationString);
			AssertEquals("FK must be valid after setting location string", location.PK, parent.WF_WL);
		}

		#endregion

		#region TestLocationValidation

		public void TestLocationValidation()
		{
			// create environment
			var whs = Helper.CreateWarehouse("AAAA");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "AAAA");
			var locationTypeFIX = Helper.CreateLocationType("PF1", "Pick Face One", false, 1, LocationClasses.Codes.FIX);
			row.Locations[0].WLV_WLT_LocationType = locationTypeFIX.PK;
			Factory.Save();

			// create pickface
			var pickFace = Factory.New<WhsPickFace>();
			pickFace.LocationWhsGuid = whs.PK;
			pickFace.WF_OP = part.PK;
			pickFace.WF_OH_Client = org.PK;

			// Assign location string which does not exists
			pickFace.LocationString = "A-1";
			AssertHasErrors(pickFace.LocationStringInfo);

			// Assign location string which does exists
			pickFace.LocationString = "A";
			AssertNoErrors(pickFace.LocationStringInfo);
			Factory.Save();

			row.WR_Columns = 2;

			Factory.Save();

			var location2 = whs.FindLocation("A-1");
			location2.WLV_WLT_LocationType = locationTypeFIX.PK;

			Factory.Save();

			// Reset the locationstring
			pickFace.LocationString = "A-1";
			AssertNoErrors(pickFace.LocationStringInfo);
		}

		#endregion

		#region TestPicksAndTheirOrder

		public void TestCommittedPicksAndTheirOrder()
		{
			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 1);
			var locations = data.Locations[data.Warehouses[0]];

			int replenishMin = 1;
			var pickFace = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[0], replenishMin++);
			Factory.Save();

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var receive1PK = transactionHelper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[0].PK, "R1", Helper.Notify);
			transactionHelper.CreateWhsReceiveInventoryLine(receive1PK, data.Parts[0].PK, 100m, locations[0].PK);
			transactionHelper.FinaliseDocket(receive1PK);
			Factory.Save();

			var order1 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order1");
			transactionHelper.CreateWhsOrderLine(order1.PK, data.Parts[0].PK, 30m);
			var pick1 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order1.PK);
			pick1.PickPriority = 0;
			var order2 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order2");
			transactionHelper.CreateWhsOrderLine(order2.PK, data.Parts[0].PK, 40m);
			var pick2 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order2.PK);
			pick2.PickPriority = 2;
			var order3 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order3");
			transactionHelper.CreateWhsOrderLine(order3.PK, data.Parts[0].PK, 30m);
			var pick3 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order3.PK);
			pick3.PickPriority = 3;
			Factory.Save();

			var pickFaceBizO = Factory.Load<WhsPickFace>(pickFace.PK);
			var collection = pickFaceBizO.CommittedPicks;
			AssertEquals("Expected 3 picks in the collection", 3, collection.Count);
			AssertEquals("Expected first pick to be pick 2", pick2.PK, collection[0].WCP_WP);
			AssertEquals("Expected second pick to be pick 3", pick3.PK, collection[1].WCP_WP);
			AssertEquals("Expected third pick to be pick 1", pick1.PK, collection[2].WCP_WP);
		}

		public void TestAwaitingPicksAndTheirOrder()
		{
			var data = new PickFaceViewTestData(Factory, clients: 1, locationsPerWarehouse: 2);
			var locations = data.Locations[data.Warehouses[0]];
			var normalLocation =
				Helper.CreateLocationType("NOR", "NormalLocation", false, 1, LocationClasses.Codes.NOR);
			locations[0].WLV_WLT_LocationType = normalLocation.PK;

			int replenishMin = 1;
			var pickFace = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[1], replenishMin++);
			Factory.Save();

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var receive1PK = transactionHelper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[0].PK, "R1", Helper.Notify);
			transactionHelper.CreateWhsReceiveInventoryLine(receive1PK, data.Parts[0].PK, 100m, locations[0].PK);
			transactionHelper.FinaliseDocket(receive1PK);
			Factory.Save();

			var transfer = transactionHelper.CreateWhsTransfer(data.Clients[0].PK, data.Warehouses[0].PK, "T1", Helper.Notify);
			var transferLine = transactionHelper.CreateWhsTransferLine(transfer, data.Parts[0].PK, 100m, locations[0].PK, locations[1].PK);
			var transferLineBizO = Factory.GetBizOsForPK(transferLine.ToGuid());
			transferLineBizO[0].RunPreSaveValidation();
			Factory.Save();

			var order1 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order1");
			transactionHelper.CreateWhsOrderLine(order1.PK, data.Parts[0].PK, 30m);
			var pick1 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order1.PK);
			pick1.PickPriority = 0;
			var order2 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order2");
			transactionHelper.CreateWhsOrderLine(order2.PK, data.Parts[0].PK, 40m);
			var pick2 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order2.PK);
			pick2.PickPriority = 1;
			var order3 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order3");
			transactionHelper.CreateWhsOrderLine(order3.PK, data.Parts[0].PK, 30m);
			var pick3 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order3.PK);
			pick3.PickPriority = 2;
			Factory.Save();

			var pickFaceBizO = Factory.Load<WhsPickFace>(pickFace.PK);
			var collection = pickFaceBizO.AwaitingPicks;
			AssertEquals("Expected 3 picks in the collection", 3, collection.Count);
			AssertEquals("Expected first pick to be pick 2", pick2.PK, collection[0].WWP_WP);
			AssertEquals("Expected second pick to be pick 3", pick3.PK, collection[1].WWP_WP);
			AssertEquals("Expected third pick to be pick 1", pick1.PK, collection[2].WWP_WP);
		}

		#endregion

		#region TestAwaitingPicksWithMultipleProductsAndClients

		public void TestAwaitingPicksWithMultipleProducts()
		{
			var data = new PickFaceViewTestData(Factory, clients: 2, locationsPerWarehouse: 2);
			var locations = data.Locations[data.Warehouses[0]];
			var normalLocation =
				Helper.CreateLocationType("NOR", "NormalLocation", false, 1, LocationClasses.Codes.NOR);
			locations[0].WLV_WLT_LocationType = normalLocation.PK;

			int replenishMin = 1;
			var pickFace1 = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[1], replenishMin++);
			var pickFace2 = Helper.CreateProductPickFace(data.Parts[1], data.Clients[0], locations[1], replenishMin++);
			Factory.Save();

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			transactionHelper.CreateProductClientRelationShip(data.Clients[0].PK, data.Parts[1].PK);

			var receive1PK = transactionHelper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[0].PK, "R1", Helper.Notify);
			transactionHelper.CreateWhsReceiveInventoryLine(receive1PK, data.Parts[0].PK, 100m, locations[0].PK);
			transactionHelper.FinaliseDocket(receive1PK);

			var receive2PK = transactionHelper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[0].PK, "R2", Helper.Notify);
			transactionHelper.CreateWhsReceiveInventoryLine(receive2PK, data.Parts[1].PK, 100m, locations[0].PK);
			transactionHelper.FinaliseDocket(receive2PK);
			Factory.Save();

			var transfer1 = transactionHelper.CreateWhsTransfer(data.Clients[0].PK, data.Warehouses[0].PK, "T1", Helper.Notify);
			var transferLine1 = transactionHelper.CreateWhsTransferLine(transfer1, data.Parts[0].PK, 100m, locations[0].PK, locations[1].PK);
			var transferLineBizO1 = Factory.GetBizOsForPK(transferLine1.ToGuid());
			transferLineBizO1[0].RunPreSaveValidation();

			var transfer2 = transactionHelper.CreateWhsTransfer(data.Clients[0].PK, data.Warehouses[0].PK, "T2", Helper.Notify);
			var transferLine2 = transactionHelper.CreateWhsTransferLine(transfer2, data.Parts[1].PK, 100m, locations[0].PK, locations[1].PK);
			var transferLineBizO2 = Factory.GetBizOsForPK(transferLine2.ToGuid());
			transferLineBizO2[0].RunPreSaveValidation();
			Factory.Save();

			var order1 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order1");
			transactionHelper.CreateWhsOrderLine(order1.PK, data.Parts[0].PK, 30m);
			transactionHelper.CreateWhsOrderLine(order1.PK, data.Parts[1].PK, 40m);
			var pick1 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order1.PK);
			pick1.PickPriority = 0;
			Factory.Save();

			var pickFaceBizO = Factory.Load<WhsPickFace>(pickFace1.PK);
			var collection = pickFaceBizO.AwaitingPicks;
			AssertEquals("Expected 1 pick in the collection", 1, collection.Count);
			AssertEquals("Expected first pick to be pick 1", pick1.PK, collection[0].WWP_WP);
		}

		public void TestAwaitingPicksWithMultipleClients()
		{
			var data = new PickFaceViewTestData(Factory, clients: 2, locationsPerWarehouse: 2);
			var locations = data.Locations[data.Warehouses[0]];
			var normalLocation =
				Helper.CreateLocationType("NOR", "NormalLocation", false, 2, LocationClasses.Codes.NOR);
			locations[0].WLV_WLT_LocationType = normalLocation.PK;

			int replenishMin = 1;
			var pickFace1 = Helper.CreateProductPickFace(data.Parts[0], data.Clients[0], locations[1], replenishMin++);
			var pickFace2 = Helper.CreateProductPickFace(data.Parts[0], data.Clients[1], locations[1], replenishMin++);
			Factory.Save();

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			transactionHelper.CreateProductClientRelationShip(data.Clients[1].PK, data.Parts[0].PK);

			var receive1PK = transactionHelper.CreateWhsReceive(data.Clients[0].PK, data.Warehouses[0].PK, "R1", Helper.Notify);
			transactionHelper.CreateWhsReceiveInventoryLine(receive1PK, data.Parts[0].PK, 100m, locations[0].PK);
			transactionHelper.FinaliseDocket(receive1PK);

			var receive2PK = transactionHelper.CreateWhsReceive(data.Clients[1].PK, data.Warehouses[0].PK, "R2", Helper.Notify);
			transactionHelper.CreateWhsReceiveInventoryLine(receive2PK, data.Parts[0].PK, 100m, locations[0].PK);
			transactionHelper.FinaliseDocket(receive2PK);
			Factory.Save();

			var transfer1 = transactionHelper.CreateWhsTransfer(data.Clients[0].PK, data.Warehouses[0].PK, "T1", Helper.Notify);
			var transferLine1 = transactionHelper.CreateWhsTransferLine(transfer1, data.Parts[0].PK, 100m, locations[0].PK, locations[1].PK);
			var transferLineBizO1 = Factory.GetBizOsForPK(transferLine1.ToGuid());
			transferLineBizO1[0].RunPreSaveValidation();

			var transfer2 = transactionHelper.CreateWhsTransfer(data.Clients[1].PK, data.Warehouses[0].PK, "T2", Helper.Notify);
			var transferLine2 = transactionHelper.CreateWhsTransferLine(transfer2, data.Parts[0].PK, 100m, locations[0].PK, locations[1].PK);
			var transferLineBizO2 = Factory.GetBizOsForPK(transferLine2.ToGuid());
			transferLineBizO2[0].RunPreSaveValidation();
			Factory.Save();

			var order1 = transactionHelper.CreateWhsOrder(data.Clients[0].PK, data.Warehouses[0].PK, data.Clients[0].PK, "order1");
			transactionHelper.CreateWhsOrderLine(order1.PK, data.Parts[0].PK, 30m);
			var order2 = transactionHelper.CreateWhsOrder(data.Clients[1].PK, data.Warehouses[0].PK, data.Clients[1].PK, "order2");
			transactionHelper.CreateWhsOrderLine(order2.PK, data.Parts[0].PK, 40m);
			var pick1 = (IWhsPick)transactionHelper.CreatePickNew(false, false, order1.PK, order2.PK);
			pick1.PickPriority = 0;
			Factory.Save();

			var pickFaceBizO = Factory.Load<WhsPickFace>(pickFace1.PK);
			var collection = pickFaceBizO.AwaitingPicks;
			AssertEquals("Expected 1 pick in the collection", 1, collection.Count);
			AssertEquals("Expected first pick to be pick 1", pick1.PK, collection[0].WWP_WP);
		}

		#endregion
	}
}
