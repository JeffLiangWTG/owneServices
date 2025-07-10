using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsWorkOrderLineLookupsTest : WhsComponentOrderLineLookupsTest<WhsWorkOrder, WhsWorkOrderLine>
	{
		#region TestSupplierPartsContainsOnlyBOMParts.

		public void TestSupplierPartsContainsOnlyBOMParts()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			WhsWorkOrder workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			WhsWorkOrderLine line = workOrder.Lines.AddNew();
			line.FillWithValidTestData();

			OrgSupplierPart nonBomPart = data.Part1;
			OrgSupplierPart bomPart = data.Part2;
			bomPart.BillOfMaterials.AddNew().FillWithValidTestData();
			Factory.Save();

			OrgSupplierPartCollection parts = line.Lookups.SupplierParts;
			parts.Load();
			AssertCollectionContains(bomPart, parts);
			AssertCollectionNotContains(nonBomPart, parts);
		}

		#endregion

		#region TestProductsIncludeNonSellableItems

		public void TestProductsIncludeNonSellableItems()
		{
			var data = new TestDataForBOM(Factory);
			data.CreateBOMProducts();
			data.BOM.Bike.OP_CanResell = true;
			data.BOM.BikeWheel.OP_CanResell = true;
			data.BOM.BikeEngine.OP_CanResell = false;
			data.BOM.EnginePiston.OP_CanResell = false;
			Factory.Save();
			// Lookups had been cached early, before OP_CanResell values were set, clear to make sure they are filtered properly.
			Factory.ClearCachedValue<OrgSupplierPartCollection>(data.Org1.PK.ToStringKey());

			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var line = workOrder.AllLines.AddNew();
			var products = line.Lookups.SupplierParts;
			products.Load();

			AssertEquals(4, products.Count);
			AssertCollectionContains(data.BOM.Bike, products);
			AssertCollectionContains(data.BOM.BikeEngine, products);
			AssertCollectionContains(data.BOM.BikeWheel, products);
			AssertCollectionContains(data.BOM.EnginePiston, products);
		}

		#endregion
	}
}
