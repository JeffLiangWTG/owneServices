using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	public class AirMessagingPluginTest : SqlBillingTransactionsPluginTest<Plugins.AirMessaging.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "ClientID", "MsgType", "AirlineCodeOrPrefix", "Network", "MAWB", "HAWB", "StatusCode", "AM_ReceivedFromSenderUTC", "MessageTrackingID", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {"888CNSCNS", "FWB", "CX", "CCN", "16046397223", DBNull.Value, DBNull.Value, DateTime.Parse("2014-09-30 23:09:16.013"), new Guid("5e8387d6-3acd-4095-8548-fd7632a32fca"), DateTime.Parse("2014-09-30 23:10:16.013")},
					new object[] {"YASYJCPRD", DBNull.Value, DBNull.Value, "Delta", DBNull.Value, DBNull.Value, DBNull.Value, DateTime.Parse("2014-09-22 04:48:16.000"), new Guid("8cdb20ee-9322-4423-8809-451b4071978f"), DateTime.Parse("2014-09-22 04:49:16.000")},
					new object[] {"ABCDEFXYZ", "FHL", "101", "Traxon", "1001", "2001", DBNull.Value, DateTime.Parse("2014-09-30 23:09:17.013"), new Guid("6ca91142-d130-4f3d-bb50-63920449175b"), DateTime.Parse("2014-09-30 23:10:17.013")},
					new object[] {"ABCDEFXYZ", "-HL", "101", "Traxon_Test", "1002", "2002", DBNull.Value, DateTime.Parse("2014-09-30 23:09:18.013"), new Guid("065a7ffe-b71b-44ad-99db-fd542ef4ccae"), DateTime.Parse("2014-09-30 23:10:18.013")},
					new object[] {"ABCDEFXYZ", "-WB", "101", "Traxon_WithUnsupported", "1003", "2003", DBNull.Value, DateTime.Parse("2014-09-30 23:09:19.013"), new Guid("a703cfdc-cae5-4ab2-a3d4-09a4a6ddea85"), DateTime.Parse("2014-09-30 23:10:19.013")},
					new object[] {"ABCDEFXYZ", "FSU", "101", "Traxon_EDP", "1004", "2004", "RCF", DateTime.Parse("2014-09-30 23:09:20.013"), new Guid("bd5ef72b-6d2e-49e8-b39f-127062239785"), DateTime.Parse("2014-09-30 23:10:20.013")},
                                        new object[] {"ABCDEFXYZ", "FMA", "101", "Traxon_RCF", "1005", "2005", DBNull.Value, DateTime.Parse("2014-09-30 23:09:21.013"), new Guid("acc5e337-af74-473e-b367-7c91341ae7cd"), DateTime.Parse("2014-09-30 23:10:21.013")},
                                        new object[] {"OVERRIDECLIENT", "FWB", "CX", "APROVIDER", "16046397299", DBNull.Value, DBNull.Value, DateTime.Parse("2014-09-30 23:09:22.013"), new Guid("11111111-1111-1111-1111-111111111111"), DateTime.Parse("2014-09-30 23:10:22.013")},
                                };
                        }
                }

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
                        Assert.That(transactions.Length, Is.EqualTo(16));
			// Message 1
			AssertTransaction(transactions[0], DateTime.Parse("2014-09-30 23:10:16.013"), 1, "888CNSCNS", null, null, "AMG", "ALM", "5e8387d6-3acd-4095-8548-fd7632a32fca", "CCN", null, null, null, "HUB", DateTime.Parse("2014-09-30 23:09:16.013"), "5e8387d6-3acd-4095-8548-fd7632a32fca");
			AssertTransaction(transactions[1], DateTime.Parse("2014-09-30 23:10:16.013"), 1, "888CNSCNS", null, null, "AMG", "FWB", "16046397223", null, "CX", "CCN", null, "HUB", DateTime.Parse("2014-09-30 23:09:16.013"), null);
			// Message 2
			AssertTransaction(transactions[2], DateTime.Parse("2014-09-22 04:49:16.000"), 1, "YASYJCPRD", null, null, "AMG", "ALM", "8cdb20ee-9322-4423-8809-451b4071978f", "Delta", null, null, null, "HUB", DateTime.Parse("2014-09-22 04:48:16.000"), "8cdb20ee-9322-4423-8809-451b4071978f");
			AssertTransaction(transactions[3], DateTime.Parse("2014-09-22 04:49:16.000"), 1, "YASYJCPRD", null, null, "AMG", null, null, null, null, "Delta", null, "HUB", DateTime.Parse("2014-09-22 04:48:16.000"), null);
			// Message 3
			AssertTransaction(transactions[4], DateTime.Parse("2014-09-30 23:10:17.013"), 1, "ABCDEFXYZ", null, null, "AMG", "ALM", "6ca91142-d130-4f3d-bb50-63920449175b", "Traxon", null, null, null, "HUB", DateTime.Parse("2014-09-30 23:09:17.013"), "6ca91142-d130-4f3d-bb50-63920449175b");
			AssertTransaction(transactions[5], DateTime.Parse("2014-09-30 23:10:17.013"), 1, "ABCDEFXYZ", null, null, "AMG", "FHL", "1001", "2001", "101", "Traxon", null, "HUB", DateTime.Parse("2014-09-30 23:09:17.013"), null);
			// Message 4
			AssertTransaction(transactions[6], DateTime.Parse("2014-09-30 23:10:18.013"), 1, "ABCDEFXYZ", null, null, "AMG", "ALM", "065a7ffe-b71b-44ad-99db-fd542ef4ccae", "Traxon", null, null, null, "HUB", DateTime.Parse("2014-09-30 23:09:18.013"), "065a7ffe-b71b-44ad-99db-fd542ef4ccae");
			AssertTransaction(transactions[7], DateTime.Parse("2014-09-30 23:10:18.013"), 1, "ABCDEFXYZ", null, null, "AMG", "-HL", "1002", "2002", "101", "Traxon", null, "HUB", DateTime.Parse("2014-09-30 23:09:18.013"), null);
			// Message 5
			AssertTransaction(transactions[8], DateTime.Parse("2014-09-30 23:10:19.013"), 1, "ABCDEFXYZ", null, null, "AMG", "ALM", "a703cfdc-cae5-4ab2-a3d4-09a4a6ddea85", "Traxon", null, null, null, "HUB", DateTime.Parse("2014-09-30 23:09:19.013"), "a703cfdc-cae5-4ab2-a3d4-09a4a6ddea85");
			AssertTransaction(transactions[9], DateTime.Parse("2014-09-30 23:10:19.013"), 1, "ABCDEFXYZ", null, null, "AMG", "-WB", "1003", "2003", "101", "Traxon", null, "HUB", DateTime.Parse("2014-09-30 23:09:19.013"), null);
			// Message 6
			AssertTransaction(transactions[10], DateTime.Parse("2014-09-30 23:10:20.013"), 1, "ABCDEFXYZ", null, null, "AMG", "ALM", "bd5ef72b-6d2e-49e8-b39f-127062239785", "TraxonEDP", null, null, null, "HUB", DateTime.Parse("2014-09-30 23:09:20.013"), "bd5ef72b-6d2e-49e8-b39f-127062239785");
			AssertTransaction(transactions[11], DateTime.Parse("2014-09-30 23:10:20.013"), 1, "ABCDEFXYZ", null, null, "AMG", "FSU", "1004", "2004", "101", "TraxonEDP", "RCF", "HUB", DateTime.Parse("2014-09-30 23:09:20.013"), null);
			// Message 7
			AssertTransaction(transactions[12], DateTime.Parse("2014-09-30 23:10:21.013"), 1, "ABCDEFXYZ", null, null, "AMG", "ALM", "acc5e337-af74-473e-b367-7c91341ae7cd", "TraxonRCF", null, null, null, "HUB", DateTime.Parse("2014-09-30 23:09:21.013"), "acc5e337-af74-473e-b367-7c91341ae7cd");
                        AssertTransaction(transactions[13], DateTime.Parse("2014-09-30 23:10:21.013"), 1, "ABCDEFXYZ", null, null, "AMG", "FMA", "1005", "2005", "101", "TraxonRCF", null, "HUB", DateTime.Parse("2014-09-30 23:09:21.013"), null);
                        // Message 8 - Air_CARGO_MESSAGEING sender with subject override
                        AssertTransaction(transactions[14], DateTime.Parse("2014-09-30 23:10:22.013"), 1, "OVERRIDECLIENT", null, null, "AMG", "ALM", "11111111-1111-1111-1111-111111111111", "APROVIDER", null, null, null, "HUB", DateTime.Parse("2014-09-30 23:09:22.013"), "11111111-1111-1111-1111-111111111111");
                        AssertTransaction(transactions[15], DateTime.Parse("2014-09-30 23:10:22.013"), 1, "OVERRIDECLIENT", null, null, "AMG", "FWB", "16046397299", null, "CX", "APROVIDER", null, "HUB", DateTime.Parse("2014-09-30 23:09:22.013"), null);
                }

		protected override void AssertLogs()
		{
			var corruptedMessageDateTime = DateTime.Parse("2014-09-22 04:48:16.000");
			var corruptedMessageTimeStamp = DateTime.Parse("2014-09-22 04:49:16.000");

			var errorMessage = "Unexpected NULL value while running plugin [CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.AirMessaging]";
			var exceptionMessages = new []
			{
				$"Unexpected NULL value in the field MAWB: [ClientID: YASYJCPRD, MsgType: , AirlineCodeOrPrefix: , Network: Delta, MAWB: , HAWB: , StatusCode: , AM_ReceivedFromSenderUTC: {corruptedMessageDateTime}, MessageTrackingID: 8cdb20ee-9322-4423-8809-451b4071978f, AM_ArchivedUTC: {corruptedMessageTimeStamp}]",
				$"Unexpected NULL value in the field AirlineCodeOrPrefix: [ClientID: YASYJCPRD, MsgType: , AirlineCodeOrPrefix: , Network: Delta, MAWB: , HAWB: , StatusCode: , AM_ReceivedFromSenderUTC: {corruptedMessageDateTime}, MessageTrackingID: 8cdb20ee-9322-4423-8809-451b4071978f, AM_ArchivedUTC: {corruptedMessageTimeStamp}]",
				$"Unexpected NULL value in the field MsgType: [ClientID: YASYJCPRD, MsgType: , AirlineCodeOrPrefix: , Network: Delta, MAWB: , HAWB: , StatusCode: , AM_ReceivedFromSenderUTC: {corruptedMessageDateTime}, MessageTrackingID: 8cdb20ee-9322-4423-8809-451b4071978f, AM_ArchivedUTC: {corruptedMessageTimeStamp}]"
			};

			foreach (var msg in exceptionMessages)
			{
				mockLogger.Verify(LoggerTestHelper.GetLogSetupExpression<Exception>(LogLevel.Error, errorMessage, ex => ex.Message.Equals(msg)), Times.Once);
			}

			foreach (var issue in issueDescriptions)
			{
				var doc = XDocument.Parse(issue);
				var key = doc.Descendants(XName.Get("Key")).First().Value;
				var issueMessage = doc.Descendants(XName.Get("Message")).First().Value;
				Assert.That(key, Is.EqualTo("Unexpected NULL value while running plugin [CargoWise.eServices.Billing.Collector.eHub.WindowsService.Plugins.AirMessaging]"));
				Assert.That(issueMessage, Does.StartWith("Unexpected NULL value in the field "));
			}
		}

		protected override void SetUpInternal()
		{
			var mockLoggerFactory = new Mock<ILoggerFactory>();
			loggerFactory = mockLoggerFactory.Object;
			base.SetUpInternal();
			mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
			ConfigurationManager.AppSettings.Set("IssueManagerUri", "http://thegloriouspurpose");
			mockPlugin.Setup(_ => _.PluginNamespace).Returns(typeof(Plugins.AirMessaging.Plugin).Namespace);
		}

		protected override void TearDownInternal()
		{
			base.TearDownInternal();
			ConfigurationManager.AppSettings.Set("IssueManagerUri", null);
		}
	}
}
