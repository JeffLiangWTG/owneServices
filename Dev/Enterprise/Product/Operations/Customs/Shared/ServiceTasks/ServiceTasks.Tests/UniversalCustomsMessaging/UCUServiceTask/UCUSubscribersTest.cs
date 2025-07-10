using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	sealed class UCUSubscribersTest : TestCaseWithFactory
	{
		public void TestHasUniversalCustomsInterchangeUnpackers()
		{
			using (new UCUInterchangeUnpackersRegistrationSubstitute())
			{
				AssertEquals("HasUniversalCustomsInterchangeUnpackers()", false, UCUSubscribers.HasUniversalCustomsInterchangeUnpackers());
			}
		}

		public void TestHasUniversalCustomsInterchangeUnpackers_EnableUCMTest()
		{
			var subscribers = new Dictionary<string, IDataCreator<IUniversalCustomsInterchangeUnpacker>>();
			var testTypeCollection = new UCMEDIMessageTestTypeCollection();
			var testType = testTypeCollection.AddNew();
			testType.ApplicationCode = "_T1";
			using (CustomsDataRegistry.Instance.UCMTestApplicationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testTypeCollection))
			using (CustomsDataRegistry.Instance.EnableUCMTest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (new UCUInterchangeUnpackersRegistrationSubstitute())
			{
				AssertEquals("HasUniversalCustomsInterchangeUnpackers()", true, UCUSubscribers.HasUniversalCustomsInterchangeUnpackers());
			}
		}

		public void TestGetApplicationCodes()
		{
			var unpacker1 = new InterchangeUnpackerForTest();
			var unpacker2 = new InterchangeUnpackerForTest();
			var unpacker3 = new InterchangeUnpackerForTest();
			using (new UCUInterchangeUnpackersRegistrationSubstitute(
				("A3$", unpacker1),
				("A2$", unpacker2),
				("A1$", unpacker3)
			))
			{
				AssertContainsExactElementsInAnyOrder("GetApplicationCodes()", new[] { "A1$", "A2$", "A3$" }, UCUSubscribers.GetApplicationCodes());
			}
		}

		public void TestGetMessageProcessor_EnableUCMTest()
		{
			var testTypeCollection = new UCMEDIMessageTestTypeCollection();
			var testType = testTypeCollection.AddNew();
			testType.ApplicationCode = "_T1";
			using (CustomsDataRegistry.Instance.UCMTestApplicationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testTypeCollection))
			using (CustomsDataRegistry.Instance.EnableUCMTest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (new UCUInterchangeUnpackersRegistrationSubstitute())
			{
				AssertSame("T1InterchangeUnpacker", typeof(T1InterchangeUnpacker), UCUSubscribers.GetInterchangeUnpacker("_T1").GetType());
			}
		}

		public void TestGetMessageProcessor()
		{
			var unpacker1 = new InterchangeUnpackerForTest();
			var unpacker2 = new InterchangeUnpackerForTest();
			using (new UCUInterchangeUnpackersRegistrationSubstitute(
				("A2$", unpacker1),
				("A1$", unpacker2)
			))
			{
				AssertSame("messageProcessor2", unpacker1, UCUSubscribers.GetInterchangeUnpacker("A2$"));
				AssertSame("messageProcessor1", unpacker2, UCUSubscribers.GetInterchangeUnpacker("A1$"));
				AssertNull("Unknown", UniversalCustomsMessagingSubscribers.GetMessageProcessor("A3$"));
			}
		}

		public class InterchangeUnpackerForTest : IUniversalCustomsInterchangeUnpacker
		{
			public InterchangeUnpackerForTest() { }

			public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, LoggingInformation logger)
			{
				var message = interchange.Factory.New<EDIMessage>();
				message.EM_ApplicationCode = interchange.EI_ApplicationCode;
				return new EDIInterchangeUnpackerResult(new[] { message });
			}
		}
	}
}
