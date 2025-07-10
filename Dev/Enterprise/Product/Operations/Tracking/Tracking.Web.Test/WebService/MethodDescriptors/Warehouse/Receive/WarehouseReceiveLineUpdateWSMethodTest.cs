using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class WarehouseReceiveLineUpdateWSMethodTest : WarehouseDocketLineUpdateWSMethodTest<WarehouseReceiveLineUpdateWSMethod>
	{
		public void TestExpiryDateChange()
		{
			var updateParameters = GetLineUpdateParameters();
			var warehouseOrderLineUpdateWSMethod = GetNewWebServiceMethod();

			SetupForTesting(updateParameters);

			var org = TestHelper.TestOrg;
			org.MiscServ.OM_IMUseExpiryDate = true;

			var part1Relation = Part1.RelatedOrganisations.FindByOrganisationPKAndRelationship(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			part1Relation.OU_UseExpiryDate = true;

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
				new SetReadOnlyResponseToken("ExpiryDate", false)
			};

			var actualResponse = warehouseOrderLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);

			Line.WE_ExpiryDate = new ZDate(2023, 07, 17);

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

			var updateSerialNumberToken = new UpdateValueResponseToken("ExpiryDate", "");
			updateSerialNumberToken.Conditions.Add(new ResponseConditionEqualToken("ExpiryDate", new ZDate(2023, 07, 17)));

			expectedResponse = new WebServiceResponse
			{
				updatePacksUQListToken,
				updateDescriptionToken,
				updatePacksUQToken,
				updateQuantityUQToken,
				updateSerialNumberToken,
				new SetReadOnlyResponseToken("ExpiryDate", true)
			};

			actualResponse = warehouseOrderLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
		}

		public void TestExpectedQuantityChange()
		{
			var updateParameters = (WarehouseReceiveLineUpdateParameters)GetLineUpdateParameters();
			var warehouseOrderLineUpdateWSMethod = GetNewWebServiceMethod();

			SetupForTesting(updateParameters);
			Line.WE_OP = Part1.PK;
			Line.WE_ClientOrderedUnits = 5;

			updateParameters.ModifiedControlID = updateParameters.ExpectedQuantityControlID;
			updateParameters.NewValue = "10";

			var updatePacksToken = new UpdateValueResponseToken("Packs", "10");
			updatePacksToken.Conditions.Add(new ResponseConditionEqualToken("Packs", 5m));

			var updateQuantityToken = new UpdateValueResponseToken("Quantity", "10");
			updateQuantityToken.Conditions.Add(new ResponseConditionEqualToken("Quantity", 5m));

			var expectedResponse = new WebServiceResponse
			{
				updatePacksToken,
				updateQuantityToken
			};

			var actualResponse = warehouseOrderLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
			AssertEquals(10m, Line.WE_ClientOrderedUnits);
		}

		protected override void AddExpectedTokensForPacksChange(WebServiceResponse expectedResponse)
		{
			base.AddExpectedTokensForPacksChange(expectedResponse);

			var updateValueToken = new UpdateValueResponseToken("ExpectedQuantity", "2");
			updateValueToken.Conditions.Add(new ResponseConditionEqualToken("ExpectedQuantity", 1m));
			expectedResponse.Add(updateValueToken);
		}

		protected override WarehouseDocketLineUpdateParameters GetLineUpdateParameters()
		{
			var parameters = (WarehouseReceiveLineUpdateParameters)base.GetLineUpdateParameters();
			parameters.ExpectedQuantityControlID = "ExpectedQuantity";
			parameters.ExpiryDateControlID = "ExpiryDate";

			return parameters;
		}

		protected override WarehouseDocketLineUpdateParameters GetNewLineUpdateParameters() => new WarehouseReceiveLineUpdateParameters();

		protected override BusinessObject GetTrackingDocketForTesting() => TrackingWhsReceive.GetTrackingWhsReceive(WarehouseHelper.CreateWhsReceive(TestHelper.TestOrg, Warehouse));

		protected override WhsDocketLine CreateNewDocketLines(BusinessObject trackingDocket, out string lineRef)
		{
			var receive = (TrackingWhsReceive)trackingDocket;
			receive.Lines.AddNew();
			var line = receive.Lines.AddNew();
			receive.Lines.AddNew();
			lineRef = line.PK.ToString();

			return line.WhsReceiveLine;
		}

		protected override string GetExpectedMethodSpecificServiceScriptFileName() => "WarehouseReceiveLineUpdateWSMethod.js";

		protected override string GetExpectedMethodName() => "WarehouseReceiveLineUpdate";

		protected override WarehouseReceiveLineUpdateWSMethod GetNewWebServiceMethod() => new WarehouseReceiveLineUpdateWSMethod();
	}
}
