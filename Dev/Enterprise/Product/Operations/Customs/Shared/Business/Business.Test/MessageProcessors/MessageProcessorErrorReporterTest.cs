using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessageProcessors.ErrorReporting.Testing
{
	sealed class MessageProcessorErrorReporterTest : TestCaseWithFactory
	{
		public void TestProcessException()
		{
			const string errorMessage = "Test message error";
			var messageProcessor = new ErrorNotificationForTesting();
			MessageProcessorErrorReporter.ProcessException(new MessageProcessorException(errorMessage, message, messageProcessor), true);

			AssertContains("LastLog", "Test Message Processor: Test message error", messageProcessor.LastLog);
			AssertEquals("Email Error should be sent", true, messageProcessor.SendErrorRun);
			AssertEquals("Email Error should be sent to Post Master", true, messageProcessor.SendErrorToPostMasterRun);

			AssertEquals("Email subject", "Test Message Processor Error Report", messageProcessor.SentEmail.Subject);
			const string emailBody = "There has been a problem processing the attached REL (EDI Message) message.</b>\r\n<br />\r\n<br />\r\nError Details:<br />\r\n<br />\r\nTest message error";
			AssertContains("Email body", emailBody, messageProcessor.SentEmail.Body);
			AssertContains("EM_MessageInterpretation", emailBody, message.EM_MessageInterpretation);
			AssertEquals("Attachments Count", 3, messageProcessor.SentEmail.Attachments.Count);
			AssertEquals("Attachment DisplayName", "Message.txt", messageProcessor.SentEmail.Attachments[2].DisplayName);
			AssertEquals("Attachment Data", "Junk Email Message", Encoding.ASCII.GetString(messageProcessor.SentEmail.Attachments[2].Data));
		}

		public void TestProcessExceptionWithOverrides()
		{
			const string errorMessage = "Test message error";
			var messageProcessor = new ErrorNotificationForTesting();
			MessageProcessorErrorReporter.ProcessException(new MessageProcessorException(errorMessage, message, messageProcessor, "Subject Override", true));

			AssertContains("LastLog", "Test Message Processor: Test message error", messageProcessor.LastLog);
			AssertEquals("Email Error should be sent", true, messageProcessor.SendErrorRun);
			AssertEquals("Email Error should be sent to Post Master", true, messageProcessor.SendErrorToPostMasterRun);

			AssertEquals("Email subject", "Subject Override", messageProcessor.SentEmail.Subject);
			AssertContains("Email body", "There has been a problem processing the attached REL (EDI Message) message.</b>\r\n<br />\r\n<br />\r\nError Details:<br />\r\n<br />\r\nTest message error", messageProcessor.SentEmail.Body);
			AssertEquals("Attachments Count", 2, messageProcessor.SentEmail.Attachments.Count);
		}

		public void TestProcessCriticalException()
		{
			const string errorMessage = "Test message error";
			var messageProcessor = new ErrorNotificationForTesting();
			MessageProcessorErrorReporter.ProcessException(new CriticalMessageProcessorException("Test Error", errorMessage, message, messageProcessor));
			AssertContains("LastLog", "Test Message Processor: Test message error", messageProcessor.LastLog);
			AssertEquals("Email Error should be sent", true, messageProcessor.SendErrorRun);
			AssertEquals("Email Error should be sent to Post Master", true, messageProcessor.SendErrorToPostMasterRun);
			AssertEquals("LastKeyReported", "Message Processor: Test, Message Type: REL, Error Type: Test Error", ErrorReporter.LastKeyReported);
			AssertContains("LastMessageReported", "Error: Test message error. Message: Junk Email Message.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			message = Factory.New<EDIMessage>();
			message.EM_MessageText = "Junk Email Message";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = "REL";
		}

		EDIMessage message;
	}
}
