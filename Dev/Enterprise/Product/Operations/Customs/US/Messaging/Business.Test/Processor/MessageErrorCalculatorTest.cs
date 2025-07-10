using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.MessageProcessors;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class MessageErrorCalculatorTest : TestCaseWithFactory
	{
		public void TestReportAbnormalityInResponseMessageIfNeeded()
		{
			var bizObj = Factory.New<DummyBusinessObject>();
			bizObj.AddRowMessageError("FOOD IS TASTELESS");
			var messageTrx = Factory.New<CBPMessageForTesting>();
			messageTrx.EM_MessageNum = "~121~";
			messageTrx.EM_LinkedObject = bizObj;
			messageTrx.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Transmit;
			messageTrx.EM_MessageType = "JDS";
			var messageRcv = Factory.New<CBPMessageForTesting>();
			messageRcv.EM_MessageNum = "~121~";
			messageRcv.EM_ReceiveTransmit = CBPMessageForTesting.Direction.Receive;
			messageRcv.EM_MessageType = "JDG";
			var attacheeMock = new Mock<IMessageAttachee>();
			attacheeMock.Setup(m => m.TopLevelBusinessObject).Returns(bizObj);
			var attachee = attacheeMock.Object;
			var calculatorMock = new Mock<MessageErrorCalculator>(Factory);
			var calculator = calculatorMock.Object;
			ErrorReporter.Clear();
			calculator.ReportAbnormalityInResponseMessageIfNeeded(attachee, messageRcv, false);
			AssertEquals("LastKeyReported", "", ErrorReporter.LastKeyReported);
			AssertEquals("LastMessageReported", "", ErrorReporter.LastMessageReported);
			messageTrx.EM_SendWithMessageErrors = true;
			calculator.ReportAbnormalityInResponseMessageIfNeeded(attachee, messageRcv, false);
			AssertEquals("LastKeyReported", ResponseMessageAbnormalityReporter.ClearedWhenMessageWasSendWithErrorKey + " for Message Type 'JDG' and Application Code '" + messageRcv.EM_ApplicationCode + "'", ErrorReporter.LastKeyReported);
			AssertContains("LastMessageReported contains message errors", "DummyBizo: FOOD IS TASTELESS", ErrorReporter.LastMessageReported);
			calculatorMock.Protected().Setup<bool>("IsAbnormalityMessageReportingDisable").Returns(true);
			ErrorReporter.Clear();
			calculator.ReportAbnormalityInResponseMessageIfNeeded(attachee, messageRcv, false);
			AssertEquals("LastKeyReported", "", ErrorReporter.LastKeyReported);
			AssertEquals("LastMessageReported", "", ErrorReporter.LastMessageReported);
			calculatorMock.VerifyAll();
		}

		public void TestGetLongDescription()
		{
			MessageErrorCalculator calculator = new MessageErrorCalculator(Factory);
			AssertContains("BAD FOOD", calculator.GetLongDescription("DSJ", "BAD FOOD"));
		}
	}
}
