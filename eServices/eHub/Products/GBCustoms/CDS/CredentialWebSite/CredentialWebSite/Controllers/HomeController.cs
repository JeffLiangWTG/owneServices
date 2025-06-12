using CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.CredentialWebService;
using CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.Models;
using CargoWise.eHub.Shared.IssueManager;
using log4net;
using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Mvc;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.Controllers
{
	public class HomeController : Controller
	{
		protected internal Func<ILog> GetLogger = () => LogManager.GetLogger(typeof(HomeController).Name);
		protected internal ILog logger;
		protected internal string logPrefix;
		protected internal IssueManager issueManager => GetIssueManager();

		private ICredentialWebService credentialServiceClient;
		public ICredentialWebService CredentialServiceClient
		{
			get { return credentialServiceClient ?? new CredentialWebServiceClient(); }
			set { credentialServiceClient = value; }
		}

		public HomeController()
		{
			logger = GetLogger();
			logPrefix = $"[{Guid.NewGuid()}] ";
		}

		public ActionResult Index(string state, string code, string error, string error_description, string error_code)
		{
			var ipAddresses = GetIPAddresses(System.Web.HttpContext.Current);
			var requestURL = Request == null || Request.Url == null ? "REQUEST URL MISSING" : Request.Url.AbsoluteUri;

			if (string.IsNullOrEmpty(state) || string.IsNullOrEmpty(code))
			{
				EmptyRequestLogger.Debug($"{logPrefix}Empty state or code: [State: {state}] [Authorisation: {code}] [IPAddresses: {ipAddresses}] [Request URL: {requestURL}]");
				return View("Error", new ErrorDetails("", "", "", "WTG"));
			}

			ViewBag.ReferenceNumber = Guid.NewGuid().ToString();
			NaturalKey naturalKey = default(NaturalKey);
			try
			{
				naturalKey = new NaturalKey(state);
			}
			catch (ArgumentException ex)
			{
				logger.Error($"{logPrefix}[State: {state}] [Authorisation: {code}] [{ViewBag.ReferenceNumber}] [IPAddresses: {ipAddresses}] [Request URL: {requestURL}] ErrorDescription:{ex.Message}");
				issueManager.ReportToIssueManager("GBCustoms Credential Website - ArgumentException in method Index", ex, logger, ConfigurationManager.AppSettings);
				return View("Error", new ErrorDetails("", ex.Message, "", "WTG"));
			}

			if (error != null)
			{
				var errorFromGBCustoms = new ErrorDetails(error, error_description, error_code, "CDS");

				logger.Info($"{logPrefix}[State: {state}] [Authorisation: {code}] [{ViewBag.ReferenceNumber}] [IPAddresses: {ipAddresses}] [Request URL: {requestURL}] ErrorFromGBCustoms - Error:{errorFromGBCustoms.Error}, ErrorDescription:{errorFromGBCustoms.ErrorDescription}, ErrorCode:{errorFromGBCustoms.ErrorCode}");
				return View("Error", errorFromGBCustoms);
			}

			var response = CallWebServiceToPersist(state, code);

			if (!response.IsSuccess)
			{
				var errorFromWebService = new ErrorDetails("", response.ErrorMessage, "", "WTG");
				logger.Error($"{logPrefix}[State: {state}] [Authorisation: {code}] [{ViewBag.ReferenceNumber}] [IPAddresses: {ipAddresses}] [Request URL: {requestURL}] FailToPersistAuthorisationToken - ErrorDescription:{response.ErrorMessage}");
				return View("Error", errorFromWebService);
			}

			logger.Info($"{logPrefix}[State: {state}] [Authorisation: {code}] [{ViewBag.ReferenceNumber}] [IPAddresses: {ipAddresses}] [Request URL: {requestURL}] SuccessToPersistAuthorisationToken");

			return View("Index", naturalKey);

		}

		public Response CallWebServiceToPersist(string state, string code)
		{
			var ipAddresses = GetIPAddresses(System.Web.HttpContext.Current);
			var requestURL = Request == null || Request.Url == null ? "REQUEST URL MISSING" : Request.Url.AbsoluteUri;
			try
			{
				var originalUri = string.Empty;
				var newUri = string.Empty;

				if (Request?.Url != null)
				{
					var uriDelimiterAuthority = Uri.SchemeDelimiter + Request.Url.Authority;
					originalUri = Request.Url.Scheme + uriDelimiterAuthority;
					newUri = "https" + uriDelimiterAuthority;
					logger.Debug($"{logPrefix}CallWebServiceToPersist - Replaced '{originalUri}' with '{newUri}'.");
				}

				var utcNow = UtcNow;
				logger.Debug($"{logPrefix}CallWebServiceToPersist - PersistAuthorisationToken with parameters: state: '{state}', code: '{code}', uri: '{newUri}', issuedUtc: '{utcNow.ToString("MM/dd/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture)}', IPAddresses: '{ipAddresses}', requestURL: '{requestURL}'.");

				return CredentialServiceClient.PersistAuthorisationToken(state, code, newUri, utcNow);
			}
			catch (WebException ex)
			{
				logger.Error($"{logPrefix}[State: {state}] [Authorisation: {code}]  [IPAddresses: {ipAddresses}] [Request URL: {requestURL}]", ex);
				issueManager.ReportToIssueManager("GBCustoms Credential Website - CallWebServiceToPersistWebException", ex, logger, ConfigurationManager.AppSettings);
				return new Response { IsSuccess = false, ErrorMessage = "Web Issue" };
			}
			catch (CommunicationException ex)
			{
				logger.Error($"{logPrefix}[State: {state}] [Authorisation: {code}]  [IPAddresses: {ipAddresses}] [Request URL: {requestURL}]", ex);
				issueManager.ReportToIssueManager("GBCustoms Credential Website - CallWebServiceToPersistCommunicationException", ex, logger, ConfigurationManager.AppSettings);
				return new Response { IsSuccess = false, ErrorMessage = "Communication Issue" };
			}
		}

		public virtual DateTime UtcNow => DateTime.UtcNow;
		protected internal virtual IssueManager GetIssueManager() => new IssueManager();

		#region EmptyRequestLogger

		static readonly ILog EmptyRequestLogger = LogManager.GetLogger(typeof(HomeController).Name + "_EmptyRequest");

		#endregion

		public static string GetIPAddresses(HttpContext httpContext)
		{
			if (httpContext != null)
			{
				string ipAddresses = httpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
				if (!string.IsNullOrEmpty(ipAddresses) && !ipAddresses.Equals("unknown", StringComparison.OrdinalIgnoreCase))
				{
					return ipAddresses;
				}

				return httpContext.Request.ServerVariables["REMOTE_ADDR"];
			}

			var operationContext = OperationContext.Current;
			var msgProps = operationContext?.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
			if (msgProps != null)
			{
				return msgProps.Address;
			}

			return "<UNKNOWN>";
		}
	}
}
