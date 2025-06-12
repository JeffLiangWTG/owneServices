using System.Linq;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using NUnit.Framework;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests
{
	[TestFixture]
	public class MAERSK_RoutingRuleTest_BK : RoutingRuleIntegrationTestBase
	{
    [Test]
    public void TestMAERSK_RoutingRuleTest_BK()
    {
      var message = new TestingMessage("MAERSK_BK1", "SHIPPING_INSTRUCTION");
      message.SCACUniShip = "MAEU";
      message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/BookingRequest/1";
      message.DocumentName = "Booking Request";

      var result = Evaluate(message).Single();

      Assert.AreEqual("MAERSK_BK1", result.Context.ReadPropertyString<BTS.DestinationParty>());
    }
	}
}
