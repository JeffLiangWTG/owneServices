using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class ShipmentAmendmentCodesTest : TestCase
	{
		public void TestShipmentAmendmentCodes()
		{
			AssertEquals("Shipment amendment codes, non-in-bond movement", "01, 02, 03, 04, 05, 06, 11, 18, 19, 27, 28, 29, 30", new ShipmentAmendmentCodes(false).CodesAsString);
			AssertEquals("Shipment amendment codes, in-bond movement", "16, 17, 18, 19, 26, 27, 28, 29, 30", new ShipmentAmendmentCodes(true).CodesAsString);
		}
	}
}
