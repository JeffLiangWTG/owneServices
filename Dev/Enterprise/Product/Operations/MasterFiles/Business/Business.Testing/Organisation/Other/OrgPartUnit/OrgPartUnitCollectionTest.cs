using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartUnitCollection))]
	sealed class OrgPartUnitCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgSupplierPart parentSupplierPart = OrgSupplierPart.New(Factory);
			return new OrgPartUnitCollection(parentSupplierPart, Factory);
		}

		#region TestUpdateConversionToStockKeepingUnit

		public void TestUpdateConversionToStockKeepingUnit_UpdateExistUnit_PartInDatabase()
		{
			TestUpdateConversionToStockKeepingUnit_UpdateExistUnit(isPartInDatabase: true);
		}

		public void TestUpdateConversionToStockKeepingUnit_UpdateExistUnit_PartNotInDatabase()
		{
			TestUpdateConversionToStockKeepingUnit_UpdateExistUnit(isPartInDatabase: false);
		}

		public void TestUpdateConversionToStockKeepingUnit_UpdateExistUnit(bool isPartInDatabase)
		{
			var parentSupplierPart = OrgSupplierPart.New(Factory);
			parentSupplierPart.OP_StockKeepingUnit = "UNT";
			parentSupplierPart.OP_Weight = 10m;
			parentSupplierPart.OP_WeightUQ = "KG";
			parentSupplierPart.OP_PartNum = "PartNum";
			if (isPartInDatabase)
			{
				Factory.Save();
			}

			var collection = new OrgPartUnitCollection(parentSupplierPart, Factory);

			collection.UpdateConversionToStockKeepingUnit(parentSupplierPart.OP_WeightUQInfo, 10m);

			AssertEquals("collection has one unit record now", 1, collection.Count);
			var partUnit = collection[0];
			AssertEquals("Parent Unit", parentSupplierPart.OP_StockKeepingUnit, partUnit.OF_ParentPackType);
			AssertEquals("Child Unit", parentSupplierPart.OP_WeightUQ, partUnit.OF_PackType);
			AssertEquals("Unit Conversion Factor", 10m, partUnit.OF_QuantityInParent);

			parentSupplierPart.OP_Weight = 20m;
			collection.UpdateConversionToStockKeepingUnit(parentSupplierPart.OP_WeightUQInfo, 20m);

			AssertEquals("collection still has one unit record now", 1, collection.Count);
			AssertEquals("Unit Conversion Factor", 20m, partUnit.OF_QuantityInParent);
		}

		#endregion

		#region TestWeightConversionIsAutoCreatedIfNeeded

		public void TestWeightConversionIsAutoCreatedIfNeeded()
		{
			AssertEquals("Precondition: no unit conversions", 0, TestPart.PartUnits.Count);
			TestPart.OP_StockKeepingUnit = "BAG";
			TestPart.OP_Weight = 10;
			TestPart.OP_WeightUQ = "KG";
			Factory.Save();
			AssertEquals("Conversion should be created", 1, TestPart.PartUnits.Count);
			AssertEquals(10m, TestPart.PartUnits[0].OF_QuantityInParent);
			AssertEquals("KG", TestPart.PartUnits[0].OF_PackType);
			AssertEquals("BAG", TestPart.PartUnits[0].OF_ParentPackType);

			TestPart.OP_Weight = 0;
			Factory.Save();
			AssertEquals("Conversion should be deleted", 0, TestPart.PartUnits.Count);
		}

		#endregion

		#region TestVolumeConversionIsAutoCreatedIfNeeded

		public void TestVolumeConversionIsAutoCreatedIfNeeded()
		{
			AssertEquals("Precondition: no unit conversions", 0, TestPart.PartUnits.Count);
			TestPart.OP_StockKeepingUnit = "BAG";
			TestPart.OP_Cubic = 8;
			TestPart.OP_CubicUQ = "M3";
			Factory.Save();
			AssertEquals("Conversion should be created", 1, TestPart.PartUnits.Count);
			AssertEquals(8m, TestPart.PartUnits[0].OF_QuantityInParent);
			AssertEquals("M3", TestPart.PartUnits[0].OF_PackType);
			AssertEquals("BAG", TestPart.PartUnits[0].OF_ParentPackType);

			TestPart.OP_Cubic = 0;
			Factory.Save();
			AssertEquals("Conversion should be deleted", 0, TestPart.PartUnits.Count);
		}

		#endregion

		#region ConversionIsNotAutoCreatedWhenMatchesStockUnit

		public void TestConversionIsNotAutoCreatedWhenMatchesStockUnit()
		{
			AssertEquals("Precondition: no unit conversions", 0, TestPart.PartUnits.Count);
			// making Stock unit same as weight units
			TestPart.OP_StockKeepingUnit = "KG";
			TestPart.OP_Weight = 10;
			TestPart.OP_WeightUQ = "KG";
			Factory.Save();

			AssertEquals("No conversions should be created", 0, TestPart.PartUnits.Count);

			// making Stock unit same as volume units
			TestPart.OP_Weight = 0;
			TestPart.OP_StockKeepingUnit = "M3";
			TestPart.OP_Cubic = 8;
			TestPart.OP_CubicUQ = "M3";
			Factory.Save();

			AssertEquals("No conversions should be created", 0, TestPart.PartUnits.Count);
		}

		public void TestConversionIsNotAutoCreatedWhenMatchesStockUnit_CaseInsensitive()
		{
			AssertEquals("Precondition: no unit conversions", 0, TestPart.PartUnits.Count);
			// making Stock unit same as weight units
			TestPart.OP_StockKeepingUnit = "KG";
			TestPart.OP_Weight = 10;
			TestPart.OP_WeightUQ = "kg";
			Factory.Save();

			AssertEquals("No conversions should be created", 0, TestPart.PartUnits.Count);

			// making Stock unit same as volume units
			TestPart.OP_Weight = 0;
			TestPart.OP_StockKeepingUnit = "m3";
			TestPart.OP_Cubic = 8;
			TestPart.OP_CubicUQ = "M3";
			Factory.Save();

			AssertEquals("No conversions should be created", 0, TestPart.PartUnits.Count);
		}

		#endregion

		public void TestUpdatePartUnitIfThereIsOneWhenWeightIsChanged()
		{
			AssertEquals("PreCondition:TestPart doesnt have a unit record now", 0, TestPart.PartUnits.Count);
			TestPart.OP_StockKeepingUnit = "BAG";
			TestPart.OP_Weight = 10;
			TestPart.OP_WeightUQ = "KG";
			TestPart.OP_PartNum = "PartNum";
			Factory.Save();

			AssertEquals("TestPart has a unit record now", 1, TestPart.PartUnits.Count);
			TestPart.OP_WeightUQ = "LB";
			Factory.Save();

			AssertEquals("TestPart has a unit record now", 1, TestPart.PartUnits.Count);
			OrgPartUnit partUnit = TestPart.PartUnits[0];
			AssertEquals("Parent Unit", TestPart.OP_StockKeepingUnit, partUnit.OF_ParentPackType);
			AssertEquals("Child Unit", TestPart.OP_WeightUQ, partUnit.OF_PackType);
			AssertEquals("Unit Conversion Factor", 10m, partUnit.OF_QuantityInParent);
		}

		public void TestGetParentPackageByPackage()
		{
			TestPart.PartUnits.RemoveAll();

			AddNewPart(10m, "BOX", "CTN");
			AddNewPart(20m, "CTN", "KG");
			AddNewPart(1000m, "KG", "T");

			AssertEquals(TestPart.PartUnits.GetParentPackageByPackage("BOX"), "CTN");
			AssertEquals(TestPart.PartUnits.GetParentPackageByPackage("CTN"), "KG");
			AssertEquals(TestPart.PartUnits.GetParentPackageByPackage("KG"), "T");
			AssertEquals(TestPart.PartUnits.GetParentPackageByPackage("BOT"), "");
		}

		public void TestGetParentPackageByPackage_CaseInsensitive()
		{
			TestPart.PartUnits.RemoveAll();

			AddNewPart(10m, "box", "CTN");
			AddNewPart(20m, "CTN", "KG");

			AssertEquals(TestPart.PartUnits.GetParentPackageByPackage("BOX"), "CTN");
			AssertEquals(TestPart.PartUnits.GetParentPackageByPackage("ctn"), "KG");
		}

		public void TestGetUnitConversion_CaseInsensitive()
		{
			TestPart.PartUnits.RemoveAll();

			AddNewPart(10m, "BOX", "CTN");
			AddNewPart(20m, "box", "plt");

			var result1 = TestPart.PartUnits.GetUnitConversion("ctn", "box");
			AssertNotNull("Should retrieve a conversion", result1);
			AssertEquals("Should retrieve the correct conversion", 10m, result1.OF_QuantityInParent);

			var result2 = TestPart.PartUnits.GetUnitConversion("PLT", "BOX");
			AssertNotNull("Should retrieve a conversion", result2);
			AssertEquals("Should retrieve the correct conversion", 20m, result2.OF_QuantityInParent);
		}

		#region Implementation

		OrgPartUnit AddNewPart(ZDecimal value, ZString package, ZString parentPackage)
		{
			OrgPartUnit part = TestPart.PartUnits.AddNew();
			part.OF_QuantityInParent = value;
			part.OF_PackType = package;
			part.OF_ParentPackType = parentPackage;

			return part;
		}

		OrgSupplierPart TestPart;
		protected override void SetUp()
		{
			base.SetUp();
			TestPart = OrgSupplierPart.New(Factory);
			TestPart.OP_PartNum = TestPart.PK.ToString().Replace("-", "");
		}

		#endregion
	}
}
