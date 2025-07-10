using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(NotImplementedCarrierMessagingValidation))]
	sealed class NotImplementedCarrierMessagingValidationTest : CarrierMessagingValidationTest
	{
		public void TestValidate()
		{
			var consol = Factory.New<ForwardingConsol>();

			Factory.Save();

			var notifications = new NotificationBufferTestClass();

			var validation = new NotImplementedCarrierMessagingValidation(consol);

			validation.Validate(notifications);

			AssertContains("validation message", "Carrier messaging is currently available for Road Consolidations only.",
				notifications.AsString);
		}

		protected override CarrierMessagingValidation GetNewValidation(ForwardingConsol consol)
		{
			return new NotImplementedCarrierMessagingValidation(consol);
		}
	}
}
