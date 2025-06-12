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

namespace CargoWise.eHub.Products.GBCustoms.CNS.InboundMessageWebService.Controllers
{
	public class CNSNotificationController : ApiController
	{
		protected internal Func<Guid> InternalGuid = () => Guid.NewGuid();
		protected internal Func<ILog> InternalLog = () => LogManager.GetLogger<CNSNotificationController>();
		protected internal ILog logger;
		protected internal HttpRequestMessage InternalRequestMessage => Request;
		protected internal HttpRequest InternalRequest => HttpContext.Current.Request;
		readonly Guid ID;
		public const string EmptyBodyApiNotificationSubstitute = "<response><code>202</code><message>Empty body tag received from CSP, it is assumed that this means Accepted</message></response>";

		public CNSNotificationController()
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
				inboundError.CreateFailedMessage(InternalRequestMessage, "GBCustoms CNS Inbound WS - ReceiveNotificationHttpException", ex);
				return InternalRequestMessage.CreateResponse((HttpStatusCode)ex.GetHttpCode(), ex.Message + $"\r\nPlease quote [{ID}] for enquiries.");
			}
			catch (Exception ex)
			{
				logger.Error($"[Service ID: {ID}] ReceiveNotification threw exception", ex);
				var inboundError = new InboundError(new eHubTransactionsContext(), logger, "GBCustoms");
				inboundError.CreateFailedMessage(InternalRequestMessage, "GBCustoms CNS Inbound WS - ReceiveNotificationException", ex);
				return InternalRequestMessage.CreateResponse(HttpStatusCode.InternalServerError, $"An internal error occured during the process. \r\nPlease quote [{ID}] for enquiries.");
			}
		}

		protected IMessageHandler GetHandler(XDocument requestXml, out XDocument bodyXml)
		{
			var body = requestXml.XPathSelectElement("/notifications/notification/body").Value;
			using (Stream bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body)))
			using (Stream decodedBody = new MemoryStream(Convert.FromBase64String(bodyStream.ReadToEnd())))
			{
				bodyXml = XDocument.Load(string.IsNullOrWhiteSpace(body) ? new MemoryStream(Encoding.UTF8.GetBytes(EmptyBodyApiNotificationSubstitute)) : decodedBody);
				var bodyType = bodyXml.Root.Name.ToString();
				var notificationType = requestXml.XPathSelectElement("/notifications/notification/headers/header[@name='X-Notification-Type']")?.Attribute("value")?.Value;
				var cspId = requestXml.XPathSelectElement("/notifications/notification/headers/header[@name='X-CSP-ID']")?.Attribute("value")?.Value;
				var conversationId = requestXml.XPathSelectElement("/notifications/notification/headers/header[@name='ConversationID']")?.Attribute("value")?.Value;
				
				if (!string.IsNullOrWhiteSpace(notificationType))
				{
					switch (notificationType)
					{
						case "API":
							return new TransportAcknowledgementHandler(ProviderType.CNS);
						case "DMS":
						{
							if (string.IsNullOrWhiteSpace(body)) throw new InvalidDataException("An empty body element is not allowed for DMS and CSP notifications");

							if (!string.IsNullOrWhiteSpace(cspId) && string.IsNullOrWhiteSpace(conversationId))
							{
								return new CSPNotificationHandler(ProviderType.CNS);
							}

							return new BusinessNotificationHandler(ProviderType.CNS);
						}
						case "CILE":
						{
							return new BusinessNotificationHandler(ProviderType.CNS);

						}
					}
				}
				switch (bodyType)
				{
					case "{urn:wco:datamodel:WCO:DocumentMetaData-DMS:2}MetaData":
					case "{http://gov.uk/customs/inventoryLinking/v1}inventoryLinkingControlResponse":
						return new BusinessNotificationHandler(ProviderType.CNS);
					case "response":
						return new TransportAcknowledgementHandler(ProviderType.CNS);
					default:
						throw new HttpException(400, $"There is no handler for notification body of type {bodyType}.");
				}
			}
		}
	}
}
