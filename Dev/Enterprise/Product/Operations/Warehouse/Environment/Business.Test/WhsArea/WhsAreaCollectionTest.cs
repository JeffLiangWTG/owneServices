using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsAreaCollection))]
	public class WhsAreaCollectionTest : WhsAreaCollectionTest<WhsAreaCollection>
	{
		protected override WhsAreaCollection GetCollectionToTest()
		{
			return new WhsAreaCollection(Factory);
		}
	}

	public abstract class WhsAreaCollectionTest<T> : WhsActiveBusinessObjectCollectionTestCase<T> where T : WhsAreaCollection
	{
		#region TestFilterBusinessObjectDefaults_Filter_Picking

		public void TestFilterBusinessObjectDefaults_Filter_Picking()
		{
			var warehouse1 = Helper.CreateWarehouse("Whs1", "Row1", 2, 2);
			var collection1 = WhsAreaCollection.GetPickingAreas(Factory, warehouse1.PK);
			AssertFilterDefaults(collection1, warehouse1.PK, WhsAreaCollection.FilterConstants.Warehouse, "Property");
			AssertFilterDefaults(collection1, ZBool.True, WhsAreaCollection.FilterConstants.IsPickingArea, "Property0");
			AssertEquals("Collection should be cached per warehouse.", collection1, WhsAreaCollection.GetPickingAreas(Factory, warehouse1.PK));

			var warehouse2 = Helper.CreateWarehouse("Whs2", "Row2", 2, 2);
			var collection2 = WhsAreaCollection.GetPickingAreas(Factory, warehouse2.PK);
			AssertFilterDefaults(collection2, warehouse2.PK, WhsAreaCollection.FilterConstants.Warehouse, "Property");
			AssertFilterDefaults(collection2, ZBool.True, WhsAreaCollection.FilterConstants.IsPickingArea, "Property0");
			AssertEquals("Collection should be cached per warehouse.", collection2, WhsAreaCollection.GetPickingAreas(Factory, warehouse2.PK));
			AssertNotEquals("Collection should be cached per warehouse.", collection1, collection2);
		}

		public void TestFilterBusinessObjectDefaults_Filter_Picking_NoWarehouse()
		{
			var collection = WhsAreaCollection.GetPickingAreas(Factory, ZGuid.Empty);
			AssertFilterDefaults(collection, ZBool.True, WhsAreaCollection.FilterConstants.IsPickingArea, "Property0");
			AssertEquals("Collection should be cached without warehouse.", collection, WhsAreaCollection.GetPickingAreas(Factory, ZGuid.Empty));
		}

		#endregion

		#region TestFilterBusinessObjectDefaults_Filter_Putaway

		public void TestFilterBusinessObjectDefaults_Filter_Putaway()
		{
			var warehouse1 = Helper.CreateWarehouse("Whs1", "Row1", 2, 2);
			var collection1 = WhsAreaCollection.GetPutawayAreas(Factory, warehouse1.PK);
			AssertFilterDefaults(collection1, warehouse1.PK, WhsAreaCollection.FilterConstants.Warehouse, "Property");
			AssertFilterDefaults(collection1, ZBool.True, WhsAreaCollection.FilterConstants.IsPutawayArea, "Property0");
			AssertEquals("Collection should be cached per warehouse.", collection1, WhsAreaCollection.GetPutawayAreas(Factory, warehouse1.PK));

			var warehouse2 = Helper.CreateWarehouse("Whs2", "Row2", 2, 2);
			var collection2 = WhsAreaCollection.GetPutawayAreas(Factory, warehouse2.PK);
			AssertFilterDefaults(collection2, warehouse2.PK, WhsAreaCollection.FilterConstants.Warehouse, "Property");
			AssertFilterDefaults(collection2, ZBool.True, WhsAreaCollection.FilterConstants.IsPutawayArea, "Property0");
			AssertEquals("Collection should be cached per warehouse.", collection2, WhsAreaCollection.GetPutawayAreas(Factory, warehouse2.PK));
			AssertNotEquals("Collection should be cached per warehouse.", collection1, collection2);
		}

		public void TestFilterBusinessObjectDefaults_Filter_Putaway_NoWarehouse()
		{
			var collection = WhsAreaCollection.GetPutawayAreas(Factory, ZGuid.Empty);
			AssertFilterDefaults(collection, ZBool.True, WhsAreaCollection.FilterConstants.IsPutawayArea, "Property0");
			AssertEquals("Collection should be cached without warehouse.", collection, WhsAreaCollection.GetPutawayAreas(Factory, ZGuid.Empty));
		}

		#endregion

		#region TestFilterBusinessObjectDefaults

		public void TestFilterBusinessObjectDefaults()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "Row1", 2, 2);
			var collection = new WhsAreaCollection(Factory, warehouse);
			AssertFilterDefaults(collection, warehouse.PK, WhsAreaCollection.FilterConstants.Warehouse, "Property");
		}

		#endregion

		#region Implementation

		protected static void AssertFilterDefaults(WhsAreaCollection collection, IZType value, string filterName, string propertyName)
		{
			var filterDefault = collection.FilterBusinessObjectDefaults[$"{filterName}:{propertyName}"];
			AssertEquals(propertyName, filterDefault.PropertyName);
			AssertEquals(value, filterDefault.Value);
			AssertEquals(false, filterDefault.IsRemovable);
		}

		#endregion
	}
}
