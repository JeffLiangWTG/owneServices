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
	public class CSPNotificationHandler : MessageHandlerBase
	{
		internal Func<eHubTransactionsContext> ContextFactory = () => new eHubTransactionsContext();
		const string WrapperMessage = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms""><s0:ResponseHeader><s0:eHubTrackingId/><s0:CSPEntryTrackingID/></s0:ResponseHeader><s0:ResponseBody/></s0:GBCustomsBusinessResponse>";
		const string ResponseNamespace = "http://cargowise.com/ehub/products/GBCustoms";

		string FallbackID;

		public CSPNotificationHandler(ProviderType provider) : base(provider)
		{
		}

		public override string GetName()
		{
			return "CSP Notification Handler";
		}

		protected override void HandleMessage(SqlTransaction transaction, XDocument requestMessage, XDocument body, string[] recipients, string stringBody = "")
		{
			using (var response = new MemoryStream())
			{
				var subscribedMessage = GetSubscription(transaction, recipients[0], MessageID, SubscriptionType);

				if (string.IsNullOrWhiteSpace(subscribedMessage))
				{ 
					subscribedMessage = GetSubscription(transaction, recipients[0], MessageID, SecondarySubscriptionType);
				}

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
				var eHubTrackingID = messageDoc.XPathSelectElement("//*[local-name()='eHubMessageTrackingId']").Value;
				WrapMessage(body, eHubTrackingID, MessageID, response);
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
				case ProviderType.CNS:
				case ProviderType.MCP:
					return GetSettings("GBCustomsSubscriptionTypeByCSPID");
				default:
					return null;
			}
		}

		protected override string GetSecondarySubscriptionType()
		{
			switch (Provider)
			{
				case ProviderType.CNS:
				case ProviderType.MCP:
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
					case ProviderType.MCP:
						return "CSP/MCP-ID";
					case ProviderType.CNS:
						return "CSP/CNS-ID";
					default:
						return "Message ID";
				}
			}
		}

		void WrapMessage(XDocument messageDoc, string eHubTrackingID, string cspID, Stream outputStream)
		{
			if (eHubTrackingID == null) throw new HttpException(400, "eHubTrackingID is expected.");
			var wrapperDoc = XDocument.Parse(WrapperMessage);
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("s0", ResponseNamespace);
			wrapperDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:eHubTrackingId", namespaceManager)?.SetValue(eHubTrackingID);
			wrapperDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseHeader/s0:CSPEntryTrackingID", namespaceManager)?.SetValue(cspID);
			wrapperDoc.XPathSelectElement("/s0:GBCustomsBusinessResponse/s0:ResponseBody", namespaceManager)?.Add(messageDoc.Root);
			wrapperDoc.Save(outputStream);
			outputStream.SeekBegin();
		}

		protected internal virtual string GetSubscription(SqlTransaction transaction, string recipient, string value, string subscriptionType)
		{
			using (var command = new SqlCommand("dbo.SelectSubscribedReference", transaction.Connection, transaction))
			{
				command.CommandType = System.Data.CommandType.StoredProcedure;
				command.Parameters.Add("@reference", SqlDbType.NVarChar, -1).Direction = System.Data.ParameterDirection.Output;
				command.Parameters.AddWithValue("@senderId", SenderID);
				command.Parameters.AddWithValue("@recipientId", recipient);
				command.Parameters.AddWithValue("@ST_ID", subscriptionType);
				command.Parameters.AddWithValue("@value", value);
				command.ExecuteNonQuery();
				return command.Parameters["@reference"].Value.ToString();
			}
		}

        protected override string GetSubscribedCorrelationID(string body)
        {
            throw new System.NotImplementedException();
        }

		protected override string GetFallbackSubscriptionID(XDocument requestMessage)
		{
			const string fallbackPath = "//*[local-name()='Declaration']/*[local-name()='FunctionalReferenceID']";
			var functionalReferenceIDElement = requestMessage.XPathSelectElement(fallbackPath);
			if (functionalReferenceIDElement == null)
			{
				var body = requestMessage.XPathSelectElement("//*[local-name()='body']");
				if (body != null && TryDecodeBase64(body.Value, out var decodedBody))
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

	}
}
