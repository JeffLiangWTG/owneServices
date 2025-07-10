using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	sealed class ContainerLoadPlanMessagingExtensionsTest : TestCase
	{
		#region TestContinueWithSendingMessageAmendment

		public void TestContinueWithSendingMessageAmendment()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(new ContainerLoadPlan("zzz", "zzz", "zzz"));

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new ContainerLoadPlanMessagingExtensions(document.Object);
			var res = extensions.ContinueWithSendingMessageAmendment(notifications.Object);

			AssertEquals("Disallow sending amendments for Container Load Plan", false, res);

			dynamicData.Verify(fake => fake.Value, Times.Once);
			document.Verify(fake => fake.Data, Times.Once);
			notifications.Verify(fake => fake.ShowMessage("A message for this or a different container has previously been sent. Ningbo EDI Center does not support amendments or cancellation. Only original messages are accepted so to resend this message, use the \"Reset to Original\" option.", "Information"), Times.Once);
		}

		#endregion
	}
}
