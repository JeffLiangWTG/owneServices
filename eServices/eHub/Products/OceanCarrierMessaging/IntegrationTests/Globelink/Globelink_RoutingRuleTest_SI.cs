using System.Linq;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using NUnit.Framework;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests
{
	[TestFixture]
	public class Globelink_RoutingRuleTest_SI : RoutingRuleIntegrationTestBase
	{
    [Test]
    public void TestRoutingRule_CWTG_SI1()
    {
      var message = new TestingMessage("CWTG_SI1", "SHIPPING_INSTRUCTION");
      message.SCACUniShip = "CWTG";
      message.DocumentName = "Shipping Instruction";

      message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ShippingInstruciton/1";

      var result = Evaluate(message).Single();

      Assert.AreEqual("CWTG_SI1", result.Context.ReadPropertyString<BTS.DestinationParty>());
    }
	}
}
