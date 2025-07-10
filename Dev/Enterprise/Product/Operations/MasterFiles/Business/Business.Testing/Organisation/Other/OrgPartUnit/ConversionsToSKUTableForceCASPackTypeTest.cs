using System.Linq;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ConversionsToSKUTableForceCASPackTypeTest : TestConversionsToSKUTable
	{
		#region TestFindAndRemoveInvalidConversionPaths

		public void TestFindAndRemoveInvalidConversionPaths_ProductHasCASAndPLTConversions()
		{
			// Arrange
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";
			CreateUnitPart(product, 10, "UNT", "CAS");
			CreateUnitPart(product, 5, "CAS", "PLT");
			var packTypes = product.Lookups.PackTypes.ToDictionary(p => p.F3_Code);
			packTypes["UNT"].F3_UOMType = "UNT";
			packTypes["CAS"].F3_UOMType = "CAS";
			packTypes["PLT"].F3_UOMType = "PLT";

			// Act
			var table = new ConversionsToSKUTableForceCASPackType(product);

			// Assert
			AssertEquals(1.0M, table["UNT"].QtySKU);
			AssertEquals(10.0M, table["CAS"].QtySKU);
			AssertNull("Since the product has a conversion from UNT to CAS, the conversion to PLT should be removed.", table["PLT"]);
		}

		public void TestFindAndRemoveInvalidConversionPaths_ProductOnlyHasPLTConversion()
		{
			// Arrange
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";
			CreateUnitPart(product, 50, "UNT", "PLT");
			var packTypes = product.Lookups.PackTypes.ToDictionary(p => p.F3_Code);
			packTypes["UNT"].F3_UOMType = "UNT";
			packTypes["PLT"].F3_UOMType = "PLT";

			// Act
			var table = new ConversionsToSKUTableForceCASPackType(product);

			// Assert
			AssertEquals(1.0M, table["UNT"].QtySKU);
			AssertEquals(50.0M, table["PLT"].QtySKU);
		}

		public void TestFindAndRemoveInvalidConversionPaths_PLTSKU()
		{
			// Arrange
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "PLT";
			var packTypes = product.Lookups.PackTypes.ToDictionary(p => p.F3_Code);
			packTypes["PLT"].F3_UOMType = "PLT";

			// Act
			var table = new ConversionsToSKUTableForceCASPackType(product);

			// Assert
			AssertEquals(1.0M, table["PLT"].QtySKU);
		}

		public void TestFindAndRemoveInvalidConversionPaths_PackTypeNotFoundInDictionary()
		{
			// Arrange
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "SKU";
			CreateUnitPart(product, 1, "SKU", "UNT");

			// Act
			var table = new ConversionsToSKUTableForceCASPackType(product);

			// Assert
			AssertNoExceptionThrown(() => { var qty = table["SKU"].QtySKU; });
		}

		#endregion

		#region Implementation

		protected override string ForcedUOMTypeForUOMTestCases => UOMPackTypesList.Codes.Case;

		protected override ConversionsToSKUTable GetTable(OrgSupplierPart product) => new ConversionsToSKUTableForceCASPackType(product);

		#endregion
	}
}
