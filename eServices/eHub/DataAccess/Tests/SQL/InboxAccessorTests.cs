using System;
using System.Data.SqlClient;
using CargoWise.eHub.Common;
using CargoWise.eHub.Integration;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.SQL
{
	internal class InboxAccessorTests
	{
		[Test]
		public void InsertToInbox_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			var message = new eHubGatewayMessage();
			eHubInboxAccessor
				.InsertToInbox("senderID", Guid.Empty, message);
			sharedInboxAccessor.Verify(x => x
				.InsertToInbox("senderID", Guid.Empty, message), Times.Once);
		}

		[Test]
		public void SubmitError_Ordered_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			var message = new eHubGatewayMessage();
			eHubInboxAccessor
				.InsertToInbox("senderID", Guid.Empty, message, true);
			sharedInboxAccessor.Verify(x => x
				.InsertToInbox("senderID", Guid.Empty, message, true), Times.Once);
		}

		[Test]
		public void SubmitError_EIPK_Status_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			var message = new eHubGatewayMessage();
			eHubInboxAccessor
				.InsertToInbox("senderID", Guid.Empty, Guid.Empty, MessageStatus.Received, message);
			sharedInboxAccessor.Verify(x => x
				.InsertToInbox("senderID", Guid.Empty, Guid.Empty, MessageStatus.Received, message), Times.Once);
		}

		[Test]
		public void SubmitError_EIPK_Status_Ordered_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			var message = new eHubGatewayMessage();
			eHubInboxAccessor
				.InsertToInbox("senderID", Guid.Empty, Guid.Empty, MessageStatus.Received, message, true);
			sharedInboxAccessor.Verify(x => x
				.InsertToInbox("senderID", Guid.Empty, Guid.Empty, MessageStatus.Received, message, true), Times.Once);
		}

		[Test]
		public void SubmitError_EIPK_Status_Ordered_Context_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			var message = new eHubGatewayMessage();
			eHubInboxAccessor
				.InsertToInbox("senderID", Guid.Empty, Guid.Empty, MessageStatus.Received, message, true, "contextProperty");
			sharedInboxAccessor.Verify(x => x
				.InsertToInbox("senderID", Guid.Empty, Guid.Empty, MessageStatus.Received, message, true, "contextProperty"), Times.Once);
		}

		[Test]
		public void SubmitError_EIPK_Status_Ordered_Context_Transaction_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			var message = new eHubGatewayMessage();
			eHubInboxAccessor
				.InsertToInbox("senderID", Guid.Empty, Guid.Empty, MessageStatus.Received, message, true, (SqlTransaction)null, "contextProperty");
			sharedInboxAccessor.Verify(x => x
				.InsertToInbox("senderID", Guid.Empty, Guid.Empty, MessageStatus.Received, message, true, (SqlTransaction)null, "contextProperty"), Times.Once);
		}

		[Test]
		public void GetMessageStatus_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			eHubInboxAccessor
				.GetMessageStatus("trackingID", "senderID");
			sharedInboxAccessor.Verify(x => x
				.GetMessageStatus("trackingID", "senderID"), Times.Once);
		}

		[Test]
		public void GetUnprocessedMessageCount_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			eHubInboxAccessor
				.GetUnprocessedMessageCount("senderId", "recipientId");
			sharedInboxAccessor.Verify(x => x
				.GetUnprocessedMessageCount("senderId", "recipientId"), Times.Once);
		}

		[Test]
		public void EnqueueMessage_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			IMessageQueuer queuer = new Mock<IMessageQueuer>().Object;
			eHubGatewayMessage message = new eHubGatewayMessage();
			eHubInboxAccessor
				.EnqueueMessage(queuer, "senderID", Guid.Empty, message);
			sharedInboxAccessor.Verify(x => x
				.EnqueueMessage(queuer, "senderID", Guid.Empty, message), Times.Once);
		}

		[Test]
		public void EnqueueMessage_Lite_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			IMessageQueuerLite queuer = new Mock<IMessageQueuerLite>().Object;
			eHubGatewayMessage message = new eHubGatewayMessage();
			eHubInboxAccessor
				.EnqueueMessage(queuer, "senderID", Guid.Empty, message);
			sharedInboxAccessor.Verify(x => x
				.EnqueueMessage(queuer, "senderID", Guid.Empty, message), Times.Once);
		}

		[Test]
		public void InsertToInboxAndOutbox_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			eHubGatewayMessage message = new eHubGatewayMessage();
			eHubInboxAccessor
				.InsertToInboxAndOutbox("senderID", Guid.Empty, message);
			sharedInboxAccessor.Verify(x => x
				.InsertToInboxAndOutbox("senderID", Guid.Empty, message), Times.Once);
		}

		[Test]
		public void InsertToInboxAndOutbox_DifferentMsgs_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			eHubGatewayMessage inboxMessage = new eHubGatewayMessage();
			eHubGatewayMessage outboxMessage = new eHubGatewayMessage();
			eHubInboxAccessor
				.InsertToInboxAndOutbox("senderID", Guid.Empty, inboxMessage, outboxMessage);
			sharedInboxAccessor.Verify(x => x
				.InsertToInboxAndOutbox("senderID", Guid.Empty, inboxMessage, outboxMessage), Times.Once);
		}

		[Test]
		public void InsertToInboxAndOutbox_Transaction_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			eHubGatewayMessage message = new eHubGatewayMessage();
			eHubInboxAccessor
				.InsertToInboxAndOutbox((SqlTransaction)null, "senderID", Guid.Empty, message);
			sharedInboxAccessor.Verify(x => x
				.InsertToInboxAndOutbox((SqlTransaction)null, "senderID", Guid.Empty, message), Times.Once);
		}

		[Test]
		public void InsertToInboxAndOutbox_Transaction_DifferentSenders_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			eHubGatewayMessage message = new eHubGatewayMessage();
			eHubInboxAccessor
				.InsertToInboxAndOutbox((SqlTransaction)null, "inboxSender", "outboxSender", Guid.Empty, message);
			sharedInboxAccessor.Verify(x => x
				.InsertToInboxAndOutbox((SqlTransaction)null, "inboxSender", "outboxSender", Guid.Empty, message), Times.Once);
		}

		[Test]
		public void InsertToInboxAndOutbox_Transaction_DifferentSendersAndMsgs_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			eHubGatewayMessage inboxMessage = new eHubGatewayMessage();
			eHubGatewayMessage outboxMessage = new eHubGatewayMessage();
			eHubInboxAccessor
				.InsertToInboxAndOutbox((SqlTransaction)null, "inboxSender", "outboxSender", Guid.Empty, inboxMessage, outboxMessage);
			sharedInboxAccessor.Verify(x => x
				.InsertToInboxAndOutbox((SqlTransaction)null, "inboxSender", "outboxSender", Guid.Empty, inboxMessage, outboxMessage), Times.Once);
		}

		[Test]
		public void InsertToInboxAndInboxXmlContent_CallsSharedDataAccess()
		{
			var sharedInboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IInboxAccessor>();
			var eHubInboxAccessor = new CargoWise.eHub.DataAccess.Sql.InboxAccessor(sharedInboxAccessor.Object);
			eHubGatewayMessage message = new eHubGatewayMessage();
			eHubInboxAccessor
				.InsertToInboxAndInboxXmlContent("senderID", Guid.Empty, message);
			sharedInboxAccessor.Verify(x => x
				.InsertToInboxAndInboxXmlContent("senderID", Guid.Empty, message), Times.Once);
		}
	}
}
