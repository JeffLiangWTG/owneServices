using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsPickFaceView))]
	class WhsPickFaceViewTestCase : WhsEnvBusinessObjectTestCase
	{
		public void TestCanDelete()
		{
			AssertEquals("Not allowed to delete PickFaceView", false, GetNewBusinessObject().CanDelete);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var data = new PickFaceViewTestData(Factory);
			var pf = new WhsPickFaceCollection(data.Parts[0], Factory).AddNew();
			pf.WF_WL = data.Locations[data.Warehouses[0]][0].PK;
			pf.WF_OH_Client = data.Clients[0].PK;
			pf.WF_ReplenishMinimum = 1m;
			pf.WF_ReplenishMaximum = 2m;
			pf.WF_ReplenishmentMultiple = 1m;
			Factory.Save();
			var result = Factory.LoadTop1<WhsPickFaceView>(new ZQuery());
			AssertNotNull("Precondition:", result);
			return result;
		}

		protected override bool IsDeleteSupported()
		{
			return false;
		}

		public void TestHumanReadableName()
		{
			var pickFaceView = Factory.New<WhsPickFaceView>();
			AssertEquals("Pick Face", pickFaceView.HumanReadableName);
		}

		public void TestWPV_OH()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(WhsPickFaceView), WhsPickFaceViewSchema.Constants.WPV_OH, false,
				la => la.ListDataSourceMember == "Lookups.Clients");
		}

		public void TestWPV_OP()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(WhsPickFaceView), WhsPickFaceViewSchema.Constants.WPV_OP, false,
				la => la.ListDataSourceMember == "Lookups.Parts");
		}

		public void TestWPV_WL()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(WhsPickFaceView), WhsPickFaceViewSchema.Constants.WPV_WL, false,
				la => la.ListDataSourceMember == "Lookups.Locations");
		}

		public void TestWPV_WW_Whs()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(WhsPickFaceView), WhsPickFaceViewSchema.Constants.WPV_WW_Whs, false,
				la => la.ListDataSourceMember == "Lookups.Warehouses");
		}

		public void TestSupplierPart()
		{
			var pf = Factory.New<WhsPickFaceView>();
			AssertNull("Precondition: No supplier part yet", pf.SupplierPart);

			var org = Helper.CreateClient();
			var product = Helper.CreateProduct(org, "BIT");
			pf.WPV_OP = product.PK;

			AssertEquals("SupplierPart property loads FK.", product.PK, pf.SupplierPart.PK);
		}

		public void TestClient()
		{
			var pf = Factory.New<WhsPickFaceView>();
			AssertNull("Precondition: No client yet", pf.Client);

			var org = Helper.CreateClient();
			pf.WPV_OH = org.PK;

			AssertEquals("Client property loads FK.", org.PK, pf.Client.PK);
		}

		public void TestLocation()
		{
			var pf = Factory.New<WhsPickFaceView>();
			AssertNull("Precondition: No location yet", pf.Location);

			var whs = Helper.CreateWarehouse("Whs", "A");
			var location = whs.DefaultLocation;
			pf.WPV_WL = location.PK;

			AssertEquals("Location property loads FK.", location.PK, pf.Location.PK);
		}

		public void TestIsPickFaceAssigned()
		{
			var whs = Helper.CreateWarehouse("W01", "A", 2, 1);
			var org = Helper.CreateClient();
			var product = Helper.CreateProduct(org, "BIT");
			var locationTypePFC = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, "FIX");
			Factory.Save();

			var location1 = whs.FindLocation("A-1");
			var location2 = whs.FindLocation("A-2");
			location1.WLV_WLT_LocationType = locationTypePFC.PK;
			location2.WLV_WLT_LocationType = locationTypePFC.PK;

			var pickView = Helper.CreateProductPickFace(product, org, location1);
			Factory.Save();

			var pickFaceView_Assigned = Factory.LoadTop1<WhsPickFaceView>(new ZQuery(WhsPickFaceViewSchema.WPV_WL, location1.PK));
			var pickFaceView_UnAssigned = Factory.LoadTop1<WhsPickFaceView>(new ZQuery(WhsPickFaceViewSchema.WPV_WL, location2.PK));
			AssertEquals("Should show as assigned to pick face.", true, pickFaceView_Assigned.IsPickFaceAssigned);
			AssertEquals("Should show as not assigned to pick face.", false, pickFaceView_UnAssigned.IsPickFaceAssigned);
		}

		public void TestHasStockOrPendingTransactions()
		{
			var whs = Helper.CreateWarehouse("W01", "A", 2, 1);
			var org = Helper.CreateClient();
			var product = Helper.CreateProduct(org, "BIT");
			var locationTypePFC = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, "FIX");
			Factory.Save();

			var location = whs.FindLocation("A-1");
			location.WLV_WLT_LocationType = locationTypePFC.PK;

			var pickView = Helper.CreateProductPickFace(product, org, location);
			Factory.Save();

			var pickFaceView = Factory.LoadTop1<WhsPickFaceView>(new ZQuery(WhsPickFaceViewSchema.WPV_WL, location.PK));
			pickFaceView.WPV_TotalQuantity = 20m;
			AssertEquals("Expected pick face view to have stock.", true, pickFaceView.HasStockOrPendingTransactions);

			pickFaceView.WPV_TotalQuantity = 0m;
			pickFaceView.WPV_Incoming = 5m;
			AssertEquals("Expected pick face view to have stock.", true, pickFaceView.HasStockOrPendingTransactions);

			pickFaceView.WPV_TotalQuantity = 0m;
			pickFaceView.WPV_Incoming = 0m;
			AssertEquals("No stock or transactions expected.", false, pickFaceView.HasStockOrPendingTransactions);
		}

		public void TestReplenishMultipleTransferCanFit()
		{
			var whs = Helper.CreateWarehouse("W01", "A", 2, 1);
			var org = Helper.CreateClient();
			var product = Helper.CreateProduct(org, "BIT");
			var locationTypePFC = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, "FIX");
			Factory.Save();

			var location = whs.FindLocation("A-1");
			location.WLV_WLT_LocationType = locationTypePFC.PK;

			var pickView = Helper.CreateProductPickFace(product, org, location, replenishMin: 5m, replenishMax: 50m, replenishMultiple: 10m);
			Factory.Save();

			var pickFaceView = Factory.LoadTop1<WhsPickFaceView>(new ZQuery(WhsPickFaceViewSchema.WPV_WL, location.PK));
			pickFaceView.WPV_TotalQuantity = 20m;
			AssertEquals("Expect such a transfer to fit.", true, pickFaceView.ReplenishMultipleTransferCanFit);

			pickFaceView.WPV_TotalQuantity = 45m;
			AssertEquals("Expect such a transfer to not fit.", false, pickFaceView.ReplenishMultipleTransferCanFit);

			pickFaceView.WPV_ReplenishMaximum = 100m;
			AssertEquals("Expect such a transfer to fit.", true, pickFaceView.ReplenishMultipleTransferCanFit);

			pickFaceView.WPV_ReplenishmentMultiple = 100m;
			AssertEquals("Expect such a transfer to not fit.", false, pickFaceView.ReplenishMultipleTransferCanFit);

			pickFaceView.WPV_ReplenishmentMultiple = 10m; // reset
			AssertEquals("Expect such a transfer to fit.", true, pickFaceView.ReplenishMultipleTransferCanFit);

			pickFaceView.WPV_Incoming = 50m;
			AssertEquals("Expect such a transfer to not fit.", false, pickFaceView.ReplenishMultipleTransferCanFit);
		}
	}
}
