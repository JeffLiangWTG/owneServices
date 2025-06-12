using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using CargoWise.eServices.USCustoms.Common;
using Common.Logging;
using ServiceBroker.Interface;

namespace CargoWise.eServices.USCustoms.OutboundProcessingService
{
	public class OutboundMessageProcessingService : Service
	{
		public OutboundMessageProcessingService(SqlConnection connection, TimeSpan waitForTimeout, ILog logger, MQMessageSender messageSender)
			: this(connection, null, waitForTimeout, logger, messageSender)
		{
		}

		public OutboundMessageProcessingService(SqlConnection connection, SqlTransaction transaction, TimeSpan waitForTimeout, ILog logger, MQMessageSender messageSender)
			: base(IsProd ? ProdServiceName : TestServiceName, connection, transaction)
		{
			MessageSender = messageSender;
			Logger = logger;
		}

		static readonly bool IsProd = Convert.ToBoolean(ConfigurationManager.AppSettings["IsProd"]);
		static readonly string ProdServiceName = ServiceBrokerConstants.OutboundMessageProcessingServiceConstants.ServiceName;
		static readonly string TestServiceName = ServiceBrokerConstants.OutboundMessageProcessingServiceTestConstants.ServiceName;

		public override Guid ServiceHandle
		{
			get
			{
				return serviceHandle;
			}
		}

		static Guid serviceHandle = new Guid("C3992323-3268-4284-9B04-BA56B5EA2287");


		[BrokerMethod(ServiceBrokerConstants.OutboundMessageProcessingServiceConstants.MessageType)]
		public void ProcessRequestMessage(Message receivedMessage, SqlConnection connection, SqlTransaction transaction)
		{
			var customsMessage = new USCustomsOutboundMessage(receivedMessage.Body);

			Logger.Info(string.Format("Start processing message with inboxPK {0}", customsMessage.TrackingId));

			Stream responseMessageBody;
			MessageSender.Send(customsMessage, out responseMessageBody, IsProd);

			// Create the response
			Message msgSend = new Message(ServiceBrokerConstants.eHubOutboxServiceConstants.ReplyMessageType, responseMessageBody);

			// Send the response message back to the initiator of the conversation
			receivedMessage.Conversation.Send(msgSend, connection, transaction);

			Logger.Info(string.Format("Processing message with inboxPK {0} completed.", customsMessage.TrackingId));
		}

		[BrokerMethod(Message.EndDialogType)]
		public void EndConversation(Message ReceivedMessage, SqlConnection Connection, SqlTransaction Transaction)
		{
			// Ends the current Service Broker conversation
			ReceivedMessage.Conversation.EndConversation(Connection, Transaction);
		}

		[BrokerMethod(Message.ErrorType)]
		public void ProcessErrorMessages(Message ReceivedMessage, SqlConnection Connection, SqlTransaction Transaction)
		{
			// Ends the current Service Broker conversation due to an error
			Logger.Error(string.Format("Ending conversation {0}, conversation group id {1}", ReceivedMessage.Conversation.Handle, ReceivedMessage.ConversationGroupId));
			ReceivedMessage.Conversation.EndConversation(Connection, Transaction);
		}

		readonly MQMessageSender MessageSender;
		readonly ILog Logger;
	}
}
