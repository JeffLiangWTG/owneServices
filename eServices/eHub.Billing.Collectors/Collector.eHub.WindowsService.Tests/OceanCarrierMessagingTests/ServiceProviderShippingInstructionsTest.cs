using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class ServiceProviderShippingInstructionsTest : SqlBillingTransactionsPluginTest<Plugins.OceanCarrierMessaging.ServiceProviderShippingInstructions.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "1", "PriceItemCode", "ClientID", "MessageTrackingID", "ServiceProvider", "Consol", "CoLoadBookingConfirmationReference", "BookingConfirmationReference", "NVOCCSCAC", "NVOCCCW1", "CarrierSCAC", "CarrierCW1", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {1, "SHN","SEIHAMTS1", new Guid("6c6a337b-8f55-46fa-b494-8c5871b73eb9"), "INTTRA_SI", "C600726188", "605247251", null, "AAAA", "BBBB", "CCCC", "DDDD", DateTime.Parse("2014-09-30 23:09:17.014"), DateTime.Parse("2014-09-30 23:10:17.014")},
					new object[] {1, "SHN","SEIHAMTS1", new Guid("6c6a337b-8f55-46fa-b494-8c5871b73eb9"), "INTTRA_SI", "C600726188", "605247251", null, null, "BBBB", "CCCC", "DDDD", DateTime.Parse("2014-09-30 23:09:17.014"), DateTime.Parse("2014-09-30 23:10:17.014")},
					new object[] {1, "SHN","SEIHAMTS1", new Guid("6c6a337b-8f55-46fa-b494-8c5871b73eb9"), "INTTRA_SI", "C600726188", "605247251", null, null, null, "CCCC", "DDDD", DateTime.Parse("2014-09-30 23:09:17.014"), DateTime.Parse("2014-09-30 23:10:17.014")},
					new object[] {1, "SHN","SEIHAMTS1", new Guid("6c6a337b-8f55-46fa-b494-8c5871b73eb9"), "INTTRA_SI", "C600726188", "605247251", null, null, null, null, "DDDD", DateTime.Parse("2014-09-30 23:09:17.014"), DateTime.Parse("2014-09-30 23:10:17.014")},
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(4));
			AssertTransaction(transactions[0], DateTime.Parse("2014-09-30 23:10:17.014"), 1, "SEIHAMTS1", null, null, "SHI", "SHN", "INTTRA", "SEIHAMTS1", "C600726188", "605247251", "AAAA", "HUB", DateTime.Parse("2014-09-30 23:09:17.014"), "6c6a337b-8f55-46fa-b494-8c5871b73eb9");
			AssertTransaction(transactions[1], DateTime.Parse("2014-09-30 23:10:17.014"), 1, "SEIHAMTS1", null, null, "SHI", "SHN", "INTTRA", "SEIHAMTS1", "C600726188", "605247251", "BBBB", "HUB", DateTime.Parse("2014-09-30 23:09:17.014"), "6c6a337b-8f55-46fa-b494-8c5871b73eb9");
			AssertTransaction(transactions[2], DateTime.Parse("2014-09-30 23:10:17.014"), 1, "SEIHAMTS1", null, null, "SHI", "SHN", "INTTRA", "SEIHAMTS1", "C600726188", "605247251", "CCCC", "HUB", DateTime.Parse("2014-09-30 23:09:17.014"), "6c6a337b-8f55-46fa-b494-8c5871b73eb9");
			AssertTransaction(transactions[3], DateTime.Parse("2014-09-30 23:10:17.014"), 1, "SEIHAMTS1", null, null, "SHI", "SHN", "INTTRA", "SEIHAMTS1", "C600726188", "605247251", "DDDD", "HUB", DateTime.Parse("2014-09-30 23:09:17.014"), "6c6a337b-8f55-46fa-b494-8c5871b73eb9");
		}
	}
}
