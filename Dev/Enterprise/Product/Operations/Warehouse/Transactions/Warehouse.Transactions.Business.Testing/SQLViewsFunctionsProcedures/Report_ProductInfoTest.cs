using CargoWise.EntityFramework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class Report_ProductInfoTest : WhsTestCaseWithFactory
	{
		public void TestView_UnitsPerPallet()
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

			var sqlCommand = @"select * from dbo.vw_Report_ProductInfo";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sqlCommand);
			AssertEquals(1, result.Count);

			AssertEquals("PK", part.PK, result[0]["OP_PK"]);
			AssertEquals("Should take shortest path (PLT->BOX->UNT) to calculate pallet size (30 UNT)", 30m,
				result[0]["OP_UnitsPerPallet"]);
		}
	}
}
