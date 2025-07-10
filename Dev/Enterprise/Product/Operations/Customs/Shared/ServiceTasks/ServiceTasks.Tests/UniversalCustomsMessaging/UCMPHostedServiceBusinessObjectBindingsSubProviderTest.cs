using System;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.ServiceManager.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	[TestedType(typeof(UCMPHostedServiceBusinessObjectBindingsSubProvider))]
	sealed class UCMPHostedServiceBusinessObjectBindingsSubProviderTest : HostedServiceBusinessObjectBindingsSubProviderTest<UCMPHostedServiceBusinessObjectBindingsSubProvider>
	{
		protected override int ExpectedBindingsCount => 1;

		protected override Type ExpectedBindingItemType => typeof(UCMPBusinessObjectBinding);

		public void TestBindingItem_ReturnsIncomingMessageProcessingItem()
		{
			using (new UCMPProcessorsRegistrationSubstitute(
				("CD1", new UniversalCustomsMessageProcessorTestClass()),
				("CD3", new UniversalCustomsMessageProcessorTestClass()),
				("CD2", new UniversalCustomsMessageProcessorTestClass())))
			{
				var bindingObjects = CreateSubProvider().BusinessObjectBindings;
				AssertNotNull(bindingObjects);
				AssertEquals("Binding Object Count", 1, bindingObjects.Count());

				var bindingObject = bindingObjects.SingleOrDefault();
				AssertNotNull("Binding Item", bindingObject);

				var expectedPredicates = new[]
				{
				"EM_Status=QUE",
				"EM_ReceiveTransmit=RCV",
				"EM_IsActive=1",
				"EM_ApplicationCode IN ('CD1', 'CD2', 'CD3')",
				"EM_HeldUntilDate IS NULL OR EM_HeldUntilDate < GETUTCNOW()"
			};

				CombineAssertions("Binding Object Properties", () =>
				{
					AssertEquals("Table", "EDIMessage", bindingObject.Table);
					AssertEquals("ServiceTaskCode", UniversalCustomsMessagingConstants.ServiceTaskCodes.KeyGen, bindingObject.ServiceTaskCode);
					AssertEquals("QueueName", "UCM Incoming Message Processing", bindingObject.QueueName);
					AssertContainsExactElementsInAnyOrder(expectedPredicates, bindingObject.Predicates);
				});
			}
		}

		public void TestBindingItem_WhenNoSubscribersAreRegistered()
		{
			using (new UCMPProcessorsRegistrationSubstitute())
			{
				var bindingObjects = CreateSubProvider().BusinessObjectBindings;
				AssertNotNull(bindingObjects);
				AssertEquals("Binding Object Count", 0, bindingObjects.Count());
			}
		}
	}
}
