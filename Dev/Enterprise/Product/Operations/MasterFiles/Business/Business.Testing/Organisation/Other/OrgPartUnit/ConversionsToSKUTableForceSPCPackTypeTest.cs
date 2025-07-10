using System.Linq;
using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ConversionsToSKUTableForceSPCPackTypeTest : TestConversionsToSKUTable
	{
		public void TestFindAndRemoveInvalidConversionPaths_HasCASAndPLTConversions()
		{
			// Arrange
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";
			CreateUnitPart(product, 8, "UNT", "BOX");
			CreateUnitPart(product, 3, "BOX", "CAS");
			CreateUnitPart(product, 5, "CAS", "PLT");
			var packTypes = product.Lookups.PackTypes.ToDictionary(p => p.F3_Code);
			packTypes["UNT"].F3_UOMType = "SPC";
			packTypes["BOX"].F3_UOMType = "SPC";
			packTypes["CAS"].F3_UOMType = "CAS";
			packTypes["PLT"].F3_UOMType = "PLT";

			// Act
			var table = new ConversionsToSKUTableForceSPCPackType(product);

			// Assert
			AssertEquals(1.0M, table["UNT"].QtySKU);
			AssertEquals(8.0M, table["BOX"].QtySKU);
			AssertNull("Since the product has a split case conversion, the conversion to CAS should be removed.", table["CAS"]);
			AssertNull("Since the product has a split case conversion, the conversion to PLT should be removed.", table["PLT"]);
		}

		public void TestFindAndRemoveInvalidConversionPaths_OnlyCASConversions()
		{
			// Arrange
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "UNT";
			CreateUnitPart(product, 50, "UNT", "CAS");
			var packTypes = product.Lookups.PackTypes.ToDictionary(p => p.F3_Code);
			packTypes["UNT"].F3_UOMType = "UNT";
			packTypes["CAS"].F3_UOMType = "CAS";

			// Act
			var table = new ConversionsToSKUTableForceSPCPackType(product);

			// Assert
			AssertEquals(1.0M, table["UNT"].QtySKU);
			AssertNull("Since the product has a split case conversion, the conversion to CAS should be removed.", table["CAS"]);
		}

		public void TestFindAndRemoveInvalidConversionPaths_CASSKU()
		{
			// Arrange
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "CAS";
			var packTypes = product.Lookups.PackTypes.ToDictionary(p => p.F3_Code);
			packTypes["CAS"].F3_UOMType = "CAS";

			// Act
			var table = new ConversionsToSKUTableForceSPCPackType(product);

			// Assert
			AssertEquals(1.0M, table["CAS"].QtySKU);
		}

		public void TestFindAndRemoveInvalidConversionPaths_PLTSKU()
		{
			// Arrange
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "PLT";
			var packTypes = product.Lookups.PackTypes.ToDictionary(p => p.F3_Code);
			packTypes["PLT"].F3_UOMType = "PLT";

			// Act
			var table = new ConversionsToSKUTableForceSPCPackType(product);

			// Assert
			AssertEquals(1.0M, table["PLT"].QtySKU);
		}

		public void TestFindAndRemoveInvalidConversionPaths_OnlyPLTAndCASConversions()
		{
			// Arrange
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "CAS";
			CreateUnitPart(product, 5, "CAS", "PLT");
			var packTypes = product.Lookups.PackTypes.ToDictionary(p => p.F3_Code);
			packTypes["CAS"].F3_UOMType = "CAS";
			packTypes["PLT"].F3_UOMType = "PLT";

			// Act
			var table = new ConversionsToSKUTableForceSPCPackType(product);

			// Assert
			AssertEquals(1.0M, table["CAS"].QtySKU);
			AssertNull("Since the product has SKU that is NOT PLT, the conversion to PLT should be removed.", table["PLT"]);
		}

		public void TestFindAndRemoveInvalidConversionPaths_PackTypeNotFoundInDictionary()
		{
			// Arrange
			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = "SKU";
			CreateUnitPart(product, 1, "SKU", "UNT");

			// Act
			var table = new ConversionsToSKUTableForceSPCPackType(product);

			// Assert
			AssertNoExceptionThrown(() => { var qty = table["SKU"].QtySKU; });
		}

		#region Implementation

		protected override string ForcedUOMTypeForUOMTestCases => UOMPackTypesList.Codes.SplitCase;

		protected override ConversionsToSKUTable GetTable(OrgSupplierPart product) => new ConversionsToSKUTableForceSPCPackType(product);

		#endregion
	}
}
