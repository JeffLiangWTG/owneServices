using System;
using System.Configuration;
using System.Globalization;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider;
using CargoWise.eHub.Share.eHubServices.eHubSender.MessageContext;
using CargoWise.eHub.Share.eHubServices.eHubSender.ReplyMessageBuilder;
using Common.Logging;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.ServiceProvider
{
	[SupportedRecipient("APOMELTSH_ELM")]
	public class eLMSServiceProvider : CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider.ServiceProvider
	{
		public eLMSServiceProvider(ILog logger, string senderId, string recipientId, string message)
			: base(logger, senderId, recipientId, message)
		{
		}

		protected override CargoWise.eHub.Share.eHubServices.eHubSender.Common.MessageContext.MessageContext CeateMessageContext(string senderId, string recipientId, string message)
		{
			return new eLMSMessageContext(message, senderId, recipientId);
		}

		protected override ServiceReply CallService()
		{
			var client = new eLMSService.MailingStatementServicePortClient();
            if (client.ClientCredentials == null) throw new NullReferenceException("client.ClientCredentials");
			client.ClientCredentials.UserName.UserName = ConfigurationManager.AppSettings["eLMSUserName"];
			client.ClientCredentials.UserName.Password = ConfigurationManager.AppSettings["eLMSPassword"];
			client.Endpoint.Address = GetEndpointAddress();
			string reply = client.CreateMailingStatement(Context.Message).ToString(CultureInfo.InvariantCulture);
			return new ServiceReply(ServiceReply.Action.Success, reply);
		}

		protected override CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder.ReplyMessageBuilder CreateReplyMessageBuilder(ServiceReply reply)
		{
			return new eLMSReplyMessageBuilder(Context, reply);
		}

		protected override System.Collections.Specialized.NameValueCollection GetAppSettings()
		{
			return ConfigurationManager.AppSettings;
		}
	}
}