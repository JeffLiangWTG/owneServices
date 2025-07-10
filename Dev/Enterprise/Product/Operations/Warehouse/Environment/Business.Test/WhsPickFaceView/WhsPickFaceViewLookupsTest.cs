using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	internal class WhsPickFaceViewLookupsTest : WhsBusinessObjectLookupsTestCase
	{
		public void TestClients()
		{
			var pickFaceView = Factory.New<WhsPickFaceView>();
			AssertType<WarehouseClientCollectionWithSecurityCheck>(pickFaceView.Lookups.Clients);
		}

		public void TestParts()
		{
			var pickFaceView = Factory.New<WhsPickFaceView>();
			AssertType<OrgSupplierPartCollection>(pickFaceView.Lookups.Parts);
		}

		public void TestWarehouses()
		{
			var pickFaceView = Factory.New<WhsPickFaceView>();
			AssertType<WhsWarehouseCollectionWithSecurityCheck>(pickFaceView.Lookups.Warehouses);
		}

		public void TestLocations()
		{
			var pickFaceView = Factory.New<WhsPickFaceView>();
			AssertType<WhsLocationCollection>(pickFaceView.Lookups.Locations);
		}
	}
}
