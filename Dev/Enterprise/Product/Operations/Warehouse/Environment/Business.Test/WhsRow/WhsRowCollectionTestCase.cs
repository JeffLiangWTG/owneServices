namespace Enterprise.Warehouse.Environment.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(WhsRowCollection))]
	class WhsRowCollectionTestCase : ActiveBusinessObjectCollectionTestCase<WhsRowCollection>
	{
		#region TestWarehouseConstructor

		public void TestWarehouseConstructor()
		{
			var branch1 = Helper.CreateGlbBranch("BR1");
			var branch2 = Helper.CreateGlbBranch("BR2");
			var client = Helper.CreateClient("CLIENT");
			var warehouse1 = Helper.CreateWarehouse("WH1", client.MainAddress, branch1, shouldPreGenerateDDL: false);
			var warehouse2 = Helper.CreateWarehouse("WH2", client.MainAddress, branch2, shouldPreGenerateDDL: false);

			var collection1 = new WhsRowCollection(warehouse1, Factory);
			var collection2 = new WhsRowCollection(warehouse2, Factory);
			AssertEquals(0, collection1.Count);
			AssertEquals(0, collection2.Count);

			var row = Factory.New<WhsRow>();
			row.WR_WW_Whs = warehouse1.PK;
			AssertContainsExactElementsInAnyOrder(new[] { row }, collection1);
			AssertEquals(0, collection2.Count);
		}

		#endregion

		#region TestFilterBusinessObjectDefaults

		public void TestFilterBusinessObjectDefaults()
		{
			var collectionWithNoWarehouse = new WhsRowCollection(Factory);
			var propertyName = WhsLocationCollection.FilterSchema.Warehouse + ":Property";
			AssertEquals(false, collectionWithNoWarehouse.FilterBusinessObjectDefaults.ContainsDefaultFor(propertyName));

			var warehouse = Helper.CreateWarehouse("BigWhs");
			var collectionWithWarehouse = new WhsRowCollection(warehouse, Factory);
			var warehouseDefault = collectionWithWarehouse.FilterBusinessObjectDefaults[propertyName];
			AssertEquals("Property", warehouseDefault.PropertyName);
			AssertEquals(warehouse.PK, warehouseDefault.Value);
			AssertEquals(false, warehouseDefault.IsRemovable);
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctionsEnv Helper => helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory));
		WhsTestHelperFunctionsEnv helper;

		#endregion
	}
}
