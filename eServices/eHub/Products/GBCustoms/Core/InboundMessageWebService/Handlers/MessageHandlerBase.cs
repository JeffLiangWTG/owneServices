using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.Products.GBCustoms.Core.Correlation.Handlers;
using Common.Logging;

namespace CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers
{
	public abstract class MessageHandlerBase : IMessageHandler
	{
		protected MessageHandlerBase(ProviderType provider)
		{
			Provider = provider;
		}

		public HttpResponseMessage Process(HttpRequestMessage request, ILog logger, Guid id, XDocument requestMessage, XDocument body = null, bool isTransportLayer = false, string stringBody = "")
		{
			this.logger = logger;
			this.ID = id;
			this.request = request;
			var messageString = (requestMessage != null ? requestMessage.ToString() : stringBody);
			logger.Trace($"[Request Message: {messageString}]");
			logger.Trace($"[Request Headers: {request.Headers.ToString()}]");
			RegistrationType = GetSettings("GBCustomsRegistrationType");
			GetRecipientsRetryAttempts = GetSettingsInt("GetRecipientsRetryAttempts");
			GetRecipientsRetryWaitTime = GetSettingsInt("GetRecipientsRetryWaitTime");
			SubscriptionType = GetSubscriptionType();
			SecondarySubscriptionType = GetSecondarySubscriptionType();
			SubscriptionProviderIDList = GetSettings("SubscriptionProviderIDList").Split(',');
			string[] recipients;
			SetUpAccessors();
			using (var connection = GetConnection(eHubConnection))
			{
				DatabaseHelper.ExecuteWithRetries(connection, transaction =>
				{
					SecurityValidationType validationType = GetSecurityValidationType();
					switch (validationType)
					{
						case SecurityValidationType.AuthorisationHeader:
							ValidateAuthorisationHeader(transaction);
							break;
						case SecurityValidationType.SignatureHeader:
							ValidateSignatureHeader(messageString);
							break;
						default:
							logger.Info($"[Service ID: {ID}, Provider: {Provider}] No security validation process has been configured for this provider.]");
							throw new HttpException(460, $"No security validation has been implemented.");
					}

					MessageID = !isTransportLayer ? GetMessageID(requestMessage) : GetSubscribedCorrelationID(stringBody);
					recipients = GetRecipients(transaction, requestMessage, stringBody);
					logger.Info($"[Service ID: {id}] Identified recipients: {string.Join(", ", recipients)}");
					try
					{
						HandleMessage(transaction, requestMessage, body, recipients, stringBody);
					}
					catch (SqlException ex) when (ex.Message.StartsWith("Recipient permission denied"))
					{
						StringBuilder sbRecipients = new StringBuilder();
						foreach (var recipient in recipients)
						{
							sbRecipients.Append($"{recipient}, ");
							partyAccessor.IsPermitInboxRecipient(recipient, false);
						}
						logger.Info($"[Service ID: {ID}, Provider: {Provider}] One or more of the recipients [{sbRecipients.ToString().Substring(0, sbRecipients.Length - 2)}] were disabled and the message cannot be routed. Refreshing cache for inbound retries.");
						throw new HttpException(500, "Internal error. Recipient not routable, please retry.");
					}
			});

				return request.CreateResponse(GetResponseStatus());
			}
		}

		#region Overrideable
		public abstract string GetName();
		protected abstract void HandleMessage(SqlTransaction transaction, XDocument requestMessage, XDocument body, string[] recipients, string stringBody = "");
		protected abstract string GetSchemaName();
		protected abstract string GetSubscriptionType();
		protected abstract string GetMessageID(XDocument requestMessage);
		protected abstract string GetSubscribedCorrelationID(string body);
		protected virtual string GetSecondarySubscriptionType() => null;
		#endregion

		#region Implementation
		protected string ResolveSender()
		{
			return SenderID.Split(new[] { "-" }, StringSplitOptions.None)[0];
		}

