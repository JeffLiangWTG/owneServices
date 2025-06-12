using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.Core;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers;
using Common.Logging;

namespace CargoWise.eHub.Products.GBCustoms.MCP.InboundMessageWebService.Controllers
{
	public class MCPNotificationController : ApiController
	{
		protected internal Func<Guid> InternalGuid = () => Guid.NewGuid();
		protected internal Func<ILog> InternalLog = () => LogManager.GetLogger<MCPNotificationController>();
		protected internal ILog logger;
		protected internal HttpRequestMessage InternalRequestMessage => Request;
		protected internal HttpRequest InternalRequest => HttpContext.Current.Request;
		readonly Guid ID;

		public MCPNotificationController()
		{
			logger = InternalLog();
			ID = InternalGuid();
		}

		[HttpPost]
		public HttpResponseMessage ReceiveNotification()
		{
			logger.Info($"[Service ID: {ID}] Received Notification.");
			try
			{
				var requestXml = XDocument.Parse(InternalRequest.InputStream.ReadToEnd());
				logger.Debug($"[Service ID: {ID}] Notification: {requestXml.Root.ToString()}.");

				XDocument bodyXml;
				var handler = GetHandler(requestXml, out bodyXml);
				logger.Info($"[Service ID: {ID}] Message will be processed using {handler.GetName()}.");
				return handler.Process(InternalRequestMessage, logger, ID, requestXml, bodyXml);
			}
			catch (HttpException ex)
			{
				logger.Error($"[Service ID: {ID}] ReceiveNotification threw exception.", ex);
				var inboundError = new InboundError(new eHubTransactionsContext(), logger, "GBCustoms");
				inboundError.CreateFailedMessage(InternalRequestMessage, "GBCustoms MCP Inbound WS - ReceiveNotificationHttpException", ex);
				return InternalRequestMessage.CreateResponse((HttpStatusCode)ex.GetHttpCode(), ex.Message + $"\r\nPlease quote [{ID}] for enquiries.");
			}
			catch (Exception ex)
			{
				logger.Error($"[Service ID: {ID}] ReceiveNotification threw exception", ex);
				var inboundError = new InboundError(new eHubTransactionsContext(), logger, "GBCustoms");
				inboundError.CreateFailedMessage(InternalRequestMessage, "GBCustoms MCP Inbound WS - ReceiveNotificationException", ex);
				return InternalRequestMessage.CreateResponse(HttpStatusCode.InternalServerError, $"An internal error occured during the process. \r\nPlease quote [{ID}] for enquiries.");
			}
		}

		protected IMessageHandler GetHandler(XDocument requestXml, out XDocument bodyXml)
		{
			var body = requestXml.XPathSelectElement("/notifications/notification/body").Value;
			using (Stream bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body)))
			using (var decodedBody = new StreamReader(new MemoryStream(Convert.FromBase64String(bodyStream.ReadToEnd())), Encoding.GetEncoding("Windows-1252")))
			{
				bodyXml = XDocument.Load(decodedBody);
				var notificationType = requestXml.XPathSelectElement("/notifications/notification/headers/header[@name='X-Notification-Type']")?.Attribute("value")?.Value;
				var cspId = requestXml.XPathSelectElement("/notifications/notification/headers/header[@name='X-CSP-ID']")?.Attribute("value")?.Value;
				var conversationId = requestXml.XPathSelectElement("/notifications/notification/headers/header[@name='ConversationID']")?.Attribute("value")?.Value;

				if (!string.IsNullOrWhiteSpace(notificationType))
				{
					switch (notificationType)
					{
						case "API":
							return new TransportAcknowledgementHandler(ProviderType.MCP);
						case "DMS":
						{
							if (!string.IsNullOrWhiteSpace(cspId) && string.IsNullOrWhiteSpace(conversationId))
							{
									return new CSPNotificationHandler(ProviderType.MCP);
								}

							return new BusinessNotificationHandler(ProviderType.MCP);
						}
					}
				}
				var bodyType = bodyXml.Root.Name.ToString();
				switch (bodyType)
				{
					case "{urn:wco:datamodel:WCO:DocumentMetaData-DMS:2}MetaData":
						return new BusinessNotificationHandler(ProviderType.MCP);
					case "response":
						return new TransportAcknowledgementHandler(ProviderType.MCP);
					default:
						throw new HttpException(400, $"There is no handler for notification body of type {bodyType}.");
				}
			}
		}
	}
}
