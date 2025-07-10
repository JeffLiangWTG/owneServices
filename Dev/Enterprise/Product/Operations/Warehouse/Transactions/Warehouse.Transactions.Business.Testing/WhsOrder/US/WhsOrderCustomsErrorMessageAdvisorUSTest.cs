using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsOrderCustomsErrorMessageAdvisorUSTest : WhsTestCaseWithFactory
	{
		public void TestProductsMatches()
		{
			var clientPK = ZGuid.NewZGuid();
			var product1PK = ZGuid.NewZGuid();
			var product2PK = ZGuid.NewZGuid();
			var productWithAttributes1 =
				new ProductWithAttributes(clientPK, product1PK, "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", 10m);
			var productWithAttributes2 =
				new ProductWithAttributes(clientPK, product1PK, "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", 20m);
			var productWithAttributes3 =
				new ProductWithAttributes(clientPK, product2PK, "", "", "", "", ZDate.Empty, ZDate.Empty, "", "", 30m);

			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributes2));
			Assert(
				!WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1, productWithAttributes3));
		}

		public void TestProductsMatches_WithPartAttrib1()
		{
			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var productWithAttributes1 =
				new ProductWithAttributes(clientPK, productPK, "A", "", "", "", ZDate.Empty, ZDate.Empty, "", "", 10m);
			var productWithAttributes2 =
				new ProductWithAttributes(clientPK, productPK, "A", "", "", "", ZDate.Empty, ZDate.Empty, "", "", 20m);
			var productWithAttributesLowerCase =
				new ProductWithAttributes(clientPK, productPK, "a", "", "", "", ZDate.Empty, ZDate.Empty, "", "", 20m);
			var productWithAttributes3 =
				new ProductWithAttributes(clientPK, productPK, "B", "", "", "", ZDate.Empty, ZDate.Empty, "", "", 30m);

			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributes2));
			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributesLowerCase));
			Assert(
				!WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1, productWithAttributes3));
		}

		public void TestProductsMatches_WithPartAttrib2()
		{
			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var productWithAttributes1 =
				new ProductWithAttributes(clientPK, productPK, "", "A", "", "", ZDate.Empty, ZDate.Empty, "", "", 10m);
			var productWithAttributes2 =
				new ProductWithAttributes(clientPK, productPK, "", "A", "", "", ZDate.Empty, ZDate.Empty, "", "", 20m);
			var productWithAttributesLowerCase =
				new ProductWithAttributes(clientPK, productPK, "", "A", "", "", ZDate.Empty, ZDate.Empty, "", "", 20m);
			var productWithAttributes3 =
				new ProductWithAttributes(clientPK, productPK, "", "B", "", "", ZDate.Empty, ZDate.Empty, "", "", 30m);

			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributes2));
			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributesLowerCase));
			Assert(
				!WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1, productWithAttributes3));
		}

		public void TestProductsMatches_WithPartAttrib3()
		{
			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var productWithAttributes1 =
				new ProductWithAttributes(clientPK, productPK, "", "", "A", "", ZDate.Empty, ZDate.Empty, "", "", 10m);
			var productWithAttributes2 =
				new ProductWithAttributes(clientPK, productPK, "", "", "A", "", ZDate.Empty, ZDate.Empty, "", "", 20m);
			var productWithAttributesLowerCase =
				new ProductWithAttributes(clientPK, productPK, "", "", "a", "", ZDate.Empty, ZDate.Empty, "", "", 20m);
			var productWithAttributes3 =
				new ProductWithAttributes(clientPK, productPK, "", "", "B", "", ZDate.Empty, ZDate.Empty, "", "", 30m);

			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributes2));
			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributesLowerCase));
			Assert(
				!WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1, productWithAttributes3));
		}

		public void TestProductsMatches_WithSerialNumber()
		{
			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var productWithAttributes1 = new ProductWithAttributes(clientPK, productPK, "", "", "", "SER1", ZDate.Empty,
				ZDate.Empty, "", "", 1m);
			var productWithAttributes2 = new ProductWithAttributes(clientPK, productPK, "", "", "", "SER1", ZDate.Empty,
				ZDate.Empty, "", "", 1m);
			var productWithAttributesLowerCase = new ProductWithAttributes(clientPK, productPK, "", "", "", "ser1", ZDate.Empty,
				ZDate.Empty, "", "", 1m);
			var productWithAttributes3 = new ProductWithAttributes(clientPK, productPK, "", "", "", "SER2", ZDate.Empty,
				ZDate.Empty, "", "", 1m);

			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributes2));
			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributesLowerCase));
			Assert(
				!WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1, productWithAttributes3));
		}

		public void TestProductsMatches_WithPackingDate()
		{
			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var productWithAttributes1 =
				new ProductWithAttributes(clientPK, productPK, "", "", "", "", ZDate.Today, ZDate.Empty, "", "", 10m);
			var productWithAttributes2 =
				new ProductWithAttributes(clientPK, productPK, "", "", "", "", ZDate.Today, ZDate.Empty, "", "", 20m);
			var productWithAttributes3 = new ProductWithAttributes(clientPK, productPK, "", "", "", "",
				ZDate.Today.AddDays(2), ZDate.Empty, "", "", 30m);

			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributes2));
			Assert(
				!WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1, productWithAttributes3));
		}

		public void TestProductsMatches_WithExpiryDate()
		{
			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var productWithAttributes1 =
				new ProductWithAttributes(clientPK, productPK, "", "", "", "", ZDate.Today, ZDate.Today, "", "", 10m);
			var productWithAttributes2 =
				new ProductWithAttributes(clientPK, productPK, "", "", "", "", ZDate.Today, ZDate.Today, "", "", 20m);
			var productWithAttributes3 = new ProductWithAttributes(clientPK, productPK, "", "", "", "", ZDate.Empty,
				ZDate.Today.AddDays(2), "", "", 30m);

			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributes2));
			Assert(
				!WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1, productWithAttributes3));
		}

		public void TestProductsMatches_WithBondedEntryKey()
		{
			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var productWithAttributes1 = new ProductWithAttributes(clientPK, productPK, "", "", "", "", ZDate.Empty,
				ZDate.Empty, "ENTRY-1", "", 10m);
			var productWithAttributes2 = new ProductWithAttributes(clientPK, productPK, "", "", "", "", ZDate.Empty,
				ZDate.Empty, "ENTRY-1", "", 20m);
			var productWithAttributesLowerCase = new ProductWithAttributes(clientPK, productPK, "", "", "", "", ZDate.Empty,
				ZDate.Empty, "entry-1", "", 20m);
			var productWithAttributes3 = new ProductWithAttributes(clientPK, productPK, "", "", "", "", ZDate.Empty,
				ZDate.Empty, "ENTRY-2", "", 30m);

			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributes2));
			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributesLowerCase));
			Assert(
				!WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1, productWithAttributes3));
		}

		public void TestProductsMatches_WithAllocationKey()
		{
			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var productWithAttributes1 = new ProductWithAttributes(clientPK, productPK, "", "", "", "", ZDate.Empty,
				ZDate.Empty, "", "ALO-1", 10m);
			var productWithAttributes2 = new ProductWithAttributes(clientPK, productPK, "", "", "", "", ZDate.Empty,
				ZDate.Empty, "", "ALO-1", 20m);
			var productWithAttributesLowerCase = new ProductWithAttributes(clientPK, productPK, "", "", "", "", ZDate.Empty,
				ZDate.Empty, "", "alo-1", 20m);
			var productWithAttributes3 = new ProductWithAttributes(clientPK, productPK, "", "", "", "", ZDate.Empty,
				ZDate.Empty, "", "ALO-2", 30m);

			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributes2));
			Assert(WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1,
				productWithAttributesLowerCase));
			Assert(
				!WhsOrderCustomsErrorMessageAdvisorUS.ProductsMatches(productWithAttributes1, productWithAttributes3));
		}
	}
}
