using System;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class Product_GetByPartNumOrBarcodeForAllClientsTest : WhsSecureServiceTestCase
	{
		#region TestProduct_GetByPartNumOrBarcodeForAllClients

		public void TestProduct_GetByPartNumOrBarcodeForAllClients()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C3");
			var client4 = Helper.CreateClient("C4");
			var whs = Helper.CreateWarehouse("W1");
			var productWithSupplierOnly = Helper.CreateProduct(client1, "P_SUP", OrgPartRelation.RelationshipTypes.Supplier);
			var productForTwoClients = Helper.CreateProduct(client1, "P_Main", OrgPartRelation.RelationshipTypes.Owner);
			Helper.CreateProductClientRelationShip(client2, productForTwoClients, OrgPartRelation.RelationshipTypes.Both);

			var productForThirdClient = Helper.CreateProduct(client3, "P_Main", OrgPartRelation.RelationshipTypes.Owner);

			var productWithBarcode = Helper.CreateProduct("P_B", client4);
			Helper.CreateProductBarcode(productWithBarcode, "UNT", "P_Main");

			Helper.Factory.Save();

			var webService1 = GetNewWebService();
			webService1.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var responseNoProduct = webService1.Product_GetByPartNumOrBarcodeForAllClients("ZZ");
			AssertEquals("Product ZZ could not be found. Please provide a valid product code or barcode.", responseNoProduct.ErrorMessage);

			var webService2 = GetNewWebService();
			webService2.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var responseProductOnlySupplier = webService2.Product_GetByPartNumOrBarcodeForAllClients("P_SUP");
			AssertEquals("Product P_SUP could not be found. Please provide a valid product code or barcode.", responseProductOnlySupplier.ErrorMessage);

			var webService3 = GetNewWebService();
			webService3.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var responsePositive = webService3.Product_GetByPartNumOrBarcodeForAllClients("P_Main");
			Assert(string.IsNullOrEmpty(responsePositive.ErrorMessage));
			AssertEquals(3, responsePositive.Products.Length);
			AssertContainsExactElementsInAnyOrder(new[] { productForTwoClients.PK, productForThirdClient.PK, productWithBarcode.PK }, responsePositive.Products.Select(p => p.PK));

			AssertEquals(4, responsePositive.ProductPartAttributes.Length);
			var pairs = responsePositive.ProductPartAttributes.Select(x => new Tuple<Guid, Guid>(x.ClientPK, x.ProductPK));
			var expectedPairs = new[]
			{
				new Tuple<Guid, Guid>(client1.PK.ToGuid(), productForTwoClients.PK.ToGuid()),
				new Tuple<Guid, Guid>(client2.PK.ToGuid(), productForTwoClients.PK.ToGuid()),
				new Tuple<Guid, Guid>(client3.PK.ToGuid(), productForThirdClient.PK.ToGuid()),
				new Tuple<Guid, Guid>(client4.PK.ToGuid(), productWithBarcode.PK.ToGuid())
			};
			AssertContainsExactElementsInAnyOrder(expectedPairs, pairs);
		}

		public void TestProduct_GetByPartNumOrBarcodeForAllClients_ProductHasNoRelatedOrganisation()
		{
			var client1 = Helper.CreateClient("C1");
			var product = Helper.CreateProduct("P1", client1);
			var whs = Helper.CreateWarehouse("W1");
			var product_WithoutOrg = Helper.Factory.New<OrgSupplierPart>();
			product_WithoutOrg.OP_PartNum = "P1";

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var response = webService.Product_GetByPartNumOrBarcodeForAllClients("P1");
			Assert(string.IsNullOrEmpty(response.ErrorMessage));
			AssertContainsExactElementsInAnyOrder(new[] { product.PK }, response.Products.Select(p => p.PK));
		}

		public void TestProduct_GetByPartNumOrBarcodeForAllClients_ReturnsProductUnits()
		{
			var client1 = Helper.CreateClient("C1");
			var client2 = Helper.CreateClient("C2");
			var client3 = Helper.CreateClient("C4");
			var whs = Helper.CreateWarehouse("W1");
			var product1 = Helper.CreateProduct(client1, "P_Main", OrgPartRelation.RelationshipTypes.Owner);
			Helper.CreateProductClientRelationShip(client2, product1, OrgPartRelation.RelationshipTypes.Both);

			var product2 = Helper.CreateProduct("P_B", client3);
			Helper.CreateProductBarcode(product2, "UNT", "P_MAIN");
			Helper.Factory.Save();

			var productPartUnits = product1.PartUnits.Cast<OrgPartUnit>();
			AssertEquals("Precondition: Part has 3 part units", 3, productPartUnits.Count());
			AssertEquals("Precondition: Part unit.", true, productPartUnits.Any(partUnit => partUnit.OF_PackType == "UNT" && partUnit.OF_ParentPackType == "CTN"));
			AssertEquals("Precondition: Part unit.", true, productPartUnits.Any(partUnit => partUnit.OF_PackType == "KG" && partUnit.OF_ParentPackType == "UNT"));
			AssertEquals("Precondition: Part unit.", true, productPartUnits.Any(partUnit => partUnit.OF_PackType == "M3" && partUnit.OF_ParentPackType == "UNT"));

			AssertEquals("Precondition: Product has no units.", 0, product2.PartUnits.Count);

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;
			var response = webService.Product_GetByPartNumOrBarcodeForAllClients("P_Main");
			Assert(string.IsNullOrEmpty(response.ErrorMessage));
			AssertEquals(2, response.Products.Length);
			AssertContainsExactElementsInAnyOrder(new[] { product1.PK, product2.PK }, response.Products.Select(p => p.PK));

			var product1OnResponse = response.Products.Single(product => product.Code.Equals("P_MAIN"));
			AssertNotNull("ProductUnits", product1OnResponse.ProductUnits);
			AssertEquals("ProductUnits", 3, product1OnResponse.ProductUnits.Count);
			AssertEquals("Product Unit", true, product1OnResponse.ProductUnits.Any(productUnit => productUnit.Package == "UNT" && productUnit.Parent == "CTN"));
			AssertEquals("Product Unit", true, product1OnResponse.ProductUnits.Any(productUnit => productUnit.Package == "KG" && productUnit.Parent == "UNT"));
			AssertEquals("Product Unit", true, product1OnResponse.ProductUnits.Any(productUnit => productUnit.Package == "M3" && productUnit.Parent == "UNT"));

			var product2OnResponse = response.Products.Single(product => product.Code.Equals("P_B"));
			AssertNotNull("ProductUnits", product2OnResponse.ProductUnits);
			AssertEquals("ProductUnits", 0, product2OnResponse.ProductUnits.Count);
		}

		public void TestProduct_GetByPartNumOrBarcodeForAllClients_UsesWarehouseCountryFormatString()
		{
			var client = Helper.CreateClient("C1");
			var part1 = Helper.CreateProduct("P1", client);
			var part2 = Helper.CreateProduct("P2", client);
			var whs = Helper.CreateWarehouse("WHS");

			Helper.SetClientAllAttributeType(client, true);
			Helper.SetProductAllAttributeUse(client, part1, true);
			Helper.SetProductAllAttributeUse(client, part2, true);

			Helper.Factory.Save();

			var webService = GetNewWebService();
			webService.SecurityHeader.WarehouseCode = whs.WW_WarehouseCode;

			var response = webService.Product_GetByPartNumOrBarcodeForAllClients("P1");

			AssertEquals("ddMMyy", response.ProductPartAttributes[0].ExpiryDateFormatString);
			AssertEquals("ddMMyy", response.ProductPartAttributes[0].PackingDateFormatString);

			whs.WarehouseAddress.OA_RN_NKCountryCode = "CN";
			Helper.Factory.Save();

			response = webService.Product_GetByPartNumOrBarcodeForAllClients("P2");

			AssertEquals("yyMMdd", response.ProductPartAttributes[0].ExpiryDateFormatString);
			AssertEquals("yyMMdd", response.ProductPartAttributes[0].PackingDateFormatString);
		}

		#endregion
	}
}
