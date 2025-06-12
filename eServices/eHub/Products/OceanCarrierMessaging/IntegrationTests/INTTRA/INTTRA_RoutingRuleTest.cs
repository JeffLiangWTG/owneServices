using System.Linq;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using NUnit.Framework;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests
{
	[TestFixture]
	public class INTTRA_RoutingRuleTest : RoutingRuleIntegrationTestBase
	{
		[Test]
		public void TestINTTRA_SI()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "INTT";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11";
			message.DocumentName = "Shipping Instruction";

			var result = Evaluate(message).Single();

			Assert.AreEqual("INTTRA_SI", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void TestINTTRA_SI1()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "INTT";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/ShippingInstruction/1";
			message.DocumentName = "Shipping Instruction";

			var result = Evaluate(message).Single();

			Assert.AreEqual("INTTRA_SI1", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void TestINTTRA_BK()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "INTT";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11";
			message.DocumentName = "Booking Request";

			var result = Evaluate(message).Single();

			Assert.AreEqual("INTTRA_BK", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void TestINTTRA_BK1()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "INTT";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/BookingRequest/1";
			message.DocumentName = "Booking Request";

			var result = Evaluate(message).Single();

			Assert.AreEqual("INTTRA_BK1", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void TestINTTRA_WTH_ActionPurpose_Success()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "INTT";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11/BookingRequest/1";
			message.DocumentName = "Booking Request";
			message.ActionPurpose = "WTH";

			var result = Evaluate(message).Single();
			Assert.AreNotEqual("Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct", result.Context.ReadPropertyString<ErrorDescription>());
			Assert.AreEqual("INTTRA_BK1", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void TestINTTRA_VM()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "INTT";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11";
			message.DocumentName = "Verified Gross Container Weight";

			var result = Evaluate(message).Single();

			var log = Log;
			Assert.AreEqual("INTTRA_VM", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void TestINTTRA_WTH_ActionPurpose_Reject()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "INTT";
			message.UShipmentNamespace = "http://www.cargowise.com/Schemas/Universal/2012/11";
			message.DocumentName = "Shipping Instruction";
			message.ActionPurpose = "WTH";

			var result = Evaluate(message).Single();
			Assert.AreEqual("SHIPPING_INSTRUCTION", result.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual(null, result.Context.ReadPropertyString<ErrorCode>());
			Assert.AreEqual("MessageType=Shipping Instruction Withdrawal|Reason=not supported by carrier, contact carrier direct", result.Context.ReadPropertyString<ErrorDescription>());
		}
	}
}