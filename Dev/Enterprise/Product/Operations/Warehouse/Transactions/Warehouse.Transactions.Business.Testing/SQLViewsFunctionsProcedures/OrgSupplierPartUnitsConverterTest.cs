using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class OrgSupplierPartUnitsConverterTest : WhsTestCaseWithFactory
	{
		public void TestFunction()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "TSTPROD");

			AssertEquals(0m, GetConversionMultiplier(part, "PLT", "UNT"));
			AssertEquals(1m, GetConversionMultiplier(part, "PLT", "PLT"));

			Helper.CreateProductUnit(part, "UNT", "PLT", 10);
			Helper.CreateProductUnit(part, "UNT", "BAG", 5);
			Helper.CreateProductUnit(part, "BOX", "BAG", 2);
			Helper.CreateProductUnit(part, "SKD", "CRD", 3);

			Factory.Save();

			AssertConversion(part, "PLT", "UNT");
			AssertConversion(part, "UNT", "PLT");

			AssertConversion(part, "BAG", "UNT");
			AssertConversion(part, "UNT", "BAG");

			AssertConversion(part, "BAG", "PLT");
			AssertConversion(part, "PLT", "BAG");

			AssertConversion(part, "BOX", "PLT");
			AssertConversion(part, "PLT", "BOX");

			AssertConversion(part, "SKD", "CRD");
			AssertConversion(part, "CRD", "SKD");

			AssertConversion(part, "SKD", "PLT");
			AssertConversion(part, "PLT", "SKD");
		}

		void AssertConversion(OrgSupplierPart part, ZString fromUQ, ZString toUQ)
		{
			AssertEquals("Conversion between " + fromUQ + " and " + toUQ,
				Utilities.Round(part.UnitConverter.ConversionFactor(fromUQ, toUQ), 2),
				Utilities.Round(GetConversionMultiplier(part, fromUQ, toUQ), 2));
		}

		ZDecimal GetConversionMultiplier(OrgSupplierPart part, string fromUQ, string toUQ)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql =
				@"select ConversionFactor as Multiplier from dbo.OrgSupplierPartUnitsConverter(@ProductPK, @FromUQ, @ToUQ) as tableResult";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@ProductPK", part.PK, OrgSupplierPartSchema.PK);
			sqlParams.Add("@FromUQ", fromUQ, OrgPartUnitSchema.OF_PackType);
			sqlParams.Add("@ToUQ", toUQ, OrgPartUnitSchema.OF_ParentPackType);

			result.Load(sql, sqlParams);
			var dynamicBizO = result[0];
			return (ZDecimal)(dynamicBizO["Multiplier"]);
		}
	}
}
