using System.Collections.Generic;
using System.Web;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Tracking.Web.ServerServices;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class PackLineUpdateWSMethodTest : TrackingWebServiceMethodTest<PackLineUpdateWSMethod>
	{
		public void TestVolumeCalculationsChange()
		{
			var updateParameters = GetLineUpdateParameters();
			var packLineUpdateWSMethod = GetNewWebServiceMethod();

			SetupForBookingTesting(updateParameters);
			Line.JL_Length = 2;
			Line.JL_Width = 3;
			Line.JL_Height = 4;
			Line.JL_UnitOfDimension = Core.Constants.Length.Metres;
			Line.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

			updateParameters.ModifiedControlID = updateParameters.QuantityControlID;
			updateParameters.NewValue = "10";
			var updateVolumeToken = new UpdateValueResponseToken("Volume", "240");
			updateVolumeToken.Conditions.Add(new ResponseConditionEqualToken("Volume", 0m));
			var expectedResponse = new WebServiceResponse { updateVolumeToken };
			var actualResponse = packLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
			AssertEquals(240m, Line.JL_ActualVolume);

			updateParameters.ModifiedControlID = updateParameters.LengthControlID;
			updateParameters.NewValue = "3";
			updateVolumeToken = new UpdateValueResponseToken("Volume", "360");
			updateVolumeToken.Conditions.Add(new ResponseConditionEqualToken("Volume", 240m));
			expectedResponse = new WebServiceResponse { updateVolumeToken };
			actualResponse = packLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
			AssertEquals(360m, Line.JL_ActualVolume);

			updateParameters.ModifiedControlID = updateParameters.WidthControlID;
			updateParameters.NewValue = "4";
			updateVolumeToken = new UpdateValueResponseToken("Volume", "480");
			updateVolumeToken.Conditions.Add(new ResponseConditionEqualToken("Volume", 360m));
			expectedResponse = new WebServiceResponse { updateVolumeToken };
			actualResponse = packLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
			AssertEquals(480m, Line.JL_ActualVolume);

			updateParameters.ModifiedControlID = updateParameters.HeightControlID;
			updateParameters.NewValue = "5";
			updateVolumeToken = new UpdateValueResponseToken("Volume", "600");
			updateVolumeToken.Conditions.Add(new ResponseConditionEqualToken("Volume", 480m));
			expectedResponse = new WebServiceResponse { updateVolumeToken };
			actualResponse = packLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
			AssertEquals(600m, Line.JL_ActualVolume);

			updateParameters.ModifiedControlID = updateParameters.PackUDControlID;
			updateParameters.NewValue = Core.Constants.Length.Feet;
			updateVolumeToken = new UpdateValueResponseToken("Volume", "16.990");
			updateVolumeToken.Conditions.Add(new ResponseConditionEqualToken("Volume", 600m));
			expectedResponse = new WebServiceResponse { updateVolumeToken };
			actualResponse = packLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertResponse(updateParameters.ToString(), expectedResponse, actualResponse);
			AssertEquals(16.99m, Line.JL_ActualVolume);
		}

		public void TestEmptyValuesForDecimalFields()
		{
			var updateParameters = GetLineUpdateParameters();
			var packLineUpdateWSMethod = GetNewWebServiceMethod();

			SetupForBookingTesting(updateParameters);
			Line.JL_PackageCount = 1;
			Line.JL_Length = 2;
			Line.JL_Width = 3;
			Line.JL_Height = 4;

			updateParameters.ModifiedControlID = updateParameters.QuantityControlID;
			updateParameters.NewValue = "";
			packLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertEquals(0, Line.JL_PackageCount);

			updateParameters.ModifiedControlID = updateParameters.LengthControlID;
			updateParameters.NewValue = "";
			packLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertEquals(0m, Line.JL_Length);

			updateParameters.ModifiedControlID = updateParameters.WidthControlID;
			updateParameters.NewValue = "";
			packLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertEquals(0m, Line.JL_Width);

			updateParameters.ModifiedControlID = updateParameters.HeightControlID;
			updateParameters.NewValue = "";
			packLineUpdateWSMethod.Execute(updateParameters.ToString());
			AssertEquals(0m, Line.JL_Height);
		}

		void SetupForBookingTesting(PackLineUpdateParameters updateParameters)
		{
			TestHelper = new TestHelper(Factory);
			TestHelper.TestSiteUser.Login(TestHelper.TestOrg.OH_Code, TestHelper.TestContact.OC_Email, TestHelper.TestContact.PasswordForTesting);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var booking = new TrackingBooking(shipment, Factory, TestHelper.TestSiteUser);
			Line = shipment.OuterPackLines.AddNew();

			var session = HttpContext.Current.Session;
			var shipmentRef = shipment.PK.ToString();
			session[shipmentRef] = booking;

			updateParameters.ShipmentRef = shipmentRef;
			updateParameters.LineRef = Line.PK.ToString();
		}

		PackLineUpdateParameters GetLineUpdateParameters()
		{
			var parameters = new PackLineUpdateParameters();
			parameters.ModifiedControlID = "Quantity";
			parameters.NewValue = "10";
			parameters.QuantityControlID = "Quantity";
			parameters.LengthControlID = "Length";
			parameters.WidthControlID = "Width";
			parameters.HeightControlID = "Height";
			parameters.PackUDControlID = "PackUD";
			parameters.VolumeControlID = "Volume";
			parameters.VolumeUQControlID = "VolumeUQ";

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

		protected override string GetExpectedMethodSpecificServiceScriptFileName() => "PackLineUpdateWSMethod.js";

		protected override string GetExpectedMethodName() => "PackLineUpdate";

		protected override PackLineUpdateWSMethod GetNewWebServiceMethod() => new PackLineUpdateWSMethod();

		TestHelper TestHelper;
		PackLine Line;
	}
}
