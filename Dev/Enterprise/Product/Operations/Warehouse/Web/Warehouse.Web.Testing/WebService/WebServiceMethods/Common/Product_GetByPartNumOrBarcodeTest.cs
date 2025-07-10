using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class Product_GetByPartNumOrBarcodeTest : WhsSecureServiceTestCase
	{
		#region TestProduct

		#region TestProduct_GetByPartNumOrBarcode

		public void TestProduct_GetByPartNumOrBarcode()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var client = data.Org1;
			var part1 = data.Part1;
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.Product_GetByPartNumOrBarcode(part1.OP_PartNum, client.OH_Code);
			AssertSuccessfulResponse(response, webService);

			AssertNotNull("Product", response.Product);
			AssertEquals("Product Code", part1.OP_PartNum, response.Product.Code);
			AssertNotNull("ProductPartAttributes", response.ProductPartAttributes);
		}

		#endregion

		#region TestProduct_GetByPartNumOrBarcode_UsesWarehouseCountryFormatString

		public void TestProduct_GetByPartNumOrBarcode_UsesWarehouseCountryFormatString()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var client = data.Org1;
			var part1 = data.Part1;
			var part2 = data.Part2;
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");

			Helper.SetClientAllAttributeType(client, true);
			Helper.SetProductAllAttributeUse(client, part1, true);
			Helper.SetProductAllAttributeUse(client, part2, true);

			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			var response = webService.Product_GetByPartNumOrBarcode(part1.OP_PartNum, client.OH_Code);

			AssertEquals("ddMMyy", response.ProductPartAttributes.ExpiryDateFormatString);
			AssertEquals("ddMMyy", response.ProductPartAttributes.PackingDateFormatString);

			data.Whs1.WarehouseAddress.OA_RN_NKCountryCode = "CN";
			Helper.Factory.Save();

			response = webService.Product_GetByPartNumOrBarcode(part2.OP_PartNum, client.OH_Code);

			AssertEquals("yyMMdd", response.ProductPartAttributes.ExpiryDateFormatString);
			AssertEquals("yyMMdd", response.ProductPartAttributes.PackingDateFormatString);
		}

		#endregion

		#region TestProduct_GetByPartNumOrBarcodeWithoutClient

		public void TestProduct_GetByPartNumOrBarcodeWithoutClient()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff1 = Helper.CreateGlbStaff("ST1", "Staff1");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff1);
			var response1 = webService1.Product_GetByPartNumOrBarcode(data.Part1.OP_PartNum, "");
			AssertSuccessfulResponse(response1, webService1);

			AssertNotNull("Product", response1.Product);
			AssertEquals("Product Code", data.Part1.OP_PartNum, response1.Product.Code);
			AssertNotNull("ProductPartAttributes", response1.ProductPartAttributes);

			Helper.CreateProductBarcode(data.Part2, "UNT", "ZX123");
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff1);
			var response2 = webService2.Product_GetByPartNumOrBarcode("ZX123", "");
			AssertSuccessfulResponse(response2, webService2);

			AssertNotNull("Product", response2.Product);
			AssertEquals("Product Code", data.Part2.OP_PartNum, response2.Product.Code);
			AssertNotNull("ProductPartAttributes", response2.ProductPartAttributes);
		}

		#endregion

		#region TestProduct_GetByPartNumOrBarcode_ChecksOwnerRelationship

		public void TestProduct_GetByPartNumOrBarcode_ChecksOwnerRelationship()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			data.Part1.RelatedOrganisations.RemoveAndDeleteAll();
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.CreateProductClientRelationShip(data.Org1, data.Part1, OrgPartRelation.RelationshipTypes.Supplier);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.Product_GetByPartNumOrBarcode("P1", data.Org1.OH_Code);
			AssertSuccessfulResponse(response1, webService1);
			AssertNull("Supplier-only relationship, should not return product.", response1.Product);

			var relationShip = Helper.CreateProductClientRelationShip(data.Org1, data.Part1, OrgPartRelation.RelationshipTypes.Owner);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.Product_GetByPartNumOrBarcode("P1", data.Org1.OH_Code);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("Should return product with an 'owner' relationship.", data.Part1.PK, response2.Product.PK);

			relationShip.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			Helper.Factory.Save();

			var webService3 = GetNewWebService(data.Whs1, staff);
			var response3 = webService3.Product_GetByPartNumOrBarcode("P1", data.Org1.OH_Code);
			AssertSuccessfulResponse(response3, webService3);
			AssertEquals("Should return product with a 'both' relationship.", data.Part1.PK, response3.Product.PK);
		}

		#endregion

		#region TestProduct_GetByPartNumOrBarcode_WithBarcodes

		public void TestProduct_GetByPartNumOrBarcode_WithBarcodes()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var newProduct = Helper.CreateProduct(data.Org1, "P3");
			Helper.CreateProductBarcode(data.Part1, "UNT", "P31");
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.Product_GetByPartNumOrBarcode("P3", data.Org1.OH_Code);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(newProduct.PK, response1.Product.PK);

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.Product_GetByPartNumOrBarcode("P31", data.Org1.OH_Code);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals(data.Part1.PK, response2.Product.PK);
		}

		#endregion

		#region TestProduct_GetByPartNumOrBarcode_WithJulianBatchNoProduct

		public void TestProduct_GetByPartNumOrBarcode_WithJulianBatchNoProduct()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
			Helper.Factory.Save();

			var webService1 = GetNewWebService(data.Whs1, staff);
			var response1 = webService1.Product_GetByPartNumOrBarcode(data.Part1.OP_PartNum, data.Org1.OH_Code);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals(data.Part1.PK, response1.Product.PK);

			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true);
			Helper.Factory.Save();

			var webService2 = GetNewWebService(data.Whs1, staff);
			var response2 = webService2.Product_GetByPartNumOrBarcode(data.Part1.OP_PartNum, data.Org1.OH_Code);
			AssertNull(response2.Product);
			AssertEquals(Transactions.Business.PartAttributeValidation.JulianBatchNumberNoMaximumShelfLifeErrorMessage, response2.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);

			var productParams = WhsProduct.GetWhsProduct(data.Part1).ParamsByWhsAndClient.AddNew();
			productParams.W3_WW = data.Whs1.PK;
			productParams.W3_OH = data.Org1.PK;
			productParams.W3_MaximumShelfLife = 10;
			Helper.Factory.Save();

			var webService3 = GetNewWebService(data.Whs1, staff);
			var response3 = webService3.Product_GetByPartNumOrBarcode(data.Part1.OP_PartNum, data.Org1.OH_Code);
			AssertSuccessfulResponse(response3, webService3);
			AssertEquals(data.Part1.PK, response3.Product.PK);
		}

		#endregion

		#region TestProduct_GetByPartNumOrBarcode_ReturnsProductUnits

		public void TestProduct_GetByPartNumOrBarcode_ReturnsProductUnits()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var client = data.Org1;
			var part1 = data.Part1;
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			Helper.Factory.Save();

			var productPartUnits = part1.PartUnits.Cast<OrgPartUnit>();
			AssertEquals("Precondition: Part has 3 part units", 3, productPartUnits.Count());
			AssertEquals("Precondition: Part unit.", true, productPartUnits.Any(partUnit => partUnit.OF_PackType == "UNT" && partUnit.OF_ParentPackType == "CTN"));
			AssertEquals("Precondition: Part unit.", true, productPartUnits.Any(partUnit => partUnit.OF_PackType == "KG" && partUnit.OF_ParentPackType == "UNT"));
			AssertEquals("Precondition: Part unit.", true, productPartUnits.Any(partUnit => partUnit.OF_PackType == "M3" && partUnit.OF_ParentPackType == "UNT"));

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.Product_GetByPartNumOrBarcode(part1.OP_PartNum, client.OH_Code);
			AssertSuccessfulResponse(response, webService);

			AssertNotNull("Product", response.Product);
			AssertEquals("Product Code", part1.OP_PartNum, response.Product.Code);
			AssertNotNull("ProductPartAttributes", response.ProductPartAttributes);
			AssertNotNull("ProductUnits", response.Product.ProductUnits);

			AssertEquals("Product Unit", true, response.Product.ProductUnits.Any(productUnit => productUnit.Package == "UNT" && productUnit.Parent == "CTN"));
			AssertEquals("Product Unit", true, response.Product.ProductUnits.Any(productUnit => productUnit.Package == "KG" && productUnit.Parent == "UNT"));
			AssertEquals("Product Unit", true, response.Product.ProductUnits.Any(productUnit => productUnit.Package == "M3" && productUnit.Parent == "UNT"));
		}

		#endregion

		#region TestProduct_GetByPartNumOrBarcode_Error

		public void TestProduct_GetByPartNumOrBarcode_NoProductFound()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var client = data.Org1;
			client.OH_Code = "CLIENT";
			var staff1 = Helper.CreateGlbStaff("ST1", "ST1");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff1);
			var response = webService.Product_GetByPartNumOrBarcode("ABC", client.OH_Code);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Product could not be found.(Client: CLIENT) Please provide a valid product code or barcode.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
			AssertNull("Product", response.Product);
		}

		#endregion

		#endregion
	}
}
