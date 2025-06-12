using System.Linq;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using NUnit.Framework;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests
{
	[TestFixture]
	public class MAERSK_RoutingRuleTest_SI : RoutingRuleIntegrationTestBase
	{
    [Test]
    public void TestMAERSK_RoutingRuleTest_SI()
    {
      var message = new TestingMessage("MAERSK_SI1", "SHIPPING_INSTRUCTION");
      message.SCACUniShip = "MAEU";
      message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ShippingInstruction/1";
      message.DocumentName = "Shipping Instruction";

      var result = Evaluate(message).Single();

      Assert.AreEqual("MAERSK_SI1", result.Context.ReadPropertyString<BTS.DestinationParty>());
    }
	}
}
