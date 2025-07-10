using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsTrackingInventorySummaryItemViewTest : WhsTestCaseWithFactory
	{
		[TestDate(2021, 7, 29)]
		public void TestWhsTrackingInventorySummaryItemView_ClientIsOwner()
		{
			TestWhsTrackingInventorySummaryItemViewCore(OrgPartRelation.RelationshipTypes.Owner);
		}

		[TestDate(2021, 7, 29)]
		public void TestWhsTrackingInventorySummaryItemView_ClientIsOwnerAndSupplier()
		{
			TestWhsTrackingInventorySummaryItemViewCore(OrgPartRelation.RelationshipTypes.Both);
		}

		void TestWhsTrackingInventorySummaryItemViewCore(string clientProductRelationship)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var clientForTest = Helper.CreateClient("C2");
			var rel = Helper.CreateProductClientRelationShip(clientForTest, data.Part1, clientProductRelationship);
			Helper.SetClientAttributeType(clientForTest, AttributeNumber.One, true);
			Helper.SetClientAttributeType(clientForTest, AttributeNumber.Two, true);
			Helper.SetClientAttributeType(clientForTest, AttributeNumber.Three, true);
			Helper.SetClientAttributeType(clientForTest, AttributeNumber.Serial, true);
			Helper.SetClientAttributeType(clientForTest, AttributeNumber.ExpiryDate, true);
			Helper.SetClientAttributeType(clientForTest, AttributeNumber.PackingDate, true);
			Helper.SetProductAllAttributeUse(clientForTest, data.Part1, true);

			data.Part1.OP_StockKeepingUnit = "UNT";
			rel.OU_ClientUQ = "BAG";
			Factory.Save();

			var receive = Helper.CreateWhsReceive(clientForTest, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			receiveLine.WE_PartAttrib1 = "RED";
			receiveLine.WE_PartAttrib2 = "SMALL";
			receiveLine.WE_PartAttrib3 = "TEST";
			receiveLine.WE_SerialNumber = "SN1";
			receiveLine.WE_PackingDate = ZDate.Today;
			receiveLine.WE_ExpiryDate = ZDate.Today;
			Factory.Save();

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			Assert("Receive is finalised.", receive.IsFinalised);
			var inventory = receive.Inventory[0];

			var results = LoadDataFromView();
			AssertEquals("Only 1 inventory.", 1, results.Count);
			AssertInventoryMatch(
				inventory,
				inventory.WI_ArrivalDate,
				data.Part1.OP_RX_NKLastWeightedCostCurr,
				"UNT",
				"BAG",
				0m,
				0m,
				inventory.WI_TotalUnits,
				results[0]);
		}

		[TestDate(2021, 7, 29)]
		public void TestWhsTrackingInventorySummaryItemView_PendingInventoryStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			receive.WD_ETA = ZDateTimeOffset.Today.AddDays(10);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			Factory.Save();

			var inventory = receive.Inventory[0];
			AssertEquals("Inventory status is PND.", "PND", inventory.WI_InventoryStatus);

			var results = LoadDataFromView();
			AssertEquals("Only 1 inventory.", 1, results.Count);
			AssertInventoryMatch(
				inventory,
				receive.WD_ETA,
				data.Part1.OP_RX_NKLastWeightedCostCurr,
				data.Part1.OP_StockKeepingUnit,
				data.Part1.OP_StockKeepingUnit,
				0m,
				0m,
				0m,
				results[0]);
		}

		[TestDate(2021, 7, 29)]
		public void TestWhsTrackingInventorySummaryItemView_WithCommittedUnits()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			Factory.Save();

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			Assert("Receive is finalised.", receive.IsFinalised);
			var inventory = receive.Inventory[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			Helper.CreatePickNew(order);
			AssertEquals(1, orderLine.PickLines.Count);

			var results = LoadDataFromView();
			AssertEquals("Only 1 inventory.", 1, results.Count);
			AssertInventoryMatch(
				inventory,
				inventory.WI_ArrivalDate,
				data.Part1.OP_RX_NKLastWeightedCostCurr,
				data.Part1.OP_StockKeepingUnit,
				data.Part1.OP_StockKeepingUnit,
				5m,
				0m,
				5m,
				results[0]);
		}

		[TestDate(2021, 7, 29)]
		public void TestWhsTrackingInventorySummaryItemView_WithCommittedUnits_ReadyToPack()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Packing", false, 0, LocationClasses.Codes.CON);
			var packingLocation = Helper.CreateRowAndGenerateLocations(data.Whs1, "PS", 1, 1).Locations[0];
			packingLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			Factory.Save();

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			Assert("Receive is finalised.", receive.IsFinalised);
			var inventory = receive.Inventory[0];

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Factory.Save();

			Helper.CreatePickNew(order);
			var pickLines = orderLine.PickLines;
			AssertEquals(1, pickLines.Count);

			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLines[0], ZDateTimeOffset.Now);
			transferLine.WE_WL = packingLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			var results = LoadDataFromView();
			AssertEquals("ResultSet Count", 2, results.Count);
			var transferInventory = transferLine.Inventory[0];
			var result0 = results.Single(o => o["WI_PK"].Equals(inventory.PK));
			var result1 = results.Single(o => o["WI_PK"].Equals(transferInventory.PK));
			AssertInventoryMatch(
				inventory,
				inventory.WI_ArrivalDate,
				data.Part1.OP_RX_NKLastWeightedCostCurr,
				data.Part1.OP_StockKeepingUnit,
				data.Part1.OP_StockKeepingUnit,
				0m,
				0m,
				5m,
				result0);
			AssertInventoryMatch(
				transferInventory,
				transferInventory.WI_ArrivalDate,
				data.Part1.OP_RX_NKLastWeightedCostCurr,
				data.Part1.OP_StockKeepingUnit,
				data.Part1.OP_StockKeepingUnit,
				5m,
				0m,
				0m,
				result1);
		}

		[TestDate(2021, 7, 29)]
		public void TestWhsTrackingInventorySummaryItemView_WithCrossDockedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			order.WD_WL_CrossDock = dockDoorLocation.PK;
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertEquals("Precondition: Stock is cross-docked.", 10m, reservedPickLine.ReservedQuantity);
			AssertEquals("Precondition: location is a dockdoor location", true, dockDoorLocation.IsDockDoorLocation);
			Factory.Save();

			var results = LoadDataFromView();
			AssertEquals("Only 1 inventory.", 1, results.Count);
			AssertInventoryMatch(
				inventory,
				inventory.WI_ArrivalDate,
				data.Part1.OP_RX_NKLastWeightedCostCurr,
				data.Part1.OP_StockKeepingUnit,
				data.Part1.OP_StockKeepingUnit,
				0m,
				10m,
				0m,
				results[0]);
		}

		[TestDate(2021, 7, 29)]
		public void TestWhsTrackingInventorySummaryItemView_WithClienUQ()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			data.Part1.RelatedOrganisations[0].OU_ClientUQ = "PLT";
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation);
			Factory.Save();

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			Assert("Receive is finalised.", receive.IsFinalised);
			var inventory = receive.Inventory[0];

			var results = LoadDataFromView();
			AssertEquals("Only 1 inventory.", 1, results.Count);
			AssertInventoryMatch(
				inventory,
				inventory.WI_ArrivalDate,
				data.Part1.OP_RX_NKLastWeightedCostCurr,
				data.Part1.OP_StockKeepingUnit,
				"PLT",
				0m,
				0m,
				10m,
				results[0]);
		}

		DynamicBusinessObjectCollection LoadDataFromView()
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"SELECT * FROM dbo.WhsTrackingInventorySummaryItemView";
			result.Load(sql);

			return result;
		}

		void AssertInventoryMatch(WhsInventoryView inventory,
			ZDateTimeOffset expectedArrivalDateOrETA,
			ZString expectedCurrency,
			ZString expectedStockKeepingUnit,
			ZString expectedClientUQ,
			ZDecimal expectedCommittedUnits,
			ZDecimal expectedCrossDockQuantity,
			ZDecimal expectedAvailableUnits,
			DynamicBusinessObject resultRow)
		{
			AssertEquals("WI_PK", inventory.PK, resultRow["WI_PK"]);
			AssertEquals("WI_OH_Client", inventory.WI_OH_Client, resultRow["WI_OH_Client"]);
			AssertEquals("WI_WW_Whs", inventory.WI_WW_Whs, resultRow["WI_WW_Whs"]);
			AssertEquals("WI_OP", inventory.WI_OP, resultRow["WI_OP"]);
			AssertEquals("WI_WL", inventory.WI_WL, resultRow["WI_WL"]);
			AssertEquals("WI_WD", inventory.WI_WD, resultRow["WI_WD"]);
			AssertEquals("WI_WE_InDocketLine", inventory.WI_WE_InDocketLine, resultRow["WI_WE_InDocketLine"]);
			AssertEquals("WI_ArrivalDate", inventory.WI_ArrivalDate, resultRow["WI_ArrivalDate"]);
			AssertEquals("WI_TotalUnits", inventory.WI_TotalUnits, resultRow["WI_TotalUnits"]);
			AssertEquals("WI_InventoryStatus", inventory.WI_InventoryStatus, resultRow["WI_InventoryStatus"]);
			AssertEquals("WI_ExpiryDate", inventory.WI_ExpiryDate, resultRow["WI_ExpiryDate"]);
			AssertEquals("WI_PackingDate", inventory.WI_PackingDate, resultRow["WI_PackingDate"]);
			AssertEquals("WI_PalletID", inventory.WI_PalletID, resultRow["WI_PalletID"]);
			AssertEquals("WI_WE_OriginalInDocketLineForRating", inventory.WI_WE_OriginalInDocketLineForRating,
				resultRow["WI_WE_OriginalInDocketLineForRating"]);
			AssertEquals("WI_PartAttrib1", inventory.WI_PartAttrib1, resultRow["WI_PartAttrib1"]);
			AssertEquals("WI_PartAttrib2", inventory.WI_PartAttrib2, resultRow["WI_PartAttrib2"]);
			AssertEquals("WI_PartAttrib3", inventory.WI_PartAttrib3, resultRow["WI_PartAttrib3"]);
			AssertEquals("WI_SerialNumber", inventory.WI_SerialNumber, resultRow["WI_SerialNumber"]);
			AssertEquals("WI_ArrivalDateOrETA", expectedArrivalDateOrETA, resultRow["WI_ArrivalDateOrETA"]);
			AssertEquals("WI_Currency", expectedCurrency, resultRow["WI_Currency"]);
			AssertEquals("WI_UnitsUQ", expectedStockKeepingUnit, resultRow["WI_UnitsUQ"]);
			AssertEquals("WI_ClientUQ", expectedClientUQ, resultRow["WI_ClientUQ"]);
			AssertEquals("WI_CommittedUnits", expectedCommittedUnits, resultRow["WI_CommittedUnits"]);
			AssertEquals("WI_CrossDockQuantity", expectedCrossDockQuantity, resultRow["WI_CrossDockQuantity"]);
			AssertEquals("WI_AvailableUnits", expectedAvailableUnits, resultRow["WI_AvailableUnits"]);
		}
	}
}
