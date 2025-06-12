using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.GBCustoms.Core;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService.Handlers;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService;
using CargoWise.eHub.Products.GBCustoms.CSPs.Common;
using ILog = Common.Logging.ILog;
using LogManager = Common.Logging.LogManager;
using log4net;
using System.IO;
using System.Configuration;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.GBCustoms.Pentant.InboundMessageWebService.Controllers
{
    public class PentantNotificationController : ApiController
    {
		protected internal Func<Guid> InternalGuid = () => Guid.NewGuid();
		protected internal Func<ILog> InternalLog = () => LogManager.GetLogger<PentantNotificationController>();
		protected internal ILog logger;
		protected internal HttpRequestMessage InternalRequestMessage => Request;
		protected internal virtual Stream InternalRequestStream => HttpContext.Current.Request.InputStream;


		readonly Guid ID;
		
		public PentantNotificationController()
		{
			logger = InternalLog();
			ID = InternalGuid();
		}
		
		[HttpPost]
		public HttpResponseMessage ReceiveNotification()
		{
			logger.Info($"[Service ID: {ID}] Received Notification.");
			LogicalThreadContext.Properties["PullingRequestTime"] = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
			try
			{
				var requestXml = XDocument.Parse(InternalRequestStream.ReadToEnd());
				logger.Debug($"[Service ID: {ID}] Notification: {requestXml.Root}.");

				var handler = GetHandler(requestXml);
				logger.Info($"[Service ID: {ID}] Message will be processed using {handler.GetName()}.");
				return handler.Process(InternalRequestMessage, logger, ID, requestXml);
			}
			catch (HttpException ex)
			{
				logger.Error($"[Service ID: {ID}] ReceiveNotification threw exception.", ex);
				var inboundError = new InboundError(new eHubTransactionsContext(), logger, "GBCustoms");
				inboundError.CreateFailedMessage(InternalRequestMessage, "GBCustoms Pentant Inbound WS - ReceiveNotificationHttpException", ex);
				return InternalRequestMessage.CreateResponse((HttpStatusCode)ex.GetHttpCode(), ex.Message + $"\r\nPlease quote [{ID}] for enquiries.");
			}
			catch (Exception ex)
			{
				logger.Error($"[Service ID: {ID}] ReceiveNotification threw exception", ex);
				var inboundError = new InboundError(new eHubTransactionsContext(), logger, "GBCustoms");
				inboundError.CreateFailedMessage(InternalRequestMessage, "GBCustoms Pentant Inbound WS - ReceiveNotificationException", ex);
				return InternalRequestMessage.CreateResponse(HttpStatusCode.InternalServerError, $"An internal error occured during the process. \r\nPlease quote [{ID}] for enquiries.");
			}
		}

		internal virtual IMessageHandler GetHandler(XDocument requestXml)
		{
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("S", "http://www.w3.org/2003/05/soap-envelope");
			namespaceManager.AddNamespace("wsa", "http://schemas.xmlsoap.org/ws/2004/03/addressing");

			var actionIndicator = requestXml.XPathSelectElement("/S:Envelope/S:Header/wsa:Action", namespaceManager).Value;

			switch(GetAction(actionIndicator))
			{
				case IncomingMessageType.TransportAcknowledgement:
					return new TransportAcknowledgementHandler(ProviderType.Pentant);
				case IncomingMessageType.BusinessNotification:
					return new BusinessNotificationHandler(ProviderType.Pentant);
				case IncomingMessageType.Nack:
					return new NackHandler(ProviderType.Pentant);
				case IncomingMessageType.INVRES:
					return new INVRESHandler(ProviderType.Pentant);
				default:
					throw new HttpException(400, $"Unknown Action '{actionIndicator}'.");
			}
		}

		internal IncomingMessageType GetAction(string actionIndicator)
		{
			if (actionIndicator.Length < 13)
			{
				return IncomingMessageType.Unknown;
			}

			var actionType = actionIndicator.Substring(10, 3);

			switch(actionType)
			{
				case "ACK":
					return IncomingMessageType.TransportAcknowledgement;
				case "DMS":
					return IncomingMessageType.BusinessNotification;
				case "NAC":
					return IncomingMessageType.Nack;
				case "INV":
					if(actionIndicator.Length < 14)
					{
						return IncomingMessageType.Unknown;
					}

					var invSubTypes = new List<string>()
					{
						"INVRES",
					};

					var busSubTypes = new List<string>()
					{
						"INVECTL",
						"INVEQRES",
						"INVEMRES",
						"INVEMTR"
					};

					if(invSubTypes.Exists(x => x.Contains(actionIndicator.Substring(13))))
					{
						return IncomingMessageType.INVRES;
					}
					else if(busSubTypes.Exists(x => x.Contains(actionIndicator.Substring(13))))
					{
						return IncomingMessageType.BusinessNotification;
					}
					else
					{
						return IncomingMessageType.Unknown;
					}
				default:
					return IncomingMessageType.Unknown;
			}
		}
	}
}