		void ValidateAuthorisationHeader(SqlTransaction transaction)
		{
			var gbCustomsId = GetSettings("GBCustomsID");
			var gbCustomsTestId = GetSettings("GBCustomsTestID");
			if (TransportLayerProviders.Exists(x => x == Provider))
			{
				gbCustomsId = $"{gbCustomsId}-{Provider}";
				gbCustomsTestId = $"{gbCustomsTestId}-{Provider}";
			}
			var authorisationHeader = ExtractHeader(GetSettings("AuthorisationHeaderKey"));
			if (ValidateAuthorisationHeader(gbCustomsId, authorisationHeader, transaction)) SenderID = gbCustomsId;
			else if (ValidateAuthorisationHeader(gbCustomsTestId, authorisationHeader, transaction)) SenderID = gbCustomsTestId;
			else
			{
				logger.Info($"[Service ID: {ID}] Invalid AuthorisationHeader received from GBCustoms. [{authorisationHeader}]");
				throw new HttpException(461, $"Invalid AuthorisationHeader [{authorisationHeader}] received from GBCustoms.");
			}
		}

		bool ValidateAuthorisationHeader(string gbCustomsId, string authorisationHeader, SqlTransaction transaction)
		{
			if (string.IsNullOrWhiteSpace(authorisationHeader)) throw new HttpException(462, "Authorisation credentials in HTTP header not supplied");
			var authorisationHeaders = clientRegistrationAccessor.ReadRegistrations(transaction, gbCustomsId, RegistrationType);
			if (authorisationHeaders == null) return false;

			if (Provider == ProviderType.CDS || TransportLayerProviders.Exists(x => x == Provider))
			{
				Func<string, string> SelectHeader = qualifier => authorisationHeaders.Where(row => row["CX_Qualifier"].ToString() == qualifier).Select(row => row["CX_Code"]).FirstOrDefault()?.ToString();
				var authorisationHeaderStatus = authorisationHeaders
					.Where(row => row["CX_Code"].ToString() == authorisationHeader)
					.Select(row => row["CX_Qualifier"].ToString())
					.FirstOrDefault();
				switch (authorisationHeaderStatus)
				{
					case "CURRENT":
					case "OLD":
						logger.Info($"[Service ID: {ID}] Valid AuthorisationHeader: {authorisationHeader} from {gbCustomsId}.");
						return true;
					case "NEXT":
						clientRegistrationAccessor.UpdateRegistrationCode(SelectHeader("CURRENT"), transaction, gbCustomsId, RegistrationType, "OLD");
						clientRegistrationAccessor.UpdateRegistrationCode(SelectHeader("NEXT"), transaction, gbCustomsId, RegistrationType, "CURRENT");
						clientRegistrationAccessor.UpdateRegistrationCode(string.Empty, transaction, gbCustomsId, RegistrationType, "NEXT");
						logger.Info($"[Service ID: {ID}] AuthorisationHeaders updated for {gbCustomsId}. OLD: [{SelectHeader("OLD")}] CURRENT: [{SelectHeader("CURRENT")}] NEXT: []");
						return true;
					default:
						return false;
				}
			}

			return authorisationHeaders.Any(row => row["CX_Code"].ToString() == authorisationHeader);
		}

		void ValidateSignatureHeader(string messageString)
		{
			var gbCustomsId = GetSettings("GBCustomsID");
			var gbCustomsTestId = GetSettings("GBCustomsTestID");
			if (TransportLayerProviders.Exists(x => x == Provider))
			{
				gbCustomsId = $"{gbCustomsId}-{Provider}";
				gbCustomsTestId = $"{gbCustomsTestId}-{Provider}";
			}
			var signatureHeader = ExtractHeader("x-hub-signature");
			if (string.IsNullOrWhiteSpace(signatureHeader)) throw new HttpException(463, "No Signature header (x-hub-signature) received.");
			else
			{
				if (ValidateSignatureHeader(signatureHeader, GetSettings("PushSecretProd"), messageString)) SenderID = gbCustomsId;
				else if (ValidateSignatureHeader(signatureHeader, GetSettings("PushSecretTest"), messageString)) SenderID = gbCustomsTestId;
				else if (ValidateSignatureHeader(signatureHeader, GetSettings("PushSecretUat"), messageString)) SenderID = gbCustomsTestId;
				else
				{
					logger.Info($"[Service ID: {ID}] Could not verify the push request for the test, uat or production secret.");
					throw new HttpException(464, $"Could not verify the push request for the test, uat or production secret.");
				}
			}
		}

