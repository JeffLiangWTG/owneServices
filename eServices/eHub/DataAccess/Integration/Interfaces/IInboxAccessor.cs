using System;
using CargoWise.eHub.Common;
using CargoWise.eHub.Integration;
using System.Data.SqlClient;

namespace CargoWise.eHub.DataAccess.Integration
{
	public interface IInboxAccessor
	{
		void InsertToInbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message);
		void InsertToInbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message, bool orderedDelivery);
		void InsertToInbox(string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage message);
		void InsertToInbox(string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage message, bool orderedDelivery);
		void InsertToInbox(string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage message, bool orderedDelivery, string contextProperty);
		void InsertToInbox(string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage message, bool orderedDelivery, SqlTransaction transaction, string contextProperty);
		string GetMessageStatus(string trackingID, string senderID);
		int GetUnprocessedMessageCount(string senderId, string recipientId);
		void EnqueueMessage(IMessageQueuer queuer, string senderID, Guid inboxPk, eHubGatewayMessage message);
		void EnqueueMessage(IMessageQueuerLite queuer, string senderID, Guid inboxPk, eHubGatewayMessage message);
		void InsertToInboxAndOutbox(String senderID, Guid envelopeTrackingID, eHubGatewayMessage message);
        void InsertToInboxAndOutbox(String senderID, Guid envelopeTrackingID, eHubGatewayMessage inboxMessage, eHubGatewayMessage outboxMessage);
        void InsertToInboxAndOutbox(SqlTransaction transaction, String senderID, Guid envelopeTrackingID, eHubGatewayMessage message);
        void InsertToInboxAndOutbox(SqlTransaction transaction, String inboxSender, String outboxSender, Guid envelopeTrackingID, eHubGatewayMessage message);
        void InsertToInboxAndOutbox(SqlTransaction transaction, String inboxSender, String outboxSender, Guid envelopeTrackingID, eHubGatewayMessage inboxMessage, eHubGatewayMessage outboxMessage);
        void InsertToInboxAndInboxXmlContent(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message);
	}
}
