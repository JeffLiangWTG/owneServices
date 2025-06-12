using System.Linq;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using NUnit.Framework;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests
{
	[TestFixture]
	public class Easipass_RoutingRuleTest : RoutingRuleIntegrationTestBase
	{
		#region CarrierHandlingAgent

		[Test]
		public void TestRoutingRule_CarrierHandlingAgent_Success()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "CHHK";
			message.DocumentName = "eManifest";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/eManifest/1";
			message.RecipientCodeCHA_ENP = "ENP";

			var result = Evaluate(message).Single();
			Assert.AreEqual("EASIPASS_EM1", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void Test_FallbackToShippingLineAddress_Sucess()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "CHHK";
			message.DocumentName = "eManifest";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/eManifest/1";
			message.RecipientCodeSI_ENP = "ENP";

			var result = Evaluate(message).Single();
			Assert.AreEqual("EASIPASS_EM1", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void Test_CarrierHandlingAgent_Reject()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "CHHK";
			message.DocumentName = "eManifest";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/eManifest/1";

			var result = Evaluate(message).Single();
			Assert.AreEqual("SHIPPING_INSTRUCTION", result.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("IRJ", result.Context.ReadPropertyString<ErrorCode>());
			Assert.AreEqual("Missing ENP Code for Carrier Handling Agent/ Carrier", result.Context.ReadPropertyString<ErrorDescription>());
		}

		#endregion

		#region CarrierBookingAgent

		[Test]
		public void TestRoutingRule_CarrierBookingAgent_Success()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "CHHK";
			message.DocumentName = "Shipping Order";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ShippingOrder/1";
			message.RecipientCodeCBA_ENP = "ENP";

			var result = Evaluate(message).Single();
			Assert.AreEqual("EASIPASS_SO1", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void Test_FallbackShippingLineAddress_Success()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "CHHK";
			message.DocumentName = "Shipping Order";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ShippingOrder/1";
			message.RecipientCodeSI_ENP = "ENP";

			var result = Evaluate(message).Single();
			Assert.AreEqual("EASIPASS_SO1", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void Test_CarrierBookingAgent_Reject()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "CHHK";
			message.DocumentName = "Shipping Order";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ShippingOrder/1";

			var result = Evaluate(message).Single();
			Assert.AreEqual("SHIPPING_INSTRUCTION", result.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("IRJ", result.Context.ReadPropertyString<ErrorCode>());
			Assert.AreEqual("Missing ENP Code for Carrier Booking Agent/ Carrier", result.Context.ReadPropertyString<ErrorDescription>());
		}

		#endregion
	}
}
