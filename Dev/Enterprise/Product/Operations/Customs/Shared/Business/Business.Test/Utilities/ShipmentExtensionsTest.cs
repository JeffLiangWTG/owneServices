using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ShipmentExtensionsTest : TestCaseWithFactory
	{
		public void TestConsolForCountry()
		{
			ForwardingShipment shipment = null;
			AssertNull(shipment.ConsolForCountry(""));

			shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			AssertNull(shipment.ConsolForCountry("AU"));
			AssertNull(shipment.ConsolForCountry("US"));

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "AUSYD";
			consol1.JK_RL_NKDischargePort = "USLAX";

			AssertNull(shipment.ConsolForCountry("AU"));
			AssertEquals(consol1, shipment.ConsolForCountry("US"));

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "NZAKL";
			consol2.JK_RL_NKDischargePort = "SGSIN";

			AssertNull(shipment.ConsolForCountry("AU"));
			AssertEquals(consol1, shipment.ConsolForCountry("US"));
			AssertNull(shipment.ConsolForCountry("NZ"));
			AssertEquals(consol2, shipment.ConsolForCountry("SG"));
			AssertNull(shipment.ConsolForCountry("GB"));
			AssertNull(shipment.ConsolForCountry("DE"));
		}
	}
}
