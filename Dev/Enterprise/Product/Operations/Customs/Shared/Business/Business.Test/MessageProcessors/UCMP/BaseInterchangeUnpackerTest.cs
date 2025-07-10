using System.Linq;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP.Testing;

[TestedType(typeof(BaseInterchangeUnpacker))]
sealed class BaseInterchangeUnpackerTest : InterchangeUnpackerTest<BaseInterchangeUnpacker>
{
	public void TestXERInterchange()
	{
		var validUniversalEvent = UCMPTestHelper.GetTestUniversalEvent("IRJ", "XER", "PreProcessingError", "Message Sending Failure Reason");
		var validBodyXML = @$"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
<Header>
  <SenderID>xT</SenderID>
  <RecipientID>HYEDAECMT</RecipientID>
</Header>
<Body>
 {validUniversalEvent}
</Body>
</UniversalInterchange>";

		foreach (var applicationCode in ApplicationCodes)
		{
			AssertUnpack(applicationCode, "XER", validBodyXML, validUniversalEvent);
		}
	}

	public void TestNormalInterchange()
	{
		foreach (var applicationCode in ApplicationCodes)
		{
			AssertUnpack(applicationCode, "TST", "Normal response content", "Normal response content");
		}
	}

	void AssertUnpack(string applicationCode, string interchangeType, string interchangeBody, string expectedMessageText)
	{
		var (declaration, incomingInterchange, outgoingInterchange, outgoingMessage) = UCMPTestHelper.CreateTestInterchange(Factory, applicationCode, interchangeBody, interchangeType);
		var unpacker = new BaseInterchangeUnpacker();
		var unpackResult = unpacker.Unpack(incomingInterchange, outgoingInterchange, outgoingMessage, null);

		CombineAssertions(() =>
		{
			Assert("Success", unpackResult.IsSuccess);
			var message = unpackResult.EdiMessages.Single();
			AssertEquals("EM_ApplicationCode", applicationCode, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", interchangeType, message.EM_MessageType);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
			AssertEquals("EM_MessageNum", "INT001INT001INT001INT001INT001" + applicationCode, message.EM_MessageNum);
			AssertEquals("EM_MessageText", expectedMessageText, message.EM_MessageText);
			AssertSame("Message linked to interchange", message, incomingInterchange.ContainedMessages.Single());
		});
	}

	protected override string[] ApplicationCodes => new[]
	{
		ApplicationCodeList.Codes.INCustoms,
		ApplicationCodeList.Codes.AUCOLS
	};
}
