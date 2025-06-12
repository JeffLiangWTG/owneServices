using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class ShippingPortMessagingPluginTest : SqlBillingTransactionsPluginTest<Plugins.ShippingPortMessaging.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "ClientID", "AM_ReceivedFromSenderUTC", "MessageTrackingID", "MessageRecipient", "RecipientRoleCode", "DataSourceKey", "ContainerReleaseNumber", "ContainerCount", "ContainerNumber", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {"CSHSYDSYD", DateTime.Parse("2015-05-21 02:13:33.960"), new Guid("3b58176a-1752-4979-befb-9afa1a982386"), "Port of Auckland COPARN", "PER", "V00319523", "CSG328836-1", "1", "", DateTime.Parse("2015-05-21 02:25:19.250")},

					new object[] {"HYEDNZUAT", DateTime.Parse("2015-05-05 06:21:35.287"), new Guid("c7ad70b7-85a0-40a1-9e6c-e3000778cb61"), "Port of Lyttelton COREOR", "PIR", "D00005081", "", "1", "PRIN3300334", DateTime.Parse("2015-05-05 06:38:45.690")},

					new object[] {"HYEDNZUAT", DateTime.Parse("2015-05-22 05:16:50.037"), new Guid("cdbaa14b-4483-47d9-8fa6-cf9fda09b897"), "Port of Auckland COPRAR Load", "PEM", "V00001874", null, null, null, DateTime.Parse("2015-05-22 05:25:31.520")},
					new object[] {"HYEDNZUAT", DateTime.Parse("2015-05-21 23:38:20.500"), new Guid("e7d8d0b0-ae4e-4a9a-888f-4a3487e830af"), "Port of Auckland COPRAR Discharge", "PIM", "V00001810", null, null, null, DateTime.Parse("2015-05-22 03:05:06.180")},
					
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(4));
			AssertTransaction(transactions[0], DateTime.Parse("2015-05-21 02:25:19.250"), 1, "CSHSYDSYD", null, null, "SPM", "SPA", "3b58176a-1752-4979-befb-9afa1a982386", "Port of Auckland COPARN", "PER", "CSG328836-1", "1", "HUB", DateTime.Parse("2015-05-21 02:13:33.960"), "3b58176a-1752-4979-befb-9afa1a982386");

			AssertTransaction(transactions[1], DateTime.Parse("2015-05-05 06:38:45.690"), 1, "HYEDNZUAT", null, null, "SPM", "SPE", "c7ad70b7-85a0-40a1-9e6c-e3000778cb61", "Port of Lyttelton COREOR", "PIR", "PRIN3300334", "D00005081", "HUB", DateTime.Parse("2015-05-05 06:21:35.287"), "c7ad70b7-85a0-40a1-9e6c-e3000778cb61");

			AssertTransaction(transactions[2], DateTime.Parse("2015-05-22 05:25:31.520"), 1, "HYEDNZUAT", null, null, "SPM", "SPR", "cdbaa14b-4483-47d9-8fa6-cf9fda09b897", "Port of Auckland COPRAR Load", "PEM", "V00001874", null, "HUB", DateTime.Parse("2015-05-22 05:16:50.037"), "cdbaa14b-4483-47d9-8fa6-cf9fda09b897");
			AssertTransaction(transactions[3], DateTime.Parse("2015-05-22 03:05:06.180"), 1, "HYEDNZUAT", null, null, "SPM", "SPR", "e7d8d0b0-ae4e-4a9a-888f-4a3487e830af", "Port of Auckland COPRAR Discharge", "PIM", "V00001810", null, "HUB", DateTime.Parse("2015-05-21 23:38:20.500"), "e7d8d0b0-ae4e-4a9a-888f-4a3487e830af");
		}
	}
}
