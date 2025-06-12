using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using Common.Logging;
using Microsoft.XLANGs.BaseTypes;
using Microsoft.XLANGs.Core;
using WTG.ErrorReporting;
using System.Diagnostics;
using System.IO;

namespace CargoWise.eHub.AlertService.Helpers
{
	class IssueAlertException : Exception
	{
		public IssueAlertException(string message) : base(message)
		{
		}
	}

	internal class ErrorReporterFactory
	{
		internal virtual IErrorReportingClient BuildErrorReporter(Uri uri)
		{
			return new ErrorReportingClient(uri);
		}
	}

	public class OrchestrationHelper
	{
		private static ErrorReporterFactory DefaultErrorFactory = new ErrorReporterFactory();

		public static void ReportAlertToIssueManager(ILog logger, string errorPK, string eHUbTrackingPK, string receiveUtcDate, string utcDateTime, string sender, string recipient,
			string source, string errorMessage)
		{
			ReportAlertToIssueManager(logger, errorPK, eHUbTrackingPK, receiveUtcDate, utcDateTime, sender, recipient, source, errorMessage, "");
		}

		public static void ReportAlertToIssueManager(ILog logger, string errorPK, string eHUbTrackingPK, string receiveUtcDate, string utcDateTime, string sender, string recipient,
			string source, string errorMessage, string alertKey)
		{
			// Set IssueManagerUri in the BTSNTSvc.exe.config/BTSNTSvc64.exe.config
			// to set the URL that is used when sending to WTG.ErrorReporter
			var issueManagerUri = System.Configuration.ConfigurationManager.AppSettings["IssueManagerUri"];
			
			ReportAlertToIssueManager(logger, DefaultErrorFactory, issueManagerUri, errorPK, eHUbTrackingPK, receiveUtcDate, utcDateTime, sender, recipient, source, errorMessage, alertKey);
		}

		internal static void ReportAlertToIssueManager(ILog logger, ErrorReporterFactory builder, string issueManagerUri, string errorPK, string eHUbTrackingPK, string receiveUtcDate, string utcDateTime, string sender, string recipient,
			string source, string errorMessage, string alertKey = "")
		{
			try
			{
				var errorBuilder = new EnterpriseErrorReportBuilder();

					var issueBody = new StringBuilder();
				issueBody.AppendLine("Alert from Biztalk");
				issueBody.AppendLine("Server Name: " + System.Environment.MachineName);
				issueBody.AppendLine("eHub Message Tracking ID: " + eHUbTrackingPK);
				issueBody.AppendLine("Received From Sender UTC: " + receiveUtcDate);
				issueBody.AppendLine("Error Time UTC: " + utcDateTime);
				issueBody.AppendLine("Sender: " + sender);
				issueBody.AppendLine("Recipient: " + recipient);
				issueBody.AppendLine("Source: " + source);
				issueBody.AppendLine("Message: " + errorMessage);

				logger.DebugFormat("Issue Report: {0}.", issueBody.ToString());

				var rootException = errorMessage.Split(new[] { ExceptionHelper.IssueMessageDelimiter }, StringSplitOptions.None)[0];
				var key = string.IsNullOrWhiteSpace(alertKey) ? $"eHub.Alert {sender} {recipient} {(rootException.Length >= 300 ? rootException.Substring(0, 300) : rootException)}" : alertKey;
				var issueMessage = string.IsNullOrWhiteSpace(alertKey) ? $"eHub.Alert {sender} {recipient} {rootException}" : alertKey;

				errorBuilder = errorBuilder.SetKey(key);
				errorBuilder.SetRandomErrorReportID();
				errorBuilder.SetTimeOfException(System.DateTime.Now);
				errorBuilder.SetSubject("eHub Alert");
				errorBuilder.SetExceptionDescription(issueBody.ToString());
				errorBuilder.SetRootException(new IssueAlertException(issueMessage), new StackTrace());
				errorBuilder.SetExeCreationTime(System.DateTime.Now);
				if (string.IsNullOrWhiteSpace(issueManagerUri))
				{
					logger.DebugFormat("ErrorPK: {0}, Did not log Alert to Issue Manager due to un-set URI of appsetting: 'IssueManagerUri'", errorPK);
					logger.DebugFormat("Issue Report: {0}.", issueBody.ToString());
					return;
				}
				using (var errorReporter = builder.BuildErrorReporter(new Uri(issueManagerUri)))
				{
					var task = errorReporter.PostCrashReportAsync(errorBuilder);
					task.ConfigureAwait(false);
					task.Wait();

					logger.DebugFormat("ErrorPK: {0}, Alert logged to Issue Manager using uri: {1}, key = {2}", errorPK, issueManagerUri, key);
					if (logger.IsTraceEnabled)
						logger.TraceFormat("Issue body: {0}", issueBody);
				}
			}
			catch (Exception e)
			{
				logger.ErrorFormat("ErrorPK: {0}, Failed to log Alert to the Issue Manager. Uri: {1}", e, errorPK, issueManagerUri);
			}
		}

