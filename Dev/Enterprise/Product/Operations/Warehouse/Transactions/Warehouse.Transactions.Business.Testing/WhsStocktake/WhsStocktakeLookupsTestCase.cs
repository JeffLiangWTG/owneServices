using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsStocktakeLookupsTestCase : WhsBusinessObjectLookupsTestCase
	{
		#region TestABCAnalysisCategories

		public void TestABCAnalysisCategories()
		{
			AssertEquals(typeof(ABCAnalysisCategoryCollection), Stocktake.Lookups.ABCAnalysisCategories.GetType());
			AssertEquals("Collection should have the default four categories", 4, Stocktake.Lookups.ABCAnalysisCategories.Count);
		}

		#endregion

		#region TestWarehouses

		public void TestWarehouses()
		{
			Factory.New<WhsWarehouse>();
			AssertEquals(typeof(WhsWarehouseCollectionWithSecurityCheck), Stocktake.Lookups.Warehouses.GetType());
			AssertEquals("Collection should not be loaded", 0, Stocktake.Lookups.Warehouses.Count);
		}

		#endregion

		#region TestClients

		public void TestClients()
		{
			OrgHeader client = Helper.CreateClient();
			AssertEquals(typeof(WarehouseClientCollectionWithSecurityCheck), Stocktake.Lookups.Clients.GetType());
			AssertEquals("Collection should not be loaded", 0, Stocktake.Lookups.Clients.Count);
		}

		#endregion

		#region TestSupplierParts

		public void TestSupplierParts()
		{
			OrgHeader client = Helper.CreateClient();
			Stocktake.WS_OH_Client = client.PK;
			Helper.CreateProduct(client, "P1");
			AssertEquals("Collection should not be loaded", 0, Stocktake.Lookups.SupplierParts.Count);
			Stocktake.Lookups.SupplierParts.Load();
			AssertEquals(true, Stocktake.Lookups.SupplierParts.FilterBusinessObjectDefaults.ContainsDefaultFor("Importer/Supplier" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property1"));
		}

		#endregion

		#region TestRows

		public void TestRows()
		{
			Factory.New<WhsRow>();
			AssertEquals("Collection should not be loaded", 0, Stocktake.Lookups.Rows.Count);

			var whs = Helper.CreateWarehouse("1");
			var row1 = whs.Rows.AddNew();
			var row2 = whs.Rows.AddNew();
			Stocktake.WS_WW_Whs = whs.PK;
			AssertContainsExactElementsInAnyOrder("Collection should be loaded", new[] { row1, row2, whs.DefaultOutboundDockDoorLocation.Row }, Stocktake.Lookups.Rows);
		}

		#endregion

		#region TestAreas

		public void TestAreas()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var stocktake = Factory.New<WhsStocktake>();
			AssertEquals("When no Warehouse is set, Area Collection should be empty.", 0, stocktake.Lookups.Areas.Count);

			stocktake.WS_WW_Whs = warehouse.PK;
			AssertContainsExactElementsInAnyOrder(warehouse.Areas, stocktake.Lookups.Areas);
		}

		#endregion

		#region TestPickMethods

		public void TestPickMethods()
		{
			AssertNotNull(Stocktake.Lookups.PickMethods);
		}

		#endregion

		#region StocktakCycles

		public void TestStocktakeCycles()
		{
			AssertNotNull(Stocktake.Lookups.StockTakeCycles);
		}

		#endregion

		#region TestLocations

		public void TestLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = Helper.CreateWarehouse("Whs1", "Row1", 1, 1);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);

			Factory.Save();

			AssertEquals(3, stocktake.Lookups.Locations.Count);
			AssertCollectionContains(data.Whs1.FindLocation("A-1"), stocktake.Lookups.Locations);
			AssertCollectionContains(data.Whs1.FindLocation("A-2"), stocktake.Lookups.Locations);
			AssertCollectionContains(data.Whs1.DefaultOutboundDockDoorLocation, stocktake.Lookups.Locations);
			AssertCollectionNotContains(warehouse.FindLocation("Row1-1"), stocktake.Lookups.Locations);
		}

		#endregion

		#region TestStocktakeTypes

		public void TestStocktakeTypes()
		{
			var newStocktake = Factory.New<WhsStocktake>();
			var loadedStocktake = Factory.New<WhsStocktake>();
			var finalisedStocktake = Factory.New<WhsStocktake>();
			newStocktake.WS_StocktakeStatus = StocktakeStatus.Codes.New;
			loadedStocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Loaded;
			finalisedStocktake.WS_StocktakeStatus = StocktakeStatus.Codes.Finalised;

			AssertEquals("STD", newStocktake.Lookups.StocktakeTypes.CodesAsString);
			AssertEquals("STD, ATC, AZC", loadedStocktake.Lookups.StocktakeTypes.CodesAsString);
			AssertEquals("STD, ATC, AZC", finalisedStocktake.Lookups.StocktakeTypes.CodesAsString);
		}

		#endregion

		#region Implementation

		WhsStocktake Stocktake
		{
			get { return stocktake ?? (stocktake = Factory.New<WhsStocktake>()); }
		}
		WhsStocktake stocktake;

		#endregion
	}
}
