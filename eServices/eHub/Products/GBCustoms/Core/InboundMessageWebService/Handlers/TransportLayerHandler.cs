using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.Core.Correlation;
using CargoWise.eHub.Products.GBCustoms.Core.Correlation.Handlers;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers
{
	public class TransportLayerHandler : MessageHandlerBase
	{
		const string WrapperGBCustomsBusinessResponse = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms""><s0:ResponseHeader><s0:ConversationID/></s0:ResponseHeader><s0:ResponseBody/></s0:GBCustomsBusinessResponse>";
		const string WrapperGBCustoms = @"<ns0:GBCustoms xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms""><Header><GBCustomsRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""></GBCustomsRequest></Header><Body><PushData><MessageUri></MessageUri><RequestId></RequestId></PushData></Body></ns0:GBCustoms>";
		const string ResponseNamespace = "http://cargowise.com/ehub/products/GBCustoms";

		internal Func<eHubTransactionsContext> ContextFactory = () => new eHubTransactionsContext();
		internal List<GBCustomsCorrelation> Correlations;
		private bool IsGetPushDataRedirect = false;
		internal string InboundMessage;
		internal string ContentType = "XML";

		private static readonly IReadOnlyDictionary<ProviderType, string> PushDataProviderTypes = new Dictionary<ProviderType, string>()
		{
			{ ProviderType.CTCGB, "GetPushData" },
			{ ProviderType.EMCS, "Unsolicited" }
		};

		public TransportLayerHandler(ProviderType provider) : base(provider)
		{
		}

		public override string GetName()
		{
			return "Transport Layer Handler";
		}

		protected override void HandleMessage(SqlTransaction transaction, XDocument requestMessage, XDocument body, string[] recipients, string stringBody = "")
		{
			if (recipients.Count() == 1 && recipients[0] == "GBCustoms")
			{
				var inboundError = new InboundError(ContextFactory(), logger, "GBCustoms");
				inboundError.CreateFailedMessage(request, "GBCustoms Inbound Message", new Exception("Message does not appear to have originated from Wisetech Global or client is not permitted to receive messages, consuming the message."));
				return;
			}

			SubscribeGmrId(transaction, recipients, stringBody);

			using (var response = new MemoryStream())
			{
				WrapMessage(response, recipients);
				var message = new eHubGatewayMessage();
				message.ApplicationCode = "CDS";
				message.MessageStream = response.CompressAndEncode();
				message.SchemaName = GetSchemaName();
				message.SchemaType = MessageSchemaType.Xml;
				message.FileName = string.Empty;
				message.EmailSubject = string.Empty;
				message.MessageTrackingID = InternalGuid();
				var envelopeTrackingID = InternalGuid();

				if (IsGetPushDataRedirect)
				{
					message.ClientID = ResolveSender();
					inboxAccessor.InsertToInbox(recipients[0], envelopeTrackingID, message);
					logger.Info($"[Service ID: {ID}] Message inserted to Inbox. MessageTrackingID: {message.MessageTrackingID}");
				}
				else
				{
					foreach (var recipient in recipients)
					{
						message.ClientID = recipient;
						inboxAccessor.InsertToInboxAndOutbox(transaction, SenderID, ResolveSender(), envelopeTrackingID, message);
						logger.Info($"[Service ID: {ID}] Message inserted to Inbox and Outbox. MessageTrackingID: {message.MessageTrackingID}");
					}
				}
			}
		}

		protected override string GetSchemaName()
		{
			var schema = IsGetPushDataRedirect
				? "GBCustoms"
				: "GBCustomsBusinessResponse";
			return $"{ResponseNamespace}#{schema}";
		}

		protected override string GetSubscriptionType()
		{
			return GetSettings("GBCustomsSubscriptionType");
		}

		protected override string GetFallbackSubscriptionID(string jsonBody)
		{
			var fallbackValue = new JSONBodyExtractionHandler().Extract(logger, jsonBody, "message.gmrId");

			if(string.IsNullOrEmpty(fallbackValue))
			{
				var body = new JSONBodyExtractionHandler().Extract(logger, jsonBody, "message.messageBody");
				try
				{
					fallbackValue = XDocument.Parse(body)?.XPathSelectElement("//*[local-name()='ComAccRefMES21']")?.Value;
				}
				catch { }
			}

			return fallbackValue;
		}

		protected override string GetMessageID(XDocument requestMessage)
		{
			throw new System.NotImplementedException("GetMessageID is an invalid method for the Transport Layer Handler");
		}

		protected override string GetSubscribedCorrelationID(string body)
		{
			GetCorrelations(body);
			return VerifyAndIdentifySubscribedCorrelationID();
		}

		protected override string GetReferenceType(ProviderType providerType, string jsonBody, int retryCount = 0)
		{
			switch (providerType)
			{
				case ProviderType.CTCGB:
					var requestId = new JSONBodyExtractionHandler().Extract(logger, jsonBody, "message.requestId");
					return Regex.IsMatch(requestId, "arriv", RegexOptions.IgnoreCase)
						? "Arrive"
						: Regex.IsMatch(requestId, "depart", RegexOptions.IgnoreCase)
							? "Depart"
							: null;
				case ProviderType.EMCS:
					return (retryCount <= 1)
						? "Consignor"
						: "Consignee";
			}
			return null;
		}

		void SubscribeGmrId(SqlTransaction transaction, string[] recipients, string jsonBody)
		{
			var gmrId = new JSONBodyExtractionHandler().Extract(logger, jsonBody, "message.gmrId");
			if (!string.IsNullOrEmpty(gmrId))
			{
				subscriptionAccessor.InsertSubscribedClients(transaction, SubscriptionType, SenderID, recipients, gmrId, MessageID, "GMR-ID");
			}
		}

		private void WrapMessage(Stream outputStream, string[] recipients)
		{
			if (PushDataProviderTypes.ContainsKey(Provider))
			{
				var jsonExtractor = new JSONBodyExtractionHandler();
				var properties = new List<string>()
				{
					"message.messageBody",
					"message.response"
				};

				var bodyValue = jsonExtractor.Extract(logger, InboundMessage, properties);
				if (string.IsNullOrEmpty(bodyValue))
				{
					IsGetPushDataRedirect = true;
				}
				else
				{
					InboundMessage = bodyValue;
				}
			}

			XDocument wrapperDoc;
			wrapperDoc = IsGetPushDataRedirect
				? WrapGBCustoms(recipients)
				: WrapGBCustomsBusinessResponse();

			wrapperDoc.Save(outputStream);
			outputStream.SeekBegin();
		}

		private XDocument WrapGBCustoms(string[] recipients)
		{
			var originalMessage = "";
			var wrapperDoc = XDocument.Parse(WrapperGBCustoms);
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("s0", ResponseNamespace);

			using (var context = ContextFactory())
			{
				var recipient = recipients[0];
				Guid originalRecipientPk, originalSenderPk, subscriptionTypePK;
				subscriptionTypePK = context.eHubSubscriptionTypes.Where(_ => _.ST_ID == SubscriptionType).Select(_ => _.ST_PK).FirstOrDefault();
				originalRecipientPk = context.eHubClients.Where(c => c.CC_ID == recipient).Select(c => c.CC_PK).FirstOrDefault();
				originalSenderPk = context.eHubClients.Where(c => c.CC_ID == SenderID).Select(c => c.CC_PK).FirstOrDefault();

				originalMessage = context.eHubSubscriptionValues.Where(sub =>
					sub.SV_CC_Recipient == originalRecipientPk &&
					sub.SV_CC_Sender == originalSenderPk &&
					sub.SV_ST == subscriptionTypePK &&
					sub.SV_Value == MessageID)
					.Select(r => r.SV_Reference)
					.FirstOrDefault();
			}
			if (string.IsNullOrEmpty(originalMessage))
			{
				logger.Error($"GBCustoms Transport Layer - Provider: {Provider.ConvertToString()}. Subscribed message not found for Sender [{SenderID}], Recipient [{recipients[0]}], SubscriptionType [{SubscriptionType}], Value [{MessageID}]");
				throw new HttpException((int)HttpStatusCode.NotImplemented, $"Provider: {Provider.ConvertToString()}. Subscribed message not found for correlation [{MessageID}].");
			}

			var gbCustomsRequestElement = XDocument.Parse(originalMessage).Root.Descendants().Where(_ => _.Name == "GBCustomsRequest").FirstOrDefault();

			var contentTypeElement = gbCustomsRequestElement.Elements("ContentType");
			if (contentTypeElement != null && contentTypeElement.FirstOrDefault() != null)
			{
				if (Provider == ProviderType.EMCS)
				{
					contentTypeElement.FirstOrDefault().Value = "xml";
				}
				else
				{
					contentTypeElement.FirstOrDefault().Value = ContentType;
				}
			}

			var acceptElement = gbCustomsRequestElement.Elements("Accept");
			var acceptValue = acceptElement?.FirstOrDefault()?.Value;
			if (acceptValue != null && !acceptValue.Contains("1.0"))
			{
				acceptElement.FirstOrDefault().Value = $"{acceptValue}-xml";
			}

			var largeFileTypeElement = gbCustomsRequestElement.Elements("LargeFile");
			if (largeFileTypeElement != null && largeFileTypeElement.FirstOrDefault() != null)
			{
				largeFileTypeElement.FirstOrDefault().Remove();
			}

			var jsonExtractor = new JSONBodyExtractionHandler();
			var messageUri = jsonExtractor.Extract(logger, InboundMessage, "message.messageUri");
			var requestId = jsonExtractor.Extract(logger, InboundMessage, "message.requestId");
			var serviceRef = jsonExtractor.Extract(logger, InboundMessage, "message.departureId");
			if (string.IsNullOrEmpty(serviceRef))
			{
				serviceRef = jsonExtractor.Extract(logger, InboundMessage, "message.arrivalId");
			}
			if (string.IsNullOrEmpty(serviceRef))
			{
				serviceRef = jsonExtractor.Extract(logger, InboundMessage, "message.movementId");
			}

			wrapperDoc.XPathSelectElement("/s0:GBCustoms/Header", namespaceManager)?.ReplaceNodes(gbCustomsRequestElement);
			wrapperDoc.XPathSelectElement("/s0:GBCustoms/Header/GBCustomsRequest", namespaceManager)?.SetElementValue("ServiceReference", serviceRef);
			wrapperDoc.XPathSelectElement("/s0:GBCustoms/Header/GBCustomsRequest", namespaceManager)?.SetElementValue("Service", PushDataProviderTypes[Provider]);
			wrapperDoc.XPathSelectElement("/s0:GBCustoms/Body/PushData", namespaceManager)?.SetElementValue("MessageUri", messageUri);
			wrapperDoc.XPathSelectElement("/s0:GBCustoms/Body/PushData", namespaceManager)?.SetElementValue("RequestId", requestId);

			return wrapperDoc;
		}

		private XDocument WrapGBCustomsBusinessResponse()
		{
			var wrapperDoc = XDocument.Parse(WrapperGBCustomsBusinessResponse);
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("s0", ResponseNamespace);

			wrapperDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader", namespaceManager)?.SetAttributeValue("Provider", Provider.ConvertToString());

			foreach (var correlationList in Correlations)
			{
				foreach (var correlation in correlationList.GBCustomsCorrelationIdentifiers.Where(x => !string.IsNullOrEmpty(x.Value)))
				{
					wrapperDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader", namespaceManager)?.SetElementValue(TransformCorrelationName(correlation.Name), correlation.Value);
				}
			}

			var jsonExtractor = new JSONBodyExtractionHandler();
			var innerMessage = (Provider == ProviderType.CTCGB) ? InboundMessage : jsonExtractor.Extract(logger, InboundMessage, "message");

			var isMessageJson = IsJson(innerMessage);
			var contentType = isMessageJson ? "JSON" : "XML";
			var encodingType = isMessageJson ? "base64" : "none";
			var responseBody = isMessageJson ? EncodeBody(innerMessage) : innerMessage;

			wrapperDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseBody", namespaceManager)?.SetAttributeValue("ContentType", contentType);
			wrapperDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseBody", namespaceManager)?.SetAttributeValue("Encoding", encodingType);
			wrapperDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseBody", namespaceManager)?.Add(responseBody);

			return wrapperDoc;
		}

		private string TransformCorrelationName(string correlationName)
		{
			switch (correlationName)
			{
				case "ResponseArrival":
				case "ResponseDeparture":
				case "SubscribedArrival":
				case "SubscribedDeparture":
					return "ServiceReference";
				case "RequestId":
					return "RequestID";
			}

			return correlationName;
		}

		internal bool IsJson(string content, bool fromHeader = false)
		{
			if (fromHeader) return ExtractHeader("content-type", true).Contains("json");
			else
			{
				JObject jsonObject;
				try
				{
					jsonObject = JObject.Parse(content);
					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		private string EncodeBody(string bodyToEncode)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(bodyToEncode);
			return System.Convert.ToBase64String(plainTextBytes);
		}

		internal virtual void GetCorrelations(string body)
		{
			var isBodyJson = IsJson(body, true);
			InboundMessage = "";

			var jsonExtractor = new JSONBodyExtractionHandler();

			if (isBodyJson)
			{
				InboundMessage = body;
				if (jsonExtractor.Extract(logger, InboundMessage, "messageContentType").ToLower().Contains("json"))
				{
					ContentType = "json";
				}
			}

			var messsage = jsonExtractor.Extract(logger, InboundMessage, "message");
			if (string.IsNullOrEmpty(messsage))
			{
				logger.Error($"GBCustoms Transport Layer - Provider: {Provider.ConvertToString()}. No message found in the received notification.");
				throw new HttpException((int)HttpStatusCode.NotImplemented, $"Provider: {Provider.ConvertToString()}. No message found in the received notification.");
			}

			string headerString = GetHeaderString();
			Correlations = new List<GBCustomsCorrelation>();
			var isProd = !SenderID.Contains("Test");

			Correlations.Add(new GBCustomsCorrelation(logger, Provider, "Notification-Headers", GBCustomsSource.Notification, headerString, isProd, ClientRegistrationAccessorFactory()));
			Correlations.Add(new GBCustomsCorrelation(logger, Provider, "Notification-Body", GBCustomsSource.Notification, InboundMessage, isProd, ClientRegistrationAccessorFactory()));

			switch (Provider)
			{
				case ProviderType.GVMS:
					var contentType = Correlations.Last().GBCustomsCorrelationIdentifiers.FirstOrDefault(x => x.Name == "ContentType" && !string.IsNullOrEmpty(x.Value)).Value;
					if (string.IsNullOrEmpty(contentType))
					{
						logger.Error($"GBCustoms Transport Layer - Provider: {Provider.ConvertToString()}. No messageContentType found in the received notification.");
						throw new HttpException((int)HttpStatusCode.NotImplemented, $"Provider: {Provider.ConvertToString()}. No messageContentType found in the received notification.");
					}
					else
					{
						if (contentType.Contains("json")) Correlations.Add(new GBCustomsCorrelation(logger, Provider, "Notification-Json", GBCustomsSource.Notification, InboundMessage, isProd, ClientRegistrationAccessorFactory()));
						Correlations.Add(new GBCustomsCorrelation(logger, Provider, "Notification-Xml", GBCustomsSource.Notification, messsage, isProd, ClientRegistrationAccessorFactory()));
					}
					break;
				case ProviderType.CTCGB:
					Correlations.Add(new GBCustomsCorrelation(logger, Provider, "Notification-RequestId", GBCustomsSource.Notification, InboundMessage, isProd, ClientRegistrationAccessorFactory()));
					Correlations.Add(new GBCustomsCorrelation(logger, Provider, "Notification-ArrivalId", GBCustomsSource.Notification, InboundMessage, isProd, ClientRegistrationAccessorFactory()));
					Correlations.Add(new GBCustomsCorrelation(logger, Provider, "Notification-ArriveSubscribed", GBCustomsSource.Notification, InboundMessage, isProd, ClientRegistrationAccessorFactory()));
					Correlations.Add(new GBCustomsCorrelation(logger, Provider, "Notification-DepartureId", GBCustomsSource.Notification, InboundMessage, isProd, ClientRegistrationAccessorFactory()));
					Correlations.Add(new GBCustomsCorrelation(logger, Provider, "Notification-DepartSubscribed", GBCustomsSource.Notification, InboundMessage, isProd, ClientRegistrationAccessorFactory()));
					break;
				case ProviderType.EMCS:
					Correlations.Add(new GBCustomsCorrelation(logger, Provider, "Notification-Movement", GBCustomsSource.Notification, InboundMessage, isProd, ClientRegistrationAccessorFactory()));
					break;

			}
		}

		private string VerifyAndIdentifySubscribedCorrelationID()
		{
			var correlations = new List<string>();
			var uniqueCorrelations = new List<string>();

			foreach (var correlation in Correlations)
			{
				correlations.AddRange(correlation.GBCustomsCorrelationIdentifiers.Where(x => x.Type == CorrelationType.Subscribed && !string.IsNullOrEmpty(x.Value)).Select(y => y.Value));
			}

			if (correlations.Count() == 0)
			{
				logger.Error($"GBCustoms Transport Layer - Provider: {Provider.ConvertToString()}. No {MessageIDKey} found from the received notification.");
				throw new HttpException((int)HttpStatusCode.NotImplemented, $"Provider: {Provider.ConvertToString()}. No {MessageIDKey} found from the received notification.");
			}

			uniqueCorrelations.AddRange(correlations.Distinct());

			if (uniqueCorrelations.Count > 1)
			{
				var nonExclusiveIDs = string.Join(", ", uniqueCorrelations);
				logger.Error($"GBCustoms Transport Layer - Provider: {Provider.ConvertToString()}. More than one subscribed correlation identifier found: {nonExclusiveIDs}.");
				throw new HttpException((int)HttpStatusCode.NotImplemented, $"Provider: {Provider.ConvertToString()}. More than one subscribed correlation identifier found: {nonExclusiveIDs}.");
			}

			return uniqueCorrelations[0];
		}
	}
}
