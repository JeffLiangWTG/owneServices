using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	class UniversalCustomsApplicationTypeMessageProcessorTest : TestCase
	{
		public void TestApplicationCode()
		{
			var processor = new UniversalCustomsProcessingManagerTestClass(new LoggingInformation(), "A$#", new UniversalCustomsMessageProcessorTestClass());
			AssertEquals("ApplicationCode", "A$#", processor.ApplicationCode);
		}
	}

	class UniversalCustomsProcessingManagerTestClass : UniversalCustomsApplicationTypeMessageProcessor
	{
		public UniversalCustomsProcessingManagerTestClass(LoggingInformation logger, string applicationCode, IUniversalCustomsMessageProcessor customsMessageProcessor)
			: base(logger, applicationCode, customsMessageProcessor)
		{
		}

		public ZString[] StatusesToIncludeForTesting;
		protected override ZString[] StatusesToInclude => StatusesToIncludeForTesting;

		public Action<EDIMessage> ProcessMessageForTesting;
		protected override void ProcessMessageCore(EDIMessage message) => ProcessMessageForTesting?.Invoke(message);

		public string MessageFriendlyNameForTesting;
		protected override string MessageFriendlyNameCore => MessageFriendlyNameForTesting;
	}
}
