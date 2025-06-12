using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using Microsoft.Extensions.Logging;
using Moq;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class PortBaseTest : SqlBillingTransactionsPluginTest<Plugins.ForwardingPortMessaging.PortBase.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] {"ClientID", "PriceItemCode", "Reference1", "Reference2", "Reference3", "Reference4", "MessageTrackingID", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC"}; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] { "JASFRAHST", "PMN", "C600799753", "16DE930346632679E1", "PORTBASE", DBNull.Value, Guid.Parse("746f72ac-5c9f-4af9-9aeb-003af1babd7d"), DateTime.Parse("2014-09-29 11:57:13.443"), DateTime.Parse("2014-09-30 02:52:04.407") },
					new object[] { "JASFRAHST", "PMN", "C600799752", "16DE930346632679E1", "PORTBASE", DBNull.Value, Guid.Parse("746f72ac-5c9f-4af9-9aeb-003af1babd7d"), DateTime.Parse("2014-09-29 11:57:13.443"), DateTime.Parse("2014-09-30 02:52:04.407") },
					new object[] { "JASFRAHST", "PMN", "C600799751", DBNull.Value, "PORTBASE", DBNull.Value, Guid.Parse("746f72ac-5c9f-4af9-9aeb-003af1babd7d"), DateTime.Parse("2014-09-29 11:57:13.443"), DateTime.Parse("2014-09-30 02:52:04.407") },
					new object[] { "JASFRAHST", "PSN", "S601435519", "Z16008965643", "PORTBASE", "1", Guid.Parse("9ab07aa1-7b4b-4dbb-abf5-49194fc8bf53"), DateTime.Parse("2014-09-30 06:00:36.730"), DateTime.Parse("2014-09-30 06:11:03.130") },
					new object[] { "JASFRAHST", "PSN", "S601435519", "Z16008965643", "PORTBASE", DBNull.Value, Guid.Parse("9ab07aa1-7b4b-4dbb-abf5-49194fc8bf53"), DateTime.Parse("2014-09-30 06:00:36.730"), DateTime.Parse("2014-09-30 06:11:03.130") }
				};
			}
		}

		protected override void AssertLogs()
		{
			mockLogger.Verify(LoggerTestHelper.GetLogSetupExpression(LogLevel.Error), Times.Never);
			mockLogger.Verify(LoggerTestHelper.GetLogSetupExpression<Exception>(LogLevel.Error), Times.Never);
			mockLogger.Verify(LoggerTestHelper.GetLogSetupExpression(LogLevel.Warning), Times.Never);
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			AssertTransaction(transactions[0], DateTime.Parse("2014-09-30 02:52:04.407"), 1, "JASFRAHST", null, null, "PMG", "PMN", "C600799753", "16DE930346632679E1", "PORTBASE", null, null, "HUB", DateTime.Parse("2014-09-29 11:57:13.443"), "746f72ac-5c9f-4af9-9aeb-003af1babd7d");
			AssertTransaction(transactions[1], DateTime.Parse("2014-09-30 02:52:04.407"), 1, "JASFRAHST", null, null, "PMG", "PMN", "C600799752", "16DE930346632679E1", "PORTBASE", null, null, "HUB", DateTime.Parse("2014-09-29 11:57:13.443"), "746f72ac-5c9f-4af9-9aeb-003af1babd7d");
			AssertTransaction(transactions[2], DateTime.Parse("2014-09-30 02:52:04.407"), 1, "JASFRAHST", null, null, "PMG", "PMN", "C600799751", null, "PORTBASE", null, null, "HUB", DateTime.Parse("2014-09-29 11:57:13.443"), "746f72ac-5c9f-4af9-9aeb-003af1babd7d");
			AssertTransaction(transactions[3], DateTime.Parse("2014-09-30 06:11:03.130"), 1, "JASFRAHST", null, null, "PMG", "PSN", "S601435519", "Z16008965643", "PORTBASE", "STU(1)", null, "HUB", DateTime.Parse("2014-09-30 06:00:36.730"), "9ab07aa1-7b4b-4dbb-abf5-49194fc8bf53");
			AssertTransaction(transactions[4], DateTime.Parse("2014-09-30 06:11:03.130"), 1, "JASFRAHST", null, null, "PMG", "PSN", "S601435519", "Z16008965643", "PORTBASE", "STU", null, "HUB", DateTime.Parse("2014-09-30 06:00:36.730"), "9ab07aa1-7b4b-4dbb-abf5-49194fc8bf53");
		}
	}
}
