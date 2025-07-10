using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsWarehouseCollection))]
	class WhsWarehouseCollectionTestCase : WhsBusinessObjectCollectionTestCase
	{
		#region TestTransitWarehouseFiltering

		public void TestTransitWarehouseFiltering()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS");
			var transitWarehouse = Helper.CreateWarehouse("TRA");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var collection1 = (WhsWarehouseCollection)GetCollectionToTest();
			var collection2 = (WhsWarehouseCollection)GetCollectionToTest();
			((IWhsWarehouseCollection)collection2).WarehouseCollectionType = WarehouseCollectionType.All;

			collection1.Load();
			collection2.Load();
			AssertContainsExactElementsInAnyOrder("Default collection should not have any Transit Warehouses.", new[] { productWarehouse }, collection1);
			AssertContainsExactElementsInAnyOrder("If Collection has been set to get all Warehouses type, it should contain all.", new[] { productWarehouse, transitWarehouse }, collection2);
		}

		public void TestTransitWarehouseFilteringWithWarehouseCollectionTypeParam()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS");
			var transitWarehouse = Helper.CreateWarehouse("TRA");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var collection1 = new WhsWarehouseCollection(Factory, WarehouseCollectionType.ProductWarehouse);
			var collection2 = new WhsWarehouseCollection(Factory, WarehouseCollectionType.TransitWarehouse);
			var collection3 = new WhsWarehouseCollection(Factory, WarehouseCollectionType.All);

			collection1.Load();
			collection2.Load();
			collection3.Load();
			AssertContainsExactElementsInAnyOrder("Should not have all Product Warehouses.", new[] { productWarehouse }, collection1);
			AssertContainsExactElementsInAnyOrder("Should not have all Transit Warehouses.", new[] { transitWarehouse }, collection2);
			AssertContainsExactElementsInAnyOrder("Should contain all warehouse", new[] { productWarehouse, transitWarehouse }, collection3);
		}

		#endregion

		#region TestFTZWarehouseCollectionType

		public void TestFTZWarehouseCollectionType()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS");
			var ftzWarehouse = Helper.CreateFTZWarehouse("FTZ");

			var ftzOnlyWarehouseCollection = new WhsWarehouseCollection(Factory, WarehouseCollectionType.FTZWarehouse);
			var allWarehouseCollection = new WhsWarehouseCollection(Factory, WarehouseCollectionType.All);

			ftzOnlyWarehouseCollection.Load();
			allWarehouseCollection.Load();
			AssertContainsExactElementsInAnyOrder("Should not have all Product Warehouses.", new[] { ftzWarehouse }, ftzOnlyWarehouseCollection);
			AssertContainsExactElementsInAnyOrder("Should contain all Warehouses", new[] { productWarehouse, ftzWarehouse }, allWarehouseCollection);
		}

		#endregion

		#region TestPRWWarehouseCollectionType

		public void TestPRWWarehouseCollectionType_MustReturnPRWAndFTZ()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS");
			var ftzWarehouse = Helper.CreateFTZWarehouse("FTZ");
			var cydWarehouse = Helper.CreateCYDWarehouse("CYD");
			var transitWarehouse = Helper.CreateTRWWarehouse("TRA");

			var productWarehouseCollection = new WhsWarehouseCollection(Factory, WarehouseCollectionType.ProductWarehouse);
			var allWarehouseCollection = new WhsWarehouseCollection(Factory, WarehouseCollectionType.All);

			productWarehouseCollection.Load();
			allWarehouseCollection.Load();
			AssertContainsExactElementsInAnyOrder("Should only have Product Warehouses.", new[] { productWarehouse, ftzWarehouse }, productWarehouseCollection);
			AssertContainsExactElementsInAnyOrder("Should contain all Warehouses", new[] { transitWarehouse, cydWarehouse, productWarehouse, ftzWarehouse }, allWarehouseCollection);
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsWarehouseCollection(Factory);
		}

		#endregion
	}
}
