using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business.DangerousGoodsExtensions.Testing
{
	sealed class ForwardingDangerousGoodsExtensionsTests : TestCaseWithFactory
	{
		public void TestIsValidForCFRStandard()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			shipment.JS_RL_NKOrigin = "US3VA";
			shipment.JS_RL_NKDestination = "GBLON";

			Assert("Shipment with no legs that has origin or destination in US is CFR valid.", shipment.IsValidForCFRStandard());
		}

		public void TestIsValidForCFRStandard_ConsolHasUSRoadLegs()
		{
			var consol = Factory.New<ForwardingConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Core.Constants.TransportModes.Road;
			transport.JW_RL_NKLoadPort = "US3VA";
			transport.JW_RL_NKDiscPort = "AUSYD";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			shipment.JS_RL_NKOrigin = "US3VA";
			shipment.JS_RL_NKDestination = "GBLON";

			Assert("Shipment with Consol that has origin or destination legs in US is CFR valid.", shipment.IsValidForCFRStandard());
		}

		public void TestIsValidForCFRStandard_ShipmentWithNoUSRelation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GBLON";

			Assert("Shipment with no road origin or destination legs is not valid for CFR.", !shipment.IsValidForCFRStandard());
		}

		public void TestIsValidForJTT_NoChinaLegs()
		{
			AssertShipmentJTTValidity("Shipment with no Chinese Territory Road legs is not JTT Valid",
				loadPort: "AUSYD",
				dischargePort: "AUBNE",
				transportOnConsol: false,
				expectedResult: false);
		}

		public void TestIsValidForJTT_ChinaLegsOnShipment()
		{
			AssertShipmentJTTValidity("Shipment with Chinese Territory Road leg is JTT Valid",
				loadPort: "CNDLC",
				dischargePort: "AUBNE",
				transportOnConsol: false,
				expectedResult: true);
		}

		public void TestIsValidForJTT_HongKongLegsOnShipment()
		{
			AssertShipmentJTTValidity("Shipment with Chinese Territory (Hong Kong) Road leg is JTT Valid",
				loadPort: "HKABD",
				dischargePort: "AUBNE",
				transportOnConsol: false,
				expectedResult: true);
		}

		public void TestIsValidForJTT_TaiwanLegsOnShipment()
		{
			AssertShipmentJTTValidity("Shipment with Chinese Territory (Taiwan) Road leg is JTT Valid",
				loadPort: "TWKHH",
				dischargePort: "AUBNE",
				transportOnConsol: false,
				expectedResult: true);
		}

		public void TestIsValidForJTT_ChinaLegsOnConsol()
		{
			AssertShipmentJTTValidity("Shipment on Consol with Chinese Territory (Hong Kong) Road leg is JTT Valid",
				loadPort: "CNDLC",
				dischargePort: "AUBNE",
				transportOnConsol: true,
				expectedResult: true);
		}

		public void TestIsValidForJTT_HongKongLegsOnConsol()
		{
			AssertShipmentJTTValidity("Shipment on Consol with Chinese Territory (Hong Kong) Road leg is JTT Valid",
				loadPort: "HKABD",
				dischargePort: "AUBNE",
				transportOnConsol: true,
				expectedResult: true);
		}

		public void TestIsValidForJTT_TaiwanLegsOnConsol()
		{
			AssertShipmentJTTValidity("Shipment on Consol with Chinese Territory (Taiwan) Road leg is JTT Valid",
				loadPort: "TWKHH",
				dischargePort: "AUBNE",
				transportOnConsol: true,
				expectedResult: true);
		}

		void AssertShipmentJTTValidity(string message, string loadPort, string dischargePort, bool transportOnConsol, bool expectedResult)
		{
			var shipment = Factory.New<ForwardingShipment>();
			Transport transport;
			if (transportOnConsol)
			{
				var consol = shipment.Consols.AddNew();
				transport = consol.Transports.AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Road;
				transport.JW_RL_NKLoadPort = loadPort;
				transport.JW_RL_NKDiscPort = dischargePort;
			}
			else
			{
				transport = shipment.Transports.AddNew();
				transport.JW_TransportMode = Core.Constants.TransportModes.Road;
				transport.JW_RL_NKLoadPort = loadPort;
				transport.JW_RL_NKDiscPort = dischargePort;
			}

			AssertEquals(message, expectedResult, shipment.IsValidForJTTStandard());
		}
	}
}
