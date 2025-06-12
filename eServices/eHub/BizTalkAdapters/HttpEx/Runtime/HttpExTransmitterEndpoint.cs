using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.eHub.BizTalkAdapters.HttpEx.Admin;
using CargoWise.eHub.BizTalkAdapters.Transferrer.Adapter;
using Common.Logging;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Samples.BizTalk.Adapter.Common;

namespace CargoWise.eHub.BizTalkAdapters.HttpEx
{
	public class HttpExTransmitterEndpoint : TransmitterEndpoint
	{

		readonly IHttpExClientFactory httpExClientFactory;
		readonly IHttpExClientHandlerFactory httpExClientHandlerFactory;

		[ExcludeFromCodeCoverage]
		public HttpExTransmitterEndpoint(AsyncTransmitter asyncTransmitter)
			: this(asyncTransmitter, new HttpExClientFactory(), new HttpExClientHandlerFactory(), LoggerFactory.Instance) { }

		internal HttpExTransmitterEndpoint(AsyncTransmitter asyncTransmitter, IHttpExClientFactory httpExClientFactory, IHttpExClientHandlerFactory httpExClientHandlerFactory, ILoggerFactory loggerFactory)
			: base(asyncTransmitter, loggerFactory)
		{
			this.httpExClientFactory = httpExClientFactory;
			this.httpExClientHandlerFactory = httpExClientHandlerFactory;
		}

		protected override async Task<IBaseMessage> TransmitMessageAsync(IBaseMessage message, XmlDocument configXml, ILog log, string activityId, CancellationToken cancelToken)
		{
			log.Log(activityId, LogLevel.Debug, "Transmitting message '{0}'", message.MessageID);

			ApplyConfigDynamicOverrides(message, HttpExTransmitter.HttpExPropertyNamespace, configXml);
			log.Log(activityId, LogLevel.Trace, "Configuration: " + configXml.OuterXml);

			var config = HttpExConfiguration.Parse(configXml, message.Context);

			var isTwoWay = (bool)message.Context.Read(IsSolicitResponse.Name.Name, IsSolicitResponse.Name.Namespace);

			var httpRequest = new HttpRequestMessage(config.Method, config.DestinationUrl);

			foreach (var header in config.CustomHeaders)
			{
				// For example, in GB Customs MCP, we can have User-Agent = 'Vendor=WiseTech Global, Application=eHub, Version=1.2.3, Badge=CAW, ClientID= A. B. Clearance Agents',
				// This is not valid format for User-Agent, but MCP expect it; so MCP orchestration will add User-Agent to CustomHeaderNamesWithoutValidation!

				var isAdded =
					config.CustomHeaderNamesWithoutValidation.Contains(header.Key) &&
					httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);

				if (!isAdded)
				{
					httpRequest.Headers.Add(header.Key, header.Value);
				}
			}

			if (!config.SuppressMessageBodyForHttpVerbs.Contains(config.Method))
			{
				httpRequest.Content = new StreamContent(message.BodyPart.GetOriginalDataStream());
				if (config.ContentType != null)
					httpRequest.Content.Headers.ContentType = config.ContentType;
			}

			log.Log(activityId, LogLevel.Debug, "Sending HTTP '{0}' request to {1}", httpRequest.Method, httpRequest.RequestUri);
			HttpResponseMessage httpResponse;
			try
			{
				var handler = httpExClientHandlerFactory.CreateHandler();
				if (!string.IsNullOrEmpty(config.Certificate))
				{
					log.Log(activityId, LogLevel.Debug, "Importing client certificate");

					var certificates = new X509Certificate2Collection();
					certificates.Import(Convert.FromBase64String(config.Certificate), config.CertificatePassphrase, X509KeyStorageFlags.DefaultKeySet);
					handler.ClientCertificates.AddRange(certificates);
				}

				var httpExClient = httpExClientFactory.CreateHttpExClient(handler);

				httpResponse = await httpExClient.SendAsync(httpRequest);
				log.Log(activityId, LogLevel.Debug, "HTTP request completed with status ({0}) {1}", (int)httpResponse.StatusCode, httpResponse.StatusCode);
			}
			catch (Exception ex)
			{
				var msg = string.Format("HTTP '{0}' request to {1} failed with error: ({2}) {3}", httpRequest.Method,
					httpRequest.RequestUri, ex.GetType().FullName, ex.InnerException == null ? ex.Message : ex.InnerException.Message);
				log.Log(activityId, LogLevel.Error, msg, ex);
				throw new TransferrerException(msg);
			}

			var correlationToken = (string)message.Context.Read(CorrelationToken.Name.Name, CorrelationToken.Name.Namespace);

			if (!isTwoWay)
			{
				if (config.StatusRanges.Any(range => (int)httpResponse.StatusCode >= range.Item1 &&
													 (int)httpResponse.StatusCode <= range.Item2 &&
													 range.Item3 == HttpExConfiguration.StatusType.Success))
				{
					log.Log(activityId, LogLevel.Debug, "One-way send completed successfully for message '{0}'", message.MessageID);
					return null;
				}

				var msg = string.Format("HTTP '{0}' request to {1} failed with status ({2}) {3}", httpRequest.Method,
					httpRequest.RequestUri, (int)httpResponse.StatusCode, httpResponse.StatusCode);
				log.Log(activityId, LogLevel.Error, msg);
				throw new TransferrerException(msg);
			}
			else
			{
				//CorrelationToken != null means port used in orchestration, as BizTalk always uses correlation tokens when a Two-Way port is used in the orchestration
				if (!config.StatusRanges.Any(range => (int)httpResponse.StatusCode >= range.Item1 &&
													 (int)httpResponse.StatusCode <= range.Item2) && string.IsNullOrEmpty(correlationToken))
				{
					var msg = string.Format("HTTP '{0}' request to {1} failed with status ({2}) {3}", httpRequest.Method,
						httpRequest.RequestUri, (int)httpResponse.StatusCode, httpResponse.StatusCode);
					log.Log(activityId, LogLevel.Error, msg);
					throw new TransferrerException(msg);
				}
			}

			var responseMessage = MessageFactory.CreateMessage();
			responseMessage.AddPart("Body", MessageFactory.CreateMessagePart(), true);
			responseMessage.BodyPart.Data = await httpResponse.Content.ReadAsStreamAsync();
			responseMessage.Context.Write(ResponseStatusCode.Name.Name, ResponseStatusCode.Name.Namespace, (int)httpResponse.StatusCode);
			var headersText = new StringBuilder();
			foreach (var hdr in httpResponse.Headers.Concat(httpResponse.Content.Headers))
				headersText.AppendFormat("{0}: {1}", hdr.Key, string.Join(", ", hdr.Value)).AppendLine();
			responseMessage.Context.Write(InboundHttpHeaders.Name.Name, InboundHttpHeaders.Name.Namespace, headersText.ToString());

			log.Log(activityId, LogLevel.Debug, "Finished send for message '{0}' and returning response message '{1}'",
				message.MessageID, responseMessage.MessageID);

			return responseMessage;
		}

		static readonly BTS.IsSolicitResponse IsSolicitResponse = new BTS.IsSolicitResponse();
		static readonly BTS.CorrelationToken CorrelationToken = new BTS.CorrelationToken();
		static readonly HTTP.ResponseStatusCode ResponseStatusCode = new HTTP.ResponseStatusCode();
		static readonly HTTP.InboundHttpHeaders InboundHttpHeaders = new HTTP.InboundHttpHeaders();
	}
}
