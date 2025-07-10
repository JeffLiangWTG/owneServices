using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsStocktakeProductFilter))]
	public sealed class WhsStocktakeProductFilterTest : WhsBusinessObjectTestCase
	{
		#region TestSaveAndDeleteBusinessObject

		[ExpectNoExceptions]
		public override void TestSaveAndDeleteBusinessObject()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var stockTake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var productFilter = Helper.CreateWhsStocktakeProductFilter(stockTake, data.Part1);
			Factory.Save();

			productFilter.Delete();
			Factory.Save();
		}

		#endregion

		#region TestProductCode

		public void TestProductCode()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_PartNum = "PRODUCT-1";
			var stockTake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var productFilter = Helper.CreateWhsStocktakeProductFilter(stockTake, data.Part1);
			AssertEquals("PRODUCT-1", productFilter.ProductCode);

			data.Part1.OP_PartNum = "OTHER PRODUCT";
			AssertEquals("OTHER PRODUCT", productFilter.ProductCode);

			productFilter.WSP_OP_Product = ZGuid.Empty;
			AssertEquals("", productFilter.ProductCode);
		}

		#endregion

		#region TestProductDescription

		public void TestProductDescription()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Part1.OP_Desc = "Test description";
			var stockTake = Helper.CreateWhsStocktake(data.Org1, data.Whs1);
			var productFilter = Helper.CreateWhsStocktakeProductFilter(stockTake, data.Part1);
			AssertEquals("Test description", productFilter.ProductDescription);

			data.Part1.OP_Desc = "Another description";
			AssertEquals("Another description", productFilter.ProductDescription);

			productFilter.WSP_OP_Product = ZGuid.Empty;
			AssertEquals("", productFilter.ProductDescription);
		}

		#endregion
	}
}
