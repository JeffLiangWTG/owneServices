using System.Collections.Specialized;
using System.Configuration;
using System.ServiceModel;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.MessageContext;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider;
using Common.Logging;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubSender
{
	class TestSendToService : ServiceProvider
	{
		public TestSendToService(ILog logger, string senderId, string recipientId, string message)
			: base(logger, senderId, recipientId, message)
		{
		}

		protected override NameValueCollection GetAppSettings()
		{
			return ConfigurationManager.AppSettings;
		}

		public EndpointAddress GetEndpointAddressTest()
		{
			return GetEndpointAddress();
		}

		public void LoadContextTest()
		{
			LoadContext();
		}

		protected override MessageContext CeateMessageContext(string senderId, string recipientId, string message)
		{
			return new TestMessageContext(message, senderId, recipientId);
		}

		protected override ServiceReply CallService()
		{
			return new ServiceReply(ServiceReply.Action.Success);
		}

		protected override ReplyMessageBuilder CreateReplyMessageBuilder(ServiceReply reply)
		{
			return new TestReplyMessageBuilder(Context);
		}

	}

	class TestMessageContext : MessageContext
	{
		public TestMessageContext(string message, string eHubSenderId, string eHubRecipientId)
			: base(message, eHubSenderId, eHubRecipientId)
		{
		}

		public override void Load()
		{
		}
	}

	class TestReplyMessageBuilder : ReplyMessageBuilder
	{
		public TestReplyMessageBuilder(MessageContext context)
			: base(context)
		{
		}

		public override string GetReply()
		{
			return "";
		}
	}
}