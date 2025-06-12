using System;
using System.Data.SqlClient;
using CargoWise.eHub.Common;
using CargoWise.eHub.DataAccess.Integration;

namespace CargoWise.eHub.DataAccess.Sql
{
	[Serializable]
	public class OutboxAccessor : IOutboxAccessor
	{
		public OutboxAccessor() : this(new eServices.eHubDataAccess.Sql.OutboxAccessor()) { }

		public OutboxAccessor(eServices.eHubDataAccess.Integration.IOutboxAccessor outboxAccessor)
		{
			this.outboxAccessor = outboxAccessor;
		}

		private readonly eServices.eHubDataAccess.Integration.IOutboxAccessor outboxAccessor;

		public void UpdateMessageDistributionStatus(string messageTrackingID, string senderID, string recipientID)
			=> outboxAccessor.UpdateMessageDistributionStatus(messageTrackingID, senderID, recipientID);

		public void UpdateInboxMessageDistributionStatus(string messageTrackingID)
			=> outboxAccessor.UpdateInboxMessageDistributionStatus(messageTrackingID);

		public void UpdateBatchedMessagesDistributionStatuses(string requestID)
			=> outboxAccessor.UpdateBatchedMessagesDistributionStatuses(requestID);

		public void UpdateMessageStatus(string messageTrackingID = null, string inboxPK = null, string outboxPK = null, int? inboxStatus = null, int? outboxStatus = null)
			=> outboxAccessor.UpdateMessageStatus(messageTrackingID, inboxPK, outboxPK, inboxStatus, outboxStatus);

		public eHubGatewayMessage[] GetMessageBatch(string recipientID, string batchTrackingID, int numberOfCandidates, long batchSizeLimitInBytes, int orderedDeliveryThresholdInSeconds)
			=> outboxAccessor.GetMessageBatch(recipientID, batchTrackingID, numberOfCandidates, batchSizeLimitInBytes, orderedDeliveryThresholdInSeconds);

		public void GenerateSuccessStatusMessage(Guid trackingID, string senderID)
			=> outboxAccessor.GenerateSuccessStatusMessage(trackingID, senderID);

		public void InsertToOutbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message, Guid inboxPK)
			=> outboxAccessor.InsertToOutbox(senderID, envelopeTrackingID, message, inboxPK);

		public void InsertToOutbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message, Guid inboxPK, SqlConnection connection, SqlTransaction transaction, byte[] cropSequence)
			=> outboxAccessor.InsertToOutbox(senderID, envelopeTrackingID, message, inboxPK, connection, transaction, cropSequence);
	}
}
