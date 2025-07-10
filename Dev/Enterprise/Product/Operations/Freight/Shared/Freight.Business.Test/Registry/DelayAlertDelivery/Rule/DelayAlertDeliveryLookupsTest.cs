using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class DelayAlertDeliveryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportModes()
		{
			const string expected =
				"AIR - Air Freight\r\n" +
				"SEA - Sea Freight\r\n" +
				"ROA - Road Freight\r\n" +
				"RAI - Rail Freight\r\n" +
				"ALL - All" +
				"";

			AssertMultilineASCIIEquals("", expected, new DelayAlertDeliveryRule().Lookups.TransportModes.ElementsAsString);
		}
	}
}
