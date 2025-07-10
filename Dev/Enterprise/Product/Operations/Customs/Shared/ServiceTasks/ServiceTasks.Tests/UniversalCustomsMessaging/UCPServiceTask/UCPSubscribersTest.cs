using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	sealed class UCPSubscribersTest : TestCase
	{
		public void TestHasUniversalCustomsMessagePackers()
		{
			using (new UCPMessagePackersRegistrationSubstitute())
			{
				AssertEquals("HasUniversalCustomsMessagePackers()", false, UCPSubscribers.HasUniversalCustomsMessagePackers());
			}
		}

		public void TestGetApplicationCodes()
		{
			using (new UCPMessagePackersRegistrationSubstitute(("A3$", new MessagePackerForTest()),
				("A2$", new MessagePackerForTest()),
				("A1$", new MessagePackerForTest())))
			{
				AssertContainsExactElementsInAnyOrder("GetApplicationCodes()", new[] { "A1$", "A2$", "A3$" }, UCPSubscribers.GetApplicationCodes());
			}
		}

		public void TestGetMessageProcessor()
		{
			var packer1 = new MessagePackerForTest();
			var packer2 = new MessagePackerForTest();
			using (new UCPMessagePackersRegistrationSubstitute(("A2$", packer1),
				("A1$", packer2)))
			{
				AssertSame("messageProcessor2", packer1, UCPSubscribers.GetMessagePacker("A2$"));
				AssertSame("messageProcessor1", packer2, UCPSubscribers.GetMessagePacker("A1$"));
				AssertNull("Unknown", UniversalCustomsMessagingSubscribers.GetMessageProcessor("A3$"));
			}
		}

		public class MessagePackerForTest : IUniversalCustomsEDIMessagePacker
		{
			public bool AllowEmptyMessageBody => true;

			public ZString Pack(EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
			{
				interchange.EI_ApplicationCode = message.EM_ApplicationCode;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_InterchangeNum = message.EM_MessageNum;
				interchange.EI_SessionGUID = ZGuid.NewZGuid();
				interchange.EI_TransportType = EDIInterchange.TransportType.xT;
				return ZString.Empty;
			}
		}
	}
}
