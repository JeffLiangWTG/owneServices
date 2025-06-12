using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class ACASTest : SqlBillingTransactionsPluginTest<Plugins.ACASMessaging.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "ClientID", "ShipmentNumber", "Purpose", "HAWBNumber", "MessageTrackingID", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC", "PortOfFirstArrival", "DocumentName" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] { "HYEBNEUAT", "SBN1BNEA54637772", "ORG", "S54637772U", Guid.Parse("60304315-e6a9-43f3-9394-6d6048563a23"), DateTime.Parse("2019-09-16 04:10:57.950"), DateTime.Parse("2019-09-16 04:50:00.950"), "AU", "ACAS Shipment Report" },
					new object[] { "HYEBNEUAT", "SBN1BNEA54637772", "AMD", "S54637772U", Guid.Parse("60304315-e6a9-43f3-9394-6d6048563a23"), DateTime.Parse("2019-09-16 04:10:57.950"), DateTime.Parse("2019-09-16 04:50:00.950"), "NZ", "ACAS Shipment Report" },
					new object[] { "HYEBNEUAT", "SBN1BNEA54637772", "WTH", "S54637772U", Guid.Parse("60304315-e6a9-43f3-9394-6d6048563a23"), DateTime.Parse("2019-09-16 04:10:57.950"), DateTime.Parse("2019-09-16 04:50:00.950"), "CN", "ACAS Shipment Report" },
					new object[] { "HYEBNEUAT", "SBN1BNEA54637772", "WTH", "S54637772U", Guid.Parse("60304315-e6a9-43f3-9394-6d6048563a23"), DateTime.Parse("2019-09-16 04:10:57.950"), DateTime.Parse("2019-09-16 04:50:00.950"), "BR", "Advanced Cargo Report" },
					new object[] { "HYEBNEUAT", "SBN1BNEA54637772", "WTH", "S54637772U", Guid.Parse("60304315-e6a9-43f3-9394-6d6048563a23"), DateTime.Parse("2019-09-16 04:10:57.950"), DateTime.Parse("2019-09-16 04:50:00.950"), "BR", "ACAS Shipment Report" }
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(QueryResult.Count));
			AssertTransaction(transactions[0], DateTime.Parse("2019-09-16 04:50:00.950"), 1, "HYEBNEUAT", null, null, "ACS", "CAU", "SBN1BNEA54637772", "ACAS Shipment Report", "Original", "S54637772U", "60304315-e6a9-43f3-9394-6d6048563a23", "HUB", DateTime.Parse("2019-09-16 04:10:57.950"), "60304315-e6a9-43f3-9394-6d6048563a23");
			AssertTransaction(transactions[1], DateTime.Parse("2019-09-16 04:50:00.950"), 1, "HYEBNEUAT", null, null, "ACS", "CAU", "SBN1BNEA54637772", "ACAS Shipment Report", "Amendment", "S54637772U", "60304315-e6a9-43f3-9394-6d6048563a23", "HUB", DateTime.Parse("2019-09-16 04:10:57.950"), "60304315-e6a9-43f3-9394-6d6048563a23");
			AssertTransaction(transactions[2], DateTime.Parse("2019-09-16 04:50:00.950"), 1, "HYEBNEUAT", null, null, "ACS", "CAU", "SBN1BNEA54637772", "ACAS Shipment Report", "Withdrawal", "S54637772U", "60304315-e6a9-43f3-9394-6d6048563a23", "HUB", DateTime.Parse("2019-09-16 04:10:57.950"), "60304315-e6a9-43f3-9394-6d6048563a23");
			AssertTransaction(transactions[3], DateTime.Parse("2019-09-16 04:50:00.950"), 1, "HYEBNEUAT", null, null, "ACS", "CAB", "SBN1BNEA54637772", "Advanced Cargo Report", "Withdrawal", "S54637772U", "60304315-e6a9-43f3-9394-6d6048563a23", "HUB", DateTime.Parse("2019-09-16 04:10:57.950"), "60304315-e6a9-43f3-9394-6d6048563a23");
			AssertTransaction(transactions[4], DateTime.Parse("2019-09-16 04:50:00.950"), 1, "HYEBNEUAT", null, null, "ACS", "CAB", "SBN1BNEA54637772", "ACAS Shipment Report", "Withdrawal", "S54637772U", "60304315-e6a9-43f3-9394-6d6048563a23", "HUB", DateTime.Parse("2019-09-16 04:10:57.950"), "60304315-e6a9-43f3-9394-6d6048563a23");
		}
	}
}
