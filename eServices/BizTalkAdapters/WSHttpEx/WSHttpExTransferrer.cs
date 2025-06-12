using System;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.eHub.BizTalkAdapters.Common;
using Common.Logging;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Streaming;

namespace CargoWise.eHub.BizTalkAdapters.WSHttpEx
{
	public class WSHttpExTransferrer : IHttpExTransferrer
	{
		static readonly int IO_BUFFER_SIZE = 4096;
		readonly IWebRequestFactory requestFactory;
		readonly IBaseMessageFactory messageFactory;
		readonly string propertyNamespace;
		protected WSHttpExProperties config;

		public WSHttpExTransferrer(IBaseMessage message, string propertyNamespace, IBaseMessageFactory messageFactory, IWebRequestFactory requestFactory)
		{
			config = new WSHttpExProperties(message, propertyNamespace);
			this.requestFactory = requestFactory;
			this.messageFactory = messageFactory;
			this.propertyNamespace = propertyNamespace;
		}

		public IBaseMessage SendRequest(IBaseMessage message)
		{
			if (config.SecurityMode != null && config.SecurityMode == "Transport")
			{
				var preAuthRequest = requestFactory.PreAuthenticateRequest(message, config);
				var preAuthResponse = (HttpWebResponse)preAuthRequest.GetResponse();
				preAuthResponse.Close();
			}

			TransferrerHelpers.Log(this, config.Logger, LogLevel.Info, "Started sending Http Request containing Message ID = '{0}', Endpoint Hash Code = {1}", message.MessageID, GetHashCode());

			var request = requestFactory.CreateRequest(message, config);
			using (var webStream = request.GetRequestStream())
			{
				SoapHelpers.WrapBodyInEnvelope(message.BodyPart.GetOriginalDataStream(), webStream);
				webStream.Flush();
			}

			TransferrerHelpers.Log(this, config.Logger, LogLevel.Info, "Successfully finished sending Http Request containing Message ID = '{0}', Endpoint Hash Code = {1}.  Awaiting response.", message.MessageID, GetHashCode());

			return ProcessResponse(message, request);
		}

		IBaseMessage ProcessResponse(IBaseMessage message, WebRequest request)
		{
			VirtualStream responseStream;
			WebResponse webResponse;

			try
			{
				webResponse = request.GetResponse();
				responseStream = CreateResponseStream(webResponse);
				TransferrerHelpers.Log(this, config.Logger, LogLevel.Info, "Successfully received response for Http Request containing Message ID = '{0}', Endpoint Hash Code = {1}", message.MessageID, GetHashCode());
			}
			catch (WebException webException)
			{
				webResponse = webException.Response;
				responseStream = CreateResponseStream(webResponse);
				if (!SoapHelpers.ContainsEnvelope(responseStream))
				{
					responseStream.Close();
					throw; // It was not a soap fault, just re-throw the original exception.
				}

				if (!config.IsTwoWay)
				{
					// on a one way port we can't provide the soap fault as a response message, so we have to throw it.
					using (responseStream)
					using (var responseReader = new StreamReader(responseStream))
					{
						throw new FaultException(responseReader.ReadToEnd());
					}
				}

				TransferrerHelpers.Log(this, config.Logger, LogLevel.Info, "Received soap fault for Http Request containing Message ID = '{0}', Endpoint Hash Code = {1}", message.MessageID, GetHashCode());
			}

			if (!config.IsTwoWay)
			{
				responseStream.Close();
				return null;
			}

			TransferrerHelpers.Log(this, config.Logger, LogLevel.Info, "Started building response to Message ID = '{0}', Endpoint Hash Code = {1}", message.MessageID, GetHashCode());

			var btsResponse = messageFactory.CreateMessage();

			if (webResponse is HttpWebResponse httpWebResponse)
			{
				btsResponse.Context.Write("InboundHttpHeaders", propertyNamespace, httpWebResponse.Headers.ToString());
				btsResponse.Context.Write("InboundHttpStatusCode", propertyNamespace, httpWebResponse.StatusCode.ToString());
				btsResponse.Context.Write("InboundHttpStatusDescription", propertyNamespace, httpWebResponse.StatusDescription);
			}

			if (config.ResponseMessageEncoding == "Mtom")
			{
				responseStream = CreateMtomStream(responseStream, webResponse);
			}

			var headersElement = SoapHelpers.GetHeaderFromEnvelope(responseStream);
			btsResponse.Context.Write("InboundHeaders", propertyNamespace, headersElement.ToString(SaveOptions.DisableFormatting));
			var currentHeader = (XElement)headersElement.FirstNode;
			while (currentHeader != null)
			{
				btsResponse.Context.Write(currentHeader.Name.LocalName, currentHeader.GetDefaultNamespace().ToString(), currentHeader.ToString(SaveOptions.DisableFormatting));
				currentHeader = (XElement)currentHeader.NextNode;
			}

			switch (config.InboundBodyLocation)
			{
				case InboundBodyLocations.EntireEnvelope:
					break;

				case InboundBodyLocations.BodyContents:
				case InboundBodyLocations.BodyPath:
					var bodyPathStream = SoapHelpers.UnwrapBodyFromEnvelope(responseStream, config.InboundBodyPathExpression);
					responseStream.Close();
					responseStream = bodyPathStream;
					break;

				default:
					throw new NotImplementedException(string.Format("InboundBodyLocation {0} has not been implemented", config.InboundBodyLocation));
			}

			TransferrerHelpers.Log(this, config.Logger, LogLevel.Info, "Successfully finished building response to Message ID = '{0}', Endpoint Hash Code = {1}", message.MessageID, GetHashCode());

			var body = messageFactory.CreateMessagePart();
			body.Data = responseStream;
			btsResponse.AddPart("Body", body, true);

			return btsResponse;
		}

		VirtualStream CreateMtomStream(Stream responseStream, WebResponse response)
		{
			using (var xmlReader = XmlDictionaryReader.CreateMtomReader(responseStream, new[] { Encoding.UTF8 }, response.Headers["Content-Type"], XmlDictionaryReaderQuotas.Max))
			{
				var msgDoc = new XmlDocument();
				msgDoc.PreserveWhitespace = true;
				msgDoc.Load(xmlReader);
				return CreateStreamFromString(msgDoc.OuterXml);
			}
		}

		VirtualStream CreateResponseStream(WebResponse response)
		{
			var virtualResponseStream = new VirtualStream();

			using (Stream responseStream = response.GetResponseStream())
			{
				responseStream.CopyTo(virtualResponseStream, IO_BUFFER_SIZE);
			}

			response.Close();
			virtualResponseStream.Position = 0;

			return virtualResponseStream;
		}

		VirtualStream CreateStreamFromString(string content)
		{
			var virtualResponseStream = new VirtualStream();
			byte[] byteArray = Encoding.UTF8.GetBytes(content);
			using (Stream responseStream = new MemoryStream(byteArray))
			{
				responseStream.CopyTo(virtualResponseStream, IO_BUFFER_SIZE);
			}

			virtualResponseStream.Position = 0;

			return virtualResponseStream;
		}
	}
}
