using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsRMAOrderLineLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestWarehouses

		public void TestWarehouses()
		{
			var lookups = GetNewLookups();
			AssertNotNull(lookups.Warehouses);
			AssertType(ObjectFactory.GetType<IWhsWarehouseCollection>(), lookups.Warehouses);
		}

		public void TestWarehouses_WarehouseCollectionType()
		{
			AssertEquals(WarehouseCollectionType.ProductWarehouse, GetNewLookups().Warehouses.WarehouseCollectionType);
		}
		#endregion

		#region Implementation

		WhsRMAOrderLineLookups GetNewLookups()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var rmaOrderLine = WhsRMAOrderLine.GetWhsRMAOrderLine(receive.Lines[0], order, 10m, 10m);
			return new WhsRMAOrderLineLookups(rmaOrderLine);
		}

		#endregion
	}
}
