using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsOrderLineValidationTestUS : WhsOrderLineValidationTest
	{
		#region TestCheckWE_TransactionQuantity

		#region TestCheckWE_TransactionQuantity_DivisableByPerPackageQty

		public void TestCheckWE_TransactionQuantity_DivisableByPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			var location = data.Whs1.DefaultLocation;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var expectedErrorMessage = "Units ordered must be divisible by the sum of Per Group Quantities of all matching inventory from the package.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location.PK, "KEY-1", "123", 5m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8m, location.PK, "KEY-1", "123", 4m);
			var inventory3 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 6m, location.PK, "KEY-1", "123", 3m);
			inventory1.WI_PartAttrib1 = "Blue";
			inventory2.WI_PartAttrib1 = "Red";
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			// no Package Group ID
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "KEY-1", "", "");
			AssertNoError("Order line with no package group ID, should not be validated against any Package Group.", orderLine.WE_TransactionQuantityInfo, expectedErrorMessage);

			// with Package Group ID
			orderLine.WE_PackageGroupId = "123";
			orderLine.WE_TransactionQuantity = 10m;
			AssertHasError("Order line without any attributes should be validated against all inventories in the package with matching product.", orderLine.WE_TransactionQuantityInfo, expectedErrorMessage);

			orderLine.WE_TransactionQuantity = 9m;
			AssertNoError("Order line without any attributes should be validated against all inventories in the package with matching product.", orderLine.WE_TransactionQuantityInfo, expectedErrorMessage);

			orderLine.WE_PartAttrib1 = "Blue";
			orderLine.WE_TransactionQuantity = 9m;
			AssertHasError("Order line with attributes should be validated against all inventories with same attributes.", orderLine.WE_TransactionQuantityInfo, expectedErrorMessage);

			orderLine.WE_TransactionQuantity = 15m;
			AssertNoError("Order line with attributes should be validated against all inventories with same attributes.", orderLine.WE_TransactionQuantityInfo, expectedErrorMessage);
		}

		#endregion

		#region TestCheckWE_TransactionQuantity_OrderCanBeFulfilled

		public void TestCheckWE_TransactionQuantity_OrderCanBeFulfilled()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var location = data.Whs1.DefaultLocation;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var expectedErrorMessage = "Total Package Count inconsistent for Package Group ID. Check the Quantity ordered.";
			var expectedNotExistingItemMessage = "This order line does not match any inventory packed into Package Group ID '123'.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location.PK, "KEY-1", "123", 5m);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 6m, location.PK, "KEY-1", "123", 3m);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", WhsPickOption.Codes.Manual);
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 10m, "KEY-1", "DummyOutward-1", "123");
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part2, 3m, "KEY-1", "DummyOutward-1", "123");
			orderLine2.Validation.ValidateWE_TransactionQuantity();
			AssertHasError(orderLine2.WE_TransactionQuantityInfo, expectedErrorMessage);

			orderLine2.WE_TransactionQuantity = 6m;
			AssertNoError(orderLine2.WE_TransactionQuantityInfo, expectedErrorMessage);

			orderLine2.WE_TransactionQuantity = 9m;
			AssertHasError(orderLine2.WE_TransactionQuantityInfo, expectedErrorMessage);

			// without package group ID
			orderLine2.WE_PackageGroupId = "";
			orderLine2.WE_TransactionQuantity = 4m;
			orderLine1.Validation.ValidateWE_TransactionQuantity();
			AssertHasError(orderLine1.WE_TransactionQuantityInfo, expectedErrorMessage);

			orderLine2.WE_TransactionQuantity = 6m;
			orderLine1.Validation.ValidateWE_TransactionQuantity();
			AssertHasError(orderLine1.WE_TransactionQuantityInfo, expectedErrorMessage);

			// ordering item that is not part of package group 123
			orderLine2.WE_PackageGroupId = "123";
			orderLine1.Validation.ValidateWE_TransactionQuantity();
			AssertNoError(orderLine1.WE_TransactionQuantityInfo, expectedErrorMessage);

			var part3 = Helper.CreateProduct(data.Org1, "P3");
			orderLine2.WE_OP = part3.PK;
			orderLine1.Validation.ValidateWE_TransactionQuantity();
			orderLine2.Validation.ValidateWE_TransactionQuantity();
			AssertHasError(orderLine1.WE_TransactionQuantityInfo, expectedErrorMessage);
			AssertHasError(orderLine2.WE_TransactionQuantityInfo, expectedNotExistingItemMessage);
		}

		#endregion

		#endregion

		#region TestCheckWE_PackageGroupID_OrderingNotExistingPackageGroupdID

		public void TestCheckWE_PackageGroupID_OrderingNotExistingPackageGroupdID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var bondedArea = Helper.CreateArea(data.Whs1, "BOND", AreaTypes.Codes.Bonded);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var location = data.Whs1.DefaultLocation;
			location.WLV_WA_PutawayArea = bondedArea.PK;
			Factory.Save();

			var expectedErrorMessage = "No package with Package Group ID '{0}' exists in the warehouse.";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			receive.WD_DocketSubType = ReceiveType.Codes.Customs;
			var inventory1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location.PK, "KEY-1", "123", 5m);
			var inventory2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 6m, location.PK, "KEY-1", "123", 3m);
			receive.FinaliseDocket();
			Factory.Save();
			AssertIsFinalisedPrecondition(receive);

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order1Line1 = Helper.CreateWhsOrderLine(order1, data.Part1, 10m, "KEY-1", "DummyOutward-1", "456");
			AssertHasError(order1Line1.WE_PackageGroupIdInfo, string.Format(expectedErrorMessage, "456"));

			order1Line1.WE_PackageGroupId = "123";
			AssertNoError(order1Line1.WE_PackageGroupIdInfo, string.Format(expectedErrorMessage, "123"));

			// order in different Whs - error
			var whs2 = Helper.CreateWarehouse("WH2");
			Helper.CreateArea(whs2, "BOND", AreaTypes.Codes.Bonded);
			whs2.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			var order2 = Helper.CreateWhsOrder(data.Org1, whs2, "O2");
			var order2Line1 = Helper.CreateWhsOrderLine(order2, data.Part1, 10m, "KEY-1", "DummyOutward-1", "123");
			AssertHasError(order2Line1.WE_PackageGroupIdInfo, string.Format(expectedErrorMessage, "123"));

			// taking out all stock
			var order1Line2 = Helper.CreateWhsOrderLine(order1, data.Part2, 6m, "KEY-1", "DummyOutward-1", "123");
			var pick = Helper.CreatePickNew(order1);
			pick.RunPreSaveValidation(); // pickability status check for errors before running validation...
			pick.FinaliseAllOrders();
			pick.FinalisePick();
			Factory.Save();
			AssertIsFinalisedPrecondition(order1);
			AssertIsFinalisedPrecondition(pick);
			AssertEquals("Precondition", 0m, inventory1.WI_TotalUnits);
			AssertEquals("Precondition", 0m, inventory2.WI_TotalUnits);

			// ordering taked out stock - error
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3");
			var order3Line1 = Helper.CreateWhsOrderLine(order3, data.Part1, 10m, "KEY-1", "DummyOutward-1", "123");
			AssertHasError(order3Line1.WE_PackageGroupIdInfo, string.Format(expectedErrorMessage, "123"));
		}

		#endregion
	}
}
