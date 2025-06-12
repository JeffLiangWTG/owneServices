using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.XPath;
using CargoWise.Billing.API;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common;
using Microsoft.Extensions.Logging;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.USCustoms
{
	public class Plugin : SqlBillingTransactionsPlugin
	{
		protected override sealed string QueryName
		{
			get { return "CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.USCustoms.Query.sql"; }
		}

		protected override IEnumerable<TimeStampedTransaction> CreateTransactions(IDataRecord record)
		{
			var clientID = GetString(record, "CC_ID");
			var messageTrackingID = GetGuid(record, "MessageTrackingID").ToString();
			var receivedFromSenderUTC = GetDateTime(record, "AM_ReceivedFromSenderUTC");
			var applicationCode = GetString(record, "AM_ApplicationCode");
			var timeStamp = GetDateTime(record, "AM_ArchivedUTC");
			var priceItemCode = GetPriceItemCode(applicationCode);

			if (priceItemCode != "USI" && priceItemCode != "MAN")
			{
				yield return new TimeStampedTransaction(timeStamp, new BillingTransaction
				{
					BillableCount = 1,
					ReportingSource = "HUB",
					ClientID = clientID,
					Reference1 = messageTrackingID,
					ServiceOccuredUTC = receivedFromSenderUTC,
					Category = "USC",
					PriceItemCode = priceItemCode,
					MessageTrackingID = messageTrackingID,
				});
			}

			var messageElement = GetMessageElement(record, "AM_RecipientMessageRaw");
			var headerPath = "//*[local-name()='Header'][1]";
			var bodyPath = "//*[local-name()='Body'][1]";
			var header = messageElement.XPathSelectElement(headerPath).Value;
			var body = messageElement.XPathSelectElement(bodyPath).Value;

			if (header != null && body != null)
			{
				IEnumerable<BillingTransaction> messageContentTransactions;
				try
				{
					messageContentTransactions = CreateTransactionsFromMessageContent(applicationCode, header, body).ToList();
				}
				catch (Exception e)
				{
					Logger.LogError(e, $"Could not create billing transactions from the message content [{header}]:[{body}]");
					yield break;
				}

				foreach (var transaction in messageContentTransactions)
				{
					transaction.ClientID = clientID;
					transaction.ServiceOccuredUTC = receivedFromSenderUTC;
					transaction.Reference3 = messageTrackingID;
					transaction.Category = "USC";
					transaction.MessageTrackingID = messageTrackingID;
					yield return new TimeStampedTransaction(timeStamp, transaction);
				}
			}
		}

		static IEnumerable<BillingTransaction> CreateTransactionsFromMessageContent(string applicationCode, string header, string body)
		{
			var messageType = GetMessageType(applicationCode, header, body);
			var priceItemCode = (messageType == "SN" ? "ISF" : "U" + messageType);

			if (priceItemCode == "USI" || priceItemCode == "MAN") return Enumerable.Empty<BillingTransaction>();

			return GetMessageReferences(messageType, body).Select(tuple => new BillingTransaction
			{
				BillableCount = 1,
				ReportingSource = "HUB",
				Reference1 = tuple.Item1,
				Reference2 = tuple.Item2,
				PriceItemCode = priceItemCode,
			});
		}

		static IEnumerable<Tuple<string, string>> GetMessageReferences(string messageType, string body)
		{
			Func<IEnumerable<string>, IEnumerable<Tuple<string, string>>> parser;
			return Parsers.TryGetValue(messageType, out parser)
				? parser(SplitIntoChunks(body, 80))
				: Enumerable.Empty<Tuple<string, string>>();
		}

		static IEnumerable<string> SplitIntoChunks(string str, int chunkSize)
		{
			for (int i = 0; i < str.Length; i += chunkSize)
			{
				yield return str.Substring(i, Math.Min(chunkSize, str.Length - i));
			}
		}

		static string GetMessageType(string applicationCode, string header, string body)
		{
			switch (applicationCode)
			{
				case "USI":
					return body.Substring(10, 2);
				case "USE":
					return header.Substring(21, 2);
				default:
					return ""; // don't bill for this
			}
		}

		static string GetPriceItemCode(string applicationCode)
		{
			switch (applicationCode)
			{
				case "USI":
				case "USE":
				case "MAN":
					return applicationCode;
				case "AMS":
					return "USA";
				default:
					throw new ArgumentOutOfRangeException("applicationCode", applicationCode, "Unknown application code.");
			}
		}

		static IEnumerable<Tuple<string, string>> ParseQTMessage(IEnumerable<string> lines)
		{
			var currentJob = "";
			foreach (var messageLine in lines)
			{
				if (messageLine.StartsWith("10"))
				{
					currentJob = messageLine.Substring(5, 12);
				}
				else if ((messageLine.StartsWith("9502") || messageLine.StartsWith("9503")) && !string.IsNullOrEmpty(currentJob))
				{
					yield return Tuple.Create(currentJob, (string)null);
					currentJob = "";
				}
			}
		}

		static IEnumerable<Tuple<string, string>> ParseSNMessage(IEnumerable<string> lines)
		{
			string isfTransationNumber = null;
			bool isAddMessage = false;
			bool isAccepted = false;

			foreach (var messageLine in lines)
			{
				if (messageLine.StartsWith("SF10"))
				{
					isAddMessage = (messageLine.Substring(7, 1) == "A");
					isfTransationNumber = messageLine.Substring(38, 15);
				}
				else if (messageLine.StartsWith("SF90"))
				{
					isAccepted = messageLine.Contains("ISF ACCEPTED");
				}
			}

			if (isAddMessage && isAccepted)
			{
				string portCode = lines.First().Substring(3, 4);
				yield return Tuple.Create(portCode, isfTransationNumber);
			}
		}

		static readonly Dictionary<string, Func<IEnumerable<string>, IEnumerable<Tuple<string, string>>>> Parsers = new Dictionary<string, Func<IEnumerable<string>, IEnumerable<Tuple<string, string>>>>
		{
			{"JL", lines => lines.Where(line => line.StartsWith("D01")).Select(line => Tuple.Create(line.Substring(3, 11), (string) null))},
			{"PL", lines => lines.Where(line => line.StartsWith("P01")).Select(line => Tuple.Create(line.Substring(3, 12), (string) null))},
			{"RB", lines => lines.Where(line => line.StartsWith("R01")).Select(line => Tuple.Create(line.Substring(3, 11), (string) null))},
			{"QT", ParseQTMessage},
			{"RR", lines => lines.Where(line => line.StartsWith("R1")).Select(line => Tuple.Create(line.Substring(6, 12), line.Substring(2, 4)))},
			{"SO", lines => lines.Where(line => line.StartsWith("SO10")).Select(line => Tuple.Create(line.Substring(8, 3) + line.Substring(13, 8), line.Substring(4, 4)))},
			{"FT", lines => lines.Where(line => line.StartsWith("10")).Select(line => Tuple.Create(line.Substring(3, 17), line.Substring(20, 4)))},
			{"XT", lines => lines.Where(line => line.StartsWith("ES1") && line[7] == 'A').Select(line => Tuple.Create(line.Substring(50, 15), (string) null))},
			{"SN", ParseSNMessage}
		};
	}
}
