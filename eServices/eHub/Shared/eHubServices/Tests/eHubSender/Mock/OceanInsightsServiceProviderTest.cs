using System;
using System.Text;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder;
using CargoWise.eHub.Share.eHubServices.eHubSender.ServiceProvider;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubSender.Mock
{
	public class OceanInsightsServiceProviderTest : OceanInsightsServiceProvider
	{
		readonly Configuration configuration;

		public OceanInsightsServiceProviderTest(ILog logger, string senderId, string recipientId, string message, Configuration configuration)
			: base(logger, senderId, recipientId, message)
		{
			this.configuration = configuration;
		}

		protected override ServiceReply CallRestService(string apiBaseUrl, byte[] dataToSend, string authorizationToken, Subscription subscription)
		{
			Assert.AreEqual(configuration.ApiBaseUrl, apiBaseUrl);
			Assert.AreEqual(configuration.DataToSendString, Encoding.UTF8.GetString(dataToSend));
			Assert.AreEqual(configuration.AuthorizationToken, authorizationToken);
			Assert.AreEqual(configuration.Subscription.RequestType, subscription.RequestType);
			Assert.AreEqual(configuration.Subscription.RequestCarrierCode, subscription.RequestCarrierCode);
			Assert.AreEqual(configuration.Subscription.RequestKey, subscription.RequestKey);

			return configuration.ServiceReplyResult();
		}


		protected override void SendReplyToBiztalk(string replyText)
		{
			Assert.AreEqual(configuration.ReplyText, replyText);
		}

		public class Configuration
		{
			public string ApiBaseUrl { get; set; }
			public string DataToSendString { get; set; }
			public string AuthorizationToken { get; set; }
			public Subscription Subscription { get; set; }
			public Func<ServiceReply> ServiceReplyResult { get; set; }
			public string ReplyText { get; set; }
		}

	}
}

