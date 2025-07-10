using CargoWise.Types;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(SPTSMessage))]
	public class SPTSMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<SPTSMessage>();
			AssertEquals(Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.TRCustoms, message.EM_ApplicationCode);
			AssertEquals(ZBool.True, message.NeedToSignMessage);
		}

		public void TestRegisteredLinkedObjectType()
		{
			var header = Factory.New<Integration.Customs.TR.ICusInBondSPTSHeader>();
			header.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.TRSPTS;

			var message = Factory.New<SPTSMessage>();
			message.EM_ApplicationCode = "TRC";
			message.EM_ApplicationReference = "2020/00084/5";
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageText = "Test";
			message.EM_MessageType = TRMessageTypes.Codes.TSP;
			message.EM_ReceiveTransmit = "RCV";
			message.EM_Status = "QUE";
			header.Messages.Add(message);
			Factory.Save();

			var newFactory = NewFactory();
			var reLoadEdiMessage = newFactory.Load<SPTSMessage>(message.PK);
			var sptsHeader = reLoadEdiMessage.EM_LinkedObject;
			AssertEquals("Enterprise.Customs.TR.NCTS.Business.SPTSHeader", sptsHeader.GetType().FullName);
		}

		public void TestFormattedMessageText()
		{
			var messageTSPTransmit = Factory.New<SPTSMessage>();
			messageTSPTransmit.EM_MessageType = TRMessageTypes.Codes.TSP;
			messageTSPTransmit.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTSPTransmit.EM_MessageText = TRMessageTestHelper.GetFileText("SPTS.SPTSOutgoingTest.xml");

			var messageTSPReceive = Factory.New<SPTSMessage>();
			messageTSPReceive.EM_MessageType = TRMessageTypes.Codes.TSP;
			messageTSPReceive.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			messageTSPReceive.EM_MessageText = TRMessageTestHelper.GetFileText("SPTS.SPTSOutgoingResponse.xml");

			var messageT1PTransmit = Factory.New<SPTSMessage>();
			messageT1PTransmit.EM_MessageType = TRMessageTypes.Codes.T1P;
			messageT1PTransmit.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageT1PTransmit.EM_MessageText = TRMessageTestHelper.GetFileText("SPTS.IslemSonucGetir2.xml");

			var messageT1PReceive = Factory.New<SPTSMessage>();
			messageT1PReceive.EM_MessageType = TRMessageTypes.Codes.T1P;
			messageT1PReceive.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			messageT1PReceive.EM_MessageText = TRMessageTestHelper.GetFileText("SPTS.SPTSSuccess.xml");

			var formatted = TRMessageTestHelper.GetFileText("SPTS.SPTSFormattedSuccess.xml");

			CombineAssertions("Formatted Message Text", () =>
			{
				AssertContains("TSP | Transmit", ZString.Empty, messageTSPTransmit.EM_FormattedMessageText);
				AssertContains("TSP | Receive", messageTSPReceive.EM_MessageText, messageTSPReceive.EM_FormattedMessageText);
				AssertContains("T1P | Transmit", ZString.Empty, messageT1PTransmit.EM_FormattedMessageText);
				AssertContains("T1P | Receive", formatted, messageT1PReceive.EM_FormattedMessageText);
			});
		}

		public void TestMessageInterpretation()
		{
			var messageTransmit = Factory.New<SPTSMessage>();
			messageTransmit.EM_MessageType = TRMessageTypes.Codes.TSP;
			messageTransmit.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmit.EM_MessageText = TRMessageTestHelper.GetFileText("SPTS.SPTSOutgoingTest.xml");
			messageTransmit.EM_MessageInterpretation = messageTransmit.EM_MessageText;
			var expectedInterpretationTransmit = TRMessageTestHelper.GetFileText("SPTS.SPTSOutgoingTSP.htm");

			var messageReceive = Factory.New<SPTSMessage>();
			messageReceive.EM_MessageType = TRMessageTypes.Codes.TSP;
			messageReceive.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			messageReceive.EM_MessageText = TRMessageTestHelper.GetFileText("SPTS.SPTSOutgoingResponse.xml");
			messageReceive.EM_MessageInterpretation = messageReceive.EM_MessageText;

			var messageTransmit2 = Factory.New<SPTSMessage>();
			messageTransmit2.EM_MessageType = TRMessageTypes.Codes.T1P;
			messageTransmit2.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Transmit;
			messageTransmit2.EM_MessageText = TRMessageTestHelper.GetFileText("SPTS.IslemSonucGetir2.xml");
			messageTransmit2.EM_MessageInterpretation = messageTransmit2.EM_MessageText;
			var expectedInterpretationTransmit2 = TRMessageTestHelper.GetFileText("SPTS.IslemSonucGetir2.htm");

			var messageReceive2 = Factory.New<SPTSMessage>();
			messageReceive2.EM_MessageType = TRMessageTypes.Codes.T1P;
			messageReceive2.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			messageReceive2.EM_MessageText = TRMessageTestHelper.GetFileText("SPTS.SPTSSuccess.xml");
			messageReceive2.EM_MessageInterpretation = messageReceive2.EM_MessageText;

			CombineAssertions("Message Interpretation", () =>
			{
				AssertEquals("TSP | Transmit Interpretation", expectedInterpretationTransmit, messageTransmit.EM_MessageInterpretation);
				AssertEquals("TSP | Receive Interpretation", messageReceive.EM_MessageText, messageReceive.EM_MessageInterpretation);
				AssertEquals("T1P | Transmit Interpretation", expectedInterpretationTransmit2, messageTransmit2.EM_MessageInterpretation);
				AssertEquals("T1P | Receive Interpretation", messageReceive2.EM_MessageText, messageReceive2.EM_MessageInterpretation);
			});
		}
	}
}
