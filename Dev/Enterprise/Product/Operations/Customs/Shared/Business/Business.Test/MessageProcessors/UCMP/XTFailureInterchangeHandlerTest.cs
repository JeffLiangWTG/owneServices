using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.Business.MessageProcessors.UCMP.Testing
{
	[TestedType(typeof(XTFailureInterchangeHandler))]
	sealed class XTFailureInterchangeHandlerTest : TestCaseWithFactory
	{
		public void TestValidXERInterchange()
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

			var (declaration, incomingInterchange, outgoingInterchange, outgoingMessage) = UCMPTestHelper.CreateTestInterchange(Factory, ApplicationCodeList.Codes.INCustoms, validBodyXML, "XER");
			var unpackResult = XTFailureInterchangeHandler.Unpack(incomingInterchange, outgoingMessage);

			CombineAssertions(() =>
			{
				Assert("Success", unpackResult.IsSuccess);
				var message = unpackResult.EdiMessages.Single();
				AssertEquals("Application Code", ApplicationCodeList.Codes.INCustoms, message.EM_ApplicationCode);
				AssertEquals("Message Type", Constant.MessageTypes.XER, message.EM_MessageType);
				AssertEquals("Direction", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
				AssertEquals("Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
				AssertEquals("Message number set to interchange Universal Event", "INT001INT001INT001INT001INT001INC", message.EM_MessageNum);
				AssertEquals("Message text set to interchange Universal Event", validUniversalEvent, message.EM_MessageText);
				AssertSame("Message linked to interchange", message, incomingInterchange.ContainedMessages.Single());
			});
		}

		public void TestInvalidXERInterchange()
		{
			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_InterchangeNum = "INT001INT001INT001INT001INT001INT001INT001";
			incomingInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
			incomingInterchange.EI_InterchangeType = "XER";
			incomingInterchange.EI_BodyText = @"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
<Header>
  <SenderID>xT</SenderID>
  <RecipientID>HYEDAECMT</RecipientID>
</Header>
<Body>
</Body>
</UniversalInterchange>
";
			var unpackResult = XTFailureInterchangeHandler.Unpack(incomingInterchange, null);
			CombineAssertions(() =>
			{
				AssertEquals("Error message from unpacker", "The xT Failure interchange does not contain a valid Universal XML Event in the body text.", unpackResult.ErrorReason);
				AssertEquals("Fail", expected: false, unpackResult.IsSuccess);
				AssertEquals("No messages connected to the interchange", 0, incomingInterchange.ContainedMessages.Count);
			});
		}
	}
}
