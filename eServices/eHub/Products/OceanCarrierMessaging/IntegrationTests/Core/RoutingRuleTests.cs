using System.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using NUnit.Framework;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.IntegrationTests
{
	[TestFixture]
    public class RoutingRuleTests : RoutingRuleIntegrationTestBase
	{
		[Test]
		public void TestRule_Success()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "DUMMY";
			var result = Evaluate(message).Single();
			Assert.AreEqual("DUMMY_PROVIDER", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void TestRule_CarrierHandlingAgent()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.CarrierHandlingAgent = "C1NN";
			var result = Evaluate(message).Single();
			Assert.AreEqual("HANDLING_PROVIDER", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void TestRule_CarrierBookingAgent()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.CarrierBookingAgent = "C1ST";
			var result = Evaluate(message).Single();
			Assert.AreEqual("BOOKING_PROVIDER", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void TestRule_SplitingBlackList_SplitingWhiteList()
		{
			// Backlist for not splitting
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "DUMMY";
			message.DocumentName = "eManifest";
			message.Purpose = "WTH";
            message.Port = "CNNBO";
			message.Containers = TestingMessage.TwoContainersString;
			var result = Evaluate(message).Single();
            Assert.AreEqual("DUMMY_PROVIDER", result.Context.ReadPropertyString<BTS.DestinationParty>());

			// Whitelist for splitting
			message.ClearMessageBodyCache();
			message.Port = "";
			result = Evaluate(message).Single();
			Assert.AreEqual("OCM_CONTAINER_SPLIT", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[Test]
		public void TestRule_MultipleRecipientsCopying()
		{
			var message = new TestingMessage("DFODAUUAT", "SHIPPING_INSTRUCTION");
			message.Port = "CNNGB";
			message.SCACUniShip = "DUMMY";
			message.CarrierHandlingAgent = "C1NN";

			var result = Evaluate(message);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual("DUMMY_PROVIDER", result[0].Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("HANDLING_PROVIDER", result[1].Context.ReadPropertyString<BTS.DestinationParty>());
		}
		
		[Test]
		public void TestDefaultReject()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "INVALID";
			var result = Evaluate(message).Single();
			Assert.AreEqual("SHIPPING_INSTRUCTION", result.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("IRJ", result.Context.ReadPropertyString<ErrorCode>());
			Assert.AreEqual("Department=WiseTechGlobal|Reason=You are not registered with this Carrier for this message type. Contact WTG to register.", result.Context.ReadPropertyString<ErrorDescription>());
		}

		[Test]
		public void TestReadableLog()
		{
			var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
			message.SCACUniShip = "DUMMY";
			var result = Evaluate(message).Single();
			StringAssert.Contains(@"[INFO ] Rule evaluation results = [ { Recipient: 'DUMMY_PROVIDER' } ]
[DEBUG] Finished evaluating rule for client 'SHIPPING_INSTRUCTION'", Log);
		}

	    [Test]
	    public void TestRule_UnRegisteredClientWithServiceProviderResult()
	    {
            var message = new TestingMessage("HYEDAUUAT", "SHIPPING_INSTRUCTION");
            message.Port = "DUMMY";

			var result = Evaluate(message).Single();
            Assert.AreEqual("HYEDAUUAT", result.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("IRJ", result.Context.ReadPropertyString<ErrorCode>());
			Assert.AreEqual("Department=WiseTechGlobal|Reason=You are not registered with this Carrier for this message type. Contact WTG to register.", result.Context.ReadPropertyString<ErrorDescription>());
	    }

		[Test]
		public void TestRule_RegisteredClientWithServiceProviderResult()
		{
			var message = new TestingMessage("REGISTERED_SENDER", "SHIPPING_INSTRUCTION");
			message.Port = "DUMMY";

			var result = Evaluate(message).Single();
			Assert.AreEqual("PROVIDER_RECIPIENT", result.Context.ReadPropertyString<BTS.DestinationParty>());
		}
	}
}
