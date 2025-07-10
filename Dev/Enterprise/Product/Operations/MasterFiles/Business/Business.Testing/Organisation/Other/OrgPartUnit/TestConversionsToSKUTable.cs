using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class TestConversionsToSKUTable : TestCaseWithFactory
	{
		#region TestConversionsTableAlwaysContainsSKU

		public void TestConversionsTableAlwaysContainsSKU()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_StockKeepingUnit = "UNT";
			var table1 = GetTable(product1);

			AssertEquals(1.0M, table1["UNT"].QtySKU);
			AssertEquals(1.0M, table1["unt"].QtySKU);
			AssertEquals(1.0M, table1["uNt"].QtySKU);

			AssertEquals(ExpectedUOMTypeForSKU, table1["UNT"].UOMType);

			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_StockKeepingUnit = "BOX";
			var table2 = GetTable(product2);
			AssertEquals(ExpectedUOMTypeForSKU, table2["BOX"].UOMType);
		}

		protected virtual string ExpectedUOMTypeForSKU => "";

		public void TestConversionsTableAlwaysContainsSKU_PalletSKU()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "PLT";
			var table = GetTable(product);

			AssertEquals(1.0M, table["PLT"].QtySKU);
			AssertEquals(ExpectedUOMTypeForPLT, table["PLT"].UOMType);
		}

		protected virtual string ExpectedUOMTypeForPLT => "";

		#endregion

		#region TestConversionTableReadyToUseAfterConstruction

		public void TestConversionTableReadyToUseAfterConstruction()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			// 20 units per Pack
			CreateUnitPart(product, 20, "UNT", "PCK");
			// 40 units per Box
			CreateUnitPart(product, 40, "UNT", "BOX");

			var table = GetTable(product);

			// trying to find biggest pack type for 50 units
			var conversion = table.OrderByDescending(x => x.QtySKU).FirstOrDefault(x => x.QtySKU < 50);

			AssertNotNull(conversion);
			AssertEquals("BOX", conversion.PackType);
			AssertEquals(40.0M, conversion.QtySKU);
		}

		#endregion

		#region TestDirectConversion

		public void TestDirectConversion()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			// 20 units per Pack
			CreateUnitPart(product, 20, "UNT", "PCK");
			// 4 packs per box
			CreateUnitPart(product, 4, "PCK", "BOX");

			var table = GetTable(product);

			AssertEquals(false, table.HasErrors);
			AssertEquals(20.0M, table["PCK"].QtySKU);
			AssertEquals("Should be 20 * 4.", 80.0M, table["BOX"].QtySKU);
		}

		public void TestDirectConversion_CaseInsensitive()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "unt";

			// 20 units per Pack
			CreateUnitPart(product, 20, "unt", "pck");
			// 4 packs per box
			CreateUnitPart(product, 4, "pck", "box");
			// 2 cartons per box (so, 20*4/2 unt per ctn)
			CreateUnitPart(product, 2, "ctn", "box");

			var table = GetTable(product);

			AssertEquals(false, table.HasErrors);
			AssertEquals(20.0M, table["PCK"].QtySKU);
			AssertEquals("Should be 20 * 4.", 80.0M, table["BOX"].QtySKU);
			AssertEquals("Should be 20 * 4 / 2.", 40.0M, table["CTN"].QtySKU);
		}

		public void TestDirectConversion_UOMTypes()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "CTN").F3_UOMType = ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.Case;
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.Pallet;

			// 20 units per cartons
			CreateUnitPart(product, 20, "UNT", "CTN");
			// 4 cartons per box
			CreateUnitPart(product, 4, "CTN", "BOX");
			CreateUnitPart(product, 100, "UNT", "PLT");

			var table = GetTable(product);

			AssertEquals(false, table.HasErrors);
			AssertEquals(20.0M, table["CTN"].QtySKU);
			AssertEquals("Should be 20 * 4.", 80.0M, table["BOX"].QtySKU);

			AssertEquals(ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.SplitCase, table["UNT"].UOMType);
			AssertEquals(ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.Case, table["CTN"].UOMType);
			AssertEquals(ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.Pallet, table["BOX"].UOMType);
			AssertEquals(ExpectedUOMTypeForPLT, table["PLT"].UOMType);
		}

		public void TestDirectConversion_UOMTypes_CaseInsensitive()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "unt";

			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "CTN").F3_UOMType = ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.Case;
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.Pallet;

			// 20 units per cartons
			CreateUnitPart(product, 20, "unt", "ctn");
			// 4 cartons per box
			CreateUnitPart(product, 4, "ctn", "box");
			CreateUnitPart(product, 100, "unt", "plt");

			var table = GetTable(product);

			CombineAssertions(() =>
			{
				AssertEquals(false, table.HasErrors);
				AssertEquals(20.0M, table["CTN"].QtySKU);
				AssertEquals("Should be 20 * 4.", 80.0M, table["BOX"].QtySKU);

				AssertEquals(ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.SplitCase, table["UNT"].UOMType);
				AssertEquals(ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.Case, table["CTN"].UOMType);
				AssertEquals(ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.Pallet, table["BOX"].UOMType);
				AssertEquals(ExpectedUOMTypeForPLT, table["PLT"].UOMType);
			});
		}

		protected virtual string ForcedUOMTypeForUOMTestCases => null;

		#endregion

		#region TestInvertedConversion

		public void TestInvertedConversion()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			// 0.2 packs is a unit  (5 units per pack)
			CreateUnitPart(product, 0.2M, "PCK", "UNT");
			// 0.1 boxes per pack (10 packs per box)
			CreateUnitPart(product, 0.1M, "BOX", "PCK");

			var table = GetTable(product);

			AssertEquals(false, table.HasErrors);
			AssertEquals(5.0M, table["PCK"].QtySKU);
			AssertEquals(50.0M, table["BOX"].QtySKU);
		}

		public void TestInvertedConversion_UOMTypes()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.SplitCase;
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "CTN").F3_UOMType = ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.Case;
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.Pallet;

			// 0.2 packs is a unit  (5 units per pack)
			CreateUnitPart(product, 0.2M, "CTN", "UNT");
			// 0.1 boxes per pack (10 cartons per box)
			CreateUnitPart(product, 0.1M, "BOX", "CTN");
			CreateUnitPart(product, 0.1M, "BOX", "PLT");

			var table = GetTable(product);

			AssertEquals(false, table.HasErrors);
			AssertEquals(5.0M, table["CTN"].QtySKU);
			AssertEquals(50.0M, table["BOX"].QtySKU);

			AssertEquals(ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.SplitCase, table["UNT"].UOMType);
			AssertEquals(ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.Case, table["CTN"].UOMType);
			AssertEquals(ForcedUOMTypeForUOMTestCases ?? UOMPackTypesList.Codes.Pallet, table["BOX"].UOMType);
			AssertEquals(ExpectedUOMTypeForPLT, table["PLT"].UOMType);
		}

		#endregion

		#region TestMixedConversion

		public void TestMixedConversion()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			// 0.2 packs is a unit  (5 units per pack)
			CreateUnitPart(product, 0.2M, "PCK", "UNT");
			// 4 packs per box
			CreateUnitPart(product, 4, "PCK", "BOX");

			var table = GetTable(product);

			AssertEquals(false, table.HasErrors);
			AssertEquals(5.0M, table["PCK"].QtySKU);
			AssertEquals(20.0M, table["BOX"].QtySKU);
		}

		#endregion

		#region TestCorrectLoopConversion

		public void TestCorrectLoopConversion()
		{
			// the loops themselves are bad, but algorithm can deal with "correct" ones
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			// 20 units per pack
			CreateUnitPart(product, 20, "UNT", "PCK");
			// 0.05 packs per unit
			CreateUnitPart(product, 0.05, "PCK", "UNT");

			var table = GetTable(product);

			AssertEquals(false, table.HasErrors);
			AssertEquals(20.0M, table["PCK"].QtySKU);
		}

		#endregion

		#region TestIncorrectLoopConversion

		public void TestIncorrectLoopConversion()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			// 20 units per pack
			CreateUnitPart(product, 20, "UNT", "PCK");
			// 0.1 packs per unit
			CreateUnitPart(product, 0.1, "PCK", "UNT");

			var table = GetTable(product);

			AssertEquals(true, table.HasErrors);
		}

		#endregion

		#region TestZeroConversionAreIgnored

		public void TestZeroConversionAreIgnored()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			// 20 units per Pack
			CreateUnitPart(product, 20, "UNT", "PCK");
			// 0 packs per box - should not happen in production
			CreateUnitPart(product, 0, "PCK", "BOX");

			var table = GetTable(product);

			AssertEquals(false, table.HasErrors);
			AssertNotNull(table["PCK"]);
			AssertNotNull(table["pck"]);
			AssertNull(table["BOX"]);
			AssertNull(table["box"]);
		}

		#endregion

		#region TestQuantitativeUnitsWithFractions

		public void TestQuantitativeUnitsWithFractions()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			CreateUnitPart(product, 4.5, "UNT", "PCK");

			var table = GetTable(product);
			AssertEquals(false, table.HasErrors);
			AssertEquals(true, table.HasWarnings);
			AssertEquals("The warning should be related to PCK", 1, table.WarningsForPackTypes(new[] { "PCK" }).Count());
			AssertEquals("UNT should not have warnings because it is StockKeepingUnit", 0, table.WarningsForPackTypes(new[] { "UNT" }).Count());
		}

		#endregion

		#region TestWarningsForPackTypes

		public void TestWarningsForPackTypes_CaseInsensitive()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			CreateUnitPart(product, 4.5, "UNT", "PCK");

			var table = GetTable(product);
			AssertEquals(false, table.HasErrors);
			AssertEquals(true, table.HasWarnings);
			AssertEquals("The warning should be related to PCK", 1, table.WarningsForPackTypes(new[] { "pck" }).Count());
		}

		#endregion

		#region TestQuantativeUnitsWithFractions_WhenEndResultIsWholeNumber

		public void TestQuantativeUnitsWithFractions_WhenEndResultIsWholeNumber()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "PCK";

			CreateUnitPart(product, 4, "UNT", "PCK"); // this is fractional conversion 0.25 PCK per UNT
			CreateUnitPart(product, 20, "UNT", "BOX"); // but this one is valid - 5 PCK in a BOX

			var table = GetTable(product);
			AssertEquals(false, table.HasErrors);
			AssertEquals(true, table.HasWarnings);
			AssertEquals("Should contain a valid conversion 5 BOX -> 1 PCK.", 5m, table["BOX"].QtySKU);

			if (AllowFractionalConversion)
			{
				AssertEquals("Should contain a fractional conversion 0.25 UNT -> 1 PCK.", 0.25m, table["UNT"].QtySKU);
			}
			else
			{
				AssertNull("Should not have fractional conversion", table["UNT"]);
			}
		}

		protected virtual bool AllowFractionalConversion => false;

		#endregion

		#region TestNonQuantitativeUnitsAreIgnored

		public void TestNonQuantitativeUnitsAreIgnored()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			CreateUnitPart(product, 4.5, "UNT", "KG");
			CreateUnitPart(product, 7.33, "UNT", "L");

			var table = GetTable(product);
			AssertEquals(false, table.HasErrors);
			AssertNull(table["KG"]);
			AssertNull(table["L"]);
		}

		public void TestNonQuantitativeUnitsAreIgnored_CaseInsensitive()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			CreateUnitPart(product, 4.5, "UNT", "kg");
			CreateUnitPart(product, 7.33, "UNT", "l");

			var table = GetTable(product);
			AssertEquals(false, table.HasErrors);
			AssertNull(table["KG"]);
			AssertNull(table["L"]);
		}

		#endregion

		#region	TestNonQuantitativeUnitsWhichArePartsOfInconsistentPartsAreIgnored

		public void TestNonQuantitativeUnitsWhichArePartsOfInconsistentPartsAreIgnored()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "CAS";

			CreateUnitPart(product, 1, "DOZ", "CAS");
			CreateUnitPart(product, 12, "NMB", "DOZ");

			var table = GetTable(product);
			AssertEquals(true, table.HasWarnings);
			AssertEquals(false, table.HasErrors);

			if (AllowFractionalConversion)
			{
				AssertEquals("Should contain a fractional conversion.", 0.0833333333333333333333333333m, table["NMB"].QtySKU);
			}
			else
			{
				AssertNull("Should not have fractional conversion", table["NMB"]);
			}

			AssertNotNull(table["DOZ"]);
			AssertEquals("There should be no warnings for DOZ pack type", 0, table.WarningsForPackTypes(new[] { "DOZ" }).Count());
		}

		#endregion

		#region TestQuantitativeUnitsWithDecimals_CheckWorkOnlyAgainstStockKeepingUnit

		public void TestQuantitativeUnitsWithDecimals_CheckWorkOnlyAgainstStockKeepingUnit()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			CreateUnitPart(product, 2, "UNT", "PCK");
			CreateUnitPart(product, 0.25, "BOX", "PCK");

			var table = GetTable(product);
			AssertEquals(false, table.HasErrors);
			AssertEquals(2.0M, table["PCK"].QtySKU);
			AssertEquals(8.0M, table["BOX"].QtySKU); // no errors, because total conversion is without decimals

			product.OP_StockKeepingUnit = "PCK";
			table = GetTable(product);
			AssertEquals(true, table.HasWarnings);
			AssertEquals(4.0M, table["BOX"].QtySKU);

			if (AllowFractionalConversion)
			{
				AssertEquals("Should contain a fractional conversion.", 0.5m, table["UNT"].QtySKU);
			}
			else
			{
				AssertNull("Should not have fractional conversion", table["UNT"]);
			}
		}

		#endregion

		#region TestMultipleInconsistentPaths

		public void TestMultipleInconsistentPaths()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			CreateUnitPart(product, 5, "UNT", "PCK"); // 5 UNT in a PCK

			CreateUnitPart(product, 3, "UNT", "CAS");
			CreateUnitPart(product, 2, "CAS", "PCK"); // 6 UNT in a PCK

			CreateUnitPart(product, 4, "UNT", "BOX");
			CreateUnitPart(product, 3, "BOX", "PCK"); // 12 UNT in a PCK

			CreateUnitPart(product, 5, "PCK", "PLT"); // ??? UNT in a PLT

			CreateUnitPart(product, 20, "UNT", "KEG"); // the only valid conversion here

			var table = GetTable(product);
			AssertEquals(true, table.HasErrors);
			AssertNull("Shouldn't contain PCK because it produces inconsistent conversions", table["PCK"]);
			AssertNull("Shouldn't contain CAS because it produces inconsistent conversions", table["CAS"]);
			AssertNull("Shouldn't contain BOX because it produces inconsistent conversions", table["BOX"]);
			AssertNull("Shouldn't contain PLT because to get there you need to convert to PCK which have no valid conversion", table["PLT"]);
			AssertNotNull("KEG should have a valid conversion to UNT", table["KEG"]);
			AssertEquals(20m, table["KEG"].QtySKU);
		}

		public void TestMultipleInconsistentPaths_CaseInsensitive()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "unt";

			CreateUnitPart(product, 5, "unt", "pck"); // 5 UNT in a PCK

			CreateUnitPart(product, 3, "unt", "cas");
			CreateUnitPart(product, 2, "cas", "pck"); // 6 UNT in a PCK

			CreateUnitPart(product, 4, "unt", "box");
			CreateUnitPart(product, 3, "box", "pck"); // 12 UNT in a PCK

			CreateUnitPart(product, 5, "pck", "plt"); // ??? UNT in a PLT

			CreateUnitPart(product, 20, "unt", "keg"); // the only valid conversion here

			var table = GetTable(product);
			CombineAssertions(() =>
			{
				AssertEquals(true, table.HasErrors);
				AssertNull("Shouldn't contain PCK because it produces inconsistent conversions", table["PCK"]);
				AssertNull("Shouldn't contain CAS because it produces inconsistent conversions", table["CAS"]);
				AssertNull("Shouldn't contain BOX because it produces inconsistent conversions", table["BOX"]);
				AssertNull("Shouldn't contain PLT because to get there you need to convert to PCK which have no valid conversion", table["PLT"]);
				AssertNotNull("KEG should have a valid conversion to UNT", table["KEG"]);
				AssertEquals(20m, table["KEG"].QtySKU);
			});
		}

		#endregion

		#region TestUnreachebleConversionsAreIgnored

		public void TestUnreachebleConversionsAreIgnored()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";

			CreateUnitPart(product, 2, "UNT", "BOX");
			CreateUnitPart(product, 3, "PCK", "PAL");
			var table = GetTable(product);

			AssertEquals(false, table.HasErrors);
			AssertNotNull(table["BOX"]);
			AssertNull(table["PCK"]);
			AssertNull(table["PAL"]);
		}

		#endregion

		#region CreateUnitPart

		protected void CreateUnitPart(OrgSupplierPart product, ZDecimal qtyInParent, ZString packType, ZString parentPackType)
		{
			var unit = product.PartUnits.AddNew();
			unit.OF_PackType = packType;
			unit.OF_ParentPackType = parentPackType;
			unit.OF_QuantityInParent = qtyInParent;
		}

		#endregion

		#region Implementation

		protected virtual ConversionsToSKUTable GetTable(OrgSupplierPart product) => new ConversionsToSKUTable(product);

		#endregion
	}
}
