using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using NUnit.Framework;
using static Enterprise.Warehouse.Environment.Business.WhsAreaCollection;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsDynamicAreaCollection))]
	class WhsDynamicAreaCollectionTest : WhsAreaCollectionTest<WhsDynamicAreaCollection>
	{
		#region TestFilterBusinessObjectDefaults_Filter_DynamicAreas

		public void TestFilterBusinessObjectDefaults_Filter_DynamicAreas()
		{
			var collection = GetCollectionToTest();
			var filterDefault = collection.FilterBusinessObjectDefaults[$"{FilterConstants.AreaType}:Property"];
			CombineAssertions(() =>
			{
				AssertEquals("Property", filterDefault.PropertyName);
				AssertEquals((ZString)AreaTypes.Codes.DynamicPickFace, filterDefault.Value);
				AssertEquals(false, filterDefault.IsRemovable);
			});
		}

		#endregion

		#region TestGetDynamicPickingAreas

		public void TestGetDynamicPickingAreas_FilterDefaults()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "Row1", 2, 2);
			var collection = WhsDynamicAreaCollection.GetDynamicPickingAreas(Factory, warehouse);
			AssertFilterDefaults(collection, warehouse.PK, FilterConstants.Warehouse, "Property");
			AssertFilterDefaults(collection, ZBool.True, FilterConstants.IsPickingArea, "Property0");
			AssertFilterDefaults(collection, (ZString)AreaTypes.Codes.DynamicPickFace, FilterConstants.AreaType, "Property");
		}

		public void TestGetDynamicPickingAreas_DynamicOnly()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "Row1", 2, 2);

			var areaDynamic = Factory.New<WhsArea>();
			areaDynamic.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			areaDynamic.WA_IsPickingArea = true;
			areaDynamic.WA_WW_Whs = warehouse.PK;
			areaDynamic.WA_Name = "areaDynamic";

			var areaB = Factory.New<WhsArea>();
			areaB.WA_AreaType = AreaTypes.Codes.FreeStore;
			areaB.WA_IsPickingArea = true;
			areaB.WA_WW_Whs = warehouse.PK;
			areaB.WA_Name = "areaB";

			var collection = WhsDynamicAreaCollection.GetDynamicPickingAreas(Factory, warehouse);
			AssertEquals("Collection count correct", 1, collection.Count);
			AssertEquals("Area correct", "areaDynamic", collection[0].WA_Name);
		}

		public void TestGetDynamicPickingAreas_OnlyPicking()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "Row1", 2, 2);

			var areaDynamic = Factory.New<WhsArea>();
			areaDynamic.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			areaDynamic.WA_IsPickingArea = true;
			areaDynamic.WA_WW_Whs = warehouse.PK;
			areaDynamic.WA_Name = "areaDynamic";

			var putawayArea = Factory.New<WhsArea>();
			putawayArea.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			putawayArea.WA_IsPickingArea = false;
			putawayArea.WA_IsPutawayArea = true;
			putawayArea.WA_WW_Whs = warehouse.PK;
			putawayArea.WA_Name = "putawayArea";

			var collection = WhsDynamicAreaCollection.GetDynamicPickingAreas(Factory, warehouse);
			AssertEquals("Collection count correct", 1, collection.Count);
			AssertEquals("Area correct", "areaDynamic", collection[0].WA_Name);
		}

		public void TestGetDynamicPickingAreas_DifferentWhs()
		{
			var warehouse1 = Helper.CreateWarehouse("Whs1", "Row1", 2, 2);
			var warehouse2 = Helper.CreateWarehouse("Whs2", "Row1", 2, 2);

			var areaDynamicWhs2 = Factory.New<WhsArea>();
			areaDynamicWhs2.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			areaDynamicWhs2.WA_IsPickingArea = true;
			areaDynamicWhs2.WA_WW_Whs = warehouse2.PK;
			areaDynamicWhs2.WA_Name = "areaDynamicWhs2";

			var areaDynamicWhs1 = Factory.New<WhsArea>();
			areaDynamicWhs1.WA_AreaType = AreaTypes.Codes.DynamicPickFace;
			areaDynamicWhs1.WA_IsPickingArea = true;
			areaDynamicWhs1.WA_WW_Whs = warehouse1.PK;
			areaDynamicWhs1.WA_Name = "areaDynamicWhs1";

			var collection = WhsDynamicAreaCollection.GetDynamicPickingAreas(Factory, warehouse1);
			AssertEquals("Collection count correct", 1, collection.Count);
			AssertEquals("Area correct", "areaDynamicWhs1", collection[0].WA_Name);
			AssertEquals("Area Whs correct", warehouse1.PK, collection[0].WA_WW_Whs);
		}

		#endregion

		#region Implementation

		protected override WhsDynamicAreaCollection GetCollectionToTest()
		{
			return new WhsDynamicAreaCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			WhsArea bizo = Factory.NewWithValidTestData<WhsArea>();
			bizo.WA_AreaType = CodeLists.AreaTypes.Codes.DynamicPickFace;
			return bizo;
		}

		#endregion
	}
}