		protected internal bool ValidateSignatureHeader(string signatureHeader, string pushSecret, string messageString)
		{
			UTF8Encoding enc = new UTF8Encoding();
			byte[] secretKey = enc.GetBytes(pushSecret);
			byte[] messageBytes = enc.GetBytes(messageString);
			var hmac = new HMACSHA1(secretKey);
			hmac.Initialize();
			byte[] rawHmac = hmac.ComputeHash(messageBytes);
			var computedSignature = BitConverter.ToString(rawHmac).Replace("-", "");
			return (computedSignature.Equals(signatureHeader, StringComparison.InvariantCultureIgnoreCase));
		}

		string[] GetRecipients(SqlTransaction transaction, XDocument requestMessage, string jsonBody)
		{
			string referenceType = GetReferenceType(Provider, jsonBody);

			while (RetryCount <= GetRecipientsRetryAttempts)
			{
				logger.Debug($"[Service ID: {ID}, Provider: {Provider}] Finding subscription for Sender [{SenderID}], SubscriptionType [{SubscriptionType}], MessageID [{MessageID}], ReferenceType [{referenceType}]");
				var recipients = subscriptionAccessor.SelectSubscribedClients(transaction, SenderID, SubscriptionType, MessageID, referenceType);
				if (recipients != null && recipients.Length > 0)
				{
					return ValidateClients(recipients);
				}
				GetRecipientsWaitForRetry();
				referenceType = GetReferenceType(Provider, jsonBody, RetryCount);
			}

			var fallbackSubscriptionID = string.IsNullOrEmpty(jsonBody)
				? GetFallbackSubscriptionID(requestMessage)
				: GetFallbackSubscriptionID(jsonBody);

			if (!string.IsNullOrEmpty(fallbackSubscriptionID))
			{
				logger.Info($"[Service ID: {ID}, Provider: {Provider}] Fallback ID found - Checking subscrpitions for SenderID [{SenderID}], SubscriptionType [{SubscriptionType}], fallbackSubscriptionID [{fallbackSubscriptionID}]");
				var recipients = subscriptionAccessor.SelectSubscribedClients(transaction, SenderID, SubscriptionType, fallbackSubscriptionID, referenceType);

				if((recipients == null || recipients.Length == 0) && !string.IsNullOrEmpty(SecondarySubscriptionType))
				{
					logger.Info($"[Service ID: {ID}, Provider: {Provider}] Fallback ID found - Checking subscrpitions for SenderID [{SenderID}], SubscriptionType [{SecondarySubscriptionType}], fallbackSubscriptionID [{fallbackSubscriptionID}]");
					recipients = subscriptionAccessor.SelectSubscribedClients(transaction, SenderID, SecondarySubscriptionType, fallbackSubscriptionID, referenceType);
				}

				if (recipients == null || recipients.Length == 0)
				{
					logger.Info($"[Service ID: {ID}, Provider: {Provider}] No subscription found, trying to derive client from FallbackSubscriptionID [{fallbackSubscriptionID}]");
					if (fallbackSubscriptionID.Length >= 9)
					{
						var derivedRecipient = fallbackSubscriptionID.Substring(0, 9);
						if (partyAccessor.ClientExists(derivedRecipient))
						{
							recipients = new string[] {derivedRecipient};
						}
					}
				}

				if (recipients != null && recipients.Length > 0)
				{
					if (this is CSPNotificationHandler || this is TransportAcknowledgementHandler)
					{
						CspId = MessageID;
						MessageID = fallbackSubscriptionID;
					}
					return ValidateClients(recipients);
				}

				if (this is BusinessNotificationHandler && !Regex.IsMatch(fallbackSubscriptionID, @"^(?!.*-)[A-Z0-9]{9}[0-9].*$"))
				{
					logger.Warn($"[Service ID: {ID}, Provider: {Provider}] LRN [{fallbackSubscriptionID}] does not appear to have originated from Wisetech Global, comsuming the message to stop spam messages from provider");
					return new string[] { "GBCustoms" };
				}
			}
			else
            {
				logger.Info($"[Service ID: {ID}, Provider: {Provider}] No Fallback ID found");
			}

			if ((Provider == ProviderType.CNS && this is TransportAcknowledgementHandler)
				|| (Provider == ProviderType.EMCS && this is TransportLayerHandler))
			{
				logger.Info($"[Service ID: {ID}, Provider: {Provider}] No subcription found, but accepting the message anyway");
				return new string[] { "GBCustoms" };
			}

			throw new HttpException(465, $"No subscriber found for {MessageIDKey}: [{MessageID}]");
		}

