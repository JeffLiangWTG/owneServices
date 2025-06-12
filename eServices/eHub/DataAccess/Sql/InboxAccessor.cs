using System;
using System.Data.SqlClient;
using CargoWise.eHub.Common;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Integration;

namespace CargoWise.eHub.DataAccess.Sql
{
	[Serializable]
	public class InboxAccessor : IInboxAccessor
	{
		public InboxAccessor() : this(new eServices.eHubDataAccess.Sql.InboxAccessor()) { }

		public InboxAccessor(eServices.eHubDataAccess.Integration.IInboxAccessor inboxAccessor)
		{
			this.inboxAccessor = inboxAccessor;
		}

		private readonly eServices.eHubDataAccess.Integration.IInboxAccessor inboxAccessor;

		public void InsertToInbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
			=> inboxAccessor.InsertToInbox(senderID, envelopeTrackingID, message);

		public void InsertToInbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message, bool orderedDelivery)
			=> inboxAccessor.InsertToInbox(senderID, envelopeTrackingID, message, orderedDelivery);

		public void InsertToInbox(string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage message)
			=> inboxAccessor.InsertToInbox(senderID, envelopeTrackingID, inboxPK, status, message);

		public void InsertToInbox(string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage message, bool orderedDelivery)
			=> inboxAccessor.InsertToInbox(senderID, envelopeTrackingID, inboxPK, status, message, orderedDelivery);

		public void InsertToInbox(string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage message, bool orderedDelivery, string contextProperty)
			=> inboxAccessor.InsertToInbox(senderID, envelopeTrackingID, inboxPK, status, message, orderedDelivery, contextProperty);

		public void InsertToInbox(string senderID, Guid envelopeTrackingID, Guid inboxPK, MessageStatus status, eHubGatewayMessage message, bool orderedDelivery, SqlTransaction transaction, string contextProperty)
			=> inboxAccessor.InsertToInbox(senderID, envelopeTrackingID, inboxPK, status, message, orderedDelivery, transaction, contextProperty);

		public string GetMessageStatus(string trackingID, string senderID)
			=> inboxAccessor.GetMessageStatus(trackingID, senderID);

		public int GetUnprocessedMessageCount(string senderId, string recipientId)
			=> inboxAccessor.GetUnprocessedMessageCount(senderId, recipientId);

		public void EnqueueMessage(IMessageQueuer queuer, string senderID, Guid inboxPk, eHubGatewayMessage message)
			=> inboxAccessor.EnqueueMessage(queuer, senderID, inboxPk, message);

		public void EnqueueMessage(IMessageQueuerLite queuer, string senderID, Guid inboxPk, eHubGatewayMessage message)
			=> inboxAccessor.EnqueueMessage(queuer, senderID, inboxPk, message);

		public void InsertToInboxAndOutbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
			=> inboxAccessor.InsertToInboxAndOutbox(senderID, envelopeTrackingID, message);

		public void InsertToInboxAndOutbox(string senderID, Guid envelopeTrackingID, eHubGatewayMessage inboxMessage, eHubGatewayMessage outboxMessage)
			=> inboxAccessor.InsertToInboxAndOutbox(senderID, envelopeTrackingID, inboxMessage, outboxMessage);

		public void InsertToInboxAndOutbox(SqlTransaction transaction, string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
			=> inboxAccessor.InsertToInboxAndOutbox(transaction, senderID, envelopeTrackingID, message);

		public void InsertToInboxAndOutbox(SqlTransaction transaction, string inboxSender, string outboxSender, Guid envelopeTrackingID, eHubGatewayMessage message)
			=> inboxAccessor.InsertToInboxAndOutbox(transaction, inboxSender, outboxSender, envelopeTrackingID, message);

		public void InsertToInboxAndOutbox(SqlTransaction transaction, string inboxSender, string outboxSender, Guid envelopeTrackingID, eHubGatewayMessage inboxMessage, eHubGatewayMessage outboxMessage)
			=> inboxAccessor.InsertToInboxAndOutbox(transaction, inboxSender, outboxSender, envelopeTrackingID, inboxMessage, outboxMessage);

		public void InsertToInboxAndInboxXmlContent(string senderID, Guid envelopeTrackingID, eHubGatewayMessage message)
			=> inboxAccessor.InsertToInboxAndInboxXmlContent(senderID, envelopeTrackingID, message);
	}
}
