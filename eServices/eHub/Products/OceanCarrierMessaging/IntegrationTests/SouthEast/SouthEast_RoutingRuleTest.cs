using System.Linq;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using NUnit.Framework;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests
{
  [TestFixture]
  public class SouthEast_RoutingRuleTest : RoutingRuleIntegrationTestBase
  {
		[Test]
		public void TestSouthEast_RoutingRule_CarrierBookingAgent_Success()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "CHHK";
			message.DocumentName = "Shipping Order";

			var result = Evaluate(message).Single();
			Assert.AreEqual("SOUTHEAST_SO1", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}


		[Test]
		public void TestSouthEast_RoutingRule_CarrierBookingAgent_Failed()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "CXXX";
			message.DocumentName = "Shipping Order";

			var result = Evaluate(message).Single();
			Assert.AreEqual("IRJ", result.Context.ReadPropertyString<ErrorCode>());
			Assert.AreEqual("Department=WiseTechGlobal|Reason=You are not registered with this Carrier for this message type. Contact WTG to register.", result.Context.ReadPropertyString<ErrorDescription>());
		}
	}
}
