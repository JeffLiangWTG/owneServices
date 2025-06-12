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

namespace CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers
{
	public class BusinessNotificationHandler : MessageHandlerBase
	{
		const string WrapperMessage = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms""><s0:ResponseHeader><s0:ConversationID/><s0:eHubTrackingId/></s0:ResponseHeader><s0:ResponseBody/></s0:GBCustomsBusinessResponse>";
		const string ResponseNamespace = "http://cargowise.com/ehub/products/GBCustoms";
		string FallbackID;
		internal Func<eHubTransactionsContext> ContextFactory = () => new eHubTransactionsContext();

		public BusinessNotificationHandler(ProviderType provider) : base(provider)
		{
		}

		public override string GetName()
		{
			return "Business Notification Handler";
		}

		protected override void HandleMessage(SqlTransaction transaction, XDocument requestMessage, XDocument body, string[] recipients, string stringBody)
		{
			if (recipients.Count() == 1 && recipients[0] == "GBCustoms")
			{
				var inboundError = new InboundError(ContextFactory(), logger, "GBCustoms");
				inboundError.CreateFailedMessage(request, "GBCustoms Inbound Message", new Exception("Message does not appear to have originated from Wisetech Global or client is not permitted to receive messages, consuming the message."));
				return;
			}
				
			var conversationId = GetConversationId(requestMessage);
			var messageDoc = Provider == ProviderType.Pentant || Provider == ProviderType.CDS ? requestMessage : body;
			using (var response = new MemoryStream())
			{
				WrapMessage(messageDoc, conversationId, response);
				var message = new eHubGatewayMessage();
				message.ApplicationCode = "CDS";
				message.MessageStream = response.CompressAndEncode();
				message.SchemaName = GetSchemaName();
				message.SchemaType = MessageSchemaType.Xml;
				message.FileName = string.Empty;
				message.EmailSubject = string.Empty;
				message.MessageTrackingID = InternalGuid();
				var envelopeTrackingID = InternalGuid();
				foreach (var recipient in recipients)
				{
					message.ClientID = recipient;
					inboxAccessor.InsertToInboxAndOutbox(transaction, SenderID, ResolveSender(), envelopeTrackingID, message);
					logger.Info($"[Service ID: {ID}] Message inserted to Inbox and Outbox. MessageTrackingID: {message.MessageTrackingID}");
				}
			}
		}

		protected override string GetSchemaName()
		{
			return $"{ResponseNamespace}#GBCustomsBusinessResponse";
		}

		protected override string GetSubscriptionType()
		{
			switch (Provider)
			{
				case ProviderType.CDS:
					return GetSettings("GBCustomsSubscriptionType");
				case ProviderType.CNS:
				case ProviderType.MCP:
				case ProviderType.Pentant:
					return GetSettings("GBCustomsSubscriptionTypeByConversationID");
				default:
					return null;
			}
		}

		protected override string GetMessageID(XDocument requestMessage)
		{
			string id = null;
			switch (Provider)
			{
				case ProviderType.CDS:
					id = ExtractHeader(GetSettings("ConversationIDKey"));
					break;
				case ProviderType.CNS:
				case ProviderType.MCP:
					id = requestMessage.XPathSelectElement("/notifications/notification/headers/header[@name='ConversationID']")?.Attribute("value")?.Value;
					break;
				case ProviderType.Pentant:
					id = GetPentantId(requestMessage);
					break;
			}

			if (string.IsNullOrWhiteSpace(id))
				throw new HttpException((int)HttpStatusCode.NotImplemented, $"Provider: {Provider.ConvertToString()}. No {MessageIDKey} found from the received notification.");

			return id;
		}

		protected override string MessageIDKey
		{
			get
			{
				switch (Provider)
				{
					case ProviderType.CDS:
					case ProviderType.Pentant:
					case ProviderType.MCP:
					case ProviderType.CNS:
						return "Conversation ID";
					default:
						return "Message ID";
				}
			}
		}

