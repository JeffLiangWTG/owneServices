using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers
{
	public class TransportAcknowledgementHandler : MessageHandlerBase
	{
		XNamespace UniversalEventNamesapce = "http://www.cargowise.com/Schemas/Universal/2012/11";
		const string ResponseNamespace = "http://cargowise.com/ehub/products/GBCustoms";
		const string RootElement = "/s0:GBCustomsTransportResponse";

		const string GBCustomsTransportResponseWrapper = @"
		<ns0:GBCustomsTransportResponse xmlns:ns0=""http://cargowise.com/ehub/products/GBCustoms"">
			<CSPID></CSPID>
			<Status></Status>
			<ConversationId></ConversationId>
			<eHubMessageTrackingId></eHubMessageTrackingId>
		</ns0:GBCustomsTransportResponse>";

		const string UniversalInterchangeWrapperMessage = @"
		<UniversalInterchange xmlns='http://www.cargowise.com/Schemas/Universal/2011/11'>
			<Header>
				<SenderID/>
				<RecipientID/>
			</Header>
			<Body>
				<UniversalEvent xmlns='http://www.cargowise.com/Schemas/Universal/2012/11'>
					<Event>
						<DataContext>
							<DataTargetCollection>
								<DataTarget></DataTarget>
							</DataTargetCollection>
						</DataContext>
						<EventType>MSN</EventType>
						<ContextCollection></ContextCollection>
					</Event>
				</UniversalEvent>
			</Body>
		</UniversalInterchange>";

		internal Func<eHubTransactionsContext> ContextFactory = () => new eHubTransactionsContext();

		public TransportAcknowledgementHandler(ProviderType provider) : base(provider)
		{
		}

		public override string GetName()
		{
			return "Transport Response Handler";
		}

		protected override void HandleMessage(SqlTransaction transaction, XDocument requestMessage, XDocument body, string[] recipients, string stringBody = "")
		{
			var subscribedMessage = GetSubscription(transaction, recipients[0], MessageID);
			if (string.IsNullOrWhiteSpace(subscribedMessage))
			{
				if (recipients.Count() == 1 && recipients[0] == "GBCustoms")
				{
					var inboundError = new InboundError(ContextFactory(), logger, "GBCustoms");
					inboundError.CreateFailedMessage(request, "GBCustoms Inbound Message", new Exception("Message does not appear to have originated from Wisetech Global or client is not permitted to receive messages, consuming the message."));
					return;
				}
				else
				{
					throw new HttpException(500, "Critical internal error. Do not retry. Use the enquiry ID and contact WiseTech Global.");
				}
			}

			var messageDoc = XDocument.Parse(subscribedMessage);
			var messageTrackingID = InternalGuid();

			if (Provider == ProviderType.Pentant)
			{
				var interchangeDoc = XDocument.Parse(UniversalInterchangeWrapperMessage);
				HandlePentantResponse(transaction, requestMessage, messageDoc, interchangeDoc, messageTrackingID, recipients);
				messageDoc = interchangeDoc;
			}
			else
			{
				if (Provider == ProviderType.CNS || Provider == ProviderType.MCP)
				{
					if (messageDoc.Root.Name.LocalName.ToLower() == "ehubmessagetrackingid") //We could not find the response from the original request, so we should construct a GBCustomsTransportResponse message
					{
						var namespaceManager = new XmlNamespaceManager(new NameTable());
						namespaceManager.AddNamespace("s0", ResponseNamespace);
						var eHubTrackingID = messageDoc.XPathSelectElement("/*[local-name()='eHubMessageTrackingId']").Value;
						var cspId = requestMessage.XPathSelectElement("/notifications/notification/headers/header[@name='X-CSP-ID']")?.Attribute("value")?.Value;
						messageDoc = XDocument.Parse(GBCustomsTransportResponseWrapper);
						SetValueOrAddIfMissing(messageDoc.XPathSelectElement(RootElement, namespaceManager), "Status", "eHubMessageTrackingId", eHubTrackingID);
						SetValueOrAddIfMissing(messageDoc.XPathSelectElement(RootElement, namespaceManager), "Status", "CSPID", cspId);
					}

					HandleCNSMCPResponse(transaction, requestMessage, body, messageDoc, recipients);
				}
			}

			using (var response = new MemoryStream())
			{
				messageDoc.Save(response);
				var message = new eHubGatewayMessage();
				message.ApplicationCode = "CDS";
				message.MessageStream = response.CompressAndEncode();
				message.SchemaName = GetSchemaName();
				message.SchemaType = MessageSchemaType.Xml;
				message.FileName = string.Empty;
				message.EmailSubject = string.Empty;
				message.MessageTrackingID = messageTrackingID;
				var envelopeTrackingID = InternalGuid();
				foreach (var recipient in recipients)
				{
					message.ClientID = recipient;
					inboxAccessor.InsertToInbox(SenderID, envelopeTrackingID, InternalGuid(), MessageStatus.Received, message, false, transaction, null);
					logger.Info($"[Service ID: {ID}] Message inserted to Inbox. MessageTrackingID: {message.MessageTrackingID}");
				}
			}
		}

		void HandleCNSMCPResponse(SqlTransaction transaction, XDocument requestMessage, XDocument body, XDocument messageDoc, string[] recipients)
		{
			var responseCode = body.XPathSelectElement("/response/code")?.Value;
			if (string.IsNullOrEmpty(responseCode))
				responseCode = body.XPathSelectElement("/errorResponse/code")?.Value;
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("s0", ResponseNamespace);

			if (responseCode == "202")
			{
				var conversationId = requestMessage.XPathSelectElement("/notifications/notification/headers/header[@name='ConversationID']")?.Attribute("value")?.Value;
				if (string.IsNullOrWhiteSpace(conversationId)) throw new HttpException(400, "ConversationID is expected.");
				SetValueOrAddIfMissing(messageDoc.XPathSelectElement(RootElement, namespaceManager), "CSPID", "Status", "Accepted");
				SetValueOrAddIfMissing(messageDoc.XPathSelectElement(RootElement, namespaceManager), "Status", "ConversationId", conversationId);
				subscriptionAccessor.InsertSubscribedClients(transaction,
					SecondarySubscriptionType,
					SenderID,
					recipients,
					conversationId,
					CspId ?? MessageID,
					"CSP-ID");
				logger.Info($"[Service ID: {ID}] - Provider: {Provider.ConvertToString()}. Subscription for Conversation ID has been added. CSP: [{MessageID}], ConversationId: [{conversationId}]");
			}
			else
			{
				SetValueOrAddIfMissing(messageDoc.XPathSelectElement(RootElement, namespaceManager), "CSPID", "Status", "Error");

				if (string.IsNullOrWhiteSpace(responseCode)) throw new HttpException(400, "ErrorCode is expected.");
				SetValueOrAddIfMissing(messageDoc.XPathSelectElement(RootElement, namespaceManager), "Status", "Error", responseCode);

				var errorDescription = body.XPathSelectElement("/response/message")?.Value ?? string.Empty;
				if (string.IsNullOrEmpty(errorDescription))
				{
					errorDescription = body.XPathSelectElement("/errorResponse/message")?.Value ?? string.Empty;
				}

				if (string.IsNullOrWhiteSpace(errorDescription)) throw new HttpException(400, "ErrorDescription is expected.");
				SetValueOrAddIfMissing(messageDoc.XPathSelectElement(RootElement, namespaceManager), "Status", "ErrorText", errorDescription);

				var responseText = body.XPathSelectElement("/response")?.ToString() ?? string.Empty;
				if (string.IsNullOrEmpty(responseText))
				{
					responseText = body.XPathSelectElement("/errorResponse")?.ToString() ?? string.Empty;
				}
				SetValueOrAddIfMissing(messageDoc.XPathSelectElement(RootElement, namespaceManager), "Status", "ResponseText", responseText);
			}

			messageDoc.XPathSelectElement(RootElement, namespaceManager)?.SetElementValue("Response", string.Empty);
			messageDoc.XPathSelectElement($"{RootElement}/Response", namespaceManager)?.Add(requestMessage.Root);
		}

		void HandlePentantResponse(SqlTransaction transaction, XDocument requestMessage, XDocument messageDoc, XDocument interchangeDoc, Guid messageTrackingID, string[] recipients)
		{
			string dataTargetPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='DataContext']/*[local-name()='DataTargetCollection']/*[local-name()='DataTarget']";
			string contextCollectionPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='ContextCollection']";
			string eventTimePath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']";
			string jobNumberPath = "/*[local-name()='GBCustomsTransportResponse']/*[local-name()='GBCustoms']/*[local-name()='Header']/*[local-name()='GBCustomsRequest']/*[local-name()='JobNumber']";
			string eHubTrackingIdPath = "/*[local-name()='GBCustomsTransportResponse']/*[local-name()='eHubMessageTrackingId']";
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("S", "http://www.w3.org/2003/05/soap-envelope");
			namespaceManager.AddNamespace("ebi", "http://www.myvan.descartes.com/ebi/2004/r1");
			var jobNumber = messageDoc.XPathSelectElement(jobNumberPath).Value;
			var expectedTrackingId = messageDoc.XPathSelectElement(eHubTrackingIdPath)?.Value;
			if (string.IsNullOrWhiteSpace(jobNumber)) throw new HttpException(400, "Job/Declaration Number cannot be inferred");
			if (string.IsNullOrWhiteSpace(expectedTrackingId)) throw new HttpException(400, "eHubTrackingID cannot be inferred");

			var conversationId = requestMessage.XPathSelectElement("/S:Envelope/S:Header/ebi:CorrelationId", namespaceManager)?.Value;
			if (string.IsNullOrEmpty(conversationId)) conversationId = requestMessage.XPathSelectElement("/S:Envelope/S:Body/ResponseFields/responseHeaderField[@headerFieldName='X-Conversation-Id']", namespaceManager)?.Value;

			if (string.IsNullOrWhiteSpace(conversationId)) throw new HttpException((int)HttpStatusCode.NotImplemented, "The correlation id was not supplied in the body.");

			subscriptionAccessor.InsertSubscribedClients(transaction,
					SecondarySubscriptionType,
					SenderID,
					recipients,
					conversationId,
					MessageID,
					"Message Number");
			logger.Info($"[Service ID: {ID}] - Provider: {Provider.ConvertToString()}. Subscription for Conversation ID has been added. Message Number: [{MessageID}], ConversationId: [{conversationId}]");

			SetValueOrAddIfMissing(interchangeDoc.XPathSelectElement(eventTimePath), null, "EventTime", $"{DateTimeOffset.UtcNow:s}Z");

			SetValueOrAddIfMissing(interchangeDoc.XPathSelectElement(dataTargetPath), null, "Type", "CustomsDeclaration");
			SetValueOrAddIfMissing(interchangeDoc.XPathSelectElement(dataTargetPath), null, "Key", jobNumber);

			SetValueOrAddIfMissing(interchangeDoc.XPathSelectElement(contextCollectionPath), null, ContextNode("CSPEntryTrackingID", MessageID));
			SetValueOrAddIfMissing(interchangeDoc.XPathSelectElement(contextCollectionPath), null, ContextNode("ConversationID", conversationId));
			SetValueOrAddIfMissing(interchangeDoc.XPathSelectElement(contextCollectionPath), null, ContextNode("eHubTrackingID", expectedTrackingId));
		}

		void SetValueOrAddIfMissing(XElement parent, string previousSibling, string name, string value)
		{
			if (parent != null)
			{
				if (parent.XPathSelectElement(name) == null)
				{
					var element = new XElement(Provider == ProviderType.Pentant ? UniversalEventNamesapce + name : name, value);
					SetValueOrAddIfMissing(parent, previousSibling, element);
				}
				else
				{
					parent.XPathSelectElement(name).SetValue(value);
				}
			}
		}

		void SetValueOrAddIfMissing(XElement parent, string previousSibling, XElement element)
		{
			if (parent != null)
			{
				if (string.IsNullOrWhiteSpace(previousSibling))
				{
					parent.AddFirst(element);
				}
				else
				{
					parent.XPathSelectElement(previousSibling)?.AddAfterSelf(element);
				}
			}
		}

		XElement ContextNode(string type, string value)
		{
			var contextNode = new XElement(UniversalEventNamesapce + "Context");
			SetValueOrAddIfMissing(contextNode, null, "Type", type);
			SetValueOrAddIfMissing(contextNode, null, "Value", value);

			return contextNode;
		}

		protected override string GetSchemaName()
		{
			switch (Provider)
			{
				case ProviderType.CNS:
				case ProviderType.MCP:
					return $"{ResponseNamespace}#GBCustomsTransportResponse";
				case ProviderType.Pentant:
					return "http://www.cargowise.com/Schemas/Universal/2012/11";
				default:
					throw new InvalidOperationException($"Unable to resolve schema for {Provider.ConvertToString()} response");
			}
		}

		protected override string GetSubscriptionType()
		{
			switch (Provider)
			{
				case ProviderType.Pentant:
				case ProviderType.MCP:
				case ProviderType.CNS:
					return GetSettings("GBCustomsSubscriptionTypeByCSPID");
				default:
					return null;
			}
		}

		protected override string GetSecondarySubscriptionType()
		{
			switch (Provider)
			{
				case ProviderType.Pentant:
				case ProviderType.MCP:
				case ProviderType.CNS:
					return GetSettings("GBCustomsSubscriptionTypeByConversationID");
				default:
					return null;
			}
		}

		protected override string GetMessageID(XDocument requestMessage)
		{
			string id = null;

			if (Provider == ProviderType.CNS)
				id = FindValueOrDefaultFromNotificationHeader(requestMessage, "X-CSP-ID", "X-CNS-ID", "xCNSId");

			if (Provider == ProviderType.MCP)
				id = FindValueOrDefaultFromNotificationHeader(requestMessage, "X-CSP-ID", "X-MCP-ID", "xMCPId");

			if (Provider == ProviderType.Pentant)
				id = GetPentantId(requestMessage);

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
					case ProviderType.Pentant:
						return "Message Number";
					case ProviderType.MCP:
						return "CSP/MCP-ID";
					case ProviderType.CNS:
						return "CSP/CNS-ID";
					default:
						return "Message ID";
				}
			}
		}

		protected internal virtual string GetSubscription(SqlTransaction transaction, string recipient, string value)
		{
			string subscription;

			using (var command = new SqlCommand("dbo.SelectSubscribedReference", transaction.Connection, transaction))
			{
				command.CommandType = System.Data.CommandType.StoredProcedure;
				command.Parameters.Add("@reference", SqlDbType.NVarChar, -1).Direction = System.Data.ParameterDirection.Output;
				command.Parameters.AddWithValue("@senderId", SenderID);
				command.Parameters.AddWithValue("@recipientId", recipient);
				command.Parameters.AddWithValue("@ST_ID", SubscriptionType);
				command.Parameters.AddWithValue("@value", value);
				command.ExecuteNonQuery();
				subscription = command.Parameters["@reference"].Value.ToString();

				if(string.IsNullOrEmpty(subscription))
				{
					command.Parameters["@ST_ID"].Value = SecondarySubscriptionType;
					command.ExecuteNonQuery();
					subscription = command.Parameters["@reference"].Value.ToString();
				}
			}
			return subscription;
		}

		private string GetPentantId(XDocument requestMessage)
		{
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("S", "http://www.w3.org/2003/05/soap-envelope");
			namespaceManager.AddNamespace("ebi", "http://www.myvan.descartes.com/ebi/2004/r1");
			namespaceManager.AddNamespace("wsa", "http://schemas.xmlsoap.org/ws/2004/03/addressing");

			var pentantId = requestMessage.XPathSelectElement("/S:Envelope/S:Header/ebi:Sequence/ebi:MessageNumber", namespaceManager)?.Value;
			if (string.IsNullOrWhiteSpace(pentantId)) 
				pentantId = requestMessage.XPathSelectElement("/S:Envelope/S:Body/ResponseFields/responseMetaAttr[@metaAttrName='control_number']", namespaceManager)?.Value;
			return pentantId;
		}

        protected override string GetSubscribedCorrelationID(string body)
        {
            throw new NotImplementedException();
        }

		protected override string GetFallbackSubscriptionID(XDocument requestMessage)
		{
			return FindValueOrDefaultFromNotificationHeader(requestMessage, "X-FunctionalReference-ID");
		}
    }
}
