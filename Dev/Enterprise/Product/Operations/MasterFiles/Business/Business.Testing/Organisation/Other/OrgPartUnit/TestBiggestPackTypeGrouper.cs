using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TestBiggestPackTypeGrouper : TestCaseWithFactory
	{
		#region TestGetGroups

		#region TestGetGroups

		public void TestGetGroups()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			// 20 units per pack
			CreateUnitPart(product, 20, "UNT", "PCK");

			var result = BiggestPackTypeGrouper.GetGroups(product, 80).ToList();
			AssertEquals(1, result.Count);
			AssertUnitData(result[0], 4m, "PCK", 80m, "UNT");
		}

		#endregion

		#region TestGetGroups_NoExceptionWithMaxQuantity

		public void TestGetGroups_NoExceptionWithMaxQuantity()
		{
			var product = Factory.New<OrgSupplierPart>();
			AssertNoExceptionThrown(() => BiggestPackTypeGrouper.GetGroups(product, 999999999999999).ToList());
		}

		#endregion

		#region TestGetGroups_UOMType

		public void TestGetGroups_UOMType()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = UOMPackTypesList.Codes.Case;

			// 20 units per pack
			CreateUnitPart(product, 20, "UNT", "BOX");

			var result = BiggestPackTypeGrouper.GetGroups(product, 82).ToList();
			AssertEquals(2, result.Count);

			var packResult1 = result.Single(g => g.PackType == "BOX");
			var packResult2 = result.Single(g => g.PackType == "UNT");
			AssertUnitData(packResult1, 4m, "BOX", 80m, "UNT", UOMPackTypesList.Codes.Case);
			AssertUnitData(packResult2, 2m, "UNT", 2m, "UNT");

			var packResult1PackTypeInterface = (IPackTypeConversion)packResult1;
			AssertEquals(20m, packResult1PackTypeInterface.QtySKU);
			AssertEquals(4m, packResult1PackTypeInterface.PackQty);
			AssertEquals(UOMPackTypesList.Codes.Case, packResult1PackTypeInterface.UOMType);

			var packResult2PackTypeInterface = (IPackTypeConversion)packResult2;
			AssertEquals(1m, packResult2PackTypeInterface.QtySKU);
			AssertEquals(2m, packResult2PackTypeInterface.PackQty);
			AssertEquals("", packResult2PackTypeInterface.UOMType);
		}

		#endregion

		#region TestGetGroups_GroupingIgnoredIfThereIsNoUnitConversions

		public void TestGetGroups_GroupingIgnoredIfThereIsNoUnitConversions()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			var result = BiggestPackTypeGrouper.GetGroups(product, 15).ToList();
			AssertEquals(1, result.Count);
			AssertUnitData(result[0], 15m, "UNT", 15m, "UNT");
		}

		#endregion

		#region TestGetGroups_SimpleGroupingWithRemainder

		public void TestGetGroups_SimpleGroupingWithRemainder()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			// 20 units per pack
			CreateUnitPart(product, 20, "UNT", "PCK");

			var result = BiggestPackTypeGrouper.GetGroups(product, 87).ToList();
			AssertEquals(2, result.Count);
			AssertUnitData(result[0], 4m, "PCK", 80m, "UNT");
			AssertUnitData(result[1], 7m, "UNT", 7m, "UNT");
		}

		#endregion

		#region TestGetGroups_QuantityWithFractionsIsIgnored

		public void TestGetGroups_QuantityWithFractionsIsIgnored() => TestGetGroups_QuantityWithFractionsIsIgnored(uomType: "");

		public void TestGetGroups_QuantityWithFractionsIsIgnored_WithUOMType() => TestGetGroups_QuantityWithFractionsIsIgnored(uomType: UOMPackTypesList.Codes.Pallet);

		void TestGetGroups_QuantityWithFractionsIsIgnored(string uomType)
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = uomType;

			// 20 units per pack
			CreateUnitPart(product, 20, "UNT", "BOX");

			var result = BiggestPackTypeGrouper.GetGroups(product, 87.1).ToList();
			AssertEquals(1, result.Count);
			AssertUnitData(result[0], 87.1m, "UNT", 87.1m, "UNT", uomType);

			var packResultPackTypeInterface = (IPackTypeConversion)result[0];
			AssertEquals(1m, packResultPackTypeInterface.QtySKU);
			AssertEquals(87.1m, packResultPackTypeInterface.PackQty);
			AssertEquals(uomType, packResultPackTypeInterface.UOMType);
		}

		#endregion

		#region TestGetGroups_ComplexGroupingWithRemainder

		public void TestGetGroups_ComplexGroupingWithRemainder()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			CreateUnitPart(product, 20, "UNT", "PCK"); // 20 units per pack
			CreateUnitPart(product, 5, "PCK", "BOX"); // 5 packs per box
			CreateUnitPart(product, 0.5m, "BOX", "BAG"); // 2 Bags in a Box

			var result = BiggestPackTypeGrouper.GetGroups(product, 297).ToList();
			AssertEquals(4, result.Count);
			AssertUnitData(result[0], 2m, "BOX", 200m, "UNT");
			AssertUnitData(result[1], 1m, "BAG", 50m, "UNT");
			AssertUnitData(result[2], 2m, "PCK", 40m, "UNT");
			AssertUnitData(result[3], 7m, "UNT", 7m, "UNT");
		}

		#endregion

		#region TestGetGroups_UsesConversionsEvenIfThereAreWarnings

		public void TestGetGroups_UsesConversionsEvenIfThereAreWarnings()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			CreateUnitPart(product, 10, "UNT", "PCK"); // 10 units per pack - good conversion
			CreateUnitPart(product, 4, "CAS", "PCK"); // bad conversion:  2.5 UNT in a CAS - should not be used
			CreateUnitPart(product, 10, "CAS", "BOX"); // this will result in 25 UNT in a BOX - should be used

			var table = new ConversionsToSKUTable(product);
			Assert("Precondition - conversion table has warnings", table.HasWarnings);

			var result = BiggestPackTypeGrouper.GetGroups(product, 67).ToArray();
			AssertEquals(3, result.Length);
			AssertUnitData(result[0], 2m, "BOX", 50m, "UNT");
			AssertUnitData(result[1], 1m, "PCK", 10m, "UNT");
			AssertUnitData(result[2], 7m, "UNT", 7m, "UNT");
		}

		#endregion

		#region TestGetGroups_UsesConversionsEvenIfThereAreErrors

		public void TestGetGroups_UsesConversionsEvenIfThereAreErrors()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			// inconsistent conversion set
			// UNT->PCK->CAS will result in 20 UNT,  UNT->CAS is 30 UNT
			CreateUnitPart(product, 10, "UNT", "PCK");
			CreateUnitPart(product, 2, "PCK", "CAS");
			CreateUnitPart(product, 30, "UNT", "CAS");
			CreateUnitPart(product, 15, "UNT", "PLT");

			var table = new ConversionsToSKUTable(product);
			Assert("Precondition - conversion table has errors", table.HasErrors);

			// only UNT->PLT conversion will be used, because it is consistent
			var result = BiggestPackTypeGrouper.GetGroups(product, 67).ToArray();
			AssertEquals(2, result.Length);
			AssertUnitData(result[0], 4m, "PLT", 60m, "UNT");
			AssertUnitData(result[1], 7m, "UNT", 7m, "UNT");
		}

		#endregion

		#region AssertUnitData

		void AssertUnitData(GroupedQuantityItem unit, decimal groupedQty, string groupedUQ, decimal skuQty, string skuUQ, string uomType = "")
		{
			AssertEquals(groupedQty, unit.PackQty);
			AssertEquals(groupedUQ, unit.PackType);
			AssertEquals(skuQty, unit.Qty);
			AssertEquals(skuUQ, unit.StockKeepingUnit);
			AssertEquals(uomType, unit.UOMType);
		}

		#endregion

		#region CreateUnitPart

		void CreateUnitPart(OrgSupplierPart product, ZDecimal qtyInParent, ZString packType, ZString parentPackType)
		{
			var unit = product.PartUnits.AddNew();
			unit.OF_PackType = packType;
			unit.OF_ParentPackType = parentPackType;
			unit.OF_QuantityInParent = qtyInParent;
		}

		#endregion

		#endregion
	}
}
