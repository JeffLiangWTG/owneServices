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
	public class NackHandler : MessageHandlerBase
	{
		internal Func<eHubTransactionsContext> ContextFactory = () => new eHubTransactionsContext();
		XNamespace UniversalEventNamesapce = "http://www.cargowise.com/Schemas/Universal/2012/11";
		const string ResponseNamespace = "http://cargowise.com/ehub/products/GBCustoms";
		const string RootElement = "/s0:GBCustomsTransportResponse";
		const string UniversalEventWrapperMessage = @"<UniversalInterchange xmlns='http://www.cargowise.com/Schemas/Universal/2011/11'>
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
        <EventType>MRJ</EventType>
        <ContextCollection></ContextCollection>
      </Event>
    </UniversalEvent>
  </Body>
</UniversalInterchange>";

		public NackHandler(ProviderType provider) : base(provider)
		{
		}

		public override string GetName()
		{
			return "Nack Handler";
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

			var messageDoc = XDocument.Parse(Provider == ProviderType.Pentant ? UniversalEventWrapperMessage : subscribedMessage);
			var messageTrackingID = InternalGuid();

			if (Provider == ProviderType.Pentant)
			{
				HandlePentantResponse(requestMessage, messageDoc, subscribedMessage, messageTrackingID);
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

		void HandlePentantResponse(XDocument requestMessage, XDocument messageDoc, string subscribedMessage, Guid messageTrackingID)
		{
			const string dataTargetPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='DataContext']/*[local-name()='DataTargetCollection']/*[local-name()='DataTarget']";
			const string contextCollectionPath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']/*[local-name()='ContextCollection']";
			const string eventTimePath = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalEvent']/*[local-name()='Event']";
			const string jobNumberPath = "//*[local-name()='JobNumber']";
			const string eHubTrackingIdPath = "//*[local-name()='eHubMessageTrackingId']";
			string errorText = $"{Provider} Document Submission Failed";
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("S", "http://www.w3.org/2003/05/soap-envelope");
			namespaceManager.AddNamespace("ebi", "http://www.myvan.descartes.com/ebi/2004/r1");

			XDocument subscribedMessageDoc;
			try
			{
				subscribedMessageDoc = XDocument.Parse(subscribedMessage);
			}
			catch (XmlException ex)
			{
				var a = $"[Service ID: {ID}] Original XML message from subscription is not valid - {ex.Message}";
				logger.Error($"[Service ID: {ID}] Original XML message from subscription is not valid - {ex.Message}");
				throw new HttpException(500, "Critical internal error. Do not retry. Use the enquiry ID and contact WiseTech Global.");
			}

			var jobNumber = subscribedMessageDoc.XPathSelectElement(jobNumberPath).Value;
			if (string.IsNullOrWhiteSpace(jobNumber)) throw new HttpException(400, "Job/Declaration Number cannot be inferred");

			var eHubTrackingId = subscribedMessageDoc.XPathSelectElement(eHubTrackingIdPath).Value;
			if (string.IsNullOrWhiteSpace(jobNumber)) throw new HttpException(400, "eHubTrackingId cannot be inferred");

			var responseText = requestMessage.XPathSelectElement("/S:Envelope/S:Body/errorResponse", namespaceManager);

			SetValueOrAddIfMissing(messageDoc.XPathSelectElement(eventTimePath), null, "EventTime", XmlConvert.ToString(DateTime.Now, "s"));

			SetValueOrAddIfMissing(messageDoc.XPathSelectElement(dataTargetPath), null, "Type", "CustomsDeclaration");
			SetValueOrAddIfMissing(messageDoc.XPathSelectElement(dataTargetPath), null, "Key", jobNumber);

			SetValueOrAddIfMissing(messageDoc.XPathSelectElement(contextCollectionPath), null, ContextNode("ResponseText", EncodeText(responseText.ToString())));
			SetValueOrAddIfMissing(messageDoc.XPathSelectElement(contextCollectionPath), null, ContextNode("Error", errorText));
			SetValueOrAddIfMissing(messageDoc.XPathSelectElement(contextCollectionPath), null, ContextNode("eHubTrackingID", eHubTrackingId));
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
					return GetSettings("GBCustomsSubscriptionTypeByCSPID");
				default:
					return null;
			}
		}

		protected override string GetMessageID(XDocument requestMessage)
		{
			string id = null;

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
					default:
						return "Message ID";
				}
			}
		}

		protected internal virtual string GetSubscription(SqlTransaction transaction, string recipient, string value)
		{
			using (var command = new SqlCommand("dbo.SelectSubscribedReference", transaction.Connection, transaction))
			{
				command.CommandType = System.Data.CommandType.StoredProcedure;
				command.Parameters.Add("@reference", SqlDbType.NVarChar, -1).Direction = System.Data.ParameterDirection.Output;
				command.Parameters.AddWithValue("@senderId", SenderID);
				command.Parameters.AddWithValue("@recipientId", recipient);
				command.Parameters.AddWithValue("@ST_ID", SubscriptionType);
				command.Parameters.AddWithValue("@value", value);
				command.ExecuteNonQuery();
				return command.Parameters["@reference"].Value.ToString();
			}
		}

		private string GetPentantId(XDocument requestMessage)
		{
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("S", "http://www.w3.org/2003/05/soap-envelope");
			namespaceManager.AddNamespace("ebi", "http://www.myvan.descartes.com/ebi/2004/r1");
			var pentantId = requestMessage.XPathSelectElement("/S:Envelope/S:Header/ebi:Sequence/ebi:MessageNumber", namespaceManager)?.Value;
			if (string.IsNullOrWhiteSpace(pentantId))
				pentantId = requestMessage.XPathSelectElement("/S:Envelope/S:Body/ResponseFields/responseMetaAttr[@metaAttrName='control_number']", namespaceManager)?.Value;
			return pentantId;
		}

		protected override string GetSubscribedCorrelationID(string body)
		{
			throw new NotImplementedException();
		}

		protected string ConversationIDSubscriptionType => GetSettings("GBCustomsSubscriptionTypeByConversationID");

		private string EncodeText(string textToEncode)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(textToEncode);
			return System.Convert.ToBase64String(plainTextBytes);
		}

	}
}
