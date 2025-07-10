using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ConversionsToSKUTableForAllocationTest : TestConversionsToSKUTable
	{
		#region TestConversionsTable_SKUHasUOMType

		public void TestConversionsTable_SKUHasUOMType_SplitCase() => TestConversionsTable_SKUHasUOMType(uomType: UOMPackTypesList.Codes.SplitCase);
		public void TestConversionsTable_SKUHasUOMType_Pallet() => TestConversionsTable_SKUHasUOMType(uomType: UOMPackTypesList.Codes.Pallet);
		public void TestConversionsTable_SKUHasUOMType_Case() => TestConversionsTable_SKUHasUOMType(uomType: UOMPackTypesList.Codes.Case);
		public void TestConversionsTable_SKUHasUOMType_CaseInsensitive_SplitCase() => TestConversionsTable_SKUHasUOMType(uomType: UOMPackTypesList.Codes.SplitCase, skuUpperCase: false);
		public void TestConversionsTable_SKUHasUOMType_CaseInsensitive_Pallet() => TestConversionsTable_SKUHasUOMType(uomType: UOMPackTypesList.Codes.Pallet, skuUpperCase: false);
		public void TestConversionsTable_SKUHasUOMType_CaseInsensitive_Case() => TestConversionsTable_SKUHasUOMType(uomType: UOMPackTypesList.Codes.Case, skuUpperCase: false);

		void TestConversionsTable_SKUHasUOMType(string uomType, bool skuUpperCase = true)
		{
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOT").F3_UOMType = uomType;

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_StockKeepingUnit = skuUpperCase ? "BOT" : "bot";
			var table1 = GetTable(product1);

			AssertEquals(1.0M, table1["BOT"].QtySKU);
			AssertEquals(uomType, table1["BOT"].UOMType);
		}

		#endregion

		#region TestConversionsTable_SKUAlias

		public void TestConversionsTable_SKUAlias() => TestConversionsTable_SKUAlias(skuUomType: string.Empty, aliasUomType: string.Empty);
		public void TestConversionsTable_SKUAlias_SKUIsPallet() => TestConversionsTable_SKUAlias(skuUomType: UOMPackTypesList.Codes.Pallet, aliasUomType: string.Empty);
		public void TestConversionsTable_SKUAlias_SKUIsCase() => TestConversionsTable_SKUAlias(skuUomType: UOMPackTypesList.Codes.Case, aliasUomType: string.Empty);
		public void TestConversionsTable_SKUAlias_SKUIsSplitCase() => TestConversionsTable_SKUAlias(skuUomType: UOMPackTypesList.Codes.SplitCase, aliasUomType: string.Empty);
		public void TestConversionsTable_SKUAlias_SplitCase() => TestConversionsTable_SKUAlias(skuUomType: UOMPackTypesList.Codes.Pallet, aliasUomType: UOMPackTypesList.Codes.SplitCase);
		public void TestConversionsTable_SKUAlias_Pallet() => TestConversionsTable_SKUAlias(skuUomType: UOMPackTypesList.Codes.SplitCase, aliasUomType: UOMPackTypesList.Codes.Pallet);
		public void TestConversionsTable_SKUAlias_Case() => TestConversionsTable_SKUAlias(skuUomType: UOMPackTypesList.Codes.Pallet, aliasUomType: UOMPackTypesList.Codes.Case);
		public void TestConversionsTable_SKUAlias_CaseInsensitive() => TestConversionsTable_SKUAlias(skuUomType: string.Empty, aliasUomType: string.Empty, skuUpperCase: false);
		public void TestConversionsTable_SKUAlias_CaseInsensitive_SKUIsPallet() => TestConversionsTable_SKUAlias(skuUomType: UOMPackTypesList.Codes.Pallet, aliasUomType: string.Empty, skuUpperCase: false);
		public void TestConversionsTable_SKUAlias_CaseInsensitive_SKUIsCase() => TestConversionsTable_SKUAlias(skuUomType: UOMPackTypesList.Codes.Case, aliasUomType: string.Empty, skuUpperCase: false);
		public void TestConversionsTable_SKUAlias_CaseInsensitive_SKUIsSplitCase() => TestConversionsTable_SKUAlias(skuUomType: UOMPackTypesList.Codes.SplitCase, aliasUomType: string.Empty, skuUpperCase: false);
		public void TestConversionsTable_SKUAlias_CaseInsensitive_SplitCase() => TestConversionsTable_SKUAlias(skuUomType: UOMPackTypesList.Codes.Pallet, aliasUomType: UOMPackTypesList.Codes.SplitCase, skuUpperCase: false);
		public void TestConversionsTable_SKUAlias_CaseInsensitive_Pallet() => TestConversionsTable_SKUAlias(skuUomType: UOMPackTypesList.Codes.SplitCase, aliasUomType: UOMPackTypesList.Codes.Pallet, skuUpperCase: false);
		public void TestConversionsTable_SKUAlias_CaseInsensitive_Case() => TestConversionsTable_SKUAlias(skuUomType: UOMPackTypesList.Codes.Pallet, aliasUomType: UOMPackTypesList.Codes.Case, skuUpperCase: false);

		void TestConversionsTable_SKUAlias(string skuUomType, string aliasUomType, bool skuUpperCase = true)
		{
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "UNT").F3_UOMType = skuUomType;
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOT").F3_UOMType = aliasUomType;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = skuUpperCase ? "UNT" : "unt";

			CreateUnitPart(product, 1m, "UNT", "BOT"); // We consider a bottle to be a "SKU alias", and use the SKU's UOM type in the absence of its own. 

			var table = GetTable(product);

			AssertEquals(1.0M, table["BOT"].QtySKU);
			AssertEquals(string.IsNullOrEmpty(aliasUomType) ? (string)table["UNT"].UOMType : aliasUomType, table["BOT"].UOMType);
		}

		#endregion

		#region TestConversionsTable_PLTHasUOMType

		public void TestConversionsTable_PLTHasUOMType_SplitCase() => TestConversionsTable_PLTHasUOMType(uomType: UOMPackTypesList.Codes.SplitCase);
		public void TestConversionsTable_PLTHasUOMType_Pallet() => TestConversionsTable_PLTHasUOMType(uomType: UOMPackTypesList.Codes.Pallet);
		public void TestConversionsTable_PLTHasUOMType_Case() => TestConversionsTable_PLTHasUOMType(uomType: UOMPackTypesList.Codes.Case);
		public void TestConversionsTable_PLTHasUOMType_CaseInsensitive_SplitCase() => TestConversionsTable_PLTHasUOMType(uomType: UOMPackTypesList.Codes.SplitCase, skuUpperCase: false);
		public void TestConversionsTable_PLTHasUOMType_CaseInsensitive_Pallet() => TestConversionsTable_PLTHasUOMType(uomType: UOMPackTypesList.Codes.Pallet, skuUpperCase: false);
		public void TestConversionsTable_PLTHasUOMType_CaseInsensitive_Case() => TestConversionsTable_PLTHasUOMType(uomType: UOMPackTypesList.Codes.Case, skuUpperCase: false);

		void TestConversionsTable_PLTHasUOMType(string uomType, bool skuUpperCase = true)
		{
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "PLT").F3_UOMType = uomType;

			var product1 = Factory.New<OrgSupplierPart>();
			product1.OP_StockKeepingUnit = skuUpperCase ? "PLT" : "plt";

			var table1 = GetTable(product1);
			AssertEquals(1.0M, table1["PLT"].QtySKU);

			// The user should never set a non-Pallet UOM type for PLT, PLT has special meanings in CW1 as a Pallet.
			AssertEquals(UOMPackTypesList.Codes.Pallet, table1["PLT"].UOMType);
		}

		#endregion

		#region TestConversionsTable_Conversion

		public void TestConversionsTable_Conversion() => TestConversionsTable_Conversion(uomType: string.Empty);
		public void TestConversionsTable_Conversion_SplitCase() => TestConversionsTable_Conversion(uomType: UOMPackTypesList.Codes.SplitCase);
		public void TestConversionsTable_Conversion_Pallet() => TestConversionsTable_Conversion(uomType: UOMPackTypesList.Codes.Pallet);
		public void TestConversionsTable_Conversion_Case() => TestConversionsTable_Conversion(uomType: UOMPackTypesList.Codes.Case);
		public void TestConversionsTable_Conversion_CaseInsensitive() => TestConversionsTable_Conversion(uomType: string.Empty, skuUpperCase: false);
		public void TestConversionsTable_Conversion_CaseInsensitive_SplitCase() => TestConversionsTable_Conversion(uomType: UOMPackTypesList.Codes.SplitCase, skuUpperCase: false);
		public void TestConversionsTable_Conversion_CaseInsensitive_Pallet() => TestConversionsTable_Conversion(uomType: UOMPackTypesList.Codes.Pallet, skuUpperCase: false);
		public void TestConversionsTable_Conversion_CaseInsensitive_Case() => TestConversionsTable_Conversion(uomType: UOMPackTypesList.Codes.Case, skuUpperCase: false);

		void TestConversionsTable_Conversion(string uomType, bool skuUpperCase = true)
		{
			Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, "BOX").F3_UOMType = uomType;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_StockKeepingUnit = skuUpperCase ? "UNT" : "unt";

			CreateUnitPart(product, 10m, "UNT", "BOX");

			var table = GetTable(product);

			AssertEquals(10m, table["BOX"].QtySKU);
			AssertEquals(UOMPackTypesList.Codes.SplitCase, table["UNT"].UOMType);
			AssertEquals(string.IsNullOrEmpty(uomType) ? UOMPackTypesList.Codes.Case : uomType, table["BOX"].UOMType);
		}

		#endregion

		#region Implementation

		protected override string ExpectedUOMTypeForPLT => UOMPackTypesList.Codes.Pallet;

		protected override string ExpectedUOMTypeForSKU => UOMPackTypesList.Codes.SplitCase;

		protected override ConversionsToSKUTable GetTable(OrgSupplierPart product) => new ConversionsToSKUTableForAllocation(product);

		#endregion
	}
}
