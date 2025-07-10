using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Moq;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;

namespace Enterprise.Freight.Forwarding.Documents.Testing.BE
{
	sealed class CertifiedPickupMessagingExtensionsTest : TestCaseWithFactory
	{
		public void TestContinueWithSendingMessage_TransferMode_MultipleTransferSentAwaitingResponse_NoneToSend()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();

			var containers = new List<CertifiedPickupContainer>();
			containers.AddRange(new CertifiedPickupContainer[] {
				new CertifiedPickupContainer(null, CertifiedPickup.FormModeTransfer)
				{
					CurrentStatus = CertifiedPickupConstants.Status.TransferSentAwaitingResponse,
				},
				new CertifiedPickupContainer(null, CertifiedPickup.FormModeTransfer)
				{
					CurrentStatus = CertifiedPickupConstants.Status.TransferSentAwaitingResponse,
				} });

			containers.ForEach(c => c.Action.IsTransferToForwarder = true);

			AssertEquals("Pre-condition: container 1 has transfer sent.", CertifiedPickupConstants.Status.TransferSentAwaitingResponse, containers[0].CurrentStatus);
			Assert("Pre-condition: container 1 is set to transfer.", containers[0].Action.IsTransferToForwarder);
			AssertEquals("Pre-condition: container 2 has transfer sent.", CertifiedPickupConstants.Status.TransferSentAwaitingResponse, containers[1].CurrentStatus);
			Assert("Pre-condition: container 2 is set to transfer.", containers[1].Action.IsTransferToForwarder);

			var certifiedPickup = new CertifiedPickup("zzz", "zzz");
			certifiedPickup.FormMode = CertifiedPickup.FormModeTransfer;
			certifiedPickup.Containers = containers;

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(certifiedPickup);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new CertifiedPickupMessagingExtensions(document.Object, null);

			AssertNullOrEmpty("MessagePurposeCodeOverride should be null or empty.", extensions.MessagePurposeCodeOverride);
			Assert("Cannot send message.", !extensions.ContinueWithSendingMessage(notifications.Object).Value);
			notifications.Verify(n => n.ShowMessage("No message will be sent. Please select any container that has not been transferred.", "Information"), Times.Once);
		}

		public void TestContinueWithSendingMessage_TransferMode_SingleTransferSentAwaitingResponse_OneToSend()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();

			var containers = new List<CertifiedPickupContainer>();
			containers.AddRange(new CertifiedPickupContainer[] {
				new CertifiedPickupContainer(null, CertifiedPickup.FormModeTransfer)
				{
					CurrentStatus = CertifiedPickupConstants.Status.Accepted,
				},
				new CertifiedPickupContainer(null, CertifiedPickup.FormModeTransfer)
				{
					CurrentStatus = CertifiedPickupConstants.Status.TransferSentAwaitingResponse,
				}
			});

			containers.ForEach(c => c.Action.IsTransferToForwarder = true);

			AssertEquals("Pre-condition: container 1 has transfer sent.", CertifiedPickupConstants.Status.Accepted, containers[0].CurrentStatus);
			Assert("Pre-condition: container 1 is set to transfer.", containers[0].Action.IsTransferToForwarder);
			AssertEquals("Pre-condition: container 2 has transfer sent.", CertifiedPickupConstants.Status.TransferSentAwaitingResponse, containers[1].CurrentStatus);
			Assert("Pre-condition: container 2 is set to transfer.", containers[1].Action.IsTransferToForwarder);

			var certifiedPickup = new CertifiedPickup("zzz", "zzz");
			certifiedPickup.FormMode = CertifiedPickup.FormModeTransfer;
			certifiedPickup.Containers = containers;

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(certifiedPickup);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new CertifiedPickupMessagingExtensions(document.Object, null);

			AssertNullOrEmpty("MessagePurposeCodeOverride should be null or empty.", extensions.MessagePurposeCodeOverride);
			Assert("Allow sending message.", extensions.ContinueWithSendingMessage(notifications.Object).Value);
			notifications.Verify(n => n.ShowMessage("No message will be sent. Please select any container to revoke.", "Information"), Times.Never);
		}

		public void TestMessagePurposeCodeOverride_RevokeMode()
		{
			var dynamicData = new Mock<IDynamicData>();
			var document = new Mock<IDocument>();
			var notifications = new Mock<IUserNotifications>();

			var containers = new List<CertifiedPickupContainer>();
			containers.AddRange(new CertifiedPickupContainer[] {
				new CertifiedPickupContainer(null, CertifiedPickup.FormModeRevoke)
				{
					CurrentStatus = CertifiedPickupConstants.Status.TransferSent,
				},
				new CertifiedPickupContainer(null, CertifiedPickup.FormModeRevoke)
				{
					CurrentStatus = CertifiedPickupConstants.Status.TransferSent,
				}
			});

			containers.ForEach(c =>
			{
				c.Action.IsTransferToForwarder = true;
				c.Action.IsRevokeMode = true;
			});

			AssertEquals("Pre-condition: container 1 has transfer sent.", CertifiedPickupConstants.Status.TransferSent, containers[0].CurrentStatus);
			Assert(containers[0].Action.IsRevokeMode);
			AssertEquals("Pre-condition: container 2 has transfer sent.", CertifiedPickupConstants.Status.TransferSent, containers[1].CurrentStatus);
			Assert(containers[1].Action.IsRevokeMode);

			var certifiedPickup = new CertifiedPickup("zzz", "zzz");
			certifiedPickup.FormMode = CertifiedPickup.FormModeRevoke;
			certifiedPickup.Containers = containers;

			dynamicData
				.SetupGet(d => d.Value)
				.Returns(certifiedPickup);

			document
				.SetupGet(d => d.Data)
				.Returns(dynamicData.Object);

			var extensions = new CertifiedPickupMessagingExtensions(document.Object, null);

			AssertEquals("MessagePurposeCodeOverride should be 'WTH'", MessagePurposes.Codes.Withdrawal, extensions.MessagePurposeCodeOverride);
		}
	}
}
