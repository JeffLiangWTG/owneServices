using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class ShippingInstructionReceivedTest : SqlBillingTransactionsPluginTest<Plugins.OceanCarrierMessaging.ShippingInstructionReceived.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "1", "ClientID", "MessageTrackingID", "Provider", "JobNumber", "EventType", "RefNumber", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {1, "SEIHAMTS1", new Guid("a54aa5bf-0cba-409e-9930-16ecdf5f2923"), "INTTRA", "JOB123","EVT", "REF123", DateTime.Parse("2014-09-30 23:09:17.014"), DateTime.Parse("2014-09-30 23:10:17.014")},
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(1));
			AssertTransaction(transactions[0], DateTime.Parse("2014-09-30 23:10:17.014"), 1, "SEIHAMTS1", null, null, "SHI", "SHR", "a54aa5bf-0cba-409e-9930-16ecdf5f2923", "INTTRA", "JOB123", "EVT", "REF123", "HUB", DateTime.Parse("2014-09-30 23:09:17.014"), "a54aa5bf-0cba-409e-9930-16ecdf5f2923");
		}
	}
}
