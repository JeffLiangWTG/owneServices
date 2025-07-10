using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing
{
	sealed class MessageProcessorTest : TestCase
	{
		public void TestOverrides()
		{
			var processor = new MessageProcessorForTesting();
			AssertEquals("ApplicationCode", EDIMessage.ApplicationCodes.USeManifest, processor.MessageProcessor_Exposed.ApplicationCode);
			AssertEquals("MessageFriendlyName", "e-Manifest Response", processor.MessageProcessor_Exposed.MessageFriendlyName);
		}

		sealed class MessageProcessorForTesting : MessageProcessor
		{
			internal ApplicationTypeMessageProcessor MessageProcessor_Exposed => MessageProcessors[0];
		}
	}
}
