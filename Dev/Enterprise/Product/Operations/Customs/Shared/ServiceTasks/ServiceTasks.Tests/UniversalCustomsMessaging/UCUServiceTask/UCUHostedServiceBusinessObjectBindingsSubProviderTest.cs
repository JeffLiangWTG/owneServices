using System;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ServiceManager.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	[TestedType(typeof(UCUHostedServiceBusinessObjectBindingsSubProvider))]
	sealed class UCUHostedServiceBusinessObjectBindingsSubProviderTest : HostedServiceBusinessObjectBindingsSubProviderTest<UCUHostedServiceBusinessObjectBindingsSubProvider>
	{
		protected override int ExpectedBindingsCount => 1;

		protected override Type ExpectedBindingItemType => typeof(UCMPBusinessObjectBinding);

		public void TestBindingItem_ReturnsIncomingInterchangeProcessingItem()
		{
			var unpacker1 = new UCUSubscribersTest.InterchangeUnpackerForTest();
			var unpacker2 = new UCUSubscribersTest.InterchangeUnpackerForTest();
			var unpacker3 = new UCUSubscribersTest.InterchangeUnpackerForTest();
			using (new UCUInterchangeUnpackersRegistrationSubstitute(new (string ApplicationCode, IUniversalCustomsInterchangeUnpacker Unpacker)[]
			{
				("CD3", unpacker1),
				("CD1", unpacker2),
				("CD2", unpacker3)
			}))
			{
				var bindingObjects = CreateSubProvider().BusinessObjectBindings;
				AssertNotNull(bindingObjects);
				AssertEquals("Binding Object Count", 1, bindingObjects.Count());

				var bindingObject = bindingObjects.SingleOrDefault();
				AssertNotNull("Binding Item", bindingObject);

				var expectedPredicates = new[]
				{
					"EI_Status=QUE",
					"EI_ReceiveTransmit=RCV",
					"EI_IsActive=1",
					"EI_ApplicationCode IN ('CD1', 'CD2', 'CD3')",
					"EI_TransportType=XTT"
				};

				CombineAssertions("Binding Object Properties", () =>
				{
					AssertEquals("Table", "EDIInterchange", bindingObject.Table);
					AssertEquals("ServiceTaskCode", UniversalCustomsMessagingConstants.ServiceTaskCodes.Unpacking, bindingObject.ServiceTaskCode);
					AssertEquals("QueueName", "UCU Interchange Unpacking", bindingObject.QueueName);
					AssertContainsExactElementsInAnyOrder(expectedPredicates, bindingObject.Predicates);
				});
			}
		}

		public void TestBindingItem_WhenNoSubscribersAreRegistered()
		{
			using (new UCUInterchangeUnpackersRegistrationSubstitute())
			{
				var bindingObjects = CreateSubProvider().BusinessObjectBindings;
				AssertNotNull(bindingObjects);
				AssertEquals("Binding Object Count", 0, bindingObjects.Count());
			}
		}
	}
}
