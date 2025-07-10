using System;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging;
using Enterprise.ServiceManager.Shared.Testing;
using NUnit.Framework;
using static Enterprise.Customs.ServiceTasks.Testing.UCPSubscribersTest;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	[TestedType(typeof(UCPHostedServiceBusinessObjectBindingsSubProvider))]
	sealed class UCPHostedServiceBusinessObjectBindingsSubProviderTest : HostedServiceBusinessObjectBindingsSubProviderTest<UCPHostedServiceBusinessObjectBindingsSubProvider>
	{
		protected override int ExpectedBindingsCount => 1;

		protected override Type ExpectedBindingItemType => typeof(UCMPBusinessObjectBinding);

		public void TestBindingItem_ReturnsIncomingMessageProcessingItem()
		{
			var bindingObjects = CreateSubProvider().BusinessObjectBindings;
			AssertNotNull(bindingObjects);
			AssertEquals("Binding Object Count", 1, bindingObjects.Count());

			var bindingObject = bindingObjects.SingleOrDefault();
			AssertNotNull("Binding Item", bindingObject);

			var expectedPredicates = new[]
			{
				"EM_Status=QUE",
				"EM_ReceiveTransmit=TRX",
				"EM_IsActive=1",
				"EM_ApplicationCode IN ('CD1', 'CD2', 'CD3')",
				"EM_HeldUntilDate IS NULL OR EM_HeldUntilDate < GETUTCNOW()"
			};

			CombineAssertions("Binding Object Properties", () =>
			{
				AssertEquals("Table", "EDIMessage", bindingObject.Table);
				AssertEquals("ServiceTaskCode", UniversalCustomsMessagingConstants.ServiceTaskCodes.Packing, bindingObject.ServiceTaskCode);
				AssertEquals("QueueName", "UCP Outgoing Message Packing", bindingObject.QueueName);
				AssertContainsExactElementsInAnyOrder(expectedPredicates, bindingObject.Predicates);
			});
		}

		public void TestBindingItem_WhenNoSubscribersAreRegistered()
		{
			using (new UCPMessagePackersRegistrationSubstitute())
			{
				var bindingObjects = CreateSubProvider().BusinessObjectBindings;
				AssertNotNull(bindingObjects);
				AssertEquals("Binding Object Count", 0, bindingObjects.Count());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			substituteDisposable = new UCPMessagePackersRegistrationSubstitute(("CD1", new MessagePackerForTest()),
				("CD2", new MessagePackerForTest()),
				("CD3", new MessagePackerForTest()));
		}

		protected override void TearDown()
		{
			base.TearDown();
			substituteDisposable?.Dispose();
			substituteDisposable = null;
		}

		IDisposable substituteDisposable;
	}
}
