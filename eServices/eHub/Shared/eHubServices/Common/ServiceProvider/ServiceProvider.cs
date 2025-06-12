using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.Xml;
using Common.Logging;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.Extensions;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder;
using System.Collections.Specialized;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider
{
	public abstract class ServiceProvider
	{
		protected MessageContext.MessageContext Context { get; private set; }

		readonly ILog logger;
		readonly string senderId;
		readonly string recipientId;
		string message;

		public void Process()
		{
			string replyText;

			try
			{
				message = ModifyMessage(message);
				LoadContext();
				replyText = GetReply();
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException()) throw;
				replyText = ProcessException(ex);
				if (string.IsNullOrEmpty(replyText)) throw;
			}

			if (!string.IsNullOrEmpty(replyText)) SendReplyToBiztalk(replyText);
		}

		protected virtual string ProcessException(Exception exception)
		{
			return string.Empty;
		}

		protected ServiceProvider(ILog logger, string senderId, string recipientId, string message)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;
			this.senderId = senderId;
			this.recipientId = recipientId;
			this.message = message;
		}

		protected void LoadContext()
		{
			Context = CeateMessageContext(senderId, recipientId, message);
			Context.Load();
		}

		protected string GetReply()
		{
			logger.Debug(Context);
			logger.DebugFormat("Message: \r\n {0}", message);
			var replyText = CallService();
			logger.DebugFormat("Reply from service: \r\n{0}", replyText);
			return CreateReplyMessageBuilder(replyText).GetReply();
		}

		protected virtual string ModifyMessage(string text)
		{
			return text;
		}

		protected virtual void SendReplyToBiztalk(string replyText)
		{
			var messageToSend = System.ServiceModel.Channels.Message.CreateMessage(System.ServiceModel.Channels.MessageVersion.Default, "SendMessage", new XmlTextReader(new StringReader(replyText)));
			using (var channelFactory = new ChannelFactory<ISendToBiztalk>("SendToBiztalkService"))
			{
				channelFactory.Endpoint.Contract.SessionMode = SessionMode.Allowed;
				var channel = channelFactory.CreateChannel();
				channel.SendMessage(messageToSend);
			}
		}

		protected abstract MessageContext.MessageContext CeateMessageContext(string senderId, string recipientId, string message);
		protected abstract ServiceReply CallService();
		protected abstract ReplyMessageBuilder.ReplyMessageBuilder CreateReplyMessageBuilder(ServiceReply reply);
		protected abstract NameValueCollection GetAppSettings();

		protected EndpointAddress GetEndpointAddress()
		{
			try
			{
				return new EndpointAddress(new Uri(GetAppSettings()[Context.eHubRecipientId]));
			}
			catch
			{
				throw new EndpointNotFoundException("Can not resolve Endpoint url by recipientId");
			}
		}
	}
}