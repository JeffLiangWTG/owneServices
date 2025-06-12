using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder;
using CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider;
using CargoWise.eHub.Share.eHubServices.eHubSender.Extensions;
using CargoWise.eHub.Share.eHubServices.eHubSender.MessageContext;
using CargoWise.eHub.Share.eHubServices.eHubSender.ReplyMessageBuilder;
using Common.Logging;
using Newtonsoft.Json;

namespace CargoWise.eHub.Share.eHubServices.eHubSender.ServiceProvider
{
	[SupportedRecipient("OceanInsights_CSS")]
	public class OceanInsightsServiceProvider : CargoWise.eHub.Share.eHubServices.eHubSender.Common.ServiceProvider.ServiceProvider
	{
		readonly ILog logger;

		public OceanInsightsServiceProvider(ILog logger, string senderId, string recipientId, string message)
			: base(logger, senderId, recipientId, message)
		{
			if (logger == null) throw new ArgumentNullException("logger");
			this.logger = logger;
		}

		protected override CargoWise.eHub.Share.eHubServices.eHubSender.Common.MessageContext.MessageContext CeateMessageContext(string senderId, string recipientId, string message)
		{
			return new OceanInsightsMessageContext(message, senderId, recipientId);
		}

		protected override ServiceReply CallService()
		{
			string subscriptionUrl = ConfigurationManager.AppSettings["OceanInsights"];
			string authorizationToken = ConfigurationManager.AppSettings["OceanInsightsAuthorizationToken"];
			if (string.IsNullOrWhiteSpace(subscriptionUrl)) throw new ConfigurationErrorsException("Please add 'OceanInsights' into appSettings of config file.");
			string reference = Context.GetValue("Reference");

			var subscription = new Subscription()
			{
				RequestCarrierCode = Context.GetValue("CarrierCode"),
				SubscriptionReference = reference,
			};

			string subscriptionReference = Context.GetValue("SubscriptionType");

			if(subscriptionReference == "CARRIERBOOKINGREFERENCE")
			{
				subscription.RequestType = "b_id";
				subscription.RequestKey = Context.GetValue("CarriersBookingReference");
			}

			if (subscriptionReference == "MASTERBILLNUMBER")
			{
				subscription.RequestType = "m_bl";
				subscription.RequestKey = Context.GetValue("MasterBillNumber");
			}

			if (subscriptionReference == "CONTAINERNUMBER")
			{
				subscription.RequestType = "c_id";
				subscription.RequestKey = Context.GetValue("ContainerNumber");
			}

			string jsonText = JsonConvert.SerializeObject(subscription);

			var dataToSend = Encoding.UTF8.GetBytes(jsonText);

			return CallRestService(subscriptionUrl, dataToSend, authorizationToken, subscription);
		}

		protected virtual ServiceReply CallRestService(string url, byte[] dataToSend, string authorizationToken, Subscription subscription)
		{
			var request = (HttpWebRequest)WebRequest.Create(url);
			request.ContentType = "application/json";
			request.Method = "POST";
			request.ContentLength = dataToSend.Length;
			request.Headers.Add("Authorization", authorizationToken);
			request.GetRequestStream().Write(dataToSend, 0, dataToSend.Length);
			request.Timeout = 2 * 60 * 1000;

			try
			{
				using (var response = (HttpWebResponse) request.GetResponse())
				{
					if (response.StatusCode == HttpStatusCode.Created)
					{
						using (var responseStream = response.GetResponseStream())
						{
							string responseText = "";
							if (responseStream != null) responseText = new StreamReader(responseStream).ReadToEnd();
							return new ServiceReply(ServiceReply.Action.Success, responseText);
						}
					}

					return ReportError((int)response.StatusCode, response.StatusDescription, subscription);
				}
			}
			catch (WebException ex)
			{
				var webResponse = ex.Response as HttpWebResponse;
				int statusCode = webResponse != null ? (int)webResponse.StatusCode : (int)ex.Status;

				string errorDescription = ex.Message;
				if (ex.Response != null)
				{
					using (var errorStream = ex.Response.GetResponseStream())
					{
						if (errorStream != null) errorDescription = new StreamReader(errorStream).ReadToEnd();
					}
				}

				return ReportError(statusCode, errorDescription, subscription);
			}
		}

		ServiceReply ReportError(int statusCode, string statusDescription, Subscription subscription)
		{
			string lowerCaseStatusDescription = statusDescription.ToLower();

			if (statusCode == 400 && lowerCaseStatusDescription.Contains("is not a valid choice"))
			{
				return new ServiceReply(ServiceReply.Action.Error, string.Format("{0} carrier code is not supported by service provider.", subscription.RequestCarrierCode));
			}

			if (statusCode == 400 && lowerCaseStatusDescription.Contains("there is already an identical subscription in place"))
			{
				return new ServiceReply(ServiceReply.Action.Error, "Duplicate subscription. Subscription rejected.");
			}

			if (statusCode >= 400 && statusCode < 500)
			{
				return new ServiceReply(ServiceReply.Action.Error, string.Format("Subscription rejected by service provider. Status Code '{0}', Description '{1}'.", statusCode, statusDescription));
			}

			throw new InvalidOperationException(string.Format("Error during subscribing. Status Code '{0}', Description '{1}'. Please resubmit the message.", statusCode, statusDescription));
		}

		protected override CargoWise.eHub.Share.eHubServices.eHubSender.Common.ReplyMessageBuilder.ReplyMessageBuilder CreateReplyMessageBuilder(ServiceReply reply)
		{
			return new OceanInsightsReplyMessageBuilder(Context, reply);
		}

		protected override string ProcessException(Exception exception)
		{
			if (exception is OceanInsighsSubscriptionValidationException)
			{
				return new OceanInsightsReplyMessageBuilder(Context, new ServiceReply(ServiceReply.Action.Error, exception.Message)).GetReply();
			}

			return string.Empty;
		}

		protected override System.Collections.Specialized.NameValueCollection GetAppSettings()
		{
			return ConfigurationManager.AppSettings;
		}

		[Serializable]
		public class Subscription
		{
			[JsonProperty(PropertyName = "request_key")]
			public string RequestKey { get; set; }

			[JsonProperty(PropertyName = "request_carrier_code")]
			public string RequestCarrierCode { get; set; }

			[JsonProperty(PropertyName = "request_type")]
			public string RequestType { get; set; }

			[JsonProperty(PropertyName = "descriptive_name")]
			public string SubscriptionReference { get; set; }
		}
	}
}