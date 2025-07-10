using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsPickFaceViewTest : WhsTestCaseWithFactory
	{
		#region Unique PK

		public void TestView_UniquePKSameLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part2), data.Org1, fixLocation,
				60m, 120m, 12m);
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { pickFace1.PK, pickFace2.PK },
				results.Select(r => (ZGuid)r["WPV_WF"]));
		}

		public void TestView_UniquePK_NoProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation1 = data.Whs1.FindLocation("A-1");
			fixLocation1.WLV_WLT_LocationType = fixLocationType.PK;
			var fixLocation2 = data.Whs1.FindLocation("A-2");
			fixLocation2.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation1,
				50m, 200m, 10m);
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertContainsExactElementsInAnyOrder(new[] { fixLocation1.PK, fixLocation2.PK },
				results.Select(r => (ZGuid)r["WPV_WL"]));
		}

		public void TestView_UniquePK_UnassignedFixLocation_WithTwoProductsInStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			var receivePart1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, fixLocation, "PLT1");
			var receivePart2 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 20m, fixLocation, "PLT2");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			Factory.Save();

			var results = LoadOrderedDataFromView(orderByColumn: WhsPickFaceViewSchema.WPV_TotalQuantity.Name);
			AssertContainsExactElementsInAnyOrder(new[] { data.Part1.PK, data.Part2.PK },
				results.Select(r => (ZGuid)r["WPV_OP"]));
		}

		#endregion

		#region Available To Pick

		public void TestView_AvailableToPick_CrossDocking()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 8m, fixLocation,
				"", finalise: false);
			var inventory = receive.Inventory[0];
			AssertEquals("Precondition: inventory is putaway.", InventoryStatus.Codes.Putaway,
				inventory.WI_InventoryStatus);
			Factory.Save();

			var reservedCount = 3m;
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, reservedCount);
			Helper.CreateReservePickLine(order1.Lines[0], receive.Inventory[0], reservedCount);
			Factory.Save();

			var results1 = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results1.Count);
			AssertRow(results1[0], data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick", 0m);

			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: Inventory should be available.", InventoryStatus.Codes.Available,
				inventory.WI_InventoryStatus);

			var results2 = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results2.Count);
			AssertRow(results2[0], data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick", 5m);
		}

		public void TestView_AvailableToPick_DifferentProductsSameClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part2), data.Org1, fixLocation,
				60m, 120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, fixLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 32m, fixLocation, "");
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick", 64m);
			AssertRow(results[1], data.Whs1, pickFace2, data.Org1, data.Part2, fixLocation, "WPV_AvailableToPick", 32m);
		}

		public void TestView_AvailableToPick_SameProductsDifferentClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var org2 = Helper.CreateClient("222", "222");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			AssertNotEquals("Precondition: Different Clients", data.Org1.PK, org2.PK);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), org2, fixLocation, 60m,
				120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 17m, fixLocation, "");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 39m, fixLocation, "");
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick", 17m);
			AssertRow(results[1], data.Whs1, pickFace2, org2, data.Part1, fixLocation, "WPV_AvailableToPick", 39m);
		}

		public void TestView_AvailableToPick_DifferentLocationsSameClientSameProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation1 = data.Whs1.FindLocation("A-1");
			fixLocation1.WLV_WLT_LocationType = fixLocationType.PK;
			var fixLocation2 = data.Whs1.FindLocation("A-2");
			fixLocation2.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation1,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation2,
				60m, 120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 17m, fixLocation1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 39m, fixLocation2, "");
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation1, "WPV_AvailableToPick",
				17m);
			AssertRow(results[1], data.Whs1, pickFace2, data.Org1, data.Part1, fixLocation2, "WPV_AvailableToPick",
				39m);
		}

		public void TestView_AvailableToPick_Held()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, fixLocation, "");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 8m, fixLocation,
				"", finalise: false);
			var inventory = receive.Inventory[0];
			inventory.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertRow(results[0], data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick", 64m);
		}

		public void TestView_AvailbleToPick_CommittedToPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, fixLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertRow(results[0], data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick", 60m);
		}

		public void TestView_AvailableToPick_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation1 = data.Whs1.FindLocation("A-1");
			fixLocation1.WLV_WLT_LocationType = fixLocationType.PK;
			var fixLocation2 = data.Whs1.FindLocation("A-2");
			fixLocation2.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation1,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation2,
				60m, 120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, fixLocation1,
				string.Empty);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "A-2");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Precondition: Created In-Transit inventory.", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation1, "WPV_AvailableToPick",
				63m);
			AssertRow(results[1], data.Whs1, pickFace2, data.Org1, data.Part1, fixLocation2, "WPV_AvailableToPick", 0m);
		}

		public void TestView_AvailableToPick_UnfinalizedReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, fixLocation, "");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 8m, fixLocation,
				"", finalise: false);
			AssertEquals("Precondition", false, receive.IsFinalised);
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertRow(results[0], data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick", 64m);
		}

		public void TestView_AvailableToPick_OnlyHasHeldInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, fixLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var results1 = LoadOrderedDataFromView();
			AssertRow(results1.Single(), data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick",
				20m);

			receiveLine.IsInventoryEditForm = true;
			receiveLine.HeldCodeChangeQuantity = 6m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var statusChangeLine =
				Factory.LoadTop1<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, false));
			AssertNotNull("Precondition: HoldCode Changed.", statusChangeLine);

			var results2 = LoadOrderedDataFromView();
			AssertRow(results2.Single(), data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick",
				14m);

			// Commit Held Inventory
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 6m, "A-1", "A-2",
				heldCode: InventoryStatus.Codes.Held);
			transfer.RunPreSaveValidation();
			Factory.Save();

			var results3 = LoadOrderedDataFromView();
			AssertRow(results3.Single(), data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick",
				14m);
		}

		public void TestView_AvailableToPick_AvailableCommittedAndHeldCommitted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, fixLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			var results1 = LoadOrderedDataFromView();
			AssertRow(results1.Single(), data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick",
				20m);

			receiveLine.IsInventoryEditForm = true;
			receiveLine.HeldCodeChangeQuantity = 3m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var statusChangeLine =
				Factory.LoadTop1<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, false));
			AssertNotNull("Precondition: HoldCode Changed.", statusChangeLine);

			var results2 = LoadOrderedDataFromView();
			AssertRow(results2.Single(), data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick",
				17m);

			// Commit Available Inventory
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(order1);
			Factory.Save();

			var results3 = LoadOrderedDataFromView();
			AssertRow(results3.Single(), data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick",
				7m);

			// Commit Held Inventory
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "A-2",
				heldCode: InventoryStatus.Codes.Held);
			transfer.RunPreSaveValidation();
			Factory.Save();

			var results4 = LoadOrderedDataFromView();
			AssertRow(results4.Single(), data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_AvailableToPick",
				7m);
		}

		#endregion

		#region Committed

		public void TestView_Committed_CrossDocking()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, fixLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 32m, fixLocation, "");
			Factory.Save();

			var reservedCount = 11m;
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, reservedCount);
			Helper.CreateReservePickLine(order1.Lines[0], receive.Inventory[0], reservedCount);
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertRow(results[0], data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_Committed",
				reservedCount);
		}

		public void TestView_Committed_DifferentProductsSameClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part2), data.Org1, fixLocation,
				60m, 120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, fixLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			Helper.CreatePickNew(order1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 32m, fixLocation, "");
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation, "WPV_Committed", 4m);
			AssertRow(results[1], data.Whs1, pickFace2, data.Org1, data.Part2, fixLocation, "WPV_Committed", 0m);
		}

		public void TestView_Committed_SameProductsDifferentClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var org2 = Helper.CreateClient("222", "222");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			AssertNotEquals("Precondition: Different Clients", data.Org1.PK, org2.PK);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), org2, fixLocation, 60m,
				120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 17m, fixLocation, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			Helper.CreatePickNew(order1);
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 39m, fixLocation, "");
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation, "WPV_Committed", 4m);
			AssertRow(results[1], data.Whs1, pickFace2, org2, data.Part1, fixLocation, "WPV_Committed", 0m);
		}

		public void TestView_Committed_DifferentLocationsSameClientSameProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation1 = data.Whs1.FindLocation("A-1");
			fixLocation1.WLV_WLT_LocationType = fixLocationType.PK;
			var fixLocation2 = data.Whs1.FindLocation("A-2");
			fixLocation2.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation1,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation2,
				60m, 120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 17m, fixLocation1, "");
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			Helper.CreatePickNew(order1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 39m, fixLocation2, "");
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation1, "WPV_Committed", 4m);
			AssertRow(results[1], data.Whs1, pickFace2, data.Org1, data.Part1, fixLocation2, "WPV_Committed", 0m);
		}

		public void TestView_Committed_Held()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, fixLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(receive);

			receiveLine.IsInventoryEditForm = true;
			receiveLine.HeldCodeChangeQuantity = 3m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var statusChangeLine =
				Factory.LoadTop1<WhsReceiveLine>(new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, false));
			AssertNotNull("Precondition: HoldCode Changed.", statusChangeLine);

			var results1 = LoadOrderedDataFromView();
			AssertRow(results1.Single(), data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_Committed", 0m);

			// Commit Available Inventory
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			Helper.CreatePickNew(order1);
			Factory.Save();

			var results2 = LoadOrderedDataFromView();
			AssertRow(results2.Single(), data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_Committed", 10m);

			// Commit Held Inventory
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "T1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, "A-1", "A-2",
				heldCode: InventoryStatus.Codes.Held);
			transfer.RunPreSaveValidation();
			Factory.Save();

			var results3 = LoadOrderedDataFromView();
			AssertRow(results3.Single(), data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_Committed", 13m);
		}

		#endregion

		#region Incoming

		public void TestView_Incoming_Transfer_Unpicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var originalLocation = data.Whs1.FindLocation("A-2");
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m,
				originalLocation, string.Empty);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-2", "A-1");
			Helper.CreateWhsPickLine(transferLine, receive.Inventory[0], 1m);
			Factory.Save();
			AssertEquals("Precondition: Existing Unpicked inventory.", InventoryStatus.Codes.Available,
				transferLine.WE_CurrentInventoryStatus);

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation, "WPV_Incoming", 1m);
		}

		public void TestView_Incoming_Transfer_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var originalLocation = data.Whs1.FindLocation("A-2");
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1,
				originalLocation, 60m, 120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, originalLocation,
				string.Empty);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-2", "A-1");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Precondition: Created In-Transit inventory.", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation, "WPV_Incoming", 1m);
			AssertRow(results[1], data.Whs1, pickFace2, data.Org1, data.Part1, originalLocation, "WPV_Incoming", 0m);
		}

		public void TestView_Incoming_Transfer_Finalized()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var originalLocation = data.Whs1.FindLocation("A-2");
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, originalLocation,
				string.Empty);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-2", "A-1");
			transfer.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(transfer);

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation, "WPV_Incoming", 0m);
		}

		public void TestView_Incoming_Recieve_PutAway()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, fixLocation, "");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 8m, fixLocation,
				"", finalise: false);
			var inventory = receive.Inventory[0];
			AssertEquals("Precondition: inventory is putaway.", InventoryStatus.Codes.Putaway,
				inventory.WI_InventoryStatus);
			Factory.Save();

			var results1 = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results1.Count);
			AssertRow(results1[0], data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_Incoming", 0m);

			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			AssertEquals("Precondition: inventory is available.", InventoryStatus.Codes.Available,
				inventory.WI_InventoryStatus);
			Factory.Save();

			var results2 = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results2.Count);
			AssertRow(results2[0], data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_Incoming", 0m);
		}

		public void TestView_Incoming_UnfinalizedAdjustment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var originalLocation = data.Whs1.FindLocation("A-2");
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1,
				originalLocation, 60m, 120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, originalLocation,
				string.Empty);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-2", "A-1");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, "A-1");
			Factory.Save();
			AssertEquals("Precondition: Created In-Transit inventory.", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			Assert("Precondition: Created unfinalized adjustment.", !adjustment.IsFinalised);

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation, "WPV_Incoming", 1m);
			AssertRow(results[1], data.Whs1, pickFace2, data.Org1, data.Part1, originalLocation, "WPV_Incoming", 0m);
		}

		public void TestView_Incoming_FinalizedAdjustment()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var originalLocation = data.Whs1.FindLocation("A-2");
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1,
				originalLocation, 60m, 120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, originalLocation,
				string.Empty);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-2", "A-1");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, "A-1");
			adjustment.FinaliseDocket();
			Factory.Save();
			AssertEquals("Precondition: Created In-Transit inventory.", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			AssertIsFinalisedPrecondition(adjustment);

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation, "WPV_Incoming", 1m);
			AssertRow(results[1], data.Whs1, pickFace2, data.Org1, data.Part1, originalLocation, "WPV_Incoming", 0m);
		}

		#endregion

		#region Total Quantity

		public void TestView_TotalQuantity_DifferentProductsSameClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part2), data.Org1, fixLocation,
				60m, 120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, fixLocation, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 32m, fixLocation, "");
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation, "WPV_TotalQuantity", 64m);
			AssertRow(results[1], data.Whs1, pickFace2, data.Org1, data.Part2, fixLocation, "WPV_TotalQuantity", 32m);
		}

		public void TestView_TotalQuantity_SameProductsDifferentClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var org2 = Helper.CreateClient("222", "222");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			AssertNotEquals("Different Clients", data.Org1.PK, org2);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), org2, fixLocation, 60m,
				120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 17m, fixLocation, "");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part1, 39m, fixLocation, "");
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation, "WPV_TotalQuantity", 17m);
			AssertRow(results[1], data.Whs1, pickFace2, org2, data.Part1, fixLocation, "WPV_TotalQuantity", 39m);
		}

		public void TestView_TotalQuantity_DifferentLocationsSameClientSameProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation1 = data.Whs1.FindLocation("A-1");
			fixLocation1.WLV_WLT_LocationType = fixLocationType.PK;
			var fixLocation2 = data.Whs1.FindLocation("A-2");
			fixLocation2.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation1,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation2,
				60m, 120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 17m, fixLocation1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 39m, fixLocation2, "");
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation1, "WPV_TotalQuantity", 17m);
			AssertRow(results[1], data.Whs1, pickFace2, data.Org1, data.Part1, fixLocation2, "WPV_TotalQuantity", 39m);
		}

		public void TestView_TotalQuantity_Held()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, fixLocation, "");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 8m, fixLocation,
				"", finalise: false);
			var inventory = receive.Inventory[0];
			inventory.OriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertRow(results[0], data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_TotalQuantity", 72m);
		}

		public void TestView_TotalQuantity_CommittedToPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, fixLocation, "");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertRow(results[0], data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_TotalQuantity", 64m);
		}

		public void TestView_TotalQuantity_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var fixLocation1 = data.Whs1.FindLocation("A-1");
			fixLocation1.WLV_WLT_LocationType = fixLocationType.PK;
			var fixLocation2 = data.Whs1.FindLocation("A-2");
			fixLocation2.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation1,
				50m, 200m, 10m);
			var pickFace2 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation2,
				60m, 120m, 12m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, fixLocation1,
				string.Empty);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 1m, "A-1", "A-2");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Precondition: Created In-Transit inventory.", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertNotEquals(results[0]["WPV_PK"], results[1]["WPV_PK"]);
			AssertRow(results[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation1, "WPV_TotalQuantity", 63m);
			AssertRow(results[1], data.Whs1, pickFace2, data.Org1, data.Part1, fixLocation2, "WPV_TotalQuantity", 0m);
		}

		public void TestView_TotalQuantity_UnfinalizedReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 64m, fixLocation, "");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 8m, fixLocation,
				"", finalise: false);
			AssertEquals("Precondition", false, receive.IsFinalised);
			AssertEquals("Precondition", InventoryStatus.Codes.Putaway, receive.Lines[0].WE_CurrentInventoryStatus);
			Factory.Save();

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertRow(results[0], data.Whs1, pickFace, data.Org1, data.Part1, fixLocation, "WPV_TotalQuantity", 72m);
		}

		public void TestView_TotalQuantity_AdjustmentIns()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace1 = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, 200m, 10m);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, 2m, "A");
			Factory.Save();
			Assert("Precondition: Created unfinalized adjustment.", !adjustment.IsFinalised);
			var results1 = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results1.Count);
			AssertRow(results1[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation, "WPV_TotalQuantity", 0m);

			adjustment.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(adjustment);
			var results2 = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results2.Count);
			AssertRow(results2[0], data.Whs1, pickFace1, data.Org1, data.Part1, fixLocation, "WPV_TotalQuantity", 2m);
		}

		public void TestView_TotalQuantity_PutawayTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 1, LocationClasses.Codes.FIX);
			var pickfaceLocation = data.Whs1.FindLocation("A-2");
			pickfaceLocation.WLV_WLT_LocationType = fixLocationType.PK;

			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1,
				pickfaceLocation, 50m, 200m, 10m);
			var receive1 = Helper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", ZDateTimeOffset.Empty);
			var inv1InRec1ForPart1 =
				Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation, "A", 1m);
			var inv2InRec1ForPart1 =
				Helper.CreateInventoryForDockDoorLocation(receive1, data.Part1, dockDoorLocation, "A", 2m);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			transfer.WD_IsPutawayTransfer = true;
			var transferLineForPart1 = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1,
				dockDoorLocation, pickfaceLocation, "A", 3m);
			transferLineForPart1.WE_PalletID = ZString.Empty; // In case this is ever supported functionally.
			transfer.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(transfer);
			AssertEquals("Precondition: Is putaway transfer", InventoryStatus.Codes.Putaway,
				transferLineForPart1.WE_CurrentInventoryStatus);
			Assert("Precondition: Is putaway transfer", transfer.WD_IsPutawayTransfer);

			var results = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertRow(results[0], data.Whs1, pickFace, data.Org1, data.Part1, pickfaceLocation, "WPV_TotalQuantity",
				3m);
		}

		#endregion

		#region UnassignedPickFaces

		public void TestUnassignedPickFaces_ShownInView()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);
			var fixLocation1 = data.Whs1.FindLocation("A-1");
			var fixLocation2 = data.Whs1.FindLocation("A-2");
			fixLocation1.WLV_WLT_LocationType = fixLocationType.PK;
			fixLocation2.WLV_WLT_LocationType = fixLocationType.PK;

			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation1,
				5m, 10m, 1m);
			Factory.Save();

			var results = LoadOrderedDataFromView();

			AssertEquals("Should contain 2 rows", 2, results.Count);
			AssertUnassignedPickFaceLocation(results[0], data.Whs1, fixLocation2, clientPK: null, productPK: null);
			AssertRow(results[1], data.Whs1, pickFace, data.Org1, data.Part1, fixLocation1, "WPV_TotalQuantity", 0m);
		}

		public void TestUnassignedPickFaces_WithStock_TotalQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");

			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, fixLocation,
					"PLT001");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			Factory.Save();

			var results = LoadOrderedDataFromView(orderByColumn: WhsPickFaceViewSchema.PK.Name);

			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertUnassignedPickFaceLocation(results.Single(), data.Whs1, fixLocation, clientPK: data.Org1.PK,
				productPK: data.Part1.PK);
			AssertTotals(results.Single(), totalQuantity: 10m, availableToPick: 10m);
		}

		public void TestUnassignedPickFaces_WithStock_Incoming()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			var normalLocation = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m,
				normalLocation, "PLT001");
			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 50m, normalLocation, fixLocation);
			var pickLine = Helper.CreateWhsPickLine(transferLine, receive.Inventory[0], 50m);
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			Factory.Save();

			AssertEquals("Precondition: Transfer should not be finalized.", false, transfer.IsFinalised);

			var results = LoadOrderedDataFromView(orderByColumn: WhsPickFaceViewSchema.PK.Name);

			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertUnassignedPickFaceLocation(results.Single(), data.Whs1, fixLocation, clientPK: data.Org1.PK,
				productPK: data.Part1.PK);
			AssertTotals(results.Single(), totalQuantity: 0m, incoming: 50m);
		}

		public void TestUnassignedPickFaces_WithStock_CommittedAndAvailableToPick()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");

			var receive =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, fixLocation,
					"PLT001");
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 20m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = Helper.CreateWhsPickLine(order.Lines.Single(), receive.Inventory[0], 6m);
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			Factory.Save();

			AssertEquals("Precondition: Order should not be finalized.", false, order.IsFinalised);

			var results = LoadOrderedDataFromView(orderByColumn: WhsPickFaceViewSchema.PK.Name);

			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertUnassignedPickFaceLocation(results.Single(), data.Whs1, fixLocation, clientPK: data.Org1.PK,
				productPK: data.Part1.PK);
			AssertTotals(results.Single(), totalQuantity: 10m, incoming: 0m, committed: 6m, availableToPick: 4m);
		}

		public void TestUnassignedPickFaces_WithStock_InventoryLineWithZeroQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventoryLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, fixLocation);
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			Factory.Save();

			AssertEquals("Precondition: Receive should not be finalized.", false, receive.IsFinalised);

			var results = LoadOrderedDataFromView(orderByColumn: WhsPickFaceViewSchema.PK.Name);

			AssertEquals("Should contain 1 row", 1, results.Count);
			AssertUnassignedPickFaceLocation(results.Single(), data.Whs1, fixLocation, clientPK: null, productPK: null);
			AssertTotals(results.Single(), totalQuantity: 0m);
		}

		#endregion

		#region ABC Category

		public void TestView_AbcCategory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation, 50m, 200m, 10m);
			var abcCategory = Factory.NewWithValidTestData<WhsABCCategory>();
			abcCategory.WJ_WW_Warehouse = data.Whs1.PK;
			abcCategory.WJ_Category = "A";
			abcCategory.WJ_OP_Product = data.Part1.PK;
			abcCategory.WJ_OH_Client = data.Org1.PK;
			abcCategory.WJ_AnalysisDateTo = ZDateTimeOffset.Today;
			Factory.Save();

			var result = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row.", 1, result.Count);
			AssertEquals("ABC Category is A.", abcCategory.WJ_Category, result[0]["WPV_ABCCategory"]);
		}

		public void TestView_AbcCategory_MultipleCategoriesForProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation, 50m, 200m, 10m);
			var abcCategory1 = Factory.NewWithValidTestData<WhsABCCategory>();
			abcCategory1.WJ_WW_Warehouse = data.Whs1.PK;
			abcCategory1.WJ_Category = "A";
			abcCategory1.WJ_OP_Product = data.Part1.PK;
			abcCategory1.WJ_OH_Client = data.Org1.PK;
			abcCategory1.WJ_AnalysisDateTo = ZDateTimeOffset.Today;
			var abcCategory2 = Factory.NewWithValidTestData<WhsABCCategory>();
			abcCategory2.WJ_WW_Warehouse = data.Whs1.PK;
			abcCategory2.WJ_Category = "B";
			abcCategory2.WJ_OP_Product = data.Part1.PK;
			abcCategory2.WJ_OH_Client = data.Org1.PK;
			abcCategory2.WJ_AnalysisDateTo = ZDateTimeOffset.Today.AddDays(-1);
			Factory.Save();

			var result = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row.", 1, result.Count);
			AssertEquals("ABC Category is A.", abcCategory1.WJ_Category, result[0]["WPV_ABCCategory"]);
		}

		public void TestView_AbcCategory_NoCategoryFound()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation, 50m, 200m, 10m);
			Factory.Save();

			var result = LoadOrderedDataFromView();
			AssertEquals("Should contain 1 row.", 1, result.Count);
			AssertEquals("ABC Category is empty.", string.Empty, result[0]["WPV_ABCCategory"]);
		}

		#endregion

		#region PercentageFull

		public void TestView_PercentageFull()
		{
			var maxStock = 250m;
			var availableStock = 50m;
			var expectedPercentage = 20m;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, maxStock, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, availableStock, fixLocation,
				"");

			Factory.Save();

			var result = LoadOrderedDataFromView();
			AssertEquals(expectedPercentage, result[0]["WPV_PercentageFull"]);
		}

		public void TestView_PercentageFull_Rounding()
		{
			var maxStock = 850m;
			var availableStock = 750m;
			var expectedPercentage = 88.24m;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, maxStock, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, availableStock, fixLocation,
				"");

			Factory.Save();

			var result = LoadOrderedDataFromView();
			AssertEquals(expectedPercentage, result[0]["WPV_PercentageFull"]);
		}

		public void TestView_PercentageFull_NoPickFace()
		{
			var availableStock = 10m;
			var expectedPercentage = 100m;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, availableStock, fixLocation,
				"PLT001", finalise: false);

			Factory.Save();

			var result = LoadOrderedDataFromView();
			AssertEquals(expectedPercentage, result[0]["WPV_PercentageFull"]);
		}

		public void TestView_PercentageFull_NoPickFaceAndNoInventory()
		{
			var expectedPercentage = 0m;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;

			Factory.Save();

			var result = LoadOrderedDataFromView();
			AssertEquals(expectedPercentage, result[0]["WPV_PercentageFull"]);
		}

		public void TestView_PercentageFull_Max100()
		{
			var maxStock = 10m;
			var availableStock = 15m;
			var expectedPercentage = 100m;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				0m, maxStock, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, availableStock, fixLocation,
				"");

			Factory.Save();

			var result = LoadOrderedDataFromView();
			AssertEquals(expectedPercentage, result[0]["WPV_PercentageFull"]);
		}

		public void TestView_PercentageFull_HeldStock()
		{
			var maxStock = 250m;
			var availableStock = 50m;
			var heldStock = 10m;
			var expectedPercentage = 20m;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, maxStock, 10m);

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, availableStock,
				fixLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, heldStock,
				fixLocation, "", finalise: false);
			receive2.Lines[0].WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);

			Factory.Save();

			var result = LoadOrderedDataFromView();
			AssertEquals(expectedPercentage, result[0]["WPV_PercentageFull"]);
		}

		public void TestView_PercentageFull_CommittedToPick()
		{
			var maxStock = 250m;
			var availableStock = 50m;
			var committedStock = 10m;
			var expectedPercentage = 20m;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;

			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, maxStock, 10m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, availableStock, fixLocation,
				"");
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, committedStock);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var result = LoadOrderedDataFromView();
			AssertEquals(expectedPercentage, result[0]["WPV_PercentageFull"]);
		}

		public void TestView_PercentageFull_InTransit()
		{
			var maxStock = 250m;
			var availableStock = 50m;
			var inTransitStock = 10m;
			var expectedPercentage = 20m;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);

			var otherLocation = data.Whs1.FindLocation("A-2");
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;

			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, maxStock, 10m);
			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, availableStock,
				fixLocation, "");
			var receive2 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, inTransitStock,
				otherLocation, "");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1");
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, inTransitStock,
				otherLocation.WLV_LocationString, fixLocation.WLV_LocationString);
			transferLine.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();
			AssertEquals("Precondition: Created In-Transit inventory.", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var result = LoadOrderedDataFromView();
			AssertEquals(expectedPercentage, result[0]["WPV_PercentageFull"]);
		}

		public void TestView_PercentageFull_UnfinalizedReceive()
		{
			var maxStock = 250m;
			var availableStock = 50m;
			var expectedPercentage = 20m;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var fixLocationType = Helper.CreateLocationType("FIX", "FixLocation", false, 2, LocationClasses.Codes.FIX);
			var fixLocation = data.Whs1.FindLocation("A-1");
			fixLocation.WLV_WLT_LocationType = fixLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(WhsProduct.GetWhsProduct(data.Part1), data.Org1, fixLocation,
				50m, maxStock, 10m);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, availableStock, fixLocation,
				"");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, availableStock, fixLocation,
				"", finalise: false);

			Factory.Save();

			var result = LoadOrderedDataFromView();
			AssertEquals(expectedPercentage, result[0]["WPV_PercentageFull"]);
		}

		#endregion

		#region Implementation

		DynamicBusinessObjectCollection LoadOrderedDataFromView(string orderByColumn = "WPV_ReplenishMinimum")
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var queryText = string.Format("SELECT * FROM dbo.WhsPickFaceView ORDER BY {0} ASC", orderByColumn);
			result.Load(queryText);
			Assert($"Found two entries with the same value for \"{orderByColumn}\"",
				!result.GroupBy(r => r[orderByColumn]).Any(g => g.IsCountMoreThan(1))); // Ensure sort is determinstic
			return result;
		}

		void AssertRow(DynamicBusinessObject dynamicObject, WhsWarehouse warehouse, WhsPickFace pickFace,
			OrgHeader organisation, OrgSupplierPart supplierPart, WhsLocation location, string column, decimal value)
		{
			AssertEquals("Warehouse: WLV_WW_Whs", warehouse.PK, dynamicObject["WPV_WW_Whs"]);

			AssertEquals("PickFace: WPV_WF", pickFace.PK, dynamicObject["WPV_WF"]);
			AssertEquals("PickFace: WPV_ReplenishMinimum", pickFace.WF_ReplenishMinimum,
				dynamicObject["WPV_ReplenishMinimum"]);
			AssertEquals("PickFace: WPV_ReplenishMaximum", pickFace.WF_ReplenishMaximum,
				dynamicObject["WPV_ReplenishMaximum"]);
			AssertEquals("PickFace: WPV_ReplenishmentMultiple", pickFace.WF_ReplenishmentMultiple,
				dynamicObject["WPV_ReplenishmentMultiple"]);

			AssertEquals("Organisation: WPV_OH", organisation.PK, dynamicObject["WPV_OH"]);

			AssertEquals("Supplier Part: WPV_OP", supplierPart.PK, dynamicObject["WPV_OP"]);

			AssertEquals("Location: WPV_WL", location.PK, dynamicObject["WPV_WL"]);

			AssertEquals(column, value, dynamicObject[column]);
		}

		void AssertUnassignedPickFaceLocation(DynamicBusinessObject dynamicObject, WhsWarehouse warehouse,
			WhsLocation location, ZGuid? clientPK, ZGuid? productPK)
		{
			AssertEquals("Warehouse: WLV_WW_Whs", warehouse.PK, dynamicObject["WPV_WW_Whs"]);
			AssertEquals("PickFace: WPV_WF", ZGuid.Empty, dynamicObject["WPV_WF"]);
			AssertEquals("PickFace: WPV_ReplenishMinimum", 0m, dynamicObject["WPV_ReplenishMinimum"]);
			AssertEquals("PickFace: WPV_ReplenishMaximum", 0m, dynamicObject["WPV_ReplenishMaximum"]);
			AssertEquals("PickFace: WPV_ReplenishmentMultiple", 0m, dynamicObject["WPV_ReplenishmentMultiple"]);
			AssertEquals("Organisation: WPV_OH", clientPK ?? ZGuid.Empty, dynamicObject["WPV_OH"]);
			AssertEquals("Supplier Part: WPV_OP", productPK ?? ZGuid.Empty, dynamicObject["WPV_OP"]);
			AssertEquals("Location: WPV_WL", location.PK, dynamicObject["WPV_WL"]);
		}

		void AssertTotals(DynamicBusinessObject dynamicObject, decimal totalQuantity, decimal incoming = 0,
			decimal committed = 0, decimal availableToPick = 0)
		{
			AssertEquals("WPV_TotalQuantity: ", totalQuantity, dynamicObject["WPV_TotalQuantity"]);
			AssertEquals("WPV_Incoming: ", incoming, dynamicObject["WPV_Incoming"]);
			AssertEquals("WPV_Committed: ", committed, dynamicObject["WPV_Committed"]);
			AssertEquals("WPV_AvailableToPick: ", availableToPick, dynamicObject["WPV_AvailableToPick"]);
		}

		#endregion
	}
}
