using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using eServices.Configuration.Schemas;
using CargoWise.eHub.DataAccess.Integration;
using eServices.eHubDataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.Core.ConfigurationHandler;
using Common.Logging;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService
{
	public class ConfigurationHelper : IConfigurationHelper
	{
		protected GBCustomsConfigurationHandler handler;
		protected ILog logger;
		protected const string configurationMessageType = "http://www.wisetechglobal.com/Schemas/Configuration#Configuration";
		public Func<eHubTransactionsContext> ContextFactory = () => new eHubTransactionsContext();

		public ConfigurationHelper(ILog logger)
			: this(new GBCustomsConfigurationHandler(), logger)
		{
		}

		public ConfigurationHelper(GBCustomsConfigurationHandler handler, ILog logger)
		{
			this.handler = handler;
			this.logger = logger;
		}

		public virtual string FindRecipient(string naturalKey)
		{
			var key = naturalKey.Split('.')[0];

            if (AppWideSystems.Contains(key))
            {
				return string.Empty;
            }

            if (MemoryCache.Default.Contains(key))
			{
				return MemoryCache.Default.Get(key) as string;
			}

			var enterpriseCode = key.Substring(0, 3);
			var serverCode = key.Substring(3, 3);
			var recipient = "";

			if (!IsWisetechInternalSystem(enterpriseCode))
			{
				try
				{
					using (var connection = eHubTransactionsSecondary)
					{
						connection.Open();
						using (var comm = connection.CreateCommand())
						{
							comm.CommandType = CommandType.StoredProcedure;
							comm.CommandText = @"[dbo].[SelectFirstActiveeHubClientPerSystem]";
							comm.Parameters.Add(new SqlParameter("@enterpriseCode", enterpriseCode));
							comm.Parameters.Add(new SqlParameter("@serverCode", serverCode));

							comm.CommandTimeout = 60;

							recipient = (string)comm.ExecuteScalar();
						}
					}
				}
				catch (Exception ex)
				{
					logger.Warn($"FindRecipient failed. Exception: {ex.Message}  ");
				}
			}
			else
			{
				recipient = FindEHubClient(key);
			}

			if (string.IsNullOrWhiteSpace(recipient))
			{
				throw new ArgumentException($"A recipient for the client system '{key}' cannot be found.");
			}

			MemoryCache.Default.Set(key, recipient, Policy);
			return recipient;
		}

		protected void InsertToInboxAndOutbox(Stream content, string clientID)
		{
			NewInboxAccessor.InsertToInboxAndOutbox(
"eHub",
Guid.Empty,
new eHubGatewayMessage()
{
	MessageTrackingID = InternalNewGuid(),
	ClientID = clientID,
	ApplicationCode = "HUB",
	SchemaName = configurationMessageType,
	SchemaType = MessageSchemaType.Xml,
	EmailSubject = string.Empty,
	FileName = string.Empty,
	MessageStream = content.CompressAndEncode()
});
		}

		public void AddOrUpdateAccessToken(string naturalKey, GBCustomsResponse response)
		{
			var issuedUTC = InternalUtcNow();
			handler.AddOrUpdateAccessToken(naturalKey, response.access_token, response.refresh_token, issuedUTC, issuedUTC.AddSeconds(response.expires_in), logger);
		}

		public bool IsLicenceProduction(string naturalKey)
		{
			var aNaturalKey = new NaturalKey(naturalKey);

			if(AppWideSystems.Contains(aNaturalKey.EnterpriseDbCode))
			{
				return aNaturalKey.EnterpriseDbCode == "GBCPRD";
			}

			var licenceType = NewEnterpriseExeDetailAccessor.GetLicenceType(aNaturalKey.EnterpriseCode + "XXX" + aNaturalKey.DBCode);
			return !string.IsNullOrEmpty((licenceType)) && licenceType == "PRD";
		}

		public bool DoesAuthorisationTokenAlreadyExist(string naturalKey)
		{
			var state = new NaturalKey(naturalKey);

			using (var context = ContextFactory())
			{
				return context.eHubClientSystemRegistrations
				.Any(r => r.eHubClientSystem.EH_ID == state.EnterpriseDbCode && r.CD_Code == state.EoriBadge && r.eHubRegistrationType.RT_ID == "GBCustomsAuthorisationToken");
			}
		}

		public void AddOrUpdateAuthorisationToken(string naturalKey, string authorisationToken, string uri, DateTime issuedUtc, DateTime expiryUtc)
		{
			handler.AddOrUpdateAuthorisationToken(naturalKey, authorisationToken, uri, issuedUtc, expiryUtc);
		}

		public void NotifyCW1(string naturalKey)
		{
			var clientID = FindRecipient(naturalKey);
			var message = handler.GetConfigurationMessageWhenNewAuthorisationTokenIsReceived(naturalKey);

			InsertToInboxAndOutbox(ReadConfigurationMessage(message), clientID);
		}

		public virtual void ProcessResponse(string naturalKey, RequestType requestType, GBCustomsResponse response, string clientID)
		{
			ConfigurationMessage message = null;
			switch (requestType)
			{
				case RequestType.RequestAccessKey:
					{
						logger.Debug("GetConfigurationMessageWhenAccessTokenIsReceivedViaAuthorisationToken started.");
						message = handler.GetConfigurationMessageWhenAccessTokenIsReceivedViaAuthorisationToken(naturalKey, logger);
						logger.Debug("GetConfigurationMessageWhenAccessTokenIsReceivedViaAuthorisationToken finished.");
						break;
					}
				case RequestType.RefreshAccessKey:
					{
						logger.Debug("GetConfigurationMessageWhenAccessTokenIsRefreshed started.");
						message = handler.GetConfigurationMessageWhenAccessTokenIsRefreshed(naturalKey, logger);
						logger.Debug("GetConfigurationMessageWhenAccessTokenIsRefreshed finished.");
						break;
					}
			}

			if (requestType != RequestType.RequestAppWideAccessKey)
			{
				logger.Debug("ReadConfigurationMessage started.");
				var configMessage = ReadConfigurationMessage(message);
				logger.Debug("ReadConfigurationMessage finished.");

				logger.Debug("InsertToInboxAndOutbox started.");
				InsertToInboxAndOutbox(configMessage, clientID);
				logger.Debug("InsertToInboxAndOutbox finished.");
			}
		}

		public virtual void ProcessException(string naturalKey, string errorDescription, RequestType requestType, string clientID)
		{
			if (string.IsNullOrWhiteSpace(clientID))
			{
				clientID = FindRecipient(naturalKey);
			}
			ConfigurationMessage message = handler.GetConfigurationMessageWhenAccessTokenIsFailedToBeObtained(naturalKey, errorDescription);
			InsertToInboxAndOutbox(ReadConfigurationMessage(message), clientID);
			if (requestType == RequestType.RequestAccessKey)
			{
				UpdateAuthorisationTokenStatus(naturalKey, 255);
				logger.Info($"[{requestType.GetName()} for {naturalKey}] : AuthorisationToken status updated to 255.");
			}
			else
			{
				UpdateAccessTokenStatus(naturalKey, 255);
				logger.Info($"[{requestType.GetName()} for {naturalKey}] : AccessToken status updated to 255.");
			}
		}

		protected Stream ReadConfigurationMessage(ConfigurationMessage message)
		{
			return ConfigurationMessage.SerializeToStream(message);
		}

		public string GetAccessToken(string naturalKey)
		{
			var nk = new NaturalKey(naturalKey);

			using (var context = ContextFactory())
			{
				eHubClientSystem clientSystem = context.eHubClientSystems.FirstOrDefault(s => s.EH_ID == nk.EnterpriseDbCode);

				if (clientSystem == null) throw new ArgumentException($"Client System ID '{nk.EnterpriseDbCode}' of '{naturalKey}' cannot be found.", "naturalKey");

				var clientSystemRegistration = context.eHubClientSystemRegistrations.FirstOrDefault(
					r => r.CD_EH == clientSystem.EH_PK
						&& r.CD_Code == nk.EoriBadge
						&& r.eHubRegistrationType.RT_ID == "GBCustomsAccessToken"
					);
				return clientSystemRegistration == null ? string.Empty : clientSystemRegistration.CD_Attr1;
			}
		}

		public void UpdateAccessTokenStatus(string naturalKey, byte status)
		{
			var nk = new NaturalKey(naturalKey);

			using (var context = ContextFactory())
			{
				eHubClientSystem clientSystem = context.eHubClientSystems.FirstOrDefault(s => s.EH_ID == nk.EnterpriseDbCode);

				if (clientSystem == null) throw new ArgumentException($"Client System ID '{nk.EnterpriseDbCode}' of '{naturalKey}' cannot be found.", "naturalKey");

				var clientSystemRegistration = context.eHubClientSystemRegistrations.FirstOrDefault(
					r => r.eHubClientSystem.EH_ID == clientSystem.EH_ID
						&& r.CD_Code == nk.EoriBadge
						&& r.eHubRegistrationType.RT_ID == "GBCustomsAccessToken"
					);

				if (clientSystemRegistration == null)
				{
					throw new ArgumentException($"AccessToken for '{naturalKey}' cannot be found.");
				}
				else if (clientSystemRegistration.CD_Flag1 != status)
				{
					clientSystemRegistration.CD_Flag1 = status;
					context.SaveChanges();
				}
			}
		}

		public void UpdateAuthorisationTokenStatus(string naturalKey, byte status)
		{
			var nk = new NaturalKey(naturalKey);

			using (var context = ContextFactory())
			{
				eHubClientSystem clientSystem = context.eHubClientSystems.FirstOrDefault(s => s.EH_ID == nk.EnterpriseDbCode);

				if (clientSystem == null) throw new ArgumentException($"Client System ID '{nk.EnterpriseDbCode}' of '{naturalKey}' cannot be found.", "naturalKey");

				var clientSystemRegistration = context.eHubClientSystemRegistrations.FirstOrDefault(
					r => r.CD_EH == clientSystem.EH_PK
						&& r.CD_Code == nk.EoriBadge
						&& r.eHubRegistrationType.RT_ID == "GBCustomsAuthorisationToken"
					);

				if (clientSystemRegistration == null)
				{
					throw new ArgumentException($"AuthorisationToken for '{naturalKey}' cannot be found.");
				}
				else if (clientSystemRegistration.CD_Flag1 != status)
				{
					clientSystemRegistration.CD_Flag1 = status;

					try
					{
						context.SaveChanges();
					}
					catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
					{
						Exception raise = dbEx;
						foreach (var validationErrors in dbEx.EntityValidationErrors)
						{
							foreach (var validationError in validationErrors.ValidationErrors)
							{
								string message = string.Format("{0}:{1}",
									validationErrors.Entry.Entity.ToString(),
									validationError.ErrorMessage);
								raise = new InvalidOperationException(message, raise);
							}
						}
						throw raise;
					}
				}
			}
		}

		public bool IsTokenRefreshed(string naturalKey, string currentAccessToken, int refreshedMinutes = 10)
		{
			DateTime issueThresholdDateTime = InternalUtcNow().AddMinutes(refreshedMinutes * -1);
			var nk = new NaturalKey(naturalKey);

			using (var context = ContextFactory())
			{
				eHubClientSystem clientSystem = context.eHubClientSystems.FirstOrDefault(s => s.EH_ID == nk.EnterpriseDbCode);
				if (clientSystem == null) throw new ArgumentException($"Client System ID '{nk.EnterpriseDbCode}' of '{naturalKey}' cannot be found.", "naturalKey");

				return context.eHubClientSystemRegistrations.Any(x => 
					x.CD_Code == nk.EoriBadge
					&& x.CD_EH == clientSystem.EH_PK
					&& !string.IsNullOrEmpty(x.CD_Attr1) && x.CD_Attr1 != currentAccessToken
					&& x.eHubRegistrationType.RT_ID == "GBCustomsAccessToken" && x.CD_Flag1 == 0
					&& x.CD_IssuedUTC.HasValue && x.CD_IssuedUTC > issueThresholdDateTime);
			}
		}

		public virtual SqlConnection GeteHubTransactionsConnection()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["eHubTransactionsContext"].ConnectionString;
			return new SqlConnection(connectionString);
		}

		public virtual IEnterpriseExeDetailAccessor NewEnterpriseExeDetailAccessor
		{
			get { return DataAccessFactories.NewEnterpriseExeDetailAccessorInstance(); }
		}

		public virtual IInboxAccessor NewInboxAccessor
		{
			get { return DataAccessFactories.NewInboxAccessorInstance(); }
		}

		public virtual IDbConnection eHubTransactionsSecondary
		{
			get
			{
				var connectionString = ConfigurationManager.ConnectionStrings["CargoWise.eHub.DataAccess.eHubTransactionsSecondary"].ConnectionString;
				return new SqlConnection(connectionString);
			}
		}

		private bool IsWisetechInternalSystem(string enterpriseCode)
		{
			return WisetechInternalEnterpriseCodes.Contains(enterpriseCode);
		}

		private string FindEHubClient(string key)
		{
			using (var context = ContextFactory())
			{
				return context.eHubClients.Where(
						c => c.CC_ID.Length == 9
							 && c.CC_ID.Remove(3, 3) == key
							 && c.CC_PermitInboxRecipient == true).
					OrderBy(x => x.CC_ID).FirstOrDefault()?.CC_ID;
			}
		}

		internal Func<Guid> InternalNewGuid = Guid.NewGuid;
		internal Func<DateTime> InternalUtcNow = () => DateTime.UtcNow;
		private const string DATA_CACHE_EXPIRY_KEY = "DataCacheExpiry";
		private static readonly CacheItemPolicy Policy = new CacheItemPolicy
		{
			SlidingExpiration = TimeSpan.FromMinutes(Convert.ToDouble(ConfigurationManager.AppSettings[DATA_CACHE_EXPIRY_KEY]))
		};

		private static readonly string[] WisetechInternalEnterpriseCodes = { "EDI", "WTL", "HYE", "WUT", "INZ" };
		private static readonly string[] AppWideSystems = { "GBCTST", "GBCPRD" };
	}
}
