using System;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.MessageContext;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder;
using CargoWise.eHub.Share.eHubServices.eHubSender.ServiceProvider;
using Common.Logging;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubSender.Mock
{
	public class USDISServiceProviderTest : USDISServiceProvider
	{
		readonly ServiceReply reply;
		public string Message { get; set; }
		public string MessageType { get; private set; }
		public string ReplyToBiztalk { get; private set; }

		public MessageContext Context_Exposed { get { return this.Context; } }
		public string endpointAddress_Exposed { get; private set; }

		public USDISServiceProviderTest(ILog logger, string senderId, string recipientId, string message, string reply)
			: this(logger, senderId, recipientId, message, new ServiceReply(ServiceReply.Action.Success, reply))
		{
		}

		protected USDISServiceProviderTest(ILog logger, string senderId, string recipientId, string message, ServiceReply reply)
			: base(logger, senderId, recipientId, message)
		{
			if (reply == null) throw new ArgumentNullException("reply");
			this.reply = reply;
		}

		protected override ServiceReply CallService()
		{
			Message = Context.Message;
			MessageType = Context.GetValue("MessageType");
			endpointAddress_Exposed = GetEndpointAddress().Uri.AbsoluteUri;
			return reply;
		}
	}
}
