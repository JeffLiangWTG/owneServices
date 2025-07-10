using System.Threading;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class OutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestPackageIntoEDIInterchange()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = TestHelper.BondSubmissionXml;
			message.EM_MessageType = MessageTypeList.Codes.Submission;
			Factory.Save();
			var logger = new LoggingInformation();
			new OutgoingMessageProcessor(logger).ProcessMessage(CancellationToken.None);
			message.Reload();
			var interchange = message.Interchange;
			AssertEquals(MessageTypeList.Codes.Submission, interchange.EI_InterchangeType);
			AssertEquals("USC", interchange.EI_To);
			AssertEquals("ABC", interchange.EI_From);
			AssertEquals(interchange.EI_BodyText, message.EM_MessageText);
			AssertEquals(string.Empty, interchange.EI_HeaderText);
			AssertEquals(string.Empty, interchange.EI_FooterText);
		}
	}
}
