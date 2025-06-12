using CargoWise.eHub.Common;
using System;
using System.Data.SqlClient;

namespace CargoWise.eHub.DataAccess.Integration
{
	public interface IOutboxAccessor
	{
		void GenerateSuccessStatusMessage(Guid trackingID, string senderID);
		void UpdateMessageDistributionStatus(string messageTrackingID, string senderID, string recipientID);
		void UpdateBatchedMessagesDistributionStatuses(string trackingID);
		void UpdateMessageStatus(string messageTrackingID = null, string inboxPK = null, string outboxPK = null, int? inboxStatus = null, int? outboxStatus = null);
		void UpdateInboxMessageDistributionStatus(string messageTrackingID);
		eHubGatewayMessage[] GetMessageBatch(string recipientID, string batchTrackingID, int numberOfCandidates, long batchSizeLimitInBytes, int orderedDeliveryThresholdInSeconds);
		void InsertToOutbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message, Guid inboxPK);
		void InsertToOutbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message, Guid inboxPK, SqlConnection connection, SqlTransaction transaction, byte[] cropSequence);
	}
}
