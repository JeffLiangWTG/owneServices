using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsStocktakeLineLookupsTestCase : WhsBusinessObjectLookupsTestCase
	{
		#region TestInventoryStatuses

		public void TestInventoryStatuses()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);

			var available = new CodeDescriptionPair(StocktakeInventoryStatus.Codes.Available, StocktakeInventoryStatus.Descriptions.Available);
			var damaged = new CodeDescriptionPair(StocktakeInventoryStatus.Codes.Damaged, StocktakeInventoryStatus.Descriptions.Damaged);
			var held = new CodeDescriptionPair(InventoryStatus.Codes.Held, StocktakeInventoryStatus.Descriptions.Held);

			AssertEquals(3, line.Lookups.InventoryStatuses.Count);
			AssertCollectionContains(available, line.Lookups.InventoryStatuses);
			AssertCollectionContains(damaged, line.Lookups.InventoryStatuses);
			AssertCollectionContains(held, line.Lookups.InventoryStatuses);
		}

		#endregion

		#region TestLocations

		public void TestLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = Helper.CreateWarehouse("Whs1", "Row1");
			var locations1 = data.Whs1.Rows.Single(r => r.WR_Name == "A").Locations;
			var locations2 = warehouse.Rows.Single(r => r.WR_Name == "Row1").Locations;

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1, data.Part1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, locations1[0]);

			Factory.Save();

			AssertCollectionContains(locations1[0], line.Lookups.Locations);
			AssertCollectionContains(locations1[1], line.Lookups.Locations);
			AssertCollectionNotContains(locations2[0], line.Lookups.Locations);
		}

		#endregion

		#region TestSupplierParts

		public void TestSupplierParts()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);
			var client = Helper.CreateClient("C1", "Client");
			var productWithDifferentClient = Helper.CreateProduct(client, "P1");

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);

			Factory.Save();

			AssertEquals("It should not be loaded by default.", 0, line.Lookups.SupplierParts.Count);

			var parts = line.Lookups.SupplierParts;
			parts.Load();
			AssertContainsExactElementsInAnyOrder(new[] { data.Part1, data.Part2 }, parts);

			line.WU_OH_Client = ZGuid.Empty;
			var partsWithoutClient = line.Lookups.SupplierParts;
			partsWithoutClient.Load();
			AssertContainsExactElementsInAnyOrder(new[] { data.Part1, data.Part2, productWithDifferentClient }, partsWithoutClient);
		}

		public void TestSupplierParts_Filter()
		{
			// Setup test data
			var data = new TestDataSimpleEnvironment(Factory);

			var stocktake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var line = Helper.CreateWhsStocktakeLine(stocktake, data.Org1, data.Part1, data.Whs1.DefaultLocation);

			var supplierPartDefault = line.Lookups.SupplierParts.FilterBusinessObjectDefaults["Importer/Supplier:Property1"];

			AssertEquals(true, supplierPartDefault.IsRemovable);
		}

		#endregion
	}
}