		public static string GenerateInsertErrorWcfSqlString(string outboxPK, string inboxPK, string errorPK, string senderID,
			string recipientID, string inboxMessageTrackingID, string outboxMessageTrackingID, string errorType, string errorDescription, string currentUTCTime,
			string skipSendAlert, XLANGMessage message)
		{
			var resultStream = new VirtualStream();
			string ns1 = "http://schemas.microsoft.com/Sql/2008/05/TypedProcedures/dbo";
			var writer = XmlTextWriter.Create(resultStream,
				new XmlWriterSettings() { Encoding = Encoding.UTF8, CheckCharacters = false, ConformanceLevel = ConformanceLevel.Fragment, OmitXmlDeclaration = true });

			if (skipSendAlert == "True")
			{
				errorDescription += "\r\n---- The alert for this message was skipped";
			}

			if (message != null)
			{
				errorDescription += $"{ExceptionHelper.IssueMessageDelimiter}{GetContextPropertiesAsString(message)}";
			}
			writer.WriteStartElement("ns1", "InsertError", ns1);
			WriteElement(writer, "ErrorPK", ns1, errorPK);
			WriteElement(writer, "Source", ns1, "BTS");
			WriteElement(writer, "ErrorType", ns1, errorType.Substring(0, 3));
			WriteElement(writer, "Description", ns1, errorDescription);
			WriteElement(writer, "ErrorDetail", ns1, GetErrorDetail(senderID, recipientID));
			WriteElement(writer, "InboxPK", ns1, inboxPK);
			WriteElement(writer, "OutboxPK", ns1, outboxPK);
			WriteElement(writer, "CurrentDateTimeUTC", ns1, currentUTCTime);
			WriteElement(writer, "InboxMessageTrackingID", ns1, inboxMessageTrackingID);
			WriteElement(writer, "OutboxMessageTrackingID", ns1, outboxMessageTrackingID);
			WriteElement(writer, "Alerted", ns1, skipSendAlert);
			writer.WriteEndElement();
			writer.Flush();
			return resultStream.ReadToEnd();
		}