		protected virtual string GetFallbackSubscriptionID(XDocument requestMessage) => null;
		protected virtual string GetFallbackSubscriptionID(string jsonBody) => null;
		protected virtual string GetReferenceType(ProviderType providerType, string jsonBody, int retryCount = 0) => null;

		protected internal virtual void GetRecipientsWaitForRetry()
		{
			RetryCount++;
			if (RetryCount <= GetRecipientsRetryAttempts)
			{
				logger.Info($"No subscriber found for {MessageIDKey}: [{MessageID}] - Waiting for {GetRecipientsRetryWaitTime} seconds for retry - Attempt {RetryCount} of {GetRecipientsRetryAttempts}");
				Thread.Sleep(GetRecipientsRetryWaitTime * 1000);
			}
		}

		protected internal int GetSettingsInt(string key)
		{
			var keyValue = GetSettings(key);
			if (!int.TryParse(keyValue, out int i))
			{
				throw new ConfigurationErrorsException($"Non-integer value in AppSettings for {key}");
			}
			return i;
		}

		void SetUpAccessors()
		{
			subscriptionAccessor = SubscriptionAccessorFactory();
			inboxAccessor = InboxAccessorFactory();
			clientRegistrationAccessor = ClientRegistrationAccessorFactory();
			partyAccessor = PartyAccessorFactory();
		}

		protected internal virtual string GetSettings(string key)
		{
			var output = ConfigurationManager.AppSettings[$"{key}-{Provider}"];
			if (string.IsNullOrWhiteSpace(output))
			{
				output = ConfigurationManager.AppSettings[key];
				if (string.IsNullOrWhiteSpace(output))
				{
					throw new ConfigurationErrorsException($"No AppSettings could be found for {key}");
				}
			}
			return output;
		}

		protected string FindValueOrDefaultFromNotificationHeader(XDocument requestMessage, params string[] keys)
		{
			var xPath = "/notifications/notification/headers/header";
			if (keys.Length == 0)
			{
				return string.Empty;
			}

			foreach (var key in keys)
			{
				if (!string.IsNullOrEmpty(key))
				{
					var headerValue = requestMessage.XPathSelectElement($"{xPath}[@name='{key}']")?.Attribute("value")?.Value;
					if (headerValue != null)
					{
						return headerValue;
					}
				}
			}

			return string.Empty;
		}

		protected internal bool TryDecodeBase64(string base64String, out string decodedString)
        {
			decodedString = null;
			try
			{
				decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
				return true;
			}
			catch { return false; }
		}

