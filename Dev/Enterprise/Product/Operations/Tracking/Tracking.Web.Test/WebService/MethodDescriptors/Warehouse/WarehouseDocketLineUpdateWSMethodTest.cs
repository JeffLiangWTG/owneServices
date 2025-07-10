using System;
using System.Collections.Generic;
using System.Web;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.Testing
{
	abstract class WarehouseDocketLineUpdateWSMethodTest<T> : TrackingWebServiceMethodTest<T>
			where T : IWebServiceMethod
	{
		public void TestProductChange()
		{
			var updateParameters = GetLineUpdateParameters();
			var warehouseOrderLineUpdateWSMethod = GetNewWebServiceMethod();

			SetupForTesting(updateParameters);

			updateParameters.ModifiedControlID = updateParameters.ProductControlID;
			updateParameters.NewValue = "Part1";

			var expectedPackUQList = new CodeDescriptionPairList();
			expectedPackUQList.AddPair("PCE", "Piece");
			expectedPackUQList.AddPair("UT1", "");

			var updatePacksUQListToken = new UpdateListResponseToken("PacksUQ", expectedPackUQList);
			updatePacksUQListToken.Conditions.Add(new ResponseConditionEqualToken("Product", "Part1"));

			var updateDescriptionToken = new UpdateValueResponseToken("Description", "Part One");
			updateDescriptionToken.Conditions.Add(new ResponseConditionEqualToken("Description", ""));

			var updatePacksUQToken = new UpdateValueResponseToken("PacksUQ", "PCE");
			updatePacksUQToken.Conditions.Add(new ResponseConditionEqualToken("PacksUQ", ""));

			var updateQuantityUQToken = new UpdateValueResponseToken("QuantityUQ", "Piece");
			updateQuantityUQToken.Conditions.Add(new ResponseConditionEqualToken("QuantityUQ", "Unit"));

			var expectedResponse = new WebServiceResponse
			{
				updatePacksUQListToken,
				updateDescriptionToken,
				updatePacksUQToken,
				updateQuantityUQToken
			};

			var actualResponse = warehouseOrderLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
			AssertEquals(Part1.PK, Line.WE_OP);

			updateParameters.ModifiedControlID = updateParameters.ProductControlID;
			updateParameters.NewValue = "Part2";
			expectedResponse = new WebServiceResponse();

			expectedPackUQList = new CodeDescriptionPairList();
			expectedPackUQList.AddPair("UNT", "Unit");

			updatePacksUQListToken = new UpdateListResponseToken("PacksUQ", expectedPackUQList);
			updatePacksUQListToken.Conditions.Add(new ResponseConditionEqualToken("Product", "Part2"));

			updateDescriptionToken = new UpdateValueResponseToken("Description", "Part Two");
			updateDescriptionToken.Conditions.Add(new ResponseConditionEqualToken("Description", "Part One"));

			updatePacksUQToken = new UpdateValueResponseToken("PacksUQ", "UNT");
			updatePacksUQToken.Conditions.Add(new ResponseConditionEqualToken("PacksUQ", "PCE"));

			updateQuantityUQToken = new UpdateValueResponseToken("QuantityUQ", "Unit");
			updateQuantityUQToken.Conditions.Add(new ResponseConditionEqualToken("QuantityUQ", "Piece"));

			expectedResponse = new WebServiceResponse
			{
				updatePacksUQListToken,
				updateDescriptionToken,
				updatePacksUQToken,
				updateQuantityUQToken
			};

			actualResponse = warehouseOrderLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
			AssertEquals(Part2.PK, Line.WE_OP);
		}

		public void TestPacksChange()
		{
			var updateParameters = GetLineUpdateParameters();
			var warehouseOrderLineUpdateWSMethod = GetNewWebServiceMethod();

			SetupForTesting(updateParameters);
			Line.WE_OP = Part1.PK;
			Line.WE_PackQuantity = 1;

			updateParameters.ModifiedControlID = updateParameters.PacksControlID;
			updateParameters.NewValue = "2";
			var expectedResponse = new WebServiceResponse();

			AddExpectedTokensForPacksChange(expectedResponse);

			var actualResponse = warehouseOrderLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
			AssertEquals(2m, Line.WE_PackQuantity);
		}

		protected virtual void AddExpectedTokensForPacksChange(WebServiceResponse expectedResponse)
		{
			var updateValueToken = new UpdateValueResponseToken("Quantity", "2");
			updateValueToken.Conditions.Add(new ResponseConditionEqualToken("Quantity", 1m));
			expectedResponse.Add(updateValueToken);
		}

		public void TestPacksUQChange()
		{
			var updateParameters = GetLineUpdateParameters();
			var warehouseOrderLineUpdateWSMethod = GetNewWebServiceMethod();

			SetupForTesting(updateParameters);
			Line.WE_OP = Part1.PK;
			Line.WE_F3_NKPackType = "PCE";

			updateParameters.ModifiedControlID = updateParameters.PacksUQControlID;
			updateParameters.NewValue = "UT1";
			var expectedResponse = new WebServiceResponse();

			var actualResponse = warehouseOrderLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
			AssertEquals("UT1", Line.WE_F3_NKPackType);

			updateParameters.ModifiedControlID = updateParameters.PacksUQControlID;
			updateParameters.NewValue = "UT2";
			expectedResponse = new WebServiceResponse();

			actualResponse = warehouseOrderLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
			AssertEquals("PCE", Line.WE_F3_NKPackType);
		}

		public void TestQuantityChange()
		{
			var updateParameters = GetLineUpdateParameters();
			var warehouseOrderLineUpdateWSMethod = GetNewWebServiceMethod();

			SetupForTesting(updateParameters);
			Line.WE_OP = Part1.PK;
			Line.WE_TransactionQuantity = 1;

			updateParameters.ModifiedControlID = updateParameters.QuantityControlID;
			updateParameters.NewValue = "2";
			var expectedResponse = new WebServiceResponse();

			AddExpectedTokensForQuantityChange(expectedResponse);

			var actualResponse = warehouseOrderLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
			AssertEquals(2m, Line.WE_TransactionQuantity);
		}

		protected virtual void AddExpectedTokensForQuantityChange(WebServiceResponse expectedResponse)
		{
			var updateValueToken = new UpdateValueResponseToken("Packs", "2");
			updateValueToken.Conditions.Add(new ResponseConditionEqualToken("Packs", 1m));
			expectedResponse.Add(updateValueToken);
		}

		public void TestAttributesChange()
		{
			var updateParameters = GetLineUpdateParameters();
			var warehouseOrderLineUpdateWSMethod = GetNewWebServiceMethod();

			SetupForTesting(updateParameters);

			var org = TestHelper.TestOrg;
			org.MiscServ.OM_IMPartAttrib1Name = "Test Attribute1";
			org.MiscServ.OM_IMPartAttrib2Name = "Test Attribute2";
			org.MiscServ.OM_IMPartAttrib3Name = "Test Attribute3";

			org.MiscServ.OM_IMPartAttrib1Type = PartAttributeTypeList.Codes.BatchNumber;
			org.MiscServ.OM_IMPartAttrib2Type = PartAttributeTypeList.Codes.JulianBatchNumber;
			org.MiscServ.OM_IMPartAttrib3Type = PartAttributeTypeList.Codes.Mandatory;

			var part1Relation = Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			part1Relation.OU_UsePartAttrib1 = true;
			part1Relation.OU_UsePartAttrib2 = true;
			part1Relation.OU_UsePartAttrib3 = true;

			Line.WE_OP = Part2.PK;

			updateParameters.ModifiedControlID = updateParameters.ProductControlID;
			updateParameters.NewValue = "Part1";

			var expectedPackUQList = new CodeDescriptionPairList();
			expectedPackUQList.AddPair("PCE", "Piece");
			expectedPackUQList.AddPair("UT1", "");
			var updatePacksUQListToken = new UpdateListResponseToken("PacksUQ", expectedPackUQList);
			updatePacksUQListToken.Conditions.Add(new ResponseConditionEqualToken("Product", "Part1"));

			var updateDescriptionToken = new UpdateValueResponseToken("Description", "Part One");
			updateDescriptionToken.Conditions.Add(new ResponseConditionEqualToken("Description", "Part Two"));

			var updatePacksUQToken = new UpdateValueResponseToken("PacksUQ", "PCE");
			updatePacksUQToken.Conditions.Add(new ResponseConditionEqualToken("PacksUQ", "UNT"));

			var updateQuantityUQToken = new UpdateValueResponseToken("QuantityUQ", "Piece");
			updateQuantityUQToken.Conditions.Add(new ResponseConditionEqualToken("QuantityUQ", "Unit"));

			var expectedResponse = new WebServiceResponse
			{
				updatePacksUQListToken,
				updateDescriptionToken,
				updatePacksUQToken,
				updateQuantityUQToken,
				new SetReadOnlyResponseToken("Attribute1", false),
				new SetReadOnlyResponseToken("Attribute2", false),
				new SetReadOnlyResponseToken("Attribute3", false)
			};

			var actualResponse = warehouseOrderLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);

			Line.WE_PartAttrib1 = "Number 1";
			Line.WE_PartAttrib2 = "Number 2";
			Line.WE_PartAttrib3 = "Number 3";

			updateParameters.ModifiedControlID = updateParameters.ProductControlID;
			updateParameters.NewValue = "Part2";

			expectedPackUQList = new CodeDescriptionPairList();
			expectedPackUQList.AddPair("UNT", "Unit");
			updatePacksUQListToken = new UpdateListResponseToken("PacksUQ", expectedPackUQList);
			updatePacksUQListToken.Conditions.Add(new ResponseConditionEqualToken("Product", "Part2"));

			updateDescriptionToken = new UpdateValueResponseToken("Description", "Part Two");
			updateDescriptionToken.Conditions.Add(new ResponseConditionEqualToken("Description", "Part One"));

			updatePacksUQToken = new UpdateValueResponseToken("PacksUQ", "UNT");
			updatePacksUQToken.Conditions.Add(new ResponseConditionEqualToken("PacksUQ", "PCE"));

			updateQuantityUQToken = new UpdateValueResponseToken("QuantityUQ", "Unit");
			updateQuantityUQToken.Conditions.Add(new ResponseConditionEqualToken("QuantityUQ", "Piece"));

			var updateAttribute1Token = new UpdateValueResponseToken("Attribute1", "");
			updateAttribute1Token.Conditions.Add(new ResponseConditionEqualToken("Attribute1", "Number 1"));

			var updateAttribute2Token = new UpdateValueResponseToken("Attribute2", "");
			updateAttribute2Token.Conditions.Add(new ResponseConditionEqualToken("Attribute2", "Number 2"));

			var updateAttribute3Token = new UpdateValueResponseToken("Attribute3", "");
			updateAttribute3Token.Conditions.Add(new ResponseConditionEqualToken("Attribute3", "Number 3"));

			expectedResponse = new WebServiceResponse
			{
				updatePacksUQListToken,
				updateDescriptionToken,
				updatePacksUQToken,
				updateQuantityUQToken,
				updateAttribute1Token,
				updateAttribute2Token,
				updateAttribute3Token,
				new SetReadOnlyResponseToken("Attribute1", true),
				new SetReadOnlyResponseToken("Attribute2", true),
				new SetReadOnlyResponseToken("Attribute3", true)
			};

			actualResponse = warehouseOrderLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
		}

		public void TestSerialNumberChange()
		{
			var updateParameters = GetLineUpdateParameters();
			var warehouseOrderLineUpdateWSMethod = GetNewWebServiceMethod();

			SetupForTesting(updateParameters);

			var org = TestHelper.TestOrg;
			org.MiscServ.OM_IMUseSerialNumber = true;

			var part1Relation = Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			part1Relation.OU_UseSerialNumber = true;

			Line.WE_OP = Part2.PK;

			updateParameters.ModifiedControlID = updateParameters.ProductControlID;
			updateParameters.NewValue = "Part1";

			var expectedPackUQList = new CodeDescriptionPairList();
			expectedPackUQList.AddPair("PCE", "Piece");
			expectedPackUQList.AddPair("UT1", "");
			var updatePacksUQListToken = new UpdateListResponseToken("PacksUQ", expectedPackUQList);
			updatePacksUQListToken.Conditions.Add(new ResponseConditionEqualToken("Product", "Part1"));

			var updateDescriptionToken = new UpdateValueResponseToken("Description", "Part One");
			updateDescriptionToken.Conditions.Add(new ResponseConditionEqualToken("Description", "Part Two"));

			var updatePacksUQToken = new UpdateValueResponseToken("PacksUQ", "PCE");
			updatePacksUQToken.Conditions.Add(new ResponseConditionEqualToken("PacksUQ", "UNT"));

			var updateQuantityUQToken = new UpdateValueResponseToken("QuantityUQ", "Piece");
			updateQuantityUQToken.Conditions.Add(new ResponseConditionEqualToken("QuantityUQ", "Unit"));

			var expectedResponse = new WebServiceResponse
			{
				updatePacksUQListToken,
				updateDescriptionToken,
				updatePacksUQToken,
				updateQuantityUQToken,
				new SetReadOnlyResponseToken("SerialNumber", false)
			};

			var actualResponse = warehouseOrderLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);

			Line.WE_SerialNumber = "Number 1";

			updateParameters.ModifiedControlID = updateParameters.ProductControlID;
			updateParameters.NewValue = "Part2";

			expectedPackUQList = new CodeDescriptionPairList();
			expectedPackUQList.AddPair("UNT", "Unit");
			updatePacksUQListToken = new UpdateListResponseToken("PacksUQ", expectedPackUQList);
			updatePacksUQListToken.Conditions.Add(new ResponseConditionEqualToken("Product", "Part2"));

			updateDescriptionToken = new UpdateValueResponseToken("Description", "Part Two");
			updateDescriptionToken.Conditions.Add(new ResponseConditionEqualToken("Description", "Part One"));

			updatePacksUQToken = new UpdateValueResponseToken("PacksUQ", "UNT");
			updatePacksUQToken.Conditions.Add(new ResponseConditionEqualToken("PacksUQ", "PCE"));

			updateQuantityUQToken = new UpdateValueResponseToken("QuantityUQ", "Unit");
			updateQuantityUQToken.Conditions.Add(new ResponseConditionEqualToken("QuantityUQ", "Piece"));

			var updateSerialNumberToken = new UpdateValueResponseToken("SerialNumber", "");
			updateSerialNumberToken.Conditions.Add(new ResponseConditionEqualToken("SerialNumber", "Number 1"));

			expectedResponse = new WebServiceResponse
			{
				updatePacksUQListToken,
				updateDescriptionToken,
				updatePacksUQToken,
				updateQuantityUQToken,
				updateSerialNumberToken,
				new SetReadOnlyResponseToken("SerialNumber", true)
			};

			actualResponse = warehouseOrderLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
		}

		protected void SetupForTesting(WarehouseDocketLineUpdateParameters updateParameters)
		{
			TestHelper = new ZWebTestHelper(Factory);
			WarehouseHelper = new WhsTestHelperFunctions(Factory);
			Warehouse = WarehouseHelper.CreateWarehouse("Warehouse", "WHS", "ROW");
			Location = WarehouseHelper.CreateRowAndGenerateLocations(Warehouse, "OTHER").Locations[0];

			Part1 = WarehouseHelper.CreateProduct("Part1", TestHelper.TestOrg);
			Part1.OP_Desc = "Part One";
			Part1.OP_StockKeepingUnit = "PCE";
			WarehouseHelper.CreateProductUnit(Part1, "UT1", 100m);

			Part2 = WarehouseHelper.CreateProduct("Part2", TestHelper.TestOrg);
			Part2.OP_Desc = "Part Two";
			Part2.OP_StockKeepingUnit = "UNT";

			Factory.Save();

			TestHelper.TestSiteUser.Login(TestHelper.TestOrg.OH_Code, TestHelper.TestContact.OC_Email, TestHelper.TestContact.PasswordForTesting);

			var trackingDocket = GetTrackingDocketForTesting();
			Line = CreateNewDocketLines(trackingDocket, out var lineRef);

			var session = HttpContext.Current.Session;
			var receiveIndex = Guid.NewGuid().ToString();
			session[receiveIndex] = trackingDocket;

			updateParameters.DocketRef = receiveIndex;
			updateParameters.LineRef = lineRef;
		}

		protected virtual WarehouseDocketLineUpdateParameters GetLineUpdateParameters()
		{
			var parameters = GetNewLineUpdateParameters();
			parameters.ModifiedControlID = "Product";
			parameters.NewValue = "Part1";
			parameters.ProductControlID = "Product";
			parameters.DescriptionControlID = "Description";
			parameters.PacksControlID = "Packs";
			parameters.PacksUQControlID = "PacksUQ";
			parameters.QuantityControlID = "Quantity";
			parameters.ProductUQControlID = "QuantityUQ";
			parameters.Attribute1ControlID = "Attribute1";
			parameters.Attribute2ControlID = "Attribute2";
			parameters.Attribute3ControlID = "Attribute3";
			parameters.SerialNumberControlID = "SerialNumber";

			return parameters;
		}

		protected override void SetMethodParametersAndExpectedResponseTokens(Dictionary<string, WebServiceResponse> setting)
		{
		}

		public override void TestExecute()
		{
			Assert(true);
		}

		protected override bool MethodNeverReturnsErrors => true;

		protected abstract WarehouseDocketLineUpdateParameters GetNewLineUpdateParameters();

		protected abstract BusinessObject GetTrackingDocketForTesting();

		protected abstract WhsDocketLine CreateNewDocketLines(BusinessObject trackingDocket, out string lineRef);

		protected ZWebTestHelper TestHelper;
		protected WhsTestHelperFunctions WarehouseHelper;
		protected WhsWarehouse Warehouse;
		protected WhsLocation Location;
		protected OrgSupplierPart Part1;
		protected OrgSupplierPart Part2;
		protected WhsDocketLine Line;
	}
}
