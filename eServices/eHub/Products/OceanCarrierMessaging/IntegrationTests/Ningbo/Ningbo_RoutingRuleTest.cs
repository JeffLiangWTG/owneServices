using System.Linq;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using NUnit.Framework;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests
{
	[TestFixture]
	public class Ningbo_RoutingRuleTest : RoutingRuleIntegrationTestBase
	{
		[Test]
		public void TestRoutingRule_CarrierHandlingAgent_Success()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "CHHA";
			message.DocumentName = "eManifest";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/eManifest/1";
			message.RecipientCodeCHA_NGB = "NGB";

			var result = Evaluate(message).Single();
			Assert.AreEqual("NGBEDI_EM1", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void Test_FallbackToShippingLineAddress_Success()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "CHHA";
			message.DocumentName = "eManifest";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/eManifest/1";
			message.RecipientCodeSI_NGB = "NGB";

			var result = Evaluate(message).Single();
			Assert.AreEqual("NGBEDI_EM1", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}
	}
}
