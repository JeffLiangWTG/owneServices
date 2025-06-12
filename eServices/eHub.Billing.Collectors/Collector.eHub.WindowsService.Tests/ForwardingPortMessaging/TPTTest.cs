using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests.ForwardingPortMessaging
{
	class TPTTest : SqlBillingTransactionsPluginTest<Plugins.ForwardingPortMessaging.TPT.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "ClientID", "PriceItemCode", "Reference1Prefix", "ImportReferenceNumber", "ExportReferenceNumber", "MessageType", "DataVersion", "ActionCode", "EventType", "OrderNumber", "MessageTrackingID", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] { "TSTCLIENT", "POZ", "C00004522", "IRN0082493", "", "Service Instruction", "1", "ORG", string.Empty, string.Empty, Guid.Parse("c50612fd-e358-4bf2-bba2-9195049e47f0"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")},
					new object[] { "TSTCLIENT", "POZ", "C00421342", "", "IRN0000978", "Service Instruction", "1", "ORG", string.Empty, string.Empty, Guid.Parse("dcc999fc-32d6-4995-a597-2a17ce7b602c"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")},
					new object[] { "TSTCLIENT", "POZ", "C00000982", "IRN9237423", "", "Service Instruction", "5", "WTH", string.Empty, string.Empty, Guid.Parse("41c0058e-bc63-4525-ba8b-886437f2f82b"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")},
					new object[] { "TSTCLIENT", "POZ", "C00679358", "IRN5672834", "", "Service Instruction", "9", "AMD", string.Empty, string.Empty, Guid.Parse("406aa066-1c33-4825-8fd0-f1221165683e"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")},
					new object[] { "TSTCLIENT", "PSZ", "C00679358", "IRN5672834", "", "Port Status", string.Empty, string.Empty, "MAA", "3705518950", Guid.Parse("d496a06b-7a5f-48c0-8004-3c242649b0c3"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")},
					new object[] { "TSTCLIENT", "PSZ", "C00679358", "IRN5672834", "", "Port Status", string.Empty, string.Empty, "MRJ", "3705518950", Guid.Parse("0cc65f7b-76c9-4702-8991-79af801603d4"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")},
					new object[] { "TSTCLIENT", "PSZ", "C00679358", "IRN5672834", "", "Port Status", string.Empty, string.Empty, "MWA", "3705518950", Guid.Parse("ec0ed156-b843-41c8-a01d-b704473a3994"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")},
					new object[] { "TSTCLIENT", "PSZ", "C00679358", "IRN5672834", "", "Port Status", string.Empty, string.Empty, "STU", "3705518950", Guid.Parse("790be798-2f09-4ba2-bc9a-458648472b6b"), DateTime.Parse("2015-01-11 09:31:00.000"), DateTime.Parse("2015-01-11 09:35:00.000")},
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(QueryResult.Count));
			AssertTransaction(transactions[0], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "POZ", "C00004522_IRN0082493", "Service Instruction", "Original", "c50612fd-e358-4bf2-bba2-9195049e47f0", string.Empty, "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "c50612fd-e358-4bf2-bba2-9195049e47f0");
			AssertTransaction(transactions[1], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "POZ", "C00421342_IRN0000978", "Service Instruction", "Original", "dcc999fc-32d6-4995-a597-2a17ce7b602c", string.Empty, "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "dcc999fc-32d6-4995-a597-2a17ce7b602c");
			AssertTransaction(transactions[2], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "POZ", "C00000982_IRN9237423", "Service Instruction", "Withdrawal", "41c0058e-bc63-4525-ba8b-886437f2f82b", string.Empty, "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "41c0058e-bc63-4525-ba8b-886437f2f82b");
			AssertTransaction(transactions[3], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "POZ", "C00679358_IRN5672834", "Service Instruction", "Amendment", "406aa066-1c33-4825-8fd0-f1221165683e", string.Empty, "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "406aa066-1c33-4825-8fd0-f1221165683e");
			AssertTransaction(transactions[4], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "PSZ", "C00679358_IRN5672834", "MAA", "Port Status", "3705518950", "d496a06b-7a5f-48c0-8004-3c242649b0c3", "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "d496a06b-7a5f-48c0-8004-3c242649b0c3");
			AssertTransaction(transactions[5], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "PSZ", "C00679358_IRN5672834", "MRJ", "Port Status", "3705518950", "0cc65f7b-76c9-4702-8991-79af801603d4", "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "0cc65f7b-76c9-4702-8991-79af801603d4");
			AssertTransaction(transactions[6], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "PSZ", "C00679358_IRN5672834", "MWA", "Port Status", "3705518950", "ec0ed156-b843-41c8-a01d-b704473a3994", "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "ec0ed156-b843-41c8-a01d-b704473a3994");
			AssertTransaction(transactions[7], DateTime.Parse("2015-01-11 09:35:00.000"), 1, "TSTCLIENT", null, null, "PMG", "PSZ", "C00679358_IRN5672834", "STU", "Port Status", "3705518950", "790be798-2f09-4ba2-bc9a-458648472b6b", "HUB", DateTime.Parse("2015-01-11 09:31:00.000"), "790be798-2f09-4ba2-bc9a-458648472b6b");
		}
	}
}