		public static string GenerateInsertOutboxWcfSqlString(string outboxPK, string inboxPK,
			string envelopeTrackingID, string messageTrackingID, string senderID, string recipientID,
			string overrideEmailSubject, string overrideFilename, string currentUTCTime, string inboxContent)
		{
			var resultStream = new VirtualStream();
			string ns1 = "http://schemas.microsoft.com/Sql/2008/05/TypedProcedures/dbo";
			var writer = XmlTextWriter.Create(resultStream,
				new XmlWriterSettings() { Encoding = Encoding.UTF8, CheckCharacters = false, ConformanceLevel = ConformanceLevel.Fragment, OmitXmlDeclaration = true });
			writer.WriteStartElement("ns1", "InsertMessageToOutbox", ns1);
			WriteElement(writer, "PK", ns1, outboxPK);
			WriteElement(writer, "SenderID", ns1, senderID);
			WriteElement(writer, "RecipientID", ns1, recipientID);
			WriteElement(writer, "EnvelopeTrackingID", ns1, envelopeTrackingID);
			WriteElement(writer, "MessageTrackingID", ns1, messageTrackingID);
			WriteElement(writer, "InternalTrackingID", ns1, inboxPK);
			WriteElement(writer, "OverrideEmailSubject", ns1, overrideEmailSubject);
			WriteElement(writer, "OverrideFilename", ns1, overrideFilename);
			WriteElement(writer, "Status", ns1, 255);
			WriteElement(writer, "InsertUTC", ns1, currentUTCTime);
			WriteElement(writer, "TargetMessageType", ns1, null);
			WriteElement(writer, "Content", ns1, inboxContent);
			writer.WriteEndElement();
			writer.Flush();
			return resultStream.ReadToEnd();
		}

		public static bool IsXml(XLANGMessage message)
		{
			try
			{
				new XmlDocument().Load((Stream)message[0].RetrieveAs(typeof(Stream)));
				return true;
			}
			catch (XmlException)
			{
				return false;
			}
		}

		public static string GetContextPropertiesAsString(XLANGMessage message)
		{
			try
			{
				var contextProperties = GetContextProperties(message);

				if (contextProperties == null)
				{
					return "Could not get message's context properties.";
				}
				else
				{
					StringBuilder properties = new StringBuilder();
					foreach (XmlQName key in contextProperties.Keys)
					{
						properties.AppendLine($"{key.Namespace}#{key.Name} = {contextProperties[key]}");
					}
					return properties.ToString();
				}
			}
			catch (Exception ex)
			{
				return "Could not get message's context properties with error: " + ex.ToString();
			}
		}

		internal static Func<XLANGMessage, IDictionary> GetContextProperties = message =>
		{
			foreach (Segment segment in Service.RootService._segments)
			{
				IDictionary fields =
					Context.FindFields(
						typeof(XLANGMessage)
						, segment.ExceptionContext);

				foreach (DictionaryEntry field in fields)
				{
					XMessage msg = (field.Value as XMessage);
					if (msg != null && String.CompareOrdinal(msg.Name, message.Name) == 0)
					{
						return msg.GetContextProperties();

					}
				}
			}
			return null;
		};

		private static string GetErrorDetail(string senderID, string recipientID)
		{
			var resultBuilder = new StringBuilder();
			resultBuilder.Append("<ErrorDetail>");
			if (!string.IsNullOrEmpty(senderID))
				resultBuilder.Append("<SourceParty>" + senderID + "</SourceParty>");
			if (!string.IsNullOrEmpty(recipientID))
				resultBuilder.Append("<DestinationParty>" + recipientID + "</DestinationParty>");
			resultBuilder.Append("</ErrorDetail>");
			return resultBuilder.ToString();
		}

		private static void WriteElement(XmlWriter writer, string elementName, string ns, object value)
		{
			writer.WriteStartElement("ns1", elementName, ns);
			writer.WriteString(AsString(value));
			writer.WriteEndElement();
		}

		private static string AsString(object value)
		{
			var result = value;
			if (result is string && result != null)
			{
				result = RemoveTroublesomeCharacters((string)value);
			}

			return result != null ? result.ToString() : string.Empty;
		}

		private static string RemoveTroublesomeCharacters(string inString)
		{
			var newString = new StringBuilder();
			for (int i = 0; i < inString.Length; i++)
			{
				var ch = inString[i];
				if ((ch < 0x00FD && ch > 0x001F) || ch == '\t' || ch == '\n' || ch == '\r')
				{
					newString.Append(ch);
				}
			}
			return newString.ToString();
		}
	}
}
