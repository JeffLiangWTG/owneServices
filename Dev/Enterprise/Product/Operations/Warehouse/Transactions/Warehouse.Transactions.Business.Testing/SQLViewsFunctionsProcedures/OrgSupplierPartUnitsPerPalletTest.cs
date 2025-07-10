using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class OrgSupplierPartUnitsPerPalletTest : WhsTestCaseWithFactory
	{
		public void TestReturnsNullWhenNoConversionToPltExists()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "PRODUCT");
			part.OP_StockKeepingUnit = "UNT";
			// no conversion to PLT
			Helper.CreateProductUnit(part, "CRT", "BOX", 4);
			Helper.CreateProductUnit(part, "UNT", "CRT", 2);
			Factory.Save();

			var sql =
				@"select UnitsPerPallet as UnitsPerPallet from dbo.OrgSupplierPartUnitsPerPallet(@ProductPK) as tableResult";
			var command = TestConnection.Command(sql);
			command.AddParameter("@ProductPK", SqlDbType.UniqueIdentifier, part.PK.ToGuid());
			AssertEquals("Should return NULL", DBNull.Value, command.ExecuteScalar());
		}

		public void TestWhenPalletIsStockKeepingUnit()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "PRODUCT");
			part.OP_StockKeepingUnit = "PLT";
			Helper.CreateProductUnit(part, "CRT", "PLT", 4);
			Helper.CreateProductUnit(part, "UNT", "CRT", 2);
			Factory.Save();

			AssertEquals(1m, GetUnitsPerPallet(part));
		}

		public void TestMixedConversionWherePltIsParentPackType()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "PRODUCT");
			part.OP_StockKeepingUnit = "UNT";
			Helper.CreateProductUnit(part, "BOX", "PLT", 10);
			Helper.CreateProductUnit(part, "BOX", "UNT", 0.5m); // 2 unt in a box
			Factory.Save();

			AssertEquals("Should be 20 units in a pallet", 20m, GetUnitsPerPallet(part));
		}

		public void TestMixedConversionWherePltIsPackType()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "PRODUCT");
			part.OP_StockKeepingUnit = "UNT";
			Helper.CreateProductUnit(part, "PLT", "BOX", 0.1m);
			Helper.CreateProductUnit(part, "UNT", "BOX", 2);
			Factory.Save();

			AssertEquals("Should be 20 units in a pallet", 20m, GetUnitsPerPallet(part));
		}

		public void TestConversionChoosesShortestPath()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "PRODUCT");
			part.OP_StockKeepingUnit = "UNT";

			// PLT -> 5 CAS -> 15 PCK -> 45 UNT
			Helper.CreateProductUnit(part, "CAS", "PLT", 5m);
			Helper.CreateProductUnit(part, "PCK", "CAS", 3m);
			Helper.CreateProductUnit(part, "UNT", "PCK", 3m);

			// PLT -> 10 BOX -> 30 UNT
			Helper.CreateProductUnit(part, "BOX", "PLT", 10m);
			Helper.CreateProductUnit(part, "UNT", "BOX", 3m);

			Factory.Save();

			AssertEquals("Should take shortest path (PLT->BOX->UNT) to calculate pallet size (30 UNT)", 30m,
				GetUnitsPerPallet(part));
		}

		public void TestConversionReturnsNullIfManyConversionExistsWithSameLength()
		{
			var client = Helper.CreateClient();
			var part = Helper.CreateProduct(client, "PRODUCT");
			part.OP_StockKeepingUnit = "UNT";

			// PLT -> 15 PCK -> 45 UNT
			Helper.CreateProductUnit(part, "PCK", "PLT", 15m);
			Helper.CreateProductUnit(part, "UNT", "PCK", 3m);

			// PLT -> 10 BOX -> 30 UNT
			Helper.CreateProductUnit(part, "BOX", "PLT", 10m);
			Helper.CreateProductUnit(part, "UNT", "BOX", 3m);

			Factory.Save();

			var sql = @"select UnitsPerPallet from dbo.OrgSupplierPartUnitsPerPallet(@ProductPK) as tableResult";
			var command = TestConnection.Command(sql);
			command.AddParameter("@ProductPK", SqlDbType.UniqueIdentifier, part.PK.ToGuid());
			AssertEquals("Should return NULL", DBNull.Value, command.ExecuteScalar());
		}

		ZDecimal GetUnitsPerPallet(OrgSupplierPart part)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"select UnitsPerPallet from dbo.OrgSupplierPartUnitsPerPallet(@ProductPK) as tableResult";

			var param = new ZSqlParameterCollection();
			param.Add("@ProductPK", part.PK, OrgSupplierPartSchema.PK);

			result.Load(sql, param);
			var bizo = result[0];
			return (ZDecimal)(bizo["UnitsPerPallet"]);
		}
	}
}
