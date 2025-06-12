using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class ShippingInstructionSentTest : SqlBillingTransactionsPluginTest<Plugins.OceanCarrierMessaging.ShippingInstructionSent.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "1", "ClientID", "MessageTrackingID", "Provider", "Ref3", "Ref4", "Ref5", "DocumentName", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {1, "SEIHAMTS1", new Guid("6c6a337b-8f55-46fa-b494-8c5871b73eb9"), "INTTRA_SI", "C14SSEE00024068", "MSCU", "WER", "Shipping Instruction", DateTime.Parse("2014-09-30 23:09:17.014"), DateTime.Parse("2014-09-30 23:10:17.014")},
					new object[] {1, "SEIHAMTS1", new Guid("6c6a337b-8f55-46fa-b494-8c5871b73eb9"), "INT_TRA_SI", "C14SSEE00024068", "MSCU", "WER", "Verified Gross Container Weight", DateTime.Parse("2014-09-30 23:09:17.014"), DateTime.Parse("2014-09-30 23:10:17.014")},
					new object[] {1, "SEIHAMTS1", new Guid("6c6a337b-8f55-46fa-b494-8c5871b73eb9"), "INT_TRA_SI", "C14SSEE00024068", "MSCU", "WER", "Booking Request", DateTime.Parse("2014-09-30 23:09:17.014"), DateTime.Parse("2014-09-30 23:10:17.014")},
					new object[] {1, "SEIHAMTS1", new Guid("6c6a337b-8f55-46fa-b494-8c5871b73eb9"), "INT_TRA_SI", "C14SSEE00024068", "MSCU", "WER", "Shipping Order", DateTime.Parse("2014-09-30 23:09:17.014"), DateTime.Parse("2014-09-30 23:10:17.014")},
					new object[] {1, "SEIHAMTS1", new Guid("6c6a337b-8f55-46fa-b494-8c5871b73eb9"), "INT_TRA_SI", "C14SSEE00024068", "MSCU", "WER", "eManifest", DateTime.Parse("2014-09-30 23:09:17.014"), DateTime.Parse("2014-09-30 23:10:17.014")},
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(5));
			AssertTransaction(transactions[0], DateTime.Parse("2014-09-30 23:10:17.014"), 1, "SEIHAMTS1", null, null, "SHI", "SHI", "6c6a337b-8f55-46fa-b494-8c5871b73eb9", "INTTRA", "C14SSEE00024068", "MSCU", "WER", "HUB", DateTime.Parse("2014-09-30 23:09:17.014"), "6c6a337b-8f55-46fa-b494-8c5871b73eb9");
			AssertTransaction(transactions[1], DateTime.Parse("2014-09-30 23:10:17.014"), 1, "SEIHAMTS1", null, null, "SHI", "VGM", "6c6a337b-8f55-46fa-b494-8c5871b73eb9", "INT_TRA", "C14SSEE00024068", "MSCU", "WER", "HUB", DateTime.Parse("2014-09-30 23:09:17.014"), "6c6a337b-8f55-46fa-b494-8c5871b73eb9");
			AssertTransaction(transactions[2], DateTime.Parse("2014-09-30 23:10:17.014"), 1, "SEIHAMTS1", null, null, "SHI", "BRT", "6c6a337b-8f55-46fa-b494-8c5871b73eb9", "INT_TRA", "C14SSEE00024068", "MSCU", "WER", "HUB", DateTime.Parse("2014-09-30 23:09:17.014"), "6c6a337b-8f55-46fa-b494-8c5871b73eb9");
			AssertTransaction(transactions[3], DateTime.Parse("2014-09-30 23:10:17.014"), 1, "SEIHAMTS1", null, null, "SHI", "SHO", "6c6a337b-8f55-46fa-b494-8c5871b73eb9", "INT_TRA", "C14SSEE00024068", "MSCU", "WER", "HUB", DateTime.Parse("2014-09-30 23:09:17.014"), "6c6a337b-8f55-46fa-b494-8c5871b73eb9");
			AssertTransaction(transactions[4], DateTime.Parse("2014-09-30 23:10:17.014"), 1, "SEIHAMTS1", null, null, "SHI", "EMN", "6c6a337b-8f55-46fa-b494-8c5871b73eb9", "INT_TRA", "C14SSEE00024068", "MSCU", "WER", "HUB", DateTime.Parse("2014-09-30 23:09:17.014"), "6c6a337b-8f55-46fa-b494-8c5871b73eb9");
		}
	}
}
