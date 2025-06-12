using System.Linq;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using NUnit.Framework;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests
{
	[TestFixture]
	public class Globelink_RoutingRuleTest_BR : RoutingRuleIntegrationTestBase
	{
    [Test]
    public void TestRoutingRule_CWTG_BK1()
    {
      var message = new TestingMessage("CWTG_BK1", "SHIPPING_INSTRUCTION");
      message.SCACUniShip = "CWTG";
      message.DocumentName = "Booking Request";

      message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/BookingRequest/1";

      var result = Evaluate(message).Single();

      Assert.AreEqual("CWTG_BK1", result.Context.ReadPropertyString<BTS.DestinationParty>());
    }
	}
}
