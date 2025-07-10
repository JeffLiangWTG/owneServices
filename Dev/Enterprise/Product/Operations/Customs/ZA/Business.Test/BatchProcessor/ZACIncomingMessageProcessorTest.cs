using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ZA.Business.BatchProcessor.Testing
{
	sealed class ZACIncomingMessageProcessorTest : BranchMessageProcessorTest
	{
		public void TestFilterForAnyBranch()
		{
			var processor = new ZACIncomingMessageProcessor();

			var query = processor.GetMessageProcessorQuery().FilterString;

			AssertNotContains(EDIMessage.Schema.EM_GB, query);
		}

		protected override bool ExpectedExcludeBranchFilter => true;

		protected override BranchMessageProcessor GetNewBranchMessageProcessorForFilterTest() => new ZACIncomingMessageProcessor();

		protected override IMessageProcessorForTest GetNewMessageProcessorForTest(ZGuid[] messagePKsToFailOn = null) => new ZACIncomingMessageProcessorForTest(messagePKsToFailOn, null);

		protected override IMessageProcessorForTest GetNewMessageProcessorForTestThrowingExceptionInProcessMessage(Exception exceptionToThrow, int maxRetries = 0)
			=> new ZACIncomingMessageProcessorForTestThrowingExceptionInProcessMessage(exceptionToThrow, maxRetries);

		protected override IMessageProcessorForTest GetMessageProcessorForTestExceptionOnSave() => new ZACIncomingMessageProcessorForTestExceptionOnSave();

		protected override IMessageProcessorForTest GetMessageProcessorForTestProcessInNewFactory(ZGuid[] messagePKsToFailOn = null) => new ZACIncomingMessageProcessorForTestProcessInNewFactory(messagePKsToFailOn);

		sealed class ZACIncomingMessageProcessorForTestThrowingExceptionInProcessMessage : ZACIncomingMessageProcessorForTest
		{
			public ZACIncomingMessageProcessorForTestThrowingExceptionInProcessMessage(Exception exceptionToThrow, int maxRetries = 0)
			{
				this.exceptionToThrow = exceptionToThrow;
				this.maxRetries = maxRetries;
			}
			readonly Exception exceptionToThrow;
			readonly int maxRetries;

			protected override void ProcessMessageCore(ApplicationTypeMessageProcessor processor, EDIMessage message)
			{
				Logger.Log("Processing Message #" + message.EM_MessageNum);
				throw exceptionToThrow;
			}

			protected override bool ShouldRetryOnExceptionCore(int currentExceptionsCount, EDIMessage message, Exception lastException, int retryAttempts, string additionalErrorReportMessage = null)
			{
				return maxRetries > 0
					? currentExceptionsCount < maxRetries
					: base.ShouldRetryOnExceptionCore(currentExceptionsCount, message, lastException, retryAttempts, additionalErrorReportMessage);
			}
		}

		sealed class ZACIncomingMessageProcessorForTestExceptionOnSave : ZACIncomingMessageProcessorForTest
		{
			public ZACIncomingMessageProcessorForTestExceptionOnSave()
				: base()
			{
			}

			public const string ErrorMessage = "Some error happened during saving";
			public int SaveCount { get; set; }

			protected override void SaveAfterProcessingMessagesCore(BusinessObjectFactory factory)
			{
				SaveCount++;
				if (SaveCount == 1)
				{
					throw new Exception(ErrorMessage);
				}
				else
				{
					base.SaveAfterProcessingMessagesCore(factory);
				}
			}

			protected override bool ShouldRetryOnExceptionCore(int currentExceptionsCount, EDIMessage message, Exception lastException, int retryAttempts, string additionalErrorReportMessage = null)
			{
				return false;
			}
		}

		sealed class ZACIncomingMessageProcessorForTestProcessInNewFactory : ZACIncomingMessageProcessorForTest
		{
			public ZACIncomingMessageProcessorForTestProcessInNewFactory(ZGuid[] messagePKsToFailProcessingOn)
				: base(messagePKsToFailProcessingOn, null)
			{
			}

			protected override bool MessageShouldBeProcessedInASeparateFactory => true;
		}

		#region Expected Logs
		protected override string ExpectedLogTextForFailureExceptionRetry => @"Pre-Process Message #00001
Saving...
1 message pre-processed
Processing Message #00001
Exception processing message #00001 individually: \[Save Exception Thrown during ProcessMessage\(EDIMessage message\)\]
.
Processing Message #00001
Exception processing message #00001 individually: \[Save Exception Thrown during ProcessMessage\(EDIMessage message\)\]
.
Processing Message #00001
Exception processing message #00001 individually: \[Save Exception Thrown during ProcessMessage\(EDIMessage message\)\]
Error Processing Incoming EDI Message: UDM--00001
Exception occurred 3 times whilst processing a message individually. The message's status has been set to 'Failed'.
Enterprise.Messaging.Integration.MessageProcessingBusinessFailureException: Save Exception Thrown during ProcessMessage\(EDIMessage message\)
\s+at Enterprise.Customs.ZA.Business.BatchProcessor.Testing.+
\s+at Enterprise.Messaging.Business.+";
		protected override string ExpectedLogTextForFailureExceptionNotRetry => @"Pre-Process Message #00001
Saving...
1 message pre-processed
Processing Message #00001
Exception processing message #00001 individually: \[Cannot Save Exception Thrown during ProcessMessage\(EDIMessage message\)\]
Error Processing Incoming EDI Message: UDM--00001
Exception occurred 1 times whilst processing a message individually. The message's status has been set to 'Failed'.
Enterprise.Messaging.Integration.MessageProcessingBusinessFailureException: Cannot Save Exception Thrown during ProcessMessage\(EDIMessage message\)
\s+at Enterprise.Customs.ZA.Business.BatchProcessor.Testing.+
\s+at Enterprise.Messaging.Business.+";
		protected override string ExpectedLogTextForRepeatedExceptions(int retryAttempts) => @$"Pre-Process Message #00001
Saving...
1 message pre-processed
Processing Message #00001
Exception processing message #00001 individually: \[Exception Thrown during ProcessMessage\(EDIMessage message\)\]{string.Concat(Enumerable.Repeat(@"
.
Processing Message #00001
Exception processing message #00001 individually: \[Exception Thrown during ProcessMessage\(EDIMessage message\)\]", retryAttempts - 1))}
Error Processing Incoming EDI Message: TST--00001
Exception occurred {retryAttempts} times whilst processing a message individually. The message's status has been set to 'Failed'.
System.Exception: Exception Thrown during ProcessMessage\(EDIMessage message\)
\s+at Enterprise.Customs.ZA.Business.BatchProcessor.Testing.+
\s+at Enterprise.Messaging.Business.+
.";
		#endregion
	}

	class ZACIncomingMessageProcessorForTest : BranchMessageProcessorForTest
	{
		public ZACIncomingMessageProcessorForTest(ZGuid[] messagePKsToFailProcessingOn = null, ZGuid[] messagePKsToFailPreProcessingOn = null, ZGuid[] messagePKsToDiscardPreProcessingOn = null, bool switchBranches = false)
			: base(messagePKsToFailProcessingOn, messagePKsToFailPreProcessingOn, messagePKsToDiscardPreProcessingOn, null, switchBranches)
		{
		}
	}
}
