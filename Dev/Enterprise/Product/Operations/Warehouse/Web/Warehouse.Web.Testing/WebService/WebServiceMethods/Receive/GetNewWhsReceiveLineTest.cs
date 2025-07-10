using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetNewWhsReceiveLineTest : WhsSecureServiceTestCase
	{
		#region GetNewWhsReceiveLine

		#region TestGetNewWhsReceiveLine

		public void TestGetNewWhsReceiveLine_PartCode()
		{
			var whs = Helper.CreateWarehouse("WHS1");
			CreateTestDataForNewGetWhsReceiveLine();

			var docketPK = Guid.NewGuid();

			var webService1 = GetNewWebService(whs);
			var response1 = webService1.GetNewWhsReceiveLine("Part1", "CLIENT1", docketPK);
			AssertSuccessfulResponse(response1, webService1);
			AssertDocketLineInfo(response1.LineInfo, "UNT", "PART1",
				"Attr1", true, true,
				"Attr2", false, true,
				"", false, false);
			AssertUnitConversions(response1.Conversions, response1.LineInfo.Product.PK, "UNT", "CTN");

			var webService2 = GetNewWebService(whs);
			var response2 = webService2.GetNewWhsReceiveLine("Part2", "CLIENT1", docketPK);
			AssertSuccessfulResponse(response2, webService2);
			AssertDocketLineInfo(response2.LineInfo, "KG", "PART2",
				"Attr1", true, false,
				"Attr2", false, true,
				"", false, false);
			AssertUnitConversions(response2.Conversions, response2.LineInfo.Product.PK, "KG");

			var webService3 = GetNewWebService(whs);
			var response3 = webService3.GetNewWhsReceiveLine("Part2", "CLIENT2", docketPK);
			AssertSuccessfulResponse(response3, webService3);
			AssertDocketLineInfo(response3.LineInfo, "KG", "PART2",
				"Batch No", true, false,
				"Color", false, true,
				"Vehicle No", true, true);
			AssertUnitConversions(response3.Conversions, response3.LineInfo.Product.PK, "KG");

			var webService4 = GetNewWebService(whs);
			var response4 = webService4.GetNewWhsReceiveLine("Part1", "CLIENT2", docketPK);
			AssertSuccessfulResponse(response4, webService4);
			AssertEquals("", response4.LineInfo.Product.Code);
			AssertUnitConversions(response4.Conversions, Guid.Empty);
			AssertEquals("Product could not be found.(Client: CLIENT2) Please provide a valid product code or barcode.", response4.ErrorMessage);
		}

		public void TestGetNewWhsReceiveLine_Barcode()
		{
			var whs = Helper.CreateWarehouse("WHS1");
			CreateTestDataForNewGetWhsReceiveLine();

			var docketPK = Guid.NewGuid();

			var webService1 = GetNewWebService(whs);
			var response1 = webService1.GetNewWhsReceiveLine("12345", "CLIENT1", docketPK);
			AssertSuccessfulResponse(response1, webService1);
			AssertDocketLineInfo(response1.LineInfo, "CAS", "PART1",
				"Attr1", true, true,
				"Attr2", false, true,
				"", false, false);

			var webService2 = GetNewWebService(whs);
			var response2 = webService2.GetNewWhsReceiveLine("54321", "CLIENT1", docketPK);
			AssertSuccessfulResponse(response2, webService2);
			AssertDocketLineInfo(response2.LineInfo, "LB", "PART2",
				"Attr1", true, false,
				"Attr2", false, true,
				"", false, false);

			var webService3 = GetNewWebService(whs);
			var response3 = webService3.GetNewWhsReceiveLine("54321", "CLIENT2", docketPK);
			AssertSuccessfulResponse(response3, webService3);
			AssertDocketLineInfo(response3.LineInfo, "LB", "PART2",
				"Batch No", true, false,
				"Color", false, true,
				"Vehicle No", true, true);

			var webService4 = GetNewWebService(whs);
			var response4 = webService4.GetNewWhsReceiveLine("12345", "CLIENT2", docketPK);
			AssertSuccessfulResponse(response4, webService4);
			AssertEquals("", response4.LineInfo.Product.Code);
			AssertEquals("Product could not be found.(Client: CLIENT2) Please provide a valid product code or barcode.", response4.ErrorMessage);
		}

		void CreateTestDataForNewGetWhsReceiveLine()
		{
			var client1 = Helper.CreateClient("CLIENT1");
			Helper.SetClientAttributeType(client1, AttributeNumber.One, PartAttributeTypeList.Codes.Mandatory, "Attr1");
			Helper.SetClientAttributeType(client1, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Attr2");
			client1.MiscServ.OM_IMPartAttrib3Name = "";

			var client2 = Helper.CreateClient("CLIENT2");
			Helper.SetClientAttributeType(client2, AttributeNumber.One, PartAttributeTypeList.Codes.BatchNumber, "Batch No");
			Helper.SetClientAttributeType(client2, AttributeNumber.Two, PartAttributeTypeList.Codes.NonMandatory, "Color");
			Helper.SetClientAttributeType(client2, AttributeNumber.Three, PartAttributeTypeList.Codes.VIN, "Vehicle No");

			var client3 = Helper.CreateClient("CLIENT3");
			client3.MiscServ.OM_WhsCheckPartWeightOrDimsOnReceive = "ALL";

			var part1 = Helper.CreateProduct(client1, "Part1");
			part1.OP_StockKeepingUnit = "UNT";
			Helper.CreateProductClientRelationShip(client3, part1);
			Helper.CreateProductBarcode(part1, "CAS", "12345");
			Helper.SetProductAttributeUse(client1, part1, AttributeNumber.One, true);
			Helper.SetProductAttributeUse(client1, part1, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(client1, part1, AttributeNumber.Three, false);

			var part2 = Helper.CreateProduct(client2, "Part2");
			part2.OP_StockKeepingUnit = "KG";
			Helper.CreateProductClientRelationShip(client1, part2);
			Helper.CreateProductBarcode(part2, "LB", "54321");
			Helper.SetProductAttributeUse(client2, part2, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(client2, part2, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(client2, part2, AttributeNumber.Three, true);
			Helper.SetProductAttributeUse(client1, part2, AttributeNumber.One, false);
			Helper.SetProductAttributeUse(client1, part2, AttributeNumber.Two, true);
			Helper.SetProductAttributeUse(client1, part2, AttributeNumber.Three, false);

			var part3 = Helper.CreateProduct(client3, "Part3");
			part3.OP_StockKeepingUnit = "TON";
			var partUnit = part3.PartUnits[0];
			partUnit.OF_Depth = 1m;
			partUnit.OF_Weight = 1m;
			partUnit.OF_Height = 1m;
			Helper.CreateProductBarcode(part3, "UNT", "23456");

			Helper.Factory.Save();
		}

		void AssertDocketLineInfo(WhsDocketLineInfo lineInfo, string expectedPackUQ, string expectedProductCode,
			string expectedAttribute1Caption, bool expectedAttribute1IsMandatory, bool expectedAttribute1IsUsed,
			string expectedAttribute2Caption, bool expectedAttribute2IsMandatory, bool expectedAttribute2IsUsed,
			string expectedAttribute3Caption, bool expectedAttribute3IsMandatory, bool expectedAttribute3IsUsed)
		{
			AssertEquals("PackUQ should be equal", expectedPackUQ, lineInfo.PackUQ);
			AssertNotNull("PartAttributes should not be null", lineInfo.PartAttributes);
			AssertNotNull("Product should not be null", lineInfo.Product);
			AssertEquals("ProductCode should be equal", expectedProductCode, lineInfo.Product.Code);
			AssertEquals("Attribute1Caption should be equal", expectedAttribute1Caption, lineInfo.PartAttributes.Attribute1Caption);
			AssertEquals("Attribute1IsMandatory should be equal", expectedAttribute1IsMandatory, lineInfo.PartAttributes.Attribute1IsMandatory);
			AssertEquals("Attribute1IsUsed should be equal", expectedAttribute1IsUsed, lineInfo.PartAttributes.Attribute1IsUsed);
			AssertEquals("Attribute2Caption should be equal", expectedAttribute2Caption, lineInfo.PartAttributes.Attribute2Caption);
			AssertEquals("Attribute2IsMandatory should be equal", expectedAttribute2IsMandatory, lineInfo.PartAttributes.Attribute2IsMandatory);
			AssertEquals("Attribute2IsUsed should be equal", expectedAttribute2IsUsed, lineInfo.PartAttributes.Attribute2IsUsed);
			AssertEquals("Attribute3Caprtion should be equal", expectedAttribute3Caption, lineInfo.PartAttributes.Attribute3Caption);
			AssertEquals("Attribute3IsMandatory should be equal", expectedAttribute3IsMandatory, lineInfo.PartAttributes.Attribute3IsMandatory);
			AssertEquals("Attribute3IsUsed should be equal", expectedAttribute3IsUsed, lineInfo.PartAttributes.Attribute3IsUsed);
		}

		void AssertUnitConversions(UnitConversionCollection conversions, Guid expectedProductKey, params string[] expectedPackTypes)
		{
			AssertEquals(expectedProductKey, conversions.ProductPK);
			AssertSequencesEqual(expectedPackTypes, conversions.Conversions.Select(c => c.PackType));
		}

		#endregion

		#region TestGetNewWhsReceiveLine_DBHits

		public void TestGetNewWhsReceiveLine_DBHits()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory, saveFactory_doNotUseForNewTests: false);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.CreateProductBarcode(data.Part1, "UNT", "P1Barcode");
			Helper.Factory.Save();

			// Test using product code
			var dbHits = new Dictionary<string, int>()
			{
				{ OrgHeaderSchema.Constants.TableName, 1 },                // Load Client
				{ WhsWarehouseSchema.Constants.TableName, 1 },             // Load Warehouse
				{ OrgSupplierPartSchema.Constants.TableName, 1 },          // Load Product by PartNumber or Barcode
				{ OrgMiscServSchema.Constants.TableName, 1 },              // Populate Attribute Usage + Julian Batch Validation (Data Object)
				{ OrgPartUnitSchema.Constants.TableName, 1 },              // Populate Unit Conversions (Data Object)
				{ OrgSupplierPartBarcodeSchema.Constants.TableName, 1 },   // Populate Barcodes (Data Object)
				{ OrgPartRelationSchema.Constants.TableName, 1 },          // Populate Attribute Usage + Julian Batch Validation (Data Object)
				{ WhsPickFaceSchema.Constants.TableName, 1 },              // Populate pickfaces for product
				{ RefPacksSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
			};
			var docketPK = Guid.NewGuid();

			var newService1 = GetNewWebService();
			SetupSecurityHeader(newService1, data.Whs1, staff);
			var response1 = newService1.GetNewWhsReceiveLine("P1", data.Org1.OH_Code, docketPK);
			AssertEquals("P1", response1.LineInfo.Product.Code);
			AssertEquals("", (ZString)response1.ErrorMessage);
			TestCaseWithFactory.AssertDbHits(dbHits, newService1.Factory);

			// Test using barcode
			var newService2 = GetNewWebService();
			SetupSecurityHeader(newService2, data.Whs1, staff);

			using (RowFactory.SetCachedTables())
			{
				var response2 = newService2.GetNewWhsReceiveLine("P1Barcode", data.Org1.OH_Code, docketPK);
				AssertEquals("P1", response2.LineInfo.Product.Code);
				AssertEquals("", (ZString)response2.ErrorMessage);
			}

			dbHits.Remove(GlbBranchSchema.Constants.TableName);
			dbHits.Remove(GlbCompanySchema.Constants.TableName);
			dbHits.Remove(GlbStaffSchema.Constants.TableName);
			dbHits.Add(RefPackTypeSchema.Constants.TableName, 1);

			TestCaseWithFactory.AssertDbHits(dbHits, newService2.Factory);
		}

		#endregion

		#region TestGetNewWhsReceiveLine_InActiveProduct

		public void TestGetNewWhsReceiveLine_InActiveProduct()
		{
			var warehouse = Helper.CreateWarehouse("SYD");
			var client = Helper.CreateClient("TESTCLIENT");
			var receive = Helper.CreateWhsReceive(client, warehouse, "REF123");
			var product = Helper.CreateProduct(client, "PART1");
			var barcode = product.PartBarcodes.AddNew();
			barcode.PH_F3_NKPackType = "CTN";
			barcode.PH_Barcode = "BARCODE1";
			product.OP_IsActive = false;
			Helper.Factory.Save();
			var docketPK = Guid.NewGuid();

			// Test using product code
			var webService1 = GetNewWebService(warehouse);
			var response1 = webService1.GetNewWhsReceiveLine("PART1", "TESTCLIENT", docketPK);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("", response1.LineInfo.Product.Code);
			AssertEquals("Product could not be found.(Client: TESTCLIENT) Please provide a valid product code or barcode.", response1.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);

			// Test using barcode
			var webService2 = GetNewWebService(warehouse);
			var response2 = webService2.GetNewWhsReceiveLine("BARCODE1", "TESTCLIENT", docketPK);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("", response2.LineInfo.Product.Code);
			AssertEquals("Product could not be found.(Client: TESTCLIENT) Please provide a valid product code or barcode.", response2.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);

			product.OP_IsActive = true;
			Helper.Factory.Save();

			var webService3 = GetNewWebService(warehouse);
			var response3 = webService3.GetNewWhsReceiveLine("PART1", "TESTCLIENT", docketPK);
			AssertSuccessfulResponse(response3, webService3);
			AssertEquals("Product is now active, should be found using product code", product.PK, response3.LineInfo.Product.PK);
			AssertNull(response3.ErrorMessage);
			AssertEquals(ErrorTypes.None, response3.Error);

			var webService4 = GetNewWebService(warehouse);
			var response4 = webService4.GetNewWhsReceiveLine("BARCODE1", "TESTCLIENT", docketPK);
			AssertSuccessfulResponse(response4, webService4);
			AssertEquals("Product is now active, should be found using barcode", product.PK, response4.LineInfo.Product.PK);
			AssertEquals("CTN", response4.LineInfo.PackUQ);
			AssertNull(response3.ErrorMessage);
			AssertEquals(ErrorTypes.None, response4.Error);
		}

		#endregion

		#region  TestGetNewWhsReceiveLine_InvalidCode

		public void TestGetNewWhsReceiveLine_InvalidCode()
		{
			AssertEquals("Precondition: Should not find out any products", 0, Helper.Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, "PART")).Length);
			AssertEquals("Precondition: Should not find out any barcodes", 0, Helper.Factory.Load<OrgSupplierPartBarcode>(new ZQuery(OrgSupplierPartBarcodeSchema.PH_Barcode, "PART")).Length);

			var client = Helper.CreateClient("CLIENT");
			Helper.Factory.Save();
			var docketPK = Guid.NewGuid();

			var webService = GetNewWebService();
			var response = webService.GetNewWhsReceiveLine("PART", "CLIENT", docketPK);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("", response.LineInfo.Product.Code);
			AssertEquals("Product could not be found.(Client: CLIENT) Please provide a valid product code or barcode.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		#endregion

		#region TestGetNewWhsReceiveLine_MissingCode

		public void TestGetNewWhsReceiveLine_MissingCode()
		{
			var docketPK = Guid.NewGuid();

			var webService = GetNewWebService();
			var response = webService.GetNewWhsReceiveLine("", "CLIENT", docketPK);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("", response.LineInfo.Product.Code);
			AssertEquals("Please provide a valid product code or barcode.", response.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
		}

		#endregion

		#region TestGetNewWhsReceiveLine_MissingOrInvalidClientCode

		public void TestGetNewWhsReceiveLine_MissingOrInvalidClientCode()
		{
			AssertEquals("Precondition: Client is not existed", 0, Helper.Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "CLIENT")).Length);
			var docketPK = Guid.NewGuid();

			var webService1 = GetNewWebService();
			var response1 = webService1.GetNewWhsReceiveLine("1234", "CLIENT", docketPK);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("", response1.LineInfo.Product.Code);
			AssertEquals("Please provide a valid client code.", response1.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response1.Error);

			var webService2 = GetNewWebService();
			var response2 = webService2.GetNewWhsReceiveLine("1234", "", docketPK);
			AssertSuccessfulResponse(response2, webService2);
			AssertEquals("", response2.LineInfo.Product.Code);
			AssertEquals("Please provide a valid client code.", response2.ErrorMessage);
			AssertEquals(ErrorTypes.BusinessValidationError, response2.Error);
		}

		#endregion

		#region

		public void TestGetNewWhsReceiveLine_PreventReceiveOfPartsWithoutWeightOrDims()
		{
			var whs = Helper.CreateWarehouse("WHS1");
			CreateTestDataForNewGetWhsReceiveLine();

			var docketPK = Guid.NewGuid();

			var webService1 = GetNewWebService(whs);
			var response1 = webService1.GetNewWhsReceiveLine("Part1", "CLIENT3", docketPK);
			AssertSuccessfulResponse(response1, webService1);
			AssertEquals("", response1.LineInfo.Product.Code);
			AssertUnitConversions(response1.Conversions, Guid.Empty);
			AssertEquals("Product cannot be received as the Product Master is missing weight or dimensions.", response1.ErrorMessage);

			var webService2 = GetNewWebService(whs);
			var response2 = webService2.GetNewWhsReceiveLine("Part3", "CLIENT3", docketPK);
			AssertSuccessfulResponse(response2, webService2);
			AssertDocketLineInfo(response2.LineInfo, "TON", "PART3",
				"", false, false,
				"", false, false,
				"", false, false);
			AssertUnitConversions(response2.Conversions, response2.LineInfo.Product.PK, "TON");
		}

		#endregion

		#endregion
	}
}
