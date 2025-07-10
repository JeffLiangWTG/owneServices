using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.US.Testing
{
	class WhsPickLineValidationUSTest : WhsPickLineValidationTest
	{
		#region TestCheckReservedQuantity_ChecksUnitsIsDivisibleByPerPackageQty

		public void TestCheckReservedQuantity_ChecksUnitsIsDivisibleByPerPackageQty()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "USLAX";
			Helper.EnableWarehouseForBond(data.Whs1, true);
			Helper.RemoveAreasOfTypeFromWarehouse(data.Whs1, "FRE");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, data.Whs1.DefaultLocation.PK, "123-1", "", 2m);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var reservedPickLine = orderLine.ReserveStockIfAbleTo(inventory);
			AssertNoErrors("Precondition", reservedPickLine.ReservedQuantityInfo);

			reservedPickLine.ReservedQuantity = 3m;
			AssertHasError(reservedPickLine.ReservedQuantityInfo, "Reserved Quantity must be divisible by Per Group Qty.");

			reservedPickLine.ReservedQuantity = 4m;
			AssertNoErrors(reservedPickLine.ReservedQuantityInfo);
		}

		#endregion
	}
}
