using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests.OceanCarrierMessagingTests
{
	class CBRReceivedTest : SqlBillingTransactionsPluginTest<Plugins.OceanCarrierMessaging.CBRReceived.Plugin>
	{
		protected override string[] QueryColumns 
		{
			get
			{
				return new[] {"PriceItemCode", "ClientID", "MessageTrackingID", "AgentsReference", "BookingNumber", "BillNumber",
					"Sender", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC"};
			}
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {
						"CBR",
						"TSTCLIENT",
						new Guid("6c6a337b-8f55-46fa-b494-8c5871b73eb9"),
						"CBR2DDEE10S000688848",
						"SSYD54624547",
						"54642932",
						"1STOPTEST",
						DateTime.Parse("2014-09-30 23:09:17.014"),
						DateTime.Parse("2014-09-30 23:10:17.014")
					}
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(1));
			AssertTransaction(transactions[0], DateTime.Parse("2014-09-30 23:10:17.014"), 1, "TSTCLIENT", null, null, "SHI", "CBR", "6c6a337b-8f55-46fa-b494-8c5871b73eb9", "CBR2DDEE10S000688848", "SSYD54624547", "54642932", "1STOPTEST", "HUB", DateTime.Parse("2014-09-30 23:09:17.014"), "6c6a337b-8f55-46fa-b494-8c5871b73eb9");
		}
	}
}
