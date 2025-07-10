using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Environment.CodeLists.US;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.US.Testing
{
	class WhsReceiveLineValidationUSTest : WhsReceiveLineValidationTest
	{
		#region TestCheckWE_TransactionQuantity_WithPerPackageQty

		public void TestCheckWE_TransactionQuantity_WithPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();
			var receiveLine = receive.Lines[0];
			receiveLine.WE_PerPackageQty = 5m;

			AssertNoErrors("Precondition", receiveLine.WE_TransactionQuantityInfo);

			var expectedErrorMessage = "Units received must be divisible by Per Group Quantity.";
			receiveLine.WE_TransactionQuantity = 8m;
			AssertHasError(receiveLine.WE_TransactionQuantityInfo, expectedErrorMessage);

			receiveLine.WE_TransactionQuantity = 15m;
			AssertNoErrors(receiveLine.WE_TransactionQuantityInfo);

			receiveLine.WE_TransactionQuantity = 13m;
			AssertHasError(receiveLine.WE_TransactionQuantityInfo, expectedErrorMessage);

			receiveLine.WE_PerPackageQty = 0m;
			AssertNoErrors(receiveLine.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID

		public void TestCheckWE_TransactionQuantity_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m);
			Factory.Save();

			var receiveLine1 = inventory1.InDocketLine;
			var receiveLine2 = inventory2.InDocketLine;
			var receiveLine3 = inventory3.InDocketLine;
			receiveLine1.WE_PartAttrib1 = "Red";
			receiveLine2.WE_PartAttrib1 = "Red";
			receiveLine3.WE_PartAttrib1 = "Red";
			receiveLine1.WE_PerPackageQty = 2m;
			receiveLine2.WE_PerPackageQty = 5m;
			AssertNoErrors("Precondition", receiveLine1.WE_TransactionQuantityInfo);
			AssertNoErrors("Precondition", receiveLine2.WE_TransactionQuantityInfo);

			var expectedErrorMessage = "Total Package Count inconsistent for Package Group ID within location. Check the Quantity and Per Group Quantity.";
			receiveLine1.WE_PackageGroupId = "123";
			AssertNoErrors(receiveLine1.WE_TransactionQuantityInfo);

			// inventory 1 has 10 / 2 = 5 packs
			// inventory 2 has 10 / 5 = 2 packs
			receiveLine2.WE_PackageGroupId = "123";
			AssertHasError(receiveLine2.WE_TransactionQuantityInfo, expectedErrorMessage);

			// only one inventory line with Package Group ID '123' should be no error
			receiveLine2.WE_PackageGroupId = "";
			AssertNoErrors(receiveLine2.WE_TransactionQuantityInfo);

			// no package groups
			receiveLine1.WE_PackageGroupId = "";
			AssertNoErrors(receiveLine1.WE_TransactionQuantityInfo);

			// only one inventory line with Package Group ID '123'
			receiveLine1.WE_PackageGroupId = "123";
			AssertNoErrors("Precondition", receiveLine2.WE_TransactionQuantityInfo);

			// inventory 1 has (10 / 2) = 5 packs
			// inventory 2 has (10 / 5) = 2 packs
			receiveLine2.WE_PackageGroupId = "123";
			AssertHasError(receiveLine2.WE_TransactionQuantityInfo, expectedErrorMessage);

			// inventory 1 has (10 / 2) = 5 packs
			// inventory 2 has (10 / 2) = 5 packs
			receiveLine2.WE_PerPackageQty = 2m;
			AssertNoErrors(receiveLine2.WE_TransactionQuantityInfo);

			// inventory 1 has (10 / 2) = 5 packs
			// inventory 2 has (10 / 5) = 2 packs
			receiveLine2.WE_PerPackageQty = 5m;
			AssertHasError(receiveLine2.WE_TransactionQuantityInfo, expectedErrorMessage);

			// inventory 1 has (10 / 2) = 5 packs
			// inventory 2 has (25 / 5) = 5 packs
			receiveLine2.WE_TransactionQuantity = 25m;
			AssertNoErrors(receiveLine2.WE_TransactionQuantityInfo);

			// inventory 1 + inventory 3 has (10 + 4 / 2) = 7 packs
			// inventory 2 has (25 / 5) = 5 packs
			receiveLine3.WE_PerPackageQty = 2m;
			receiveLine3.WE_PackageGroupId = "123";
			AssertNoErrors(receiveLine3.WE_PerPackageQtyInfo);
			AssertHasError(receiveLine3.WE_TransactionQuantityInfo, expectedErrorMessage);

			// inventory 1 + inventory 3 has ([10 + 4] / 2) = 7 packs
			// inventory 2 has (35 / 5) = 7 packs
			receiveLine2.WE_TransactionQuantity = 35m;
			receiveLine3.Validation.ValidateWE_TransactionQuantity();
			AssertNoErrors(receiveLine3.WE_TransactionQuantityInfo);

			// inventory 1 has (10 / 2) = 5 packs
			// inventory 2 has (25 / 5) = 5 packs
			// inventory 3 has (4 / 2) = 2 packs
			receiveLine3.WE_PartAttrib1 = "Green";
			receiveLine2.WE_TransactionQuantity = 25m;
			receiveLine3.Validation.ValidateWE_TransactionQuantity();
			AssertHasError(receiveLine3.WE_TransactionQuantityInfo, expectedErrorMessage);

			// inventory 1 has (10 / 2) = 5 packs
			// inventory 2 has (25 / 5) = 5 packs
			// inventory 3 has (10 / 2) = 5 packs
			receiveLine3.WE_TransactionQuantity = 10m;
			AssertNoErrors(receiveLine3.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID_AllocationKey

		public void TestCheckWE_TransactionQuantity_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupID_AllocationKey()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4m);
			Factory.Save();

			var receiveLine1 = inventory1.InDocketLine;
			var receiveLine2 = inventory2.InDocketLine;
			var receiveLine3 = inventory3.InDocketLine;
			receiveLine1.WE_AllocationKey = "Red";
			receiveLine2.WE_AllocationKey = "Red";
			receiveLine3.WE_AllocationKey = "Red";
			receiveLine1.WE_PerPackageQty = 2m;
			receiveLine2.WE_PerPackageQty = 5m;
			AssertNoErrors("Precondition", receiveLine1.WE_TransactionQuantityInfo);
			AssertNoErrors("Precondition", receiveLine2.WE_TransactionQuantityInfo);

			var expectedErrorMessage = "Total Package Count inconsistent for Package Group ID within location. Check the Quantity and Per Group Quantity.";
			receiveLine1.WE_PackageGroupId = "123";
			AssertNoErrors(receiveLine1.WE_TransactionQuantityInfo);

			// inventory 1 has 10 / 2 = 5 packs
			// inventory 2 has 10 / 5 = 2 packs
			receiveLine2.WE_PackageGroupId = "123";
			AssertHasError(receiveLine2.WE_TransactionQuantityInfo, expectedErrorMessage);

			// only one inventory line with Package Group ID '123' should be no error
			receiveLine2.WE_PackageGroupId = "";
			AssertNoErrors(receiveLine2.WE_TransactionQuantityInfo);

			// no package groups
			receiveLine1.WE_PackageGroupId = "";
			AssertNoErrors(receiveLine1.WE_TransactionQuantityInfo);

			// only one inventory line with Package Group ID '123'
			receiveLine1.WE_PackageGroupId = "123";
			AssertNoErrors("Precondition", receiveLine2.WE_TransactionQuantityInfo);

			// inventory 1 has (10 / 2) = 5 packs
			// inventory 2 has (10 / 5) = 2 packs
			receiveLine2.WE_PackageGroupId = "123";
			AssertHasError(receiveLine2.WE_TransactionQuantityInfo, expectedErrorMessage);

			// inventory 1 has (10 / 2) = 5 packs
			// inventory 2 has (10 / 2) = 5 packs
			receiveLine2.WE_PerPackageQty = 2m;
			AssertNoErrors(receiveLine2.WE_TransactionQuantityInfo);

			// inventory 1 has (10 / 2) = 5 packs
			// inventory 2 has (10 / 5) = 2 packs
			receiveLine2.WE_PerPackageQty = 5m;
			AssertHasError(receiveLine2.WE_TransactionQuantityInfo, expectedErrorMessage);

			// inventory 1 has (10 / 2) = 5 packs
			// inventory 2 has (25 / 5) = 5 packs
			receiveLine2.WE_TransactionQuantity = 25m;
			AssertNoErrors(receiveLine2.WE_TransactionQuantityInfo);

			// inventory 1 + inventory 3 has (10 + 4 / 2) = 7 packs
			// inventory 2 has (25 / 5) = 5 packs
			receiveLine3.WE_PerPackageQty = 2m;
			receiveLine3.WE_PackageGroupId = "123";
			AssertNoErrors(receiveLine3.WE_PerPackageQtyInfo);
			AssertHasError(receiveLine3.WE_TransactionQuantityInfo, expectedErrorMessage);

			// inventory 1 + inventory 3 has ([10 + 4] / 2) = 7 packs
			// inventory 2 has (35 / 5) = 7 packs
			receiveLine2.WE_TransactionQuantity = 35m;
			receiveLine3.Validation.ValidateWE_TransactionQuantity();
			AssertNoErrors(receiveLine3.WE_TransactionQuantityInfo);

			// inventory 1 has (10 / 2) = 5 packs
			// inventory 2 has (25 / 5) = 5 packs
			// inventory 3 has (4 / 2) = 2 packs
			receiveLine3.WE_AllocationKey = "Green";
			receiveLine2.WE_TransactionQuantity = 25m;
			receiveLine3.Validation.ValidateWE_TransactionQuantity();
			AssertHasError(receiveLine3.WE_TransactionQuantityInfo, expectedErrorMessage);

			// inventory 1 has (10 / 2) = 5 packs
			// inventory 2 has (25 / 5) = 5 packs
			// inventory 3 has (10 / 2) = 5 packs
			receiveLine3.WE_TransactionQuantity = 10m;
			AssertNoErrors(receiveLine3.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation

		public void TestCheckWE_TransactionQuantity_MustHaveTotalPackagesEqualForAllLinesWithSamePackageGroupIDPerLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = "CUS";
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m, location1.PK, "", "ABC", 5m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1.PK, "", "ABC", 5m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, location1.PK, "", "ABC", 5m);
			var inventory4 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, location1.PK, "", "ABC", 5m);
			Factory.Save();

			var receiveLine1 = inventory1.InDocketLine;
			var receiveLine2 = inventory2.InDocketLine;
			var receiveLine3 = inventory3.InDocketLine;
			var receiveLine4 = inventory4.InDocketLine;

			receiveLine1.Validation.ValidateWE_TransactionQuantity();
			AssertNoErrors(receiveLine1.WE_TransactionQuantityInfo);

			receiveLine2.WE_TransactionQuantity = 30m;
			receiveLine1.WE_WL = location2.PK;
			receiveLine1.Validation.ValidateWE_TransactionQuantity();
			AssertHasError(receiveLine1.WE_TransactionQuantityInfo, "Total Package Count inconsistent for Package Group ID within location. Check the Quantity and Per Group Quantity.");
			receiveLine2.WE_TransactionQuantity = 10m; // clean up

			receiveLine4.WE_WL = location2.PK;
			receiveLine1.Validation.ValidateWE_TransactionQuantity();
			AssertHasError(receiveLine1.WE_TransactionQuantityInfo, "Total Package Count inconsistent for Package Group ID within location. Check the Quantity and Per Group Quantity.");
			receiveLine4.WE_WL = location1.PK; // clean up

			receiveLine3.WE_WL = location2.PK;
			receiveLine1.Validation.ValidateWE_TransactionQuantity();
			AssertNoErrors(receiveLine1.WE_TransactionQuantityInfo);
		}

		#endregion

		#region TestCheckPackageGroupID_IsUniqueAcrossCurrentStock

		public void TestCheckPackageGroupID_IsUniqueAcrossCurrentStock()
		{
			TestCheckPackageGroupID_IsUniqueAcrossCurrentStockCore(isInTransit: false);
		}

		public void TestCheckPackageGroupID_IsUniqueAcrossCurrentStock_InTransit()
		{
			TestCheckPackageGroupID_IsUniqueAcrossCurrentStockCore(isInTransit: true);
		}

		void TestCheckPackageGroupID_IsUniqueAcrossCurrentStockCore(bool isInTransit)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var location = data.Whs1.DefaultLocation;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive1.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, "123-1", "ABC", 2m).WI_WL = location.PK;
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, "123-2", "XYZ", 2m).WI_WL = location.PK;
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 10m, "123-3").WI_WL = location.PK;
			receive1.AllocateLocationsWithMock();
			receive1.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive1.IsFinalised);
			Factory.Save();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order1, data.Part2, 10m, "123-2", "DummyOutward-1", "");
			orderLine1.WE_PackageGroupId = "XYZ";
			orderLine1.WE_PerPackageQty = 2m;
			var pick1 = Helper.CreatePickNew(true, true, order1);
			Factory.Save();

			if (isInTransit)
			{
				var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2");
				var orderLine2 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m, "123-1", "DummyOutward-2", "");
				orderLine2.WE_PackageGroupId = "ABC";
				orderLine2.WE_PerPackageQty = 2m;
				var pick2 = Helper.CreatePickNew(order2);
				var pickLine = pick2.GetAllPickLines().Single();
				Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Today);
				Factory.Save();
			}

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventoryThatCanFinalise1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, "568-1", "XYZ", 5m);
			var inventoryThatCanFinalise2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, "568-2", "YYY", 10m);
			var inventoryThatCanFinalise3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, "568-2", "YYY", 10m);
			var inventoryThatCanFinalise4 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, "568-3");
			var inventoryThatCannotFinalise = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, "568-4", "ABC", 1m);
			Factory.Save();

			receive2.AllocateLocationsWithMock();
			AssertNoErrors("Precondition", inventoryThatCannotFinalise.InDocketLine.WE_PackageGroupIdInfo);

			receive2.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(false, receive2.IsFinalised);
			AssertNoErrors(inventoryThatCanFinalise1.InDocketLine.WE_PackageGroupIdInfo);
			AssertNoErrors(inventoryThatCanFinalise2.InDocketLine.WE_PackageGroupIdInfo);
			AssertNoErrors(inventoryThatCanFinalise3.InDocketLine.WE_PackageGroupIdInfo);
			AssertNoErrors(inventoryThatCanFinalise4.InDocketLine.WE_PackageGroupIdInfo);
			AssertHasError(inventoryThatCannotFinalise.InDocketLine.WE_PackageGroupIdInfo, "Package Group ID previously assigned. Assign new unique Package Group ID.");
		}

		public void TestCheckPackageGroupID_Cached()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var location = data.Whs1.DefaultLocation;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-1", "ABC", 2m).WI_WL = location.PK;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, "123-2", "XYZ", 2m).WI_WL = location.PK;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventoryThatCanFinalise1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, "568-1", "ZZZ", 5m);
			var inventoryThatCanFinalise2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, "568-2", "YYY", 10m);
			var inventoryThatCanFinalise3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, "568-2", "YYY", 10m);
			receive2.AllocateLocationsWithMock();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receive2InNewFactory = newFactory.Load<WhsReceive>(receive2.PK);

			using (TestConnection.TrackExecutedCommands())
			using (RowFactory.SetCachedTables())
			{
				var receiveLine = receive2InNewFactory.Lines.Single(line => line.WE_PackageGroupId == "ZZZ");
				receiveLine.WE_PackageGroupId = "ABC";

				receive2InNewFactory.FinaliseDocketWithoutUserConfirmation();
				AssertEquals("Should have only hit the DB once during validation.", 1, TestConnection.ExecutedCommands.Count(c => c.Contains("AND WE_PackageGroupId IN (SELECT Value FROM @PackageGroupIds)")));
				AssertEquals("Should have hit the DB once during lines load.", 1, newFactory.TableSelects.Single(t => t.TableName == WhsDocketLineSchema.Constants.TableName).Value);
				AssertHasError(receiveLine.WE_PackageGroupIdInfo, "Package Group ID previously assigned. Assign new unique Package Group ID.");
				Assert(!receive2InNewFactory.IsFinalised);

				receiveLine.WE_PackageGroupId = "ZZZ";
				receive2InNewFactory.FinaliseDocketWithoutUserConfirmation();
				AssertEquals("Should have hit the DB again during validation.", 2, TestConnection.ExecutedCommands.Count(c => c.Contains("AND WE_PackageGroupId IN (SELECT Value FROM @PackageGroupIds)")));
				AssertEquals("Should have hit the DB once during lines load.", 1, newFactory.TableSelects.Single(t => t.TableName == WhsDocketLineSchema.Constants.TableName).Value);
				AssertNoErrors(receiveLine.WE_PackageGroupIdInfo);
				Assert(receive2InNewFactory.IsFinalised);
			}
		}

		public void TestCheckPackageGroupID_CaseInsensitive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var location = data.Whs1.DefaultLocation;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-1", "ABC", 2m).WI_WL = location.PK;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m, "123-2", "XYZ", 2m).WI_WL = location.PK;
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals(true, receive.IsFinalised);
			Factory.Save();

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			receive2.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventoryThatCanFinalise1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m, "568-1", "ZZZ", 5m);
			var inventoryThatCanFinalise2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, "568-2", "YYY", 10m);
			var inventoryThatCanFinalise3 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 10m, "568-2", "YYY", 10m);
			receive2.AllocateLocationsWithMock();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var receive2InNewFactory = newFactory.Load<WhsReceive>(receive2.PK);

			var receiveLine = receive2InNewFactory.Lines.Single(line => line.WE_PackageGroupId == "ZZZ");
			receiveLine.WE_PackageGroupId = "abc";

			receive2InNewFactory.FinaliseDocketWithoutUserConfirmation();
			AssertHasError(receiveLine.WE_PackageGroupIdInfo, "Package Group ID previously assigned. Assign new unique Package Group ID.");
			Assert(!receive2InNewFactory.IsFinalised);

			receiveLine.WE_PackageGroupId = "zzz";
			receive2InNewFactory.FinaliseDocketWithoutUserConfirmation();
			AssertNoErrors(receiveLine.WE_PackageGroupIdInfo);
			Assert(receive2InNewFactory.IsFinalised);
		}

		#endregion

		#region TestCheckPackageGroupID_UniquenessNotCheckedOnReceiveLinesWithNoSOH

		public void TestCheckPackageGroupID_UniquenessNotCheckedOnReceiveLinesWithNoSOH()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var dockdoor = data.Whs1.DefaultOutboundDockDoorLocation;
			var location = data.Whs1.DefaultLocation;
			var bondedArea = data.Whs1.Areas.Single(a => a.WA_AreaType == AreaTypes.Codes.Bonded);
			location.WLV_WA_PutawayArea = bondedArea.PK;
			location.WLV_WA_PickingArea = bondedArea.PK;
			dockdoor.WLV_WA_PickingArea = bondedArea.PK;
			dockdoor.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, dockdoor, "PLT-123");
			receiveLine.WE_PerPackageQty = 10m;
			receiveLine.WE_PackageGroupId = "ABC123";
			receiveLine.CustomsData.WB_EntryKey = "EntryKey";
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			transfer.WD_IsPutawayTransfer = true;
			var transferLine = Helper.SetupTransferLineForDockDoorLocation(transfer, data.Part1, dockdoor, location, "PLT-123", 10m);
			transferLine.WE_PerPackageQty = 10m;
			transferLine.WE_PackageGroupId = "ABC123";
			transferLine.CustomsData.WB_EntryKey = "EntryKey";
			transfer.RunPreSaveValidation();
			transferLine.PickedTime = ZDateTimeOffset.Now;
			Factory.Save();

			transfer.FinaliseDocket();
			AssertEquals("Transfer must be finalised.", true, transfer.IsFinalised);

			receive.FinaliseDocket();
			AssertNoErrors(receiveLine);
			AssertEquals("Receive must be finalised.", true, receive.IsFinalised);
		}

		#endregion

		#region TestCheckPerPackageQty

		public void TestCheckPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();
			var receiveLine = inventory.InDocketLine;
			AssertNoErrors("Precondition", receiveLine.WE_PerPackageQtyInfo);

			var expectedErrorMessage = "Units received must be divisible by Per Group Quantity.";
			receiveLine.WE_PerPackageQty = 4m;
			AssertHasError(receiveLine.WE_PerPackageQtyInfo, expectedErrorMessage);

			receiveLine.WE_PerPackageQty = 5m;
			AssertNoError(receiveLine.WE_PerPackageQtyInfo, expectedErrorMessage);

			receiveLine.WE_PerPackageQty = 2.5m;
			AssertNoError(receiveLine.WE_PerPackageQtyInfo, expectedErrorMessage);

			receiveLine.WE_PerPackageQty = 3m;
			AssertHasError(receiveLine.WE_PerPackageQtyInfo, expectedErrorMessage);

			receiveLine.WE_TransactionQuantity = 9m;
			AssertNoError(receiveLine.WE_PerPackageQtyInfo, expectedErrorMessage);

			receiveLine.WE_PerPackageQty = 0m;
			AssertNoError(receiveLine.WE_PerPackageQtyInfo, expectedErrorMessage);

			receiveLine.WE_PerPackageQty = -10m;
			AssertHasError(receiveLine.WE_PerPackageQtyInfo, "Per Group Qty cannot be negative.");
			AssertNoError(receiveLine.WE_PerPackageQtyInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckPerPackageQty_SameProductInSamePackageMustHaveSamePerPackageQty

		public void TestCheckPerPackageQty_SameProductInSamePackageMustHaveSamePerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			receive.WD_DocketSubType = "CUS";
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "444-1", "123", 2m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "444-2", "456", 5m);
			Factory.Save();
			var receiveLine1 = inventory1.InDocketLine;
			var receiveLine2 = inventory2.InDocketLine;
			receiveLine1.WE_PartAttrib1 = "Red";
			receiveLine2.WE_PartAttrib1 = "Red";
			AssertNoErrors("Precondition", receiveLine1.WE_PerPackageQtyInfo);
			AssertNoErrors("Precondition", receiveLine2.WE_PerPackageQtyInfo);

			var expectedErrorMessage = "Same product with the same Package Group ID must have the same Per Group Quantity.";
			receiveLine2.WE_PackageGroupId = "123";
			AssertHasError(receiveLine2.WE_PerPackageQtyInfo, expectedErrorMessage);

			receiveLine2.WE_PackageGroupId = "456";
			AssertNoErrors(receiveLine2.WE_PerPackageQtyInfo);

			receiveLine2.WE_PackageGroupId = "123";
			AssertHasError(receiveLine2.WE_PerPackageQtyInfo, expectedErrorMessage);

			receiveLine2.WE_PerPackageQty = 2m;
			AssertNoErrors(receiveLine2.WE_PerPackageQtyInfo);

			receiveLine2.WE_PerPackageQty = 5m;
			AssertHasError(receiveLine2.WE_PerPackageQtyInfo, expectedErrorMessage);

			receiveLine2.WE_PartAttrib1 = "Green";
			receiveLine2.Validation.ValidateWE_PerPackageQty();
			AssertNoErrors(receiveLine2.WE_PerPackageQtyInfo);
		}

		#endregion

		#region TestCheckPerPackageQty_PerPackageQtyHasSameDecimalsAsProduct

		public void TestCheckPerPackageQty_PerPackageQtyHasSameDecimalsAsProduct()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			data.Part1.OP_CountDecimalPlaces = 1;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();
			var receiveLine = inventory.InDocketLine;
			AssertNoErrors("Precondition", receiveLine.WE_PerPackageQtyInfo);

			receiveLine.WE_PackageGroupId = "123";
			receiveLine.WE_PerPackageQty = 2.50m;
			AssertNoErrors(receiveLine.WE_PerPackageQtyInfo);

			receiveLine.WE_TransactionQuantity = 9;
			receiveLine.WE_PerPackageQty = 2.250m;
			AssertHasError(receiveLine.WE_PerPackageQtyInfo, "Per Group Quantity must have 1 decimal place(s) as specified on Product 'P1'.");

			data.Part1.OP_CountDecimalPlaces = 2;
			receiveLine.Validation.ValidateWE_PerPackageQty();
			AssertNoErrors(receiveLine.WE_PerPackageQtyInfo);

			data.Part1.OP_CountDecimalPlaces = 0;
			receiveLine.Validation.ValidateWE_PerPackageQty();
			AssertHasError(receiveLine.WE_PerPackageQtyInfo, "Per Group Quantity must have 0 decimal place(s) as specified on Product 'P1'.");
		}

		#endregion

		#region TestCheckPerPackageQty_PackageGroupIDRequiresPerPackageQtyEntered

		public void TestCheckPerPackageQty_PackageGroupIDRequiresPerPackageQtyEntered()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			Factory.Save();
			var receiveLine = inventory.InDocketLine;
			AssertNoErrors("Precondition", receiveLine.WE_PerPackageQtyInfo);

			var expectedErrorMessage = "Per Group Quantity must be specified if Package Group ID is specified.";
			receiveLine.WE_PerPackageQty = 10m;
			AssertNoError(receiveLine.WE_PerPackageQtyInfo, expectedErrorMessage);

			receiveLine.WE_PackageGroupId = "123";
			AssertNoErrors(receiveLine.WE_PerPackageQtyInfo);

			receiveLine.WE_PerPackageQty = 0m;
			AssertHasError(receiveLine.WE_PerPackageQtyInfo, expectedErrorMessage);

			receiveLine.WE_PackageGroupId = "";
			AssertNoErrors(receiveLine.WE_PerPackageQtyInfo);

			receiveLine.WE_PackageGroupId = "123";
			AssertHasError(receiveLine.WE_PerPackageQtyInfo, expectedErrorMessage);

			receiveLine.WE_PerPackageQty = 5m;
			AssertNoErrors(receiveLine.WE_PerPackageQtyInfo);
		}

		#endregion

		#region TestCheckSplitQuantity_MustBeDivisibleByPerPackageQtyAndMustSplitWholePackageGroup

		public void TestCheckSplitQuantity_MustBeDivisibleByPerPackageQtyAndMustSplitWholePackageGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, "123-1", "", 2m);
			Factory.Save();

			var receiveLine = receive.Lines[0];
			receiveLine.WE_PerPackageQty = 2m;
			receiveLine.SplitQuantity = 10m;

			AssertNoErrors("Precondition", receiveLine.SplitQuantityInfo);

			receiveLine.SplitQuantity = 3m;
			AssertHasError(receiveLine.SplitQuantityInfo, "Split Quantity must be divisible by Per Group Quantity.");

			receiveLine.SplitQuantity = 4m;
			AssertNoErrors(receiveLine.SplitQuantityInfo);

			receiveLine.WE_PackageGroupId = "ABC";
			((WhsReceiveLineValidation)receiveLine.Validation).ValidateSplitQuantity();
			AssertHasError(receiveLine.SplitQuantityInfo, "You must Split either all or none of the inventory when in a Package Group.");

			receiveLine.SplitQuantity = 10m;
			AssertNoErrors(receiveLine.SplitQuantityInfo);

			receiveLine.SplitQuantity = 0m;
			AssertNoErrors(receiveLine.SplitQuantityInfo);
		}

		#endregion

		#region TestCheckHeldCodeToChangeTo_CannotChangeStatusIfInventoryIsInPackageGroup

		public void TestCheckHeldCodeToChangeTo_CannotChangeStatusIfInventoryIsInPackageGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation.PK, "123-1", "ABC", 2m);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			AssertNoErrors("Precondition", inventory.InDocketLine.HeldCodeToChangeToInfo);

			inventory.InDocketLine.IsInventoryEditForm = true;
			inventory.InDocketLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			AssertHasError(inventory.InDocketLine.HeldCodeToChangeToInfo, "Cannot change the Hold Code of Inventory in a Package Group.");
		}

		#endregion

		#region TestCheckHeldCodeChangeQuantity_ChangeQuantityMustBeDivisibleByPerPackageQty

		public void TestCheckHeldCodeChangeQuantity_ChangeQuantityMustBeDivisibleByPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation.PK, "123-1", "", 2m);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			AssertNoErrors("Precondition", inventory.InDocketLine.HeldCodeChangeQuantityInfo);

			inventory.InDocketLine.IsInventoryEditForm = true;
			inventory.InDocketLine.HeldCodeChangeQuantity = 3m;
			AssertHasError(inventory.InDocketLine.HeldCodeChangeQuantityInfo, "Quantity must be divisible by Per Group Quantity.");

			inventory.InDocketLine.HeldCodeChangeQuantity = 4m;
			AssertNoErrors(inventory.InDocketLine.HeldCodeChangeQuantityInfo);
		}

		#endregion

		#region TestCheckWE_WL_IsTSAKnown

		public void TestCheckWE_WL_IsKnownByTSA()
		{
			var expectedErrorMessage = "This location is not known by TSA.";
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.SetWarehouseTSAStatus(data.Whs1, TSAStatus.Codes.Known);
			Helper.SetOrgAddressTSAStatus(data.Org1.MainAddress, TSAStatus.Codes.Known);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m).InDocketLine;

			// Client and Warehouse are TSA known but not the location
			var notDockDoorLocation = data.Whs1.DefaultLocation;
			notDockDoorLocation.WLV_ApprovedKnownLocation = TSAStatus.Codes.Unknown;
			receiveLine.WE_WL = notDockDoorLocation.PK;
			AssertEquals("Inventory status is Putaway.", InventoryStatus.Codes.Putaway, receiveLine.WE_OriginalInventoryStatus);
			AssertHasError("There is an error for putaway receive lines.", receiveLine.WE_WLInfo, expectedErrorMessage);

			// Client and Warehouse are TSA known but not the location
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;
			dockDoorLocation.WLV_ApprovedKnownLocation = TSAStatus.Codes.Unknown;
			receiveLine.WE_WL = dockDoorLocation.PK;
			AssertEquals("Inventory status is Received.", InventoryStatus.Codes.Received, receiveLine.WE_OriginalInventoryStatus);
			AssertNoError("There is no error for not putaway receive lines.", receiveLine.WE_WLInfo, expectedErrorMessage);
		}

		#endregion

		#region Implementation

		protected override WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctionsUS(Factory);
		}

		protected new WhsTestHelperFunctionsUS Helper
		{
			get { return (WhsTestHelperFunctionsUS)base.Helper; }
		}

		#endregion
	}
}