		private string GetOriginalEhubTrackingID(string identifier)
		{
			var eHubID = string.Empty;
			var CSPRefs = new List<string>();

			var subscriptions = subscriptionAccessor.SelectSubscriptions(SubscriptionType, SubscriptionProviderIDList, identifier, null, null);
			if (subscriptions != null)
			{
				foreach (var subscription in subscriptions)
				{
					logger.Info($"[Service ID: {ID}] Trying to extract eHubTrackingID from subscription reference - SubscriptionType: [{subscription.Type}] SubscriptionValue: [{subscription.Value}]");
					var subRef = subscription.Reference;
					if (TryExtractEhubTrackingID(subRef, ref eHubID))
					{
						return eHubID;
					}
					if (!string.IsNullOrEmpty(subRef))
                    {
						CSPRefs.Add(subRef);
					}
				}
			}

			switch (Provider)
			{
				case ProviderType.Pentant:
				case ProviderType.CNS:
				case ProviderType.MCP:
					var subscriptionSubType = GetSettings("GBCustomsSubscriptionTypeByCSPID");
					if (subscriptionSubType != null)
					{
						subscriptions = subscriptionAccessor.SelectSubscriptions(subscriptionSubType, SubscriptionProviderIDList, identifier, null, null);
						if (subscriptions != null)
						{
							foreach (var subTypeSubscription in subscriptions)
							{
								logger.Info($"[Service ID: {ID}] Trying to extract eHubTrackingID from subscription reference - SubscriptionType: [{subTypeSubscription.Type}] SubscriptionValue: [{subTypeSubscription.Value}]");
								var subRef = subTypeSubscription.Reference;
								if (TryExtractEhubTrackingID(subRef, ref eHubID))
								{
									return eHubID;
								}
							}
						}

						foreach(var id in CSPRefs)
                        {
							subscriptions = subscriptionAccessor.SelectSubscriptions(subscriptionSubType, SubscriptionProviderIDList, id, null, null);
							if (subscriptions != null)
							{
								foreach (var subTypeSubscription in subscriptions)
								{
									logger.Info($"[Service ID: {ID}] Trying to extract eHubTrackingID from subscription reference - SubscriptionType: [{subTypeSubscription.Type}] SubscriptionValue: [{subTypeSubscription.Value}]");
									var subRef = subTypeSubscription.Reference;
									if (TryExtractEhubTrackingID(subRef, ref eHubID))
									{
										return eHubID;
									}
								}
							}
						}
					}

					if (FallbackID != null)
					{
						subscriptions = subscriptionAccessor.SelectSubscriptions(SubscriptionType, SubscriptionProviderIDList, FallbackID, null, null);
						if (subscriptions != null)
						{
							foreach (var subTypeSubscription in subscriptions)
							{
								logger.Info($"[Service ID: {ID}] Trying to extract eHubTrackingID from subscription reference - SubscriptionType: [{subTypeSubscription.Type}] SubscriptionValue: [{subTypeSubscription.Value}]");
								var subRef = subTypeSubscription.Reference;
								if (TryExtractEhubTrackingID(subRef, ref eHubID))
								{
									return eHubID;
								}
							}
						}
					}

					break;
			}
			
			return string.Empty;
		}

		private bool TryExtractEhubTrackingID(string subscriptionReference, ref string eHubID)
		{
			try
			{
				var xdoc = XDocument.Parse(subscriptionReference);
				eHubID = xdoc.XPathSelectElement("//eHubMessageTrackingId")?.Value ?? string.Empty;
				if (!string.IsNullOrEmpty(eHubID))
				{
					return true;
				}
			}
			catch (XmlException) { }

			return false;
		}

		private string GetConversationId(XDocument requestMessage)
		{
			switch (Provider)
			{
				case ProviderType.CNS:
				case ProviderType.MCP:
					return requestMessage.XPathSelectElement("/notifications/notification/headers/header[@name='ConversationID']")?.Attribute("value")?.Value;
				case ProviderType.CDS:
					return ExtractHeader(GetSettings("ConversationIDKey"));
				case ProviderType.Pentant:
					return GetPentantId(requestMessage);
				default:
					return null;
			}
		}

		private string GetPentantId(XDocument requestMessage)
		{
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("S", "http://www.w3.org/2003/05/soap-envelope");
			namespaceManager.AddNamespace("ebi", "http://www.myvan.descartes.com/ebi/2004/r1");
			var pentantId = requestMessage.XPathSelectElement("/S:Envelope/S:Header/ebi:CorrelationId", namespaceManager)?.Value;
			return pentantId;
		}

		void WrapMessage(XDocument messageDoc, string conversationID, Stream outputStream)
		{
			if (conversationID == null) throw new HttpException(400, "ConversationID is expected.");
			var wrapperDoc = XDocument.Parse(WrapperMessage);
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("s0", ResponseNamespace);
			wrapperDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:ConversationID", namespaceManager)?.SetValue(conversationID);
			wrapperDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:eHubTrackingId", namespaceManager)?.SetValue(GetOriginalEhubTrackingID(conversationID));
			wrapperDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseBody", namespaceManager)?.Add(messageDoc.Root);
			wrapperDoc.Save(outputStream);
			outputStream.SeekBegin();
		}

		protected override string GetFallbackSubscriptionID(XDocument requestMessage)
		{
			const string fallbackPath = "//*[local-name()='Declaration']/*[local-name()='FunctionalReferenceID']";
			var functionalReferenceIDElement = requestMessage.XPathSelectElement(fallbackPath);
			if(functionalReferenceIDElement == null)
            {
				var body = requestMessage.XPathSelectElement("//*[local-name()='body']");
				if(body != null && TryDecodeBase64(body.Value, out var decodedBody))
                {
					try
					{
						functionalReferenceIDElement = XDocument.Parse(decodedBody).XPathSelectElement(fallbackPath);
					}
					catch { }
				}
            }

			if (functionalReferenceIDElement != null)
			{
				FallbackID = functionalReferenceIDElement.Value;
				return FallbackID;
			}

			return null;
		}

		protected override string GetSubscribedCorrelationID(string body)
		{
			throw new System.NotImplementedException();
		}
	}
}
