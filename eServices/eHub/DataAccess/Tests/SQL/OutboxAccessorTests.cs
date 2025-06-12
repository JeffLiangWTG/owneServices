using System;
using System.Data.SqlClient;
using CargoWise.eHub.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.DataAccess.Tests.SQL
{
	internal class OutboxAccessorTests
	{
		[Test]
		public void UpdateMessageDistributionStatus_CallsSharedDataAccess()
		{
			var sharedOutboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IOutboxAccessor>();
			var eHubOutboxAccessor = new CargoWise.eHub.DataAccess.Sql.OutboxAccessor(sharedOutboxAccessor.Object);
			var message = new eHubGatewayMessage();
			eHubOutboxAccessor
				.UpdateMessageDistributionStatus("messageTrackingID", "senderID", "recipientID");
			sharedOutboxAccessor.Verify(x => x
				.UpdateMessageDistributionStatus("messageTrackingID", "senderID", "recipientID"), Times.Once);
		}

		[Test]
		public void UpdateInboxMessageDistributionStatus_CallsSharedDataAccess()
		{
			var sharedOutboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IOutboxAccessor>();
			var eHubOutboxAccessor = new CargoWise.eHub.DataAccess.Sql.OutboxAccessor(sharedOutboxAccessor.Object);
			var message = new eHubGatewayMessage();
			eHubOutboxAccessor
				.UpdateInboxMessageDistributionStatus("messageTrackingID");
			sharedOutboxAccessor.Verify(x => x
				.UpdateInboxMessageDistributionStatus("messageTrackingID"), Times.Once);
		}

		[Test]
		public void UpdateBatchedMessagesDistributionStatuses_CallsSharedDataAccess()
		{
			var sharedOutboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IOutboxAccessor>();
			var eHubOutboxAccessor = new CargoWise.eHub.DataAccess.Sql.OutboxAccessor(sharedOutboxAccessor.Object);
			var message = new eHubGatewayMessage();
			eHubOutboxAccessor
				.UpdateBatchedMessagesDistributionStatuses("requestID");
			sharedOutboxAccessor.Verify(x => x
				.UpdateBatchedMessagesDistributionStatuses("requestID"), Times.Once);
		}

		[Test]
		public void UpdateMessageStatus_CallsSharedDataAccess()
		{
			var sharedOutboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IOutboxAccessor>();
			var eHubOutboxAccessor = new CargoWise.eHub.DataAccess.Sql.OutboxAccessor(sharedOutboxAccessor.Object);
			var message = new eHubGatewayMessage();
			eHubOutboxAccessor
				.UpdateMessageStatus("messageTrackingID", "inboxPK", "outboxPK", 3, 3);
			sharedOutboxAccessor.Verify(x => x
				.UpdateMessageStatus("messageTrackingID", "inboxPK", "outboxPK", 3, 3), Times.Once);
		}

		[Test]
		public void GetMessageBatch_CallsSharedDataAccess()
		{
			var sharedOutboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IOutboxAccessor>();
			var eHubOutboxAccessor = new CargoWise.eHub.DataAccess.Sql.OutboxAccessor(sharedOutboxAccessor.Object);
			var message = new eHubGatewayMessage();
			eHubOutboxAccessor
				.GetMessageBatch("recipientID", "batchTrackingID", 50, 100000000, 30);
			sharedOutboxAccessor.Verify(x => x
				.GetMessageBatch("recipientID", "batchTrackingID", 50, 100000000, 30), Times.Once);
		}

		[Test]
		public void GenerateSuccessStatusMessage_CallsSharedDataAccess()
		{
			var sharedOutboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IOutboxAccessor>();
			var eHubOutboxAccessor = new CargoWise.eHub.DataAccess.Sql.OutboxAccessor(sharedOutboxAccessor.Object);
			var message = new eHubGatewayMessage();
			eHubOutboxAccessor
				.GenerateSuccessStatusMessage(Guid.Empty, "senderID");
			sharedOutboxAccessor.Verify(x => x
				.GenerateSuccessStatusMessage(Guid.Empty, "senderID"), Times.Once);
		}

		[Test]
		public void InsertToOutbox_CallsSharedDataAccess()
		{
			var sharedOutboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IOutboxAccessor>();
			var eHubOutboxAccessor = new CargoWise.eHub.DataAccess.Sql.OutboxAccessor(sharedOutboxAccessor.Object);
			var message = new eHubGatewayMessage();
			eHubOutboxAccessor
				.InsertToOutbox("senderID", Guid.Empty, message, Guid.Empty);
			sharedOutboxAccessor.Verify(x => x
				.InsertToOutbox("senderID", Guid.Empty, message, Guid.Empty), Times.Once);
		}

		[Test]
		public void InsertToOutbox_CropSequence_CallsSharedDataAccess()
		{
			var sharedOutboxAccessor = new Mock<eServices.eHubDataAccess.Integration.IOutboxAccessor>();
			var eHubOutboxAccessor = new CargoWise.eHub.DataAccess.Sql.OutboxAccessor(sharedOutboxAccessor.Object);
			var message = new eHubGatewayMessage();
			byte[] cropSequence = { };
			eHubOutboxAccessor
				.InsertToOutbox("senderID", Guid.Empty, message, Guid.Empty, (SqlConnection)null, (SqlTransaction)null, cropSequence);
			sharedOutboxAccessor.Verify(x => x
				.InsertToOutbox("senderID", Guid.Empty, message, Guid.Empty, (SqlConnection)null, (SqlTransaction)null, cropSequence), Times.Once);
		}
	}
}
