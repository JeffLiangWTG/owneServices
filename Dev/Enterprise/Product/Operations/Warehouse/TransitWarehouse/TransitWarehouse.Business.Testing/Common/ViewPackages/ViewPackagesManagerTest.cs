using Enterprise.Warehouse.Integration.CodeLists;

namespace Enterprise.Warehouse.Transit.Business.Testing.Common.ViewPackages
{
	class ViewPackagesManagerTest : WhsTransitTestCaseWithFactory
	{
		public void TestProperties()
		{
			var dummyParent = Factory.New<DummyBizOWithPackLines>();
			var transitWarehouse = Helper.CreateTRWWarehouse("TW1");
			var productWarehouse = Helper.CreateTRWWarehouse("PRW");
			productWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;

			var manager = new ViewPackagesManager(Factory, dummyParent);
			var packageState1 = Factory.New<WhsItemPackageState>();
			var packageState2 = Factory.New<WhsItemPackageState>();
			AssertContainsExactElementsInAnyOrder(new[] { packageState1, packageState2 }, manager.AllPackages);

			AssertEquals(dummyParent, manager.Parent);
			manager.SetCurrentWarehousePK(transitWarehouse.PK);
			AssertEquals(transitWarehouse, manager.Warehouse);
		}
	}
}
