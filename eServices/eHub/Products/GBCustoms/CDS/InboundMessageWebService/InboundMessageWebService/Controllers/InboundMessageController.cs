using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataAccess.Sql;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.Core;
using CargoWise.eHub.Products.GBCustoms.Core.BT.Schemas.declaration;
using CargoWise.eHub.Products.GBCustoms.Core.BT.Schemas.notification;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers;
using Common.Logging;

namespace CargoWise.eHub.Products.GBCustoms.CDS.InboundMessageWebService.Controllers
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
		XmlSchemaSet schemaSet = null;
		protected internal bool SchemaValidation;

		XmlSchemaSet SchemaSet
		{
			get
			{
				if (schemaSet == null)
				{
					schemaSet = new Core.BT.Schemas.notification.DocumentMetaData_2_DMS().SchemaSet;
					schemaSet.Add(new WCO_DEC_2_DMS().SchemaSet);
					schemaSet.Add(new WCO_RES_2_DMS().SchemaSet);
				}
				return schemaSet;
			}
		}

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
			bool.TryParse(GetSettings("SchemaValidation"), out SchemaValidation);
		}

		[HttpPost]
		public HttpResponseMessage ReceiveResponseToDeclarations()
		{
			logger.Info($"[Service ID: {ID}] Received ResponseToDeclaration.");
			try
			{
				var requestBody = GetRequest().Content.ReadAsStringAsync().Result;
				messageContentLogger.Debug($"[Service ID: {ID}] ResponseToDeclaration content: {requestBody}");
				ValidateBody(requestBody);
				return ProcessRequest(requestBody);
			}
			catch (HttpException ex)
			{
				logger.Error($"[Service ID: {ID}] ResponseToDeclaration threw exception.", ex);
				var inboundError = new InboundError(EhubTransactionsContext(), logger, "GBCustoms");
				inboundError.CreateFailedMessage(GetRequest(), "GBCustoms CDS Inbound WS - ResponseToDeclarationHttpException", ex);
				return GetRequest().CreateResponse<string>((HttpStatusCode)ex.GetHttpCode(), ex.Message + $"\r\nPlease quote [{ID}] for enquiries.");
			}
			catch (Exception ex)
			{
				logger.Error($"[Service ID: {ID}] ResponseToDeclaration threw exception", ex);
				var inboundError = new InboundError(EhubTransactionsContext(), logger, "GBCustoms");
				inboundError.CreateFailedMessage(GetRequest(), "GBCustoms CDS Inbound WS - ResponseToDeclarationException", ex);
				return GetRequest().CreateResponse<string>(HttpStatusCode.InternalServerError, ex.Message + $"\r\nPlease quote [{ID}] for enquiries.");
			}
		}

		[HttpPost]
		public HttpResponseMessage ReceiveResponseToInventoryRequests()
		{
			logger.Info($"[Service ID: {ID}] Received ResponseToInventoryRequest.");
			try
			{
				var requestBody = GetRequest().Content.ReadAsStringAsync().Result;
				messageContentLogger.Debug($"[Service ID: {ID}] ResponseToInventory content: {requestBody}");
				ValidateBody(requestBody);
				return ProcessRequest(requestBody);
			}
			catch (HttpException ex)
			{
				logger.Error($"[Service ID: {ID}] ResponseToInventoryRequest threw exception.", ex);
				var inboundError = new InboundError(EhubTransactionsContext(), logger, "GBCustoms");
				inboundError.CreateFailedMessage(GetRequest(), "GBCustoms CDS Inbound WS - ResponseToInventoryRequestHttpException", ex);
				return GetRequest().CreateResponse<string>((HttpStatusCode)ex.GetHttpCode(), ex.Message + $"\r\nPlease quote [{ID}] for enquiries.");
			}
			catch (Exception ex)
			{
				logger.Error($"[Service ID: {ID}] ResponseToInventoryRequest threw exception.", ex);
				var inboundError = new InboundError(EhubTransactionsContext(), logger, "GBCustoms");
				inboundError.CreateFailedMessage(GetRequest(), "GBCustoms CDS Inbound WS - ResponseToInventoryRequestException", ex);
				return GetRequest().CreateResponse<string>(HttpStatusCode.InternalServerError, ex.Message + $"\r\nPlease quote [{ID}] for enquiries.");
			}
		}

		void ValidateBody(string body)
		{
			XDocument requestXml;
			try
			{
				requestXml = XDocument.Parse(body);
			}
			catch
			{
				throw new HttpException(400, "Message is not valid XML.");
			}
			if (SchemaValidation)
			{
				requestXml.Validate(SchemaSet, (sender, e) =>
				{
					throw new HttpException(400, "Invalid Message.");
				}, true);
				if (requestXml.Root.GetSchemaInfo().Validity != XmlSchemaValidity.Valid)
				{
					throw new HttpException(400, "Invalid Message.");
				}
			}
		}

		HttpResponseMessage ProcessRequest(string response)
		{
			var handler = GetHandler();
			return handler.Process(GetRequest(), logger, ID, XDocument.Parse(response));
		}

		protected internal virtual IMessageHandler GetHandler() => new BusinessNotificationHandler(ProviderType.CDS);


		protected internal virtual HttpRequestMessage GetRequest()
		{
			return Request;
		}
	}
}
