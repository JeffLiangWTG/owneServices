using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using log4net;
using Microsoft.Extensions.Logging;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.NZCustoms
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.NZCustoms.Query.sql"; }
		}

		protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var clientID = GetString(record, "ClientID");
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();
			var receivedFromSenderUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC");
			var applicationCode = GetString(record, "AM_ApplicationCode");
			var timeStamp = GetDateTime(record, "AM_ArchivedUTC");

			yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				ClientID = clientID,
				Reference1 = messageTrackingID,
				ServiceOccuredUTC = receivedFromSenderUTC,
				Category = "NZC",
				PriceItemCode = applicationCode,
				MessageTrackingID = messageTrackingID,
			});
			var contentPath = "//*[local-name()='Content'][1]";
			var messageContent = GetMessageContent(record, "AM_SenderMessageRaw", contentPath);
			if (messageContent != null)
			{
				IEnumerable<BillingTransaction> messageContentTransactions;
				try
				{
					messageContentTransactions = CreateTransactionsFromMessageContent(applicationCode, messageContent).ToList();
				}
				catch (Exception e)
				{
					Logger.LogError(e, $"Could not create billing transactions from the message content [{messageContent}]");
					yield break;
				}

				foreach (var transaction in messageContentTransactions)
				{
					transaction.ClientID = clientID;
					transaction.ServiceOccuredUTC = receivedFromSenderUTC;
					transaction.Category = "NZC";
					yield return new TimeStampedTransaction(timeStamp, transaction);
				}
			}
		}

		string GetMessageContent(IDataRecord record, string columnName, string xPath)
		{
			var messageContent =  DecodeAndDecompress(GetString(record, columnName));
			var rawcontent = XElement.Parse(messageContent).XPathSelectElement(xPath).Value;
			return Encoding.UTF8.GetString(Convert.FromBase64String(rawcontent));
		}

		static IEnumerable<BillingTransaction> CreateTransactionsFromMessageContent(string applicationCode, string messageContent)
		{
			int TSWAttachmentTagIndex = messageContent.IndexOf(TSWAttachmentTag);
			if (TSWAttachmentTagIndex >= 0)
			{
				if (TSWAttachmentTagIndex > 0)
				{
					messageContent = messageContent.Substring(TSWAttachmentTagIndex);
				}
				messageContent = ExtractMessageContentForTSWAttachmentMessage(messageContent);
			}
			var messageType = GetMessageType(applicationCode, messageContent);
			var priceItemCode = GetPriceItemCode(messageType);
			return GetMessageIdentifiers(messageType, messageContent).Select(messageIdentifier => new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				Reference1 = messageIdentifier,
				PriceItemCode = priceItemCode,
			});
		}

		static string GetMessageType(string applicationCode, string messageContent)
		{
			switch (applicationCode)
			{
				case "NZM":
					return "eBACCa";
				case "NZC":
					if (messageContent.StartsWith("UNA"))
					{
						var messageSegments = messageContent.Split('\'');
						var messageTypeSegment = messageSegments[2];
						var messageTypeSegmentParts = messageTypeSegment.Split('+');
						return messageTypeSegmentParts[2].Substring(0, 6);
					}
					else
					{
						return GetAgencyAssignedCustomizedDocumentNameFromTSWMessage(messageContent);
					}
				default:
					throw new ArgumentOutOfRangeException("applicationCode", applicationCode, "Unknown application code.");
			}
		}

		static string GetPriceItemCode(string messageType)
		{
			switch (messageType)
			{
				case "eBACCa":
					return "eBA";
				case "CUSDEC":
					return "DEC";
				case "CUSCAR":
					return "CAR";
				case "CUSRES":
					return "RES";
				case "IM1":
				case "EX1":
				case "CRE":
				case "OCR":
				case "ICR":
					return messageType;
				case "RESIM1":
				case "RESEX1":
				case "RESCRE":
				case "RESOCR":
				case "RESICR":
					return messageType.Substring(3, 3);
				default:
					throw new ArgumentOutOfRangeException("messageType", messageType, "Unknown message type.");
			}
		}

		static IEnumerable<string> GetMessageIdentifiers(string messageType, string messageContent)
		{
			switch (messageType)
			{
				case "eBACCa":
					return GetEBACCAMessageIdentifier(messageContent);
				case "CUSCAR":
					return GetCUSCARMessageIdentifier(messageContent);
				case "CUSDEC":
					return GetCUSDECMessageIdentifier(messageContent);
				case "CUSRES":
					return GetCUSRESMessageIdentifier(messageContent);
				case "IM1":
				case "EX1":
				case "CRE":
				case "OCR":
				case "ICR":
					return GetTSWRequestMessageIdentifier(messageContent);
				case "RESIM1":
				case "RESEX1":
				case "RESCRE":
				case "RESOCR":
				case "RESICR":
					return GetTSWResponseMessageIdentifier(messageContent);
				default:
					throw new ArgumentOutOfRangeException("messageType", messageType, "Unknown message type.");
			}
		}

		static string ExtractMessageContentForTSWAttachmentMessage(string messageContent)
		{
			XElement xml = XElement.Parse(messageContent);
			var content = xml.XPathSelectElement("/*[local-name()='Document'][1]/*[local-name()='Content']").Value;
			return new MemoryStream(Encoding.UTF8.GetBytes(content)).DecodeAndDecompress().ReadToEnd();
		}

		static IEnumerable<string> GetEBACCAMessageIdentifier(string messageContent)
		{
			const string clientReferenceNumber = "ClientReferenceNumber>";
			var clientReferenceNumberIndex = messageContent.IndexOf(clientReferenceNumber, StringComparison.Ordinal) + clientReferenceNumber.Length;
			if (clientReferenceNumberIndex >= clientReferenceNumber.Length)
			{
				var closingTagIndex = messageContent.IndexOf('<', clientReferenceNumberIndex);
				if (closingTagIndex >= 0)
				{
					yield return messageContent.Substring(clientReferenceNumberIndex, closingTagIndex - clientReferenceNumberIndex);
				}
				else
				{
					throw new FormatException("ClientReferenceNumber element is not closed.");
				}
			}
			else
			{
				throw new FormatException("eBACCa Message does not contain ClientReferenceNumber element.");
			}
		}

		static string GetAgencyAssignedCustomizedDocumentNameFromTSWMessage(string messageContent)
		{
			const string tag = "AgencyAssignedCustomizedDocumentName>";
			var agencyAssignedCustomizedDocumentNameIndex = messageContent.IndexOf(tag, StringComparison.Ordinal) + tag.Length;
			if (agencyAssignedCustomizedDocumentNameIndex >= tag.Length)
			{
				var closingTagIndex = messageContent.IndexOf('<', agencyAssignedCustomizedDocumentNameIndex);
				if (closingTagIndex >= 0)
				{
					return messageContent.Substring(agencyAssignedCustomizedDocumentNameIndex, closingTagIndex - agencyAssignedCustomizedDocumentNameIndex);
				}
				else
				{
					throw new FormatException("AgencyAssignedCustomizedDocumentName element is not closed.");
				}
			}
			else
			{
				throw new FormatException("TSW Message does not contain AgencyAssignedCustomizedDocumentName element.");
			}
		}

		static IEnumerable<string> GetCUSCARMessageIdentifier(string messageContent)
		{
			return messageContent
				.Split('\'')
				.Where(segment => segment.StartsWith("BGM"))
				.Select(segment => segment.Split('+')[2]);
		}

		static IEnumerable<string> GetCUSDECMessageIdentifier(string messageContent)
		{
			return messageContent
				.Split('\'')
				.Where(segment => segment.StartsWith("BGM"))
				.Select(segment => segment.Split('+')[2]);
		}

		static IEnumerable<string> GetCUSRESMessageIdentifier(string messageContent)
		{
			return messageContent
				.Split('\'')
				.Where(segment => segment.StartsWith("UNH"))
				.Select(segment => segment.Split('+')[3]);
		}

		static IEnumerable<string> GetTSWRequestMessageIdentifier(string messageContent)
		{
			var xml = XElement.Parse(messageContent);
			yield return xml.XPathSelectElement("/*[local-name()='Declaration']/*[local-name()='FunctionalReferenceID']").Value;
		}

		static IEnumerable<string> GetTSWResponseMessageIdentifier(string messageContent)
		{
			var xml = XElement.Parse(messageContent);
			yield return xml.XPathSelectElement("/*[local-name()='Response']/*[local-name()='OverallDeclaration']/*[local-name()='Declaration']/*[local-name()='FunctionalReferenceID']").Value;
		}

		static readonly ILog ClassLogger = LogManager.GetLogger(typeof(Plugin));
		static string TSWAttachmentTag = @"<Documents xmlns=""http://cargowise.com/ehub/products/xmlwithattachments"">";
	}
}
