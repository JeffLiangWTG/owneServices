using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.Core;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers;
using Common.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace CargoWise.eHub.Products.GBCustoms.TL.InboundMessageWebService.Controllers
{
	public class InboundMessageController : ApiController
	{
		protected internal Func<ISubscriptionAccessor> SubscriptionAccessorFactory = () => new SubscriptionAccessor();
		protected internal Func<IInboxAccessor> InboxAccessorFactory = () => new InboxAccessor();
		protected internal Func<IClientRegistrationAccessor> ClientRegistrationAccessorFactory = () => new ClientRegistrationAccessor();
		protected internal Func<eHubTransactionsContext> EhubTransactionsContext = () => new eHubTransactionsContext();
		protected internal Func<Guid> InternalGuid = () => Guid.NewGuid();
		protected internal Func<string, SqlConnection> GetConnection = Key =>
		{
			var settings = ConfigurationManager.ConnectionStrings[Key];
			if (string.IsNullOrWhiteSpace(settings.ConnectionString))
			{
				throw new ConfigurationErrorsException($"Requested ConnectionString could be found for {Key}");
			}
			return new SqlConnection(settings.ConnectionString);
		};
		protected internal Func<ILog> InternalLog = () => LogManager.GetLogger(typeof(InboundMessageController).Name);
		protected internal ILog logger;
		static readonly ILog EmptyRequestLogger = LogManager.GetLogger(typeof(InboundMessageController).Name + "_EmptyRequest");
		protected internal Func<ILog> MessageContentLog = () => LogManager.GetLogger(typeof(InboundMessageController).Name + "_MessageContent");
		protected internal ILog messageContentLogger;
		readonly Guid ID;

		public InboundMessageController()
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
			logger = InternalLog();
			messageContentLogger = MessageContentLog();
			ID = Guid.NewGuid();
		}

		[HttpPost]
		[Route("Receive")]
		public HttpResponseMessage ReceivePost(string provider)
		{
			var providerType = provider.ConvertToEnum<ProviderType>();
			logger.Info($"[Service ID: {ID}] Received ResponseTo{provider}.");
			try
			{
				ValidateProvider(providerType);
				var requestBody = GetRequest().Content.ReadAsStringAsync().Result;
				var requestHeaders = GetRequest().Headers.ToString();
				messageContentLogger.Debug($"[Service ID: {ID}] ResponseTo{provider} content: {requestBody} headers: {requestHeaders}");
				return ProcessRequest(providerType, requestBody);
			}
			catch (HttpException ex)
			{
				logger.Error($"[Service ID: {ID}] ResponseTo{provider} threw exception.", ex);
				var inboundError = new InboundError(EhubTransactionsContext(), logger, "GBCustoms");
				inboundError.CreateFailedMessage(GetRequest(), "GBCustoms TL Inbound WS - ReceiveHttpException", ex);
				return GetRequest().CreateResponse<string>((HttpStatusCode)ex.GetHttpCode(), ex.Message + $"\r\nPlease quote [{ID}] for enquiries.");
			}
			catch (Exception ex)
			{
				logger.Error($"[Service ID: {ID}] ResponseTo{provider} threw exception", ex);
				var inboundError = new InboundError(EhubTransactionsContext(), logger, "GBCustoms");
				inboundError.CreateFailedMessage(GetRequest(), "GBCustoms TL Inbound WS - ReceiveException", ex);
				return GetRequest().CreateResponse<string>(HttpStatusCode.InternalServerError, ex.Message + $"\r\nPlease quote [{ID}] for enquiries.");
			}
		}

		[HttpGet]
		[Route("Receive")]
		public HttpResponseMessage ReceiveGet(string challenge)
		{
			if (string.IsNullOrEmpty(challenge))
				return new HttpResponseMessage(HttpStatusCode.BadRequest);
			return GetRequest().CreateResponse<JObject>(HttpStatusCode.OK, GetReceiveJsonString(challenge));
		}

		protected internal virtual void ValidateProvider(ProviderType providerType)
		{
			if (ValidProviders.Where(x => x.Equals(providerType)).Count() == 0)
			{
				throw new HttpException(400, $"The Provider Type [{providerType.ConvertToString()}] is not valid for this web service.");
			}
		}

		protected internal virtual JObject GetReceiveJsonString(string challenge) => new JObject(new JProperty("challenge", challenge));

		HttpResponseMessage ProcessRequest(ProviderType providerType, string responseBody)
		{
			var handler = GetHandler(providerType);
			return handler.Process(GetRequest(), logger, ID, null, null, true, responseBody);
		}

		protected internal virtual IMessageHandler GetHandler(ProviderType providerType) => new TransportLayerHandler(providerType);

		private List<ProviderType> ValidProviders => new List<ProviderType>() { ProviderType.CTCGB, ProviderType.GVMS, ProviderType.EMCS };

		public virtual HttpRequestMessage GetRequest()
		{
			return Request;
		}
	}
}
