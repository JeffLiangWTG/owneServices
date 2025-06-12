using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataModel.eHubTransactions;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Helpers
{
	public static class DbHelpers
	{
		internal static Func<eHubTransactionsContext> GetDbContext = () => new eHubTransactionsContext();

		const string GBCUSTOMS = "GBCustoms-";
		const string ACCOUNT = "Account";

		public static bool IsRegisteredWithProvider(string sourceClientSystemId, string badge, ProviderType provider)
		{
			using (var context = GetDbContext())
			{
				var registrationType = context.FindRegistrationType(GBCUSTOMS + provider.ConvertToString() + ACCOUNT);
				var sourceClientSystem = context.FindClientSystem(sourceClientSystemId);

				var matchedRegistration = context
					.eHubClientSystemRegistrations
					.SingleOrDefault(registration =>
						registration.CD_RT == registrationType.RT_PK &&
						registration.CD_EH == sourceClientSystem.EH_PK &&
						registration.CD_Code == badge);

				if (matchedRegistration == null)
				{
					return false;
				}

				return
					matchedRegistration.CD_Flag1 == 0 ||
					matchedRegistration.CD_Flag1 == 1;
			}
		}

		public static CallbackRegistration FindCallbackRegistration(string destinationClientId, ProviderType provider)
		{
			using (var context = GetDbContext())
			{
				var registrationType = context.FindRegistrationType(GBCUSTOMS + provider.ConvertToString());
				var destinationClient = context.FindClient(destinationClientId);

				var matchedClientRegistration = context
					.eHubClientRegistrations
					.SingleOrDefault(registration => 
						registration.CX_RT == registrationType.RT_PK &&
						registration.CX_CC == destinationClient.CC_PK);

				if (matchedClientRegistration == null)
				{
					throw new InvalidOperationException(
						$"No eHub client registration for registration type with ID [{registrationType.RT_ID}] for client with ID [{destinationClient.CC_ID}]!");
				}

				return CallbackRegistration.Create(
					matchedClientRegistration.CX_Attr1,
					matchedClientRegistration.CX_Code);
			}
		}

		public static string CreateClientRegistrationStatusQuery(
			string sourceClientSystemId,
			string badge,
			string topic,
			byte status,
			ProviderType provider)
		{
            if (string.IsNullOrEmpty(badge))
			{
				throw new ArgumentNullException("badge");
			}

			if (string.IsNullOrEmpty(topic))
			{
				throw new ArgumentNullException(nameof(topic));
			}

			using (var context = GetDbContext())
			{
				var registrationType = context.FindRegistrationType(GBCUSTOMS + provider.ConvertToString() + ACCOUNT);
				var sourceClientSystem = context.FindClientSystem(sourceClientSystemId);

				var matchedRegistration = context
					.eHubClientSystemRegistrations
					.SingleOrDefault(registration =>
						registration.CD_RT == registrationType.RT_PK &&
						registration.CD_EH == sourceClientSystem.EH_PK &&
						registration.CD_Code == badge);

				var queryXml = default(XDocument);

				if (matchedRegistration == null)
				{
					var issueTimestamp = $"{DateTimeOffset.UtcNow:s}Z";

					queryXml = new XDocument(
						new XElement(NS.eHub.ClientSystemRegistration + "Insert",
							new XElement(NS.eHub.ClientSystemRegistration + "Rows",
								new XElement(NS.Microsoft.Table + Schema.ClientSystemRegistration.Name,
									new XElement(Schema.ClientSystemRegistration.Pk, Guid.NewGuid()),
									new XElement(Schema.ClientSystemRegistration.RegistrationTypePk, registrationType.RT_PK),
									new XElement(Schema.ClientSystemRegistration.ClientSystemPk, sourceClientSystem.EH_PK),
									new XElement(Schema.ClientSystemRegistration.Code, badge),
									new XElement(Schema.ClientSystemRegistration.Attr1, topic),
									new XElement(Schema.ClientSystemRegistration.Flag1, status),
									new XElement(Schema.ClientSystemRegistration.IssueUtc, issueTimestamp),
									new XElement(Schema.ClientSystemRegistration.Rv, "0x0000000000000000")))));
				}
				else
				{
					queryXml = new XDocument(
						new XElement(NS.eHub.ClientSystemRegistration + "Update",
							new XElement(NS.eHub.ClientSystemRegistration + "Rows",
								new XElement(NS.eHub.ClientSystemRegistration + "RowPair",
									new XElement(NS.eHub.ClientSystemRegistration + "After",
										new XElement(Schema.ClientSystemRegistration.Flag1, status)),
									new XElement(NS.eHub.ClientSystemRegistration + "Before",
										new XElement(Schema.ClientSystemRegistration.Pk, matchedRegistration.CD_PK))))));
				}

				return queryXml.ToString();
			}
		}

		public static string CreateSubmissionReferenceQuery(
			string referenceId,
			string messageTrackingId,
			string messageBody,
			string sourceParty,
			string destinationParty,
			string subscriptionId)
		{
			if (string.IsNullOrEmpty(referenceId))
			{
				throw new ArgumentNullException(nameof(referenceId), "The response from the CSP did not contain an ID, the CSP service could be down.");
			}

			if (string.IsNullOrEmpty(messageTrackingId))
			{
				throw new ArgumentNullException(nameof(messageTrackingId));
			}

			using (var context = GetDbContext())
			{
				var sourceClient = context.FindClient(sourceParty);
				var destinationClient = context.FindClient(destinationParty);
				var subscriptionType = context.FindSubscriptionType(subscriptionId);

				var matchedSubscriptionValue = context
					.eHubSubscriptionValues
					.SingleOrDefault(value =>
						value.SV_ST == subscriptionType.ST_PK &&
						value.SV_CC_Sender == destinationClient.CC_PK &&
						value.SV_CC_Recipient == sourceClient.CC_PK &&
						value.SV_Value == referenceId);

				if (matchedSubscriptionValue != null)
				{
					return string.Empty;
				}

                var transportResponseXml = string.Empty;
                if (!string.IsNullOrEmpty(messageBody))
                {
                    var messageDocument = XDocument.Parse(messageBody);
                    var namespaceManager = new XmlNamespaceManager(new NameTable());
                    namespaceManager.AddNamespace("ns0", "http://cargowise.com/ehub/products/GBCustoms");
                    var bodyElement = messageDocument.XPathSelectElement("/ns0:GBCustoms/Body", namespaceManager);
                    if (bodyElement != null) bodyElement.Remove();

                    transportResponseXml = new XDocument(
                        new XElement(NS.eHub.GbCustoms + Schema.GbCustomsTransportResponse.Name,
                            new XAttribute(XNamespace.Xmlns + "ns0", NS.eHub.GbCustoms),
                            new XElement(Schema.GbCustomsTransportResponse.CspId, referenceId),
                            new XElement(Schema.GbCustomsTransportResponse.MessageTrackingId, messageTrackingId),
                            messageDocument.Root)).ToString();
                }
                else
                {
                    transportResponseXml = new XDocument(
                            new XElement(Schema.GbCustomsTransportResponse.MessageTrackingId, messageTrackingId)).ToString();
                }

                var subscribedTimestamp = DateTimeOffset.UtcNow;

				var expiryDayTotal = 1;

				if (subscriptionType.ST_ExpiryDays.HasValue)
				{
					expiryDayTotal = Math.Max(expiryDayTotal, subscriptionType.ST_ExpiryDays.Value);
				}

				var expiryTimestamp = subscribedTimestamp + TimeSpan.FromDays(expiryDayTotal);

				var queryXml = new XDocument(
					new XElement(NS.eHub.SubscriptionValue + "Insert",
						new XElement(NS.eHub.SubscriptionValue + "Rows",
							new XElement(NS.Microsoft.Table + Schema.SubscriptionValue.Name,
								new XElement(Schema.SubscriptionValue.Pk, Guid.NewGuid()),
								new XElement(Schema.SubscriptionValue.SubscriptionTypePk, subscriptionType.ST_PK),
								new XElement(Schema.SubscriptionValue.SenderPk, destinationClient.CC_PK),
								new XElement(Schema.SubscriptionValue.RecipientPk, sourceClient.CC_PK),
								new XElement(Schema.SubscriptionValue.Value, referenceId),
								new XElement(Schema.SubscriptionValue.Reference, new XCData(transportResponseXml)),
								new XElement(Schema.SubscriptionValue.SubscribedUtc, string.Format("{0:s}Z", subscribedTimestamp)),
								new XElement(Schema.SubscriptionValue.ExpiryUtc, string.Format("{0:s}Z", expiryTimestamp))))));

				return queryXml.ToString();
			}
		}

		public static string CreateSuccessfulTransportResponseQuery(
			string referenceId,
			string messageTrackingId,
			string messageBody,
			string sourceParty,
			string destinationParty,
			string transactionId=null)
		{
			if (string.IsNullOrEmpty(referenceId))
			{
				throw new ArgumentNullException(nameof(referenceId));
			}

			if (string.IsNullOrEmpty(messageTrackingId))
			{
				throw new ArgumentNullException(nameof(messageTrackingId));
			}

			if (string.IsNullOrEmpty(messageBody))
			{
				throw new ArgumentNullException(nameof(messageBody));
			}

			using (var context = GetDbContext())
			{
				var sourceClient = context.FindClient(sourceParty);
				var destinationClient = context.FindClient(destinationParty);

				var transportResponseXml = new XDocument(
					new XElement(NS.eHub.GbCustoms + Schema.GbCustomsTransportResponse.Name,
						new XAttribute(XNamespace.Xmlns + "ns0", NS.eHub.GbCustoms),
						new XElement(Schema.GbCustomsTransportResponse.Status, "Accepted"),
						new XElement(Schema.GbCustomsTransportResponse.CspId, transactionId),
						new XElement(Schema.GbCustomsTransportResponse.MessageTrackingId, messageTrackingId),
						XElement.Parse(messageBody)));

				return CreateTransportResponseQuery(
					transportResponseXml,
					sourceClient.CC_PK,
					destinationClient.CC_PK);
			}
		}

		public static string CreateErrorTransportResponseQuery(
			string error,
			string errorText,
			string responseText,
			string messageTrackingId,
			string messageBody,
			string sourceParty,
			string destinationParty)
		{
			if (string.IsNullOrEmpty(error))
			{
				throw new ArgumentNullException(nameof(error));
			}

			if (string.IsNullOrEmpty(errorText))
			{
				errorText = string.Empty;
			}

			if (string.IsNullOrEmpty(responseText))
			{
				responseText = string.Empty;
			}

			if (string.IsNullOrEmpty(messageTrackingId))
			{
				throw new ArgumentNullException(nameof(messageTrackingId));
			}

			if (string.IsNullOrEmpty(messageBody))
			{
				throw new ArgumentNullException(nameof(messageBody));
			}

			using (var context = GetDbContext())
			{
				
				var sourceClient = context.FindClient(sourceParty);
				var destinationClient = context.FindClient(destinationParty);
				
				// NOTE: TransportResponse2UniversalEvent mapping relies on string 'Error Description' for <ErrorText>
				// value to prepare final <ErrorDescription> value, including stack-trace removal. This will only work
				// if the exception type is [Microsoft.XLANGs.Core.XlangSoapException], but WON'T work with other
				// exception type, hence as workaround, we will prefix error text with 'Error Description'.

				if (!errorText.Contains("Error Description"))
				{
					errorText = $"Error Description: {errorText}";
				}

				var transportResponseXml = new XDocument(
					new XElement(NS.eHub.GbCustoms + Schema.GbCustomsTransportResponse.Name,
						new XAttribute(XNamespace.Xmlns + "ns0", NS.eHub.GbCustoms),
						new XElement(Schema.GbCustomsTransportResponse.Status, "Error"),
						new XElement(Schema.GbCustomsTransportResponse.Error, error),
						new XElement(Schema.GbCustomsTransportResponse.ErrorText, errorText),
						new XElement(Schema.GbCustomsTransportResponse.ResponseText, responseText),
						new XElement(Schema.GbCustomsTransportResponse.MessageTrackingId, messageTrackingId),
						XElement.Parse(messageBody)));

				return CreateTransportResponseQuery(
					transportResponseXml,
					sourceClient.CC_PK,
					destinationClient.CC_PK);
			}
		}

		private static string CreateTransportResponseQuery(
			XDocument transportResponseXml, 
			Guid sourceClientPk,
			Guid destinationClientPk)
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(transportResponseXml.ToString())))
			{
				var encodedContent = stream
					.CompressAndEncode()
					.ReadToEnd();

				var pk = Guid.NewGuid();
				var insertTimestamp = $"{DateTimeOffset.UtcNow:s}Z";
				var messageType = $"{NS.eHub.GbCustoms}#{Schema.GbCustomsTransportResponse.Name}";

				var queryXml = new XDocument(
					new XElement(NS.eHub.InboxMessage + "Insert",
						new XElement(NS.eHub.InboxMessage + "Rows",
							new XElement(NS.Microsoft.Table + Schema.InboxMessage.Name,
								new XElement(Schema.InboxMessage.Pk, pk),
								new XElement(Schema.InboxMessage.MessageTrackingId, pk),
								new XElement(Schema.InboxMessage.EnvelopeTrackingId, Guid.Empty),
								new XElement(Schema.InboxMessage.SenderPk, destinationClientPk),
								new XElement(Schema.InboxMessage.RecipientPk, sourceClientPk),
								new XElement(Schema.InboxMessage.MessageType, messageType),
								new XElement(Schema.InboxMessage.IsFlatFile, false),
								new XElement(Schema.InboxMessage.ApplicationCode, "UDM"),
								new XElement(Schema.InboxMessage.Status, 0),
								new XElement(Schema.InboxMessage.Content, encodedContent),
								new XElement(Schema.InboxMessage.InsertUtc, insertTimestamp)))));

				return queryXml.ToString();
			}
		}

		private static eHubRegistrationType FindRegistrationType(
			this eHubTransactionsContext context, 
			string registrationTypeId)
		{
			if (string.IsNullOrEmpty(registrationTypeId))
			{
				throw new ArgumentNullException("registrationTypeId");
			}

			var matchedRegistrationType = context
				.eHubRegistrationTypes
				.SingleOrDefault(type => type.RT_ID == registrationTypeId);

			if (matchedRegistrationType == null)
			{
				throw new InvalidOperationException(string.Format(
					"No eHub registration type with ID [{0}]!", 
					registrationTypeId));
			}

			return matchedRegistrationType;
		}

		private static eHubClientSystem FindClientSystem(this eHubTransactionsContext context, string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException("id");
			}

			var matchedClientSystem = context
				.eHubClientSystems
				.SingleOrDefault(system => system.EH_ID == id);

			if (matchedClientSystem == null)
			{
				throw new InvalidOperationException(string.Format("No eHub client system with ID [{0}]!", id));
			}

			return matchedClientSystem;
		}

		private static eHubClient FindClient(this eHubTransactionsContext context, string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException("id");
			}

			var matchedClient = context
				.eHubClients
				.SingleOrDefault(client => client.CC_ID == id);

			if (matchedClient == null)
			{
				throw new InvalidOperationException(string.Format("No eHub client with ID [{0}]!", id));
			}

			return matchedClient;
		}

		private static eHubSubscriptionType FindSubscriptionType(this eHubTransactionsContext context, string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				throw new ArgumentNullException("id");
			}

			var matchedSubscriptionType = context
				.eHubSubscriptionTypes
				.SingleOrDefault(type => type.ST_ID == id);

			if (matchedSubscriptionType == null)
			{
				throw new InvalidOperationException(string.Format("No eHub subscription type with ID [{0}]!", id));
			}

			return matchedSubscriptionType;
		}

	}

		

		static class NS
		{
			public static class Microsoft
			{
				public static readonly XNamespace Table = "http://schemas.microsoft.com/Sql/2008/05/Types/Tables/dbo";
			}

			public static class eHub
			{
				public static readonly XNamespace GbCustoms = "http://cargowise.com/ehub/products/GBCustoms";

				public static readonly XNamespace ClientSystemRegistration = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubClientSystemRegistration";

				public static readonly XNamespace InboxMessage = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubInboxMessage";

				public static readonly XNamespace SubscriptionValue = "http://schemas.microsoft.com/Sql/2008/05/TableOp/dbo/eHubSubscriptionValue";
			}
		}

		static class Schema
		{
			public static class GbCustomsTransportResponse
			{
				public const string Name = "GBCustomsTransportResponse";

				public static readonly XName Status = "Status";

				public static readonly XName Error = "Error";

				public static readonly XName ErrorText = "ErrorText";

				public static readonly XName ResponseText = "ResponseText";

				public static readonly XName MessageTrackingId = "eHubMessageTrackingId";

				public static readonly XName CspId = "CSPID";

				public static readonly XName ConversationId = "ConversationId";
			}

			public static class ClientSystemRegistration
			{
				public const string Name = "eHubClientSystemRegistration";

				public static readonly XName Pk = NS.Microsoft.Table + "CD_PK";

				public static readonly XName RegistrationTypePk = NS.Microsoft.Table + "CD_RT";

				public static readonly XName ClientSystemPk = NS.Microsoft.Table + "CD_EH";

				public static readonly XName Code = NS.Microsoft.Table + "CD_Code";

				public static readonly XName Attr1 = NS.Microsoft.Table + "CD_Attr1";

				public static readonly XName Flag1 = NS.Microsoft.Table + "CD_Flag1";

				public static readonly XName IssueUtc = NS.Microsoft.Table + "CD_IssuedUTC";

				public static readonly XName Rv = NS.Microsoft.Table + "CD_RV";
			}

			public static class InboxMessage
			{
				public const string Name = "eHubInboxMessage";

				public static readonly XName Pk = NS.Microsoft.Table + "EI_PK";

				public static readonly XName MessageTrackingId = NS.Microsoft.Table + "EI_MessageTrackingID";

				public static readonly XName EnvelopeTrackingId = NS.Microsoft.Table + "EI_EnvelopeTrackingID";

				public static readonly XName SenderPk = NS.Microsoft.Table + "EI_CC_Sender";

				public static readonly XName RecipientPk = NS.Microsoft.Table + "EI_CC_Recipient";

				public static readonly XName MessageType = NS.Microsoft.Table + "EI_MessageType";

				public static readonly XName IsFlatFile = NS.Microsoft.Table + "EI_IsFlatFile";

				public static readonly XName ApplicationCode = NS.Microsoft.Table + "EI_ApplicationCode";

				public static readonly XName Status = NS.Microsoft.Table + "EI_Status";

				public static readonly XName Content = NS.Microsoft.Table + "EI_Content";

				public static readonly XName InsertUtc = NS.Microsoft.Table + "EI_InsertUTC";
			}

			public static class SubscriptionValue
			{
				public const string Name = "eHubSubscriptionValue";

				public static readonly XName Pk = NS.Microsoft.Table + "SV_PK";

				public static readonly XName SubscriptionTypePk = NS.Microsoft.Table + "SV_ST";

				public static readonly XName SenderPk = NS.Microsoft.Table + "SV_CC_Sender";

				public static readonly XName RecipientPk = NS.Microsoft.Table + "SV_CC_Recipient";

				public static readonly XName Value = NS.Microsoft.Table + "SV_Value";

				public static readonly XName Reference = NS.Microsoft.Table + "SV_Reference";

				public static readonly XName SubscribedUtc = NS.Microsoft.Table + "SV_SubscribedUTC";

				public static readonly XName ExpiryUtc = NS.Microsoft.Table + "SV_ExpiryUTC";
			}
		}
	
}
