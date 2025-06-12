using System;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Services.Protocols;
using System.Xml;
using Common.Logging;

namespace CargoWise.eHub.Products.CACustoms.DocImg.BT.Helpers
{
	public class OrchestrationHelper
	{
		public static Stream CreateLPCOSendMessage(string message, string boundary, string messageId, string startId, string attachmentInBase64, string attachmentId, string senderId, string recipientId, string msgTrackingID, ILog logger)
		{
			LogDetails("Creating LPCO Send Message: ", message, startId, attachmentId, senderId, recipientId, msgTrackingID, attachmentInBase64, logger);
			MemoryStream memStream = new MemoryStream();
			string separator = "--" + boundary;
			var newLine = Environment.NewLine;

			var contentType = string.Format("multipart/related;\tstart-info=\"text/xml\";\ttype=\"application/xop+xml\";\tstart=\"<{0}>\";{1}\tboundary=\"{2}\"", startId, newLine, boundary);
			var attachmentContent = Convert.FromBase64String(attachmentInBase64);

			try
			{
				StringBuilder sBuilder = new StringBuilder(string.Format("Message-ID: <{0}>{1}", messageId, newLine));
				sBuilder.Append("MIME-Version: 1.0" + newLine);
				sBuilder.AppendFormat("Content-Type: {0}{1}", contentType, newLine);
				sBuilder.AppendFormat("{0}{1}{2}", newLine, separator, newLine);

				contentType = "application/xop+xml; type=\"text/xml\"; charset=UTF-8";
				sBuilder.AppendFormat("Content-Type: {0}{1}", contentType, newLine);
				sBuilder.Append("Content-Transfer-Encoding: 8bit" + newLine);
				sBuilder.AppendFormat("Content-ID: <{0}>{1}", startId, newLine);
				sBuilder.AppendFormat("{0}{1}{2}", newLine, message, newLine);
				sBuilder.AppendFormat("{0}{1}{2}", newLine, separator, newLine);

				contentType = "application/octet-stream";
				sBuilder.AppendFormat("Content-Type: {0}{1}", contentType, newLine);
				sBuilder.Append("Content-Transfer-Encoding: binary" + newLine);
				sBuilder.AppendFormat("Content-ID: <{0}>{1}", attachmentId, newLine);
				sBuilder.AppendFormat("Content-Length: {0}{1}{2}", attachmentContent.Length.ToString(), newLine, newLine);

				byte[] byteArrayBody = Encoding.ASCII.GetBytes(sBuilder.ToString());
				memStream.Write(byteArrayBody, 0, byteArrayBody.Length);

				//Attached file
				var imageDocument = new MemoryStream(attachmentContent);
				imageDocument.CopyTo(memStream);

				sBuilder = new StringBuilder(string.Format("{0}{1}--{2}", newLine, separator, newLine));
				byte[] byteArrayFooter = Encoding.ASCII.GetBytes(sBuilder.ToString());
				memStream.Write(byteArrayFooter, 0, byteArrayFooter.Length);

				memStream.Position = 0;

			}
			catch (Exception ex)
			{
				LogLPCOSendMessage(logger, memStream);
				LogErrorDetails(message, startId, attachmentId, ex.Message, senderId, recipientId, msgTrackingID, logger);
				throw;
			}
			LogLPCOSendMessage(logger, memStream);
			LogDetails("LPCO Send Message Created: ", message, startId, attachmentId, senderId, recipientId, msgTrackingID, attachmentInBase64, logger);
			return memStream;
		}

		static void LogLPCOSendMessage(ILog logger, MemoryStream message)
		{
			if (message.Length > 0)
			{
				if (logger.IsTraceEnabled)
					logger.TraceFormat(@"LPCO Send Message: {0}", new StreamReader(message).ReadToEnd());
			}
			else
			{
				logger.Error("Error Creating LPCO Send Message.");
			}
		}

