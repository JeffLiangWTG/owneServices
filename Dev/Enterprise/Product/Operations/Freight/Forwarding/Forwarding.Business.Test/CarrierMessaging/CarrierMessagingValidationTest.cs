using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestsSubclassesOf(typeof(CarrierMessagingValidation))]
	class CarrierMessagingValidationTest : TestCaseWithFactory
	{
		public void TestDoNotAllowToSendMessageFromNewUnsavedConsol()
		{
			AssertValidationMessageForDirtyConsol(Factory.New<ForwardingConsol>());
		}

		public void TestDoNotAllowToSendMessageFromConsolWithPendingChanges()
		{
			var consol = Factory.New<ForwardingConsol>();

			Factory.Save();

			consol.JK_BookingReference = "ABCD";

			AssertEquals("prerequisite - consol has pending changes", true, consol.HasChanges);
			AssertValidationMessageForDirtyConsol(Factory.New<ForwardingConsol>());
		}

		void AssertValidationMessageForDirtyConsol(ForwardingConsol consol)
		{
			var validation = GetNewValidation(consol);

			var notifications = new NotificationBufferTestClass();

			validation.Validate(notifications);

			AssertContains("validation message", "must be saved before sending message to a carrier.", notifications.AsString);
		}

		protected virtual CarrierMessagingValidation GetNewValidation(ForwardingConsol consol)
		{
			return new CarrierMessagingValidation(consol);
		}
	}
}
