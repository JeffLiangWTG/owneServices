using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class DAKOSYTest : SqlBillingTransactionsPluginTest<Plugins.ForwardingPortMessaging.DAKOSY.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] {"ClientID", "MessagePurpose", "Reference1", "Reference2", "MessageTrackingID", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC", "ServiceProvider"}; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {"SEIHAMHAM", "HDS", "C14SSEE00022876", "S14SSEE0034255", Guid.Parse("c94ec031-6ceb-472a-b07c-2bbf1d554c0f"), DateTime.Parse("2014-09-30 23:09:16.013"), DateTime.Parse("2014-09-30 23:10:16.013"), "DAKOSYHAM"},
					new object[] {"SEIHAMHAM", "S01", "C14SSEE00022664", DBNull.Value, Guid.Parse("b449160d-b63d-4fce-a46d-7aebca5e3c06"), DateTime.Parse("2014-09-30 23:09:17.014"), DateTime.Parse("2014-09-30 23:10:17.014"), "DAKOSYHAM"},
					new object[] {"SEIHAMHAM", "S02", "C15SSEE00031802", "S15SSEE0046226", Guid.Parse("09debd35-5bf0-4bbc-b1fb-7985a6605c43"), DateTime.Parse("2014-09-30 23:09:18.015"), DateTime.Parse("2014-09-30 23:10:18.015"), "DAKOSYHAM"},
					new object[] {"SEIBELHAM", "S03", "C15SBEL00001067", "S15SBEL0002382", Guid.Parse("c702d72e-07d2-4de8-84f1-89984ae8461d"), DateTime.Parse("2014-09-30 23:09:19.015"), DateTime.Parse("2014-09-30 23:10:19.015"), "DAKOSYHAM"},
					new object[] {"HYEDDEUAT", "G08", "C00676817", "S300000835", Guid.Parse("0e418c32-7ce2-4caa-9e78-4adcf89dba26"), DateTime.Parse("2014-09-30 23:09:20.015"), DateTime.Parse("2014-09-30 23:10:20.015"), "DAKOSYHAM"},
					new object[] { "JASFRAHST", "PM3", "S601435519", "Z16008965643", Guid.Parse("9ab07aa1-7b4b-4dbb-abf5-49194fc8bf53"), DateTime.Parse("2014-09-30 06:00:36.730"), DateTime.Parse("2014-09-30 06:11:03.130"), "DAKOSYHAM"},
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(QueryResult.Count));
			AssertTransaction(transactions[0], DateTime.Parse("2014-09-30 23:10:16.013"), 1, "SEIHAMHAM", null, null, "PMG", "PM1", "C14SSEE00022876", "S14SSEE0034255", null, "c94ec031-6ceb-472a-b07c-2bbf1d554c0f", "DAKOSYHAM", "HUB", DateTime.Parse("2014-09-30 23:09:16.013"), "c94ec031-6ceb-472a-b07c-2bbf1d554c0f");
			AssertTransaction(transactions[1], DateTime.Parse("2014-09-30 23:10:17.014"), 1, "SEIHAMHAM", null, null, "PMG", "PM2", "C14SSEE00022664", null, null, "b449160d-b63d-4fce-a46d-7aebca5e3c06", "DAKOSYHAM", "HUB", DateTime.Parse("2014-09-30 23:09:17.014"), "b449160d-b63d-4fce-a46d-7aebca5e3c06");
			AssertTransaction(transactions[2], DateTime.Parse("2014-09-30 23:10:18.015"), 1, "SEIHAMHAM", null, null, "PMG", "PM2", "C15SSEE00031802", "S15SSEE0046226", null, "09debd35-5bf0-4bbc-b1fb-7985a6605c43", "DAKOSYHAM", "HUB", DateTime.Parse("2014-09-30 23:09:18.015"), "09debd35-5bf0-4bbc-b1fb-7985a6605c43");
			AssertTransaction(transactions[3], DateTime.Parse("2014-09-30 23:10:19.015"), 1, "SEIBELHAM", null, null, "PMG", "PM2", "C15SBEL00001067", "S15SBEL0002382", null, "c702d72e-07d2-4de8-84f1-89984ae8461d", "DAKOSYHAM", "HUB", DateTime.Parse("2014-09-30 23:09:19.015"), "c702d72e-07d2-4de8-84f1-89984ae8461d");
			AssertTransaction(transactions[4], DateTime.Parse("2014-09-30 23:10:20.015"), 1, "HYEDDEUAT", null, null, "PMG", "PM2", "C00676817", "S300000835", null, "0e418c32-7ce2-4caa-9e78-4adcf89dba26", "DAKOSYHAM", "HUB", DateTime.Parse("2014-09-30 23:09:20.015"), "0e418c32-7ce2-4caa-9e78-4adcf89dba26");
			AssertTransaction(transactions[5], DateTime.Parse("2014-09-30 06:11:03.130"), 1, "JASFRAHST", null, null, "PMG", "PM3", "S601435519", "Z16008965643", null, "9ab07aa1-7b4b-4dbb-abf5-49194fc8bf53", "DAKOSYHAM", "HUB", DateTime.Parse("2014-09-30 06:00:36.730"), "9ab07aa1-7b4b-4dbb-abf5-49194fc8bf53");
		}
	}
}
