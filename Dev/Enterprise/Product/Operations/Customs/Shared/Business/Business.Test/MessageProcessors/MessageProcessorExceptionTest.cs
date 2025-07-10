using CargoWise.EntityFramework.Testing;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessageProcessors.ErrorReporting.Testing
{
	sealed class MessageProcessorExceptionTest : TestCaseWithFactory
	{
		public void TestMessageProcessorException()
		{
			var messageProcessor = new ErrorNotificationForTesting();
			var ediMessage = Factory.New<EDIMessage>();
			ediMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			ediMessage.EM_MessageNum = "1";
			const string message = "Error Text";
			const string defaultSubject = "Test Message Processor Error Report";

			var ex = new MessageProcessorException(message, ediMessage, messageProcessor, "Subject Override", true);
			AssertEquals("MessageProcessor", messageProcessor, ex.MessageProcessor);
			AssertEquals("EDIMessage", ediMessage, ex.EDIMessage);
			AssertMessageProcessorException(ex, message, "Subject Override", true);

			ex = new MessageProcessorException(message, ediMessage, messageProcessor);
			AssertMessageProcessorException(ex, message, defaultSubject, false);

			ex = new UnableToInterpretMessageException(ediMessage, messageProcessor);
			AssertMessageProcessorException(ex, "The message processor was unable to interpret received message.", defaultSubject, false);

			ex = new CouldNotFindLinkedObjectException("JobNumber", ediMessage, messageProcessor);
			AssertMessageProcessorException(ex, "Could not find an associated business object (Job) for document reference = 'JobNumber'", defaultSubject, false);

			ex = new CouldNotFindAssociatedTransmitMessageException(ediMessage, messageProcessor);
			AssertMessageProcessorException(ex, @"No associated transmit message has been found that matches the following details:
Application Code 'CAI', Message Number '1'.", defaultSubject, false);

			var criticalEx = new CriticalMessageProcessorException("Error Type", message, ediMessage, messageProcessor);
			AssertMessageProcessorException(criticalEx, message, defaultSubject, false);
			AssertEquals("ErrorType", "Error Type", criticalEx.ErrorType);

			criticalEx = new InvalidFormatMessageProcessorException(new InvalidFormatException(message), ediMessage, messageProcessor);
			AssertMessageProcessorException(criticalEx, "The message processor has thrown an exception: " + message, defaultSubject, false);
			AssertEquals("ErrorType", "Invalid Format Exception", criticalEx.ErrorType);
		}

		void AssertMessageProcessorException(MessageProcessorException ex, string expectedMessage, string expectedSubject, bool expectedDoNotReferenceMessage)
		{
			AssertEquals("ExceptionText", expectedMessage, ex.Message);
			AssertEquals("Subject", expectedSubject, ex.Subject);
			AssertEquals("DoNotReferenceMessage", expectedDoNotReferenceMessage, ex.DoNotReferenceMessage);
		}
	}
}