		static void LogDetails(string stringInfo, string message, string startId, string attachmentId, string senderId, string recipientId, string msgTrackingID, string attachmentInBase64, ILog logger)
		{
			if (logger.IsTraceEnabled)
			{
				logger.TraceFormat(@"{0}
				StartId: {1}
				AttachmentId: {2}
				Sender: {3}
				Recipient: {4}
				MsgId: {5}
				Attachment: {6}
				Message content: {7}", stringInfo, startId, attachmentId, senderId, recipientId, msgTrackingID, attachmentInBase64, message);
			}
			else
			{
				logger.InfoFormat(@"{0}
				StartId: {1}
				AttachmentId: {2}
				Sender: {3}
				Recipient: {4}
				MsgId: {5}
				Message content: {6}", stringInfo, startId, attachmentId, senderId, recipientId, msgTrackingID, message);
			}
		}

		static void LogErrorDetails(string message, string startId, string attachmentId, string error, string senderId, string recipientId, string msgTrackingID, ILog logger)
		{
			logger.ErrorFormat(@"Error while proccessing the following message: 
			StartId: {0}
			AttachmentId: {1}
			Sender: {2}
			Recipient: {3}
			MsgId: {4}
			Error: {5}
			Message content: {6}", startId, attachmentId, senderId, recipientId, msgTrackingID, error, message);
		}

		static Random rand = new Random();
		public static string GetIncludeId()
		{
			var prefix = LongRandom(100000000000000000, 9000000000000000000, rand);
			var jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return string.Format("-{0}.{1}@{2}", prefix, (long)(DateTime.UtcNow - jan1st1970).TotalMilliseconds, Environment.MachineName);
		}

		public static HttpResponse GetHttpErrorResponseWithInvalidStreamBody(SoapException soapException)
		{
			var errorDescription = soapException.Detail?.SelectSingleNode("/*[local-name()='NACK']/*[local-name()='ErrorDescription']")?.InnerText;
			if (string.IsNullOrEmpty(errorDescription))
			{
				var innerException = soapException.InnerException;
				errorDescription = innerException is WebException ? innerException.Message : soapException.Message;

				if (IsNoEndpointException(errorDescription) || IsInternalServerException(errorDescription))
				{
					return new HttpResponse("", errorDescription, true);
				}
				throw new InvalidOperationException(soapException.Message);
			}

			if (IsServerReturnedException(errorDescription))
			{
				return new HttpResponse(Regex.Match(errorDescription, @"\((\d+)\)").Groups[1].Value, soapException.Message.Split(')').Last().Trim());
			}

			if (IsNoEndpointException(errorDescription))
			{
				return new HttpResponse("", soapException.Message.Split(')').Last().Trim(), true);
			}

			if (IsTemporaryException(errorDescription) || IsInternalServerException(errorDescription))
			{
				return new HttpResponse("", errorDescription, true);
			}

			throw new InvalidOperationException(errorDescription);
		}

		private static bool IsNoEndpointException(string errorDescription)
		{
			return Regex.Match(errorDescription, @"There was no endpoint listening at .* that could accept the message. This is often caused by an incorrect address or SOAP action", RegexOptions.IgnoreCase).Success;
		}

		private static bool IsInternalServerException(string errorDescription)
		{
			return errorDescription.Contains("(500) Internal Server Error");
		}

		private static bool IsServerReturnedException(string errorDescription)
		{
			return Regex.Match(errorDescription, @"The remote server returned *", RegexOptions.IgnoreCase).Success;
		}

		private static bool IsTemporaryException(string errorDescription)
		{
			return errorDescription.Contains("This could be due to the fact that the server certificate is not configured properly with HTTP.SYS")
				|| errorDescription.Contains("This could be due to the service endpoint binding not using the HTTP protocol. This could also be due to an HTTP request context being aborted by the server (possibly due to the service shutting down).")
				|| errorDescription.Contains("A connection that was expected to be kept alive was closed by the server")
				|| errorDescription.Contains("Could not establish secure channel for SSL/TLS");
		}

		public static HttpResponse HandleExceptions(Exception exception, ILog logger)
		{
			logger.Error("Error during sending.", exception);

			if (exception != null && exception is SoapException)
			{
				var soapException = exception as SoapException;
				return GetHttpErrorResponse(soapException, logger);
			}
			else if (exception != null && exception is WebException)
			{
				var webException = exception as WebException;
				return GetHttpErrorResponse(webException, logger);
			}
			else if (exception != null && exception is CommunicationException)
			{
				var communicationException = exception as CommunicationException;
				return GetHttpErrorResponse(communicationException);
			}
			else
			{
				throw new InvalidOperationException("Error during sending.", exception);
			}
		}

		public static HttpResponse GetHttpErrorResponse(SoapException soapException, ILog logger = null)
		{
			if (soapException.Detail != null)
			{
				logger?.Debug("SoapException.Detail:\r\n" + soapException.Detail.OuterXml);

				var xmlNode = soapException.Detail.SelectSingleNode("/*[local-name()='NACK']/*[local-name()='ErrorDetail']/*[local-name()='HttpErrorDetail']/*[local-name()='Body']");

				if (xmlNode?.InnerText.StartsWith("<b2b>") ?? false)
				{
					var xml = new XmlDocument();
					xml.LoadXml(xmlNode.InnerText);

					var errorCode = xml.SelectSingleNode("/*[local-name()='b2b']/*[local-name()='response']/*[local-name()='error_code']")?.InnerText;
					var errorText = xml.SelectSingleNode("/*[local-name()='b2b']/*[local-name()='response']/*[local-name()='error_text']")?.InnerText;
					return new HttpResponse(errorCode, errorText);
				}

				return GetHttpErrorResponseWithInvalidStreamBody(soapException);
			}

			return GetHttpErrorResponseWithInvalidStreamBody(soapException);
		}

		public static HttpResponse GetHttpErrorResponse(WebException webException, ILog logger)
		{
			var errorCode = string.Empty;
			var errorText = string.Empty;
			if (webException.Response != null)
			{
				using (var response = webException.Response.GetResponseStream())
				{
					var responseDoc = new XmlDocument();
					responseDoc.Load(response);
					logger.Debug($"WebException. Response XML is\r\n{ responseDoc.DocumentElement.OuterXml }");

					var xmlNodeErrorCode = responseDoc.SelectSingleNode("//b2b/response/error_code");
					var xmlNodeErrorText = responseDoc.SelectSingleNode("//b2b/response/error_text");
					if (xmlNodeErrorCode != null)
					{
						errorCode = xmlNodeErrorCode.InnerText;
					}
					if (xmlNodeErrorText != null)
					{
						errorText = xmlNodeErrorText.InnerText;
					}
					if (!string.IsNullOrWhiteSpace(errorCode) || !string.IsNullOrWhiteSpace(errorText))
					{
						return new HttpResponse(errorCode, errorText);
					}
				}

			}
			errorCode = ((HttpWebResponse)webException.Response).StatusCode.ToString();
			errorText = ((HttpWebResponse)webException.Response).StatusDescription;

			return new HttpResponse(errorCode, errorText);
		}

		public static HttpResponse GetHttpErrorResponse(CommunicationException communicationException)
		{
			return new HttpResponse("", communicationException.Message, true);
		}

		public static string GetB2bResponse(Exception exception, HttpResponse httpResponse)
		{
			if (exception is SoapException)
			{
				var soapEx = exception as SoapException;
				return B2BResponse(soapEx, httpResponse);
			}

			if (exception is WebException)
			{
				var webEx = exception as WebException;
				return B2BResponse(httpResponse, webEx);
			}

			if (exception is CommunicationException)
			{
				return B2BResponse(httpResponse);
			}
			throw new InvalidOperationException(exception.Message);
		}

		private static string B2BResponse(HttpResponse httpResponse)
		{
			return String.Format(@"<b2b>
	<application>swi.tcp.v1.lpco</application>
	<response>
		<error_code>{0}</error_code>
		<error_text>
			<![CDATA[{1}]]>
		</error_text>
	</response>
</b2b>", httpResponse.Code, httpResponse.Description);
		}

		private static string B2BResponse(HttpResponse httpResponse, WebException webEx)
		{
			if (webEx.Response != null)
			{
				using (var response = webEx.Response.GetResponseStream())
				{
					var responseDoc = new XmlDocument();
					responseDoc.Load(response);
					var xmlNodeB2B = responseDoc.SelectSingleNode("//b2b");
					if (xmlNodeB2B != null)
					{
						return xmlNodeB2B.OuterXml;
					}
				}
			}

			return String.Format(@"<b2b>
	<application>swi.tcp.v1.lpco</application>
	<response>
		<error_code>{0}</error_code>
		<error_text>
			<![CDATA[{1}]]>
		</error_text>
	</response>
</b2b>", httpResponse.Code, httpResponse.Description);
		}

		private static string B2BResponse(SoapException soapEx, HttpResponse httpResponse)
		{
			var xmlNode = soapEx.Detail.SelectSingleNode(
				"/*[local-name()='NACK']/*[local-name()='ErrorDetail']/*[local-name()='HttpErrorDetail']/*[local-name()='Body']");
			if (xmlNode == null || string.IsNullOrEmpty(xmlNode.InnerText) || !xmlNode.InnerText.StartsWith("<b2b>"))
			{
				var responseFromDescription = GetB2BResponseFromDescription(soapEx.Detail.OuterXml);
				if (!String.IsNullOrWhiteSpace(responseFromDescription))
				{
					return responseFromDescription;
				}

				return String.Format(@"<b2b>
	<application>swi.tcp.v1.lpco</application>
	<response>
		<error_code>{0}</error_code>
		<error_text>
			<![CDATA[{1}]]>
		</error_text>
	</response>
</b2b>", httpResponse.Code, httpResponse.Description);
			}

			return xmlNode.InnerText;
		}

		public static string GetB2BResponseFromDescription(string description)
		{
			var descriptionEncode = WebUtility.HtmlDecode(description);
			var pattern = "(<b2b>.*</b2b>)";
			var match = Regex.Match(descriptionEncode, pattern);
			if (match.Success)
			{
				return match.Groups[1].Value;
			}
			return "";
		}

		static long LongRandom(long min, long max, Random random)
		{
			var buf = new byte[8];
			random.NextBytes(buf);
			var longRand = BitConverter.ToInt64(buf, 0);
			return (Math.Abs(longRand % (max - min)) + min);
		}
	}
}
