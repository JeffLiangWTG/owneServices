using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class NGBTest : SqlBillingTransactionsPluginTest<Plugins.ForwardingPortMessaging.NGB.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "ClientID", "PriceItemCode", "Reference1", "MessageType", "SubmissionType", "Reference4", "MessageTrackingID", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {"SEIHAMHAM", "PMR", "C00001235", "eTerminal Release Manifest", "ORG", "SOROE MAERSK-923E", Guid.Parse("c94ec031-6ceb-472a-b07c-2bbf1d554c0f"), DateTime.Parse("2014-09-30 23:09:16.013"), DateTime.Parse("2014-09-30 23:10:16.013")},
					new object[] {"SEIHAMHAM", "PMR", "C00001235", "eTerminal Release Manifest", "AMD", "SOROE MAERSK-923E", Guid.Parse("b449160d-b63d-4fce-a46d-7aebca5e3c06"), DateTime.Parse("2014-09-30 23:09:17.014"), DateTime.Parse("2014-09-30 23:10:17.014")},
					new object[] {"SEIHAMHAM", "PMR", "C00001235", "eTerminal Release Manifest", "WTH", "SOROE MAERSK-923E", Guid.Parse("09debd35-5bf0-4bbc-b1fb-7985a6605c43"), DateTime.Parse("2014-09-30 23:09:18.015"), DateTime.Parse("2014-09-30 23:10:18.015"), "DAKOSYHAM"},
					new object[] {"SEIHAMHAM", "PML", "C00001235", "Container Load Plan", "AMD", "MAEU6598978", Guid.Parse("c702d72e-07d2-4de8-84f1-89984ae8461d"), DateTime.Parse("2014-09-30 23:09:19.015"), DateTime.Parse("2014-09-30 23:10:19.015"), "DAKOSYHAM"},
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(QueryResult.Count));
			AssertTransaction(transactions[0], DateTime.Parse("2014-09-30 23:10:16.013"), 1, "SEIHAMHAM", null, null, "PMG", "PMR", "C00001235", "eTerminal Release Manifest", "Original", "SOROE MAERSK-923E", "c94ec031-6ceb-472a-b07c-2bbf1d554c0f", "HUB", DateTime.Parse("2014-09-30 23:09:16.013"), "c94ec031-6ceb-472a-b07c-2bbf1d554c0f");
			AssertTransaction(transactions[1], DateTime.Parse("2014-09-30 23:10:17.014"), 1, "SEIHAMHAM", null, null, "PMG", "PMR", "C00001235", "eTerminal Release Manifest", "Amendment", "SOROE MAERSK-923E", "b449160d-b63d-4fce-a46d-7aebca5e3c06", "HUB", DateTime.Parse("2014-09-30 23:09:17.014"), "b449160d-b63d-4fce-a46d-7aebca5e3c06");
			AssertTransaction(transactions[2], DateTime.Parse("2014-09-30 23:10:18.015"), 1, "SEIHAMHAM", null, null, "PMG", "PMR", "C00001235", "eTerminal Release Manifest", "Withdraw", "SOROE MAERSK-923E", "09debd35-5bf0-4bbc-b1fb-7985a6605c43", "HUB", DateTime.Parse("2014-09-30 23:09:18.015"), "09debd35-5bf0-4bbc-b1fb-7985a6605c43");
			AssertTransaction(transactions[3], DateTime.Parse("2014-09-30 23:10:19.015"), 1, "SEIHAMHAM", null, null, "PMG", "PML", "C00001235", "Container Load Plan", "Amendment", "MAEU6598978", "c702d72e-07d2-4de8-84f1-89984ae8461d", "HUB", DateTime.Parse("2014-09-30 23:09:19.015"), "c702d72e-07d2-4de8-84f1-89984ae8461d");
		}
	}
}
