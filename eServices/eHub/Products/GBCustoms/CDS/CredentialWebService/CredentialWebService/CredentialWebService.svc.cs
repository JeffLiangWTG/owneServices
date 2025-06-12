using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Shared.IssueManager;
using Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService
{
	public class CredentialWebService : ICredentialWebService
	{
		protected WebClient GBCustomsTest;
		protected WebClient GBCustoms;
		protected string ClientSecretTest;
		protected string ClientIDTest;
		protected Uri TokenAPIURI;
		protected int RefreshAccessTokenMaxRetryCount;
		protected int RefreshAccessTokenRetryInterval;
		protected int GetAccessTokenMaxRetryCount;
		protected int GetAccessTokenRetryInterval;
		protected string ClientSecret;
		protected string ClientID;
		protected ILog logger;
		protected internal IssueManager issueManager;
		protected int AuthorisationTokenExpirationMinute;
		protected Func<IConfigurationHelper> InternalConfigurationHelper;
		protected string logPrefix;

		public CredentialWebService()
		{
			Func<string, string> GetSettings = Key =>
			{
				var output = ConfigurationManager.AppSettings[Key];
				if (string.IsNullOrWhiteSpace(output))
				{
					throw new ConfigurationErrorsException($"No AppSettings could be found for {Key}");
				}
				return output;
			};
			logger = InternalLogger();
			logPrefix = $"[{Guid.NewGuid()}] ";
			issueManager = GetIssueManager();
			Uri.TryCreate(GetSettings("GBCustomsTokenAPIURI"), UriKind.Relative, out TokenAPIURI);
			int.TryParse(GetSettings("GBCustomsRefreshAccessTokenMaxRetryCount"), out RefreshAccessTokenMaxRetryCount);
			int.TryParse(GetSettings("GBCustomsRefreshAccessTokenRetryInterval"), out RefreshAccessTokenRetryInterval);
			int.TryParse(GetSettings("GBCustomsGetAccessTokenMaxRetryCount"), out GetAccessTokenMaxRetryCount);
			int.TryParse(GetSettings("GBCustomsGetAccessTokenRetryInterval"), out GetAccessTokenRetryInterval);
			int.TryParse(GetSettings("AuthorisationTokenExpirationMinute"), out AuthorisationTokenExpirationMinute);
			ClientID = GetSettings("WiseTechClientID");
			ClientSecret = GetSettings("WiseTechSecret");
			GBCustoms = new WebClient();
			GBCustoms.BaseAddress = GetSettings("GBCustomsBaseURI");
			InternalConfigurationHelper = () => new ConfigurationHelper(logger);

			GBCustomsTest = new WebClient();
			GBCustomsTest.BaseAddress = GetSettings("GBCustomsBaseURITest");
			ClientSecretTest = GetSettings("WiseTechSecretTest");
			ClientIDTest = GetSettings("WiseTechClientIDTest");

			logger.Info($"{logPrefix}Service initialized.");
		}

		public Response PersistAuthorisationToken(string naturalKey, string authorisationToken, string uri, DateTime issuedUtc)
		{
			try
			{
				var helper = InternalConfigurationHelper();
				var authorisationTokenAlreadyExist = helper.DoesAuthorisationTokenAlreadyExist(naturalKey);
				if (authorisationTokenAlreadyExist)
				{
					helper.NotifyCW1(naturalKey);
					logger.Info($"{logPrefix}PersistAuthorisationToken [Key: {naturalKey}] [Authorisation: {authorisationToken}]: Notify CW1");
				}
				var expiryUtc = issuedUtc.AddMinutes(AuthorisationTokenExpirationMinute);
				helper.AddOrUpdateAuthorisationToken(naturalKey, authorisationToken, uri, issuedUtc, expiryUtc);
				logger.Info($"{logPrefix}PersistAuthorisationToken [Key: {naturalKey}] [Authorisation: {authorisationToken}]: AddOrUpdateAuthorisationToken. uri = {uri}, issuedUtc = {issuedUtc}, expiryUtc = {expiryUtc}");

				logger.Info($"{logPrefix}PersistAuthorisationToken [Key: {naturalKey}] [Authorisation: {authorisationToken}]: Succeeded");
				return Response.Success;
			}
			catch (Exception ex)
			{
				logger.Error($"{logPrefix}PersistAuthorisationToken [Key: {naturalKey}] [Authorisation: {authorisationToken}]", ex);
				issueManager.ReportToIssueManager("GBCustoms CredentialWebService - PersistAuthorisationTokenException", ex, logger, ConfigurationManager.AppSettings);
				return new Response(false, ex.Message);
			}
		}

		public Response RefreshAccessToken(string accessToken, string refreshToken, string naturalKey)
		{
			logger.Info($"{logPrefix}Processing request to [RefreshAccessKey] for [{naturalKey}]. AccessToken: [{accessToken}], RefreshToken: [{refreshToken}].");

			return RequestCore(new NameValueCollection()
			{
				{ "grant_type", "refresh_token" },
				{ "refresh_token", refreshToken }
			},
			naturalKey,
			RequestType.RefreshAccessKey,
			RefreshAccessTokenMaxRetryCount,
			RefreshAccessTokenRetryInterval,
			accessToken);
		}

		public Response RequestAccessToken(string authorisationToken, string redirect_uri, string naturalKey)
		{
			logger.Info($"{logPrefix}Processing request to [RequestAccessKey] for [{naturalKey}]. AuthorisationToken: [{authorisationToken}], RedirectURI: [{redirect_uri}].");
			return RequestCore(new NameValueCollection()
			{
				{ "grant_type", "authorization_code" },
				{ "redirect_uri", redirect_uri },
				{ "code", authorisationToken }
			},
			naturalKey,
			RequestType.RequestAccessKey,
			GetAccessTokenMaxRetryCount,
			GetAccessTokenRetryInterval);
		}

		public Response RequestAppWideAccessToken(string accessToken, string naturalKey, string scope)
		{
			logger.Info($"{logPrefix}Processing request to [RequestAppWideAccessKey] for [{naturalKey}] with [Scope] [{scope}].");
			return RequestCore(new NameValueCollection()
			{
				{ "grant_type", "client_credentials" },
				{ "scope", scope },
			},
			naturalKey,
			RequestType.RequestAppWideAccessKey,
			GetAccessTokenMaxRetryCount,
			GetAccessTokenRetryInterval,
			accessToken);
		}

		protected GBCustomsResponse GetResponse(byte[] data)
		{
			return JsonConvert.DeserializeObject<GBCustomsResponse>(Encoding.UTF8.GetString(data));
		}

		protected GBCustomsResponse GetResponse(Stream data)
		{
			return JsonConvert.DeserializeObject<GBCustomsResponse>(data.ReadToEnd());
		}

		private Response RequestCore(NameValueCollection parameters, string naturalKey, RequestType requestType, int MaxRetryCount, int RetryInterval, string currentAccessToken = "")
		{
			var retryCount = 0;
			var errorDescription = string.Empty;
			var clientID = string.Empty;
			Exception credentialException;
			do
			{
				try
				{
					var helper = InternalConfigurationHelper();
					try
					{
						clientID = helper.FindRecipient(naturalKey);
						var isProd = helper.IsLicenceProduction(naturalKey);

						if (requestType != RequestType.RequestAppWideAccessKey)
						{
							var tokenAlreadyRefreshed = helper.IsTokenRefreshed(naturalKey, currentAccessToken, 10);
							if (tokenAlreadyRefreshed)
							{
								helper.UpdateAuthorisationTokenStatus(naturalKey, 2);
								helper.ProcessResponse(naturalKey, requestType, new GBCustomsResponse(), clientID);
								logger.Info($"{logPrefix}[{requestType.GetName()} for {naturalKey}] : CW1 notified.");
								return HandleConcurrencyException(new ArgumentException("The access token has changed and is no longer the latest in our database. This is a concurrency issue resolution and no further action is required"),
									requestType, naturalKey);
							}
						}

						if (retryCount == 0)
						{
							parameters.Add("client_secret", isProd ? ClientSecret : ClientSecretTest);
							parameters.Add("client_id", isProd ? ClientID : ClientIDTest);
						}
							
						var webClient = isProd ? GBCustoms : GBCustomsTest;

						var url = webClient.BaseAddress + TokenAPIURI;
						var parameterString = string.Join(",", parameters.AllKeys.Select(key => key + "->" + parameters[key]));
						logger.Info($"{logPrefix}IsProd: {isProd}. POSTing '{url}' with parameters: {parameterString}");

						var response = GetResponse(webClient.UploadValues(TokenAPIURI, "POST", parameters));
						if (requestType == RequestType.RequestAppWideAccessKey)
						{
							response.refresh_token = "app-wide-token";
						}

						logger.Info($"{logPrefix}[{requestType.GetName()} for {naturalKey}] : Response recieved from GBCustoms. [{response.ToString()}]");
						var latestAccessToken = helper.GetAccessToken(naturalKey);
						if (requestType == RequestType.RequestAccessKey || requestType == RequestType.RequestAppWideAccessKey || currentAccessToken == latestAccessToken)
						{
							helper.AddOrUpdateAccessToken(naturalKey, response);
							logger.Info($"{logPrefix}[{requestType.GetName()} for {naturalKey}] : AccessToken {requestType.GetAction()}.");
							helper.ProcessResponse(naturalKey, requestType, response, clientID);
							logger.Info($"{logPrefix}[{requestType.GetName()} for {naturalKey}] : CW1 notified.");
							if (requestType == RequestType.RequestAccessKey)
							{
								helper.UpdateAuthorisationTokenStatus(naturalKey, 2);
								logger.Info($"{logPrefix}[{requestType.GetName()} for {naturalKey}] : AuthorisationToken status updated to 2.");
							}
						}
						else
						{
							logger.Info($"{logPrefix}[{requestType.GetName()} for {naturalKey}] AccessToken has already been updated. Skipping the update with values: {response.ToString()}. Previous AccessToken: [{currentAccessToken}], current AccessToken: [{latestAccessToken}]");
						}

						logger.Info($"{logPrefix}Completed processing {requestType.GetName()} for {naturalKey}");
						return Response.Success;
					}
					catch (ArgumentNullException ex)
					{
						return HandleInternalException(ex, helper, requestType, naturalKey, clientID);
					}
					catch (ArgumentException ex)
					{
						return HandleInternalException(ex, helper, requestType, naturalKey, clientID);
					}
					catch (WebException ex) when (ExpectedErrors.Contains(GetResponse(ex.Response.GetResponseStream()).error_description))
					{
						return HandleExpectedWebException(ex, helper, requestType, naturalKey, clientID);
					}
				}
				catch (Exception ex)
				{
					errorDescription = ex.Message;
					if (retryCount < MaxRetryCount) Thread.Sleep(RetryInterval);
					retryCount++;
					logger.Error($"{logPrefix}[{requestType.GetName()} for {naturalKey}] threw exception with retry. Try No: {retryCount}. Skip notifying CW1.", ex);
					credentialException = ex;
				}
			}
			while (retryCount <= MaxRetryCount);

			try
			{
				var helper = InternalConfigurationHelper();
				helper.ProcessException(naturalKey, $"Max retry reached. {errorDescription}", requestType, clientID);
				logger.Debug($"{logPrefix}[{requestType.GetName()} for {naturalKey}] Notified CW1 of reaching max retry.");
				issueManager.ReportToIssueManager("GBCustoms CredentialWebService - RequestCoreException", credentialException ?? new Exception("Unknown Credential WebService Exception"), logger, ConfigurationManager.AppSettings);
			}
			catch (Exception)
			{
				logger.Error($"{logPrefix} [{requestType.GetName()} for {naturalKey}] Exception was thrown notifying CW1 after max retry reached.");
				issueManager.ReportToIssueManager("GBCustoms CredentialWebService - RequestCoreMaxRetriesException", new Exception($"[{ requestType.GetName() } for { naturalKey}] Exception was thrown notifying CW1 after max retry reached."), logger, ConfigurationManager.AppSettings);
			}
			logger.Debug($"{logPrefix} [{requestType.GetName()} for {naturalKey}] Max Retry Reached.");
			return Response.Success;
		}

		Response HandleInternalException(Exception ex, IConfigurationHelper helper, RequestType requestType, string naturalKey, string clientID)
		{
			if (string.IsNullOrEmpty(clientID))
            {
				logger.Error($"{logPrefix}[{requestType.GetName()} for {naturalKey}] : No active client is found with key {naturalKey}.");
				issueManager.ReportToIssueManager("GBCustoms CredentialWebService - RequestCoreException", ex, logger, ConfigurationManager.AppSettings);
				return new Response(false, ex.Message);
			}

			logger.Error($"{logPrefix}[{requestType.GetName()} for {naturalKey}] threw exception with no retry.", ex);
			helper.ProcessException(naturalKey, "[CredentialWebService] InternalError", requestType, clientID);
			logger.Debug($"{logPrefix} [{requestType.GetName()} for {naturalKey}] : CW1 notified of internal error.");
			return Response.Success;
		}

		Response HandleExpectedWebException(WebException ex, IConfigurationHelper helper, RequestType requestType, string naturalKey, string clientID)
		{
			var description = GetResponse(ex.Response.GetResponseStream()).error_description;
			logger.Error($"{logPrefix}[{requestType.GetName()} for {naturalKey}] returned error.{Environment.NewLine}{description}", ex);
			helper.ProcessException(naturalKey, description, requestType, clientID);
			logger.Debug($"{logPrefix}[{requestType.GetName()} for {naturalKey}] : CW1 notified of error response.");
			return Response.Success;
		}

		Response HandleConcurrencyException(Exception ex, RequestType requestType, string naturalKey)
		{
			logger.Error($"{logPrefix}[{requestType.GetName()} for {naturalKey}] had the following problem: {ex.Message}.", ex);
			return Response.Success;
		}

		protected internal Func<ILog> InternalLogger = () => LogManager.GetLogger<CredentialWebService>();
		protected internal virtual IssueManager GetIssueManager() => new IssueManager();
		protected internal static IList<string> ExpectedErrors = new List<string>
		{
			"client_id is required",
			"client_secret is required",
			"redirect_uri is required",
			"grant_type is required",
			"code is required for given grant_type",
			"invalid client id or secret",
			"code is invalid",
			"refresh_token is required for given grant_type",
			"refresh_token is invalid",
			"redirect_uri is invalid"
		};
	}
}