		private string[] ValidateClients(string[] clients)
		{
			var validClients = new List<string>(clients);
			foreach (var client in clients)
			{
				if (!partyAccessor.IsPermitInboxRecipient(client))
				{
					validClients.Remove(client);
					logger.Warn($"[Service ID: {ID}, Provider: {Provider}] ClientID [{client}] is a recipient for this message, but is not a valid CW recipient");
				}
			}

			return (validClients.Count == 0)
				? new string[] { "GBCustoms" }
				: validClients.ToArray();
		}
		#endregion

		#region Fields
		protected ILog logger;
		protected IInboxAccessor inboxAccessor;
		protected ISubscriptionAccessor subscriptionAccessor;
		protected IPartyAccessor partyAccessor;
		protected Guid ID;
		protected HttpRequestMessage request;

		IClientRegistrationAccessor clientRegistrationAccessor;

		protected const string eHubConnection = "CargoWise.eHub.DataAccess.Sql.eHubTransactions";
		protected string SenderID;
		protected string RegistrationType;
		protected string SubscriptionType;
		protected string SecondarySubscriptionType;
		protected string[] SubscriptionProviderIDList;
		protected ProviderType Provider { get; }
		protected string MessageID { private set; get; }
		protected string CspId { private set; get; }
		protected virtual string MessageIDKey => "Message ID";
		protected int RetryCount = 0;
		protected internal int GetRecipientsRetryAttempts;
		protected internal int GetRecipientsRetryWaitTime;
		#endregion
		#region Mockable
		protected internal Func<IClientRegistrationAccessor> ClientRegistrationAccessorFactory = () => new ClientRegistrationAccessor();
		protected internal Func<IInboxAccessor> InboxAccessorFactory = () => new InboxAccessor();
		protected internal Func<ISubscriptionAccessor> SubscriptionAccessorFactory = () => new SubscriptionAccessor();
		protected internal Func<IPartyAccessor> PartyAccessorFactory = () => DataAccessFactories.NewPartyAccessorInstance();
		protected internal Func<Guid> InternalGuid = () => Guid.NewGuid();
		protected internal Func<string, SqlConnection> GetConnection = Key =>
		{
			var settings = ConfigurationManager.ConnectionStrings[Key];
			if (string.IsNullOrWhiteSpace(settings.ConnectionString))
			{
				throw new ConfigurationErrorsException($"Requested ConnectionString could not be found for {Key}");
			}
			return new SqlConnection(settings.ConnectionString);
		};

		protected internal virtual string ExtractHeader(string key, bool isContentHeader = false)
		{
			try
			{
				if (isContentHeader) return request.Content.Headers.GetValues(key).FirstOrDefault() ?? string.Empty;
				return request.Headers.GetValues(key).FirstOrDefault() ?? string.Empty;
			}
			catch (InvalidOperationException ex) when (ex.Message.StartsWith("The given header was not found"))
			{
				return string.Empty;
			}
		}

		protected internal virtual string GetHeaderString()
		{
			return request.Headers.ToString();
		}

		private HttpStatusCode GetResponseStatus()
		{
			HttpStatusCode status;
			switch (Provider)
			{
				case ProviderType.CDS:
				case ProviderType.CTCGB:
				case ProviderType.GVMS:
				case ProviderType.EMCS:
					status = HttpStatusCode.OK;
					break;
				default:
					status = HttpStatusCode.Accepted;
					break;
			}
			logger.TraceFormat("GetResponseStatus returned Provider: {0}, Status: {1}, StatusInInteger: {2}", Provider, status, (int)status);
			return status;
		}

		private SecurityValidationType GetSecurityValidationType()
		{
			switch (Provider)
			{
				case ProviderType.CTCGB:
				case ProviderType.GVMS:
				case ProviderType.EMCS:
					return SecurityValidationType.SignatureHeader;
				default:
					return SecurityValidationType.AuthorisationHeader;
			}
		}

		private List<ProviderType> TransportLayerProviders => new List<ProviderType>() { ProviderType.CTCGB, ProviderType.GVMS, ProviderType.EMCS };

		#endregion
	}
}
