using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class OrgSupplierPartQuantityConverterTest : WhsTestCaseWithFactory
	{
		public void TestFunction()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "P1");
			part.OP_StockKeepingUnit = "BOX";
			part.PartUnits.RemoveAndDeleteAll(); // clean up all conversions set in CreateProduct.

			Helper.CreateProductUnit(part, "BAG", "BOX", 2);
			Helper.CreateProductUnit(part, "UNT", "BAG", 5);

			Helper.CreateProductUnit(part, "BOX", "PLT", 2);
			Helper.CreateProductUnit(part, "PLT", "CTN", 5);

			Factory.Save();

			AssertConversion(part, "BOX", "BOX", 10m); // a to a
			AssertConversion(part, "BOX", "BAG", 10m); // 1 level down
			AssertConversion(part, "BOX", "UNT", 10m); // 2 levels down
			AssertConversion(part, "BOX", "PLT", 10m); // 1 level up
			AssertConversion(part, "BOX", "CTN", 10m); // 2 levels up

			// Assert Huge Values of conversion
			AssertConversion(part, "BOX", "BOX", 10000000000m);
			AssertConversion(part, "BOX", "BAG", 10000000000m);
		}

		public void TestFunctionHandleDecimal()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "P2");
			part.OP_StockKeepingUnit = "CTN";
			part.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(part, "UNT", "CTN", 5000);
			Factory.Save();

			AssertConversion(part, "CTN", "UNT", 0.1m);
		}

		void AssertConversion(OrgSupplierPart part, ZString fromUQ, ZString toUQ, ZDecimal quantityToConvert)
		{
			var expect = Utilities.Round(part.UnitConverter.ConversionFactor(fromUQ, toUQ) * quantityToConvert, 2);
			var actual = Utilities.Round(GetConversionMultiplier(part, fromUQ, toUQ, quantityToConvert), 2);

			AssertEquals("Conversion between " + fromUQ + " and " + toUQ, expect, actual);
		}

		ZDecimal GetConversionMultiplier(OrgSupplierPart part, string fromUQ, string toUQ, ZDecimal quantityToConvert)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql =
				@"select ConversionFactor as Multiplier from dbo.OrgSupplierPartQuantityConverter(@ProductPK, @FromUQ, @ToUQ, @QuantityToConvert) as tableResult";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@ProductPK", part.PK, ZArchitecture.Schema.OrgSupplierPartSchema.PK);
			sqlParams.Add("@FromUQ", fromUQ, OrgPartUnitSchema.OF_PackType);
			sqlParams.Add("@ToUQ", toUQ, OrgPartUnitSchema.OF_ParentPackType);
			sqlParams.Add("@QuantityToConvert", quantityToConvert, OrgPartUnitSchema.OF_QuantityInParent);

			result.Load(sql, sqlParams);
			var dynamicBizO = result[0];
			return (ZDecimal)(dynamicBizO["Multiplier"]);
		}
	}
}
