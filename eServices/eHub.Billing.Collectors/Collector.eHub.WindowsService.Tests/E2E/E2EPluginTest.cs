using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class E2EPluginTest : SqlBillingTransactionsPluginTest<Plugins.E2E.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "SenderID", "RecipientID", "AM_SentToRecipientUTC",  "MessageTrackingID", "TypeKeysCollection", "AM_ArchivedUTC", "UniType" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] {"TSTENTSNT", "TSTENTRCV", DateTime.Parse("2014-09-30 11:22:33.456"), new Guid("e87afea3-aa3f-40c5-9faf-fdf4dbf93682"), "TransportBookingConsolidationCM00747875:ForwardingConsolC02386421:ForwardingShipmentSTMF0014973:;TransportBookingConsolidationCM00747875:TransportBookingTB00866160::", DateTime.Parse("2014-09-30 11:22:44.777"), "SHIPMENT"},
                    new object[] {"TSTENTSNT", "TSTENTRCV", DateTime.Parse("2014-09-30 11:22:33.456"), new Guid("e87afea3-aa3f-40c5-9faf-fdf4dbf93682"), "ForwardingConsolC00616447:ForwardingShipmentS00971190::", DateTime.Parse("2014-09-30 11:22:44.777"), "SHIPMENT"},
                    new object[] {"TSTENTSNT", "TSTENTRCV", DateTime.Parse("2014-09-30 11:22:33.456"), new Guid("e87afea3-aa3f-40c5-9faf-fdf4dbf93682"), "TypeKeyWhoseLengthLongerThan50_____________________________________________________________________:::", DateTime.Parse("2014-09-30 11:22:44.777"), "SHIPMENT"},
                    new object[] {"TSTENTSNT", "TSTENTRCV", DateTime.Parse("2014-09-30 11:22:33.456"), new Guid("e87afea3-aa3f-40c5-9faf-fdf4dbf93682"), "::::", DateTime.Parse("2014-09-30 11:22:44.777"), "SHIPMENT"},
                    new object[] {"TSTENTSNT", "TSTENTRCV", DateTime.Parse("2014-09-30 11:22:33.456"), new Guid("e87afea3-aa3f-40c5-9faf-fdf4dbf93682"), "AccountingInvoiceAP INV 00001001", DateTime.Parse("2014-09-30 11:22:44.777"), "TRANSACTION"},
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(8));
			AssertTransaction(transactions[0], DateTime.Parse("2014-09-30 11:22:44.777"), 1, "TSTENTRCV", null, null, "E2E", "E2E", "TSTENTSNT", "TransportBookingConsolidationCM00747875", null, null, null, "HUB", DateTime.Parse("2014-09-30 11:22:33.456"), "e87afea3-aa3f-40c5-9faf-fdf4dbf93682");
			AssertTransaction(transactions[1], DateTime.Parse("2014-09-30 11:22:44.777"), 1, "TSTENTRCV", null, null, "E2E", "E2E", "TSTENTSNT", "TransportBookingConsolidationCM00747875", "ForwardingConsolC02386421", null, null, "HUB", DateTime.Parse("2014-09-30 11:22:33.456"), "e87afea3-aa3f-40c5-9faf-fdf4dbf93682");
			AssertTransaction(transactions[2], DateTime.Parse("2014-09-30 11:22:44.777"), 1, "TSTENTRCV", null, null, "E2E", "E2E", "TSTENTSNT", "TransportBookingConsolidationCM00747875", "ForwardingConsolC02386421", "ForwardingShipmentSTMF0014973", null, "HUB", DateTime.Parse("2014-09-30 11:22:33.456"), "e87afea3-aa3f-40c5-9faf-fdf4dbf93682");
			AssertTransaction(transactions[3], DateTime.Parse("2014-09-30 11:22:44.777"), 1, "TSTENTRCV", null, null, "E2E", "E2E", "TSTENTSNT", "TransportBookingConsolidationCM00747875", "TransportBookingTB00866160", null, null, "HUB", DateTime.Parse("2014-09-30 11:22:33.456"), "e87afea3-aa3f-40c5-9faf-fdf4dbf93682");
            AssertTransaction(transactions[4], DateTime.Parse("2014-09-30 11:22:44.777"), 1, "TSTENTRCV", null, null, "E2E", "E2E", "TSTENTSNT", "ForwardingConsolC00616447", null, null, null, "HUB", DateTime.Parse("2014-09-30 11:22:33.456"), "e87afea3-aa3f-40c5-9faf-fdf4dbf93682");
            AssertTransaction(transactions[5], DateTime.Parse("2014-09-30 11:22:44.777"), 1, "TSTENTRCV", null, null, "E2E", "E2E", "TSTENTSNT", "ForwardingConsolC00616447", "ForwardingShipmentS00971190", null, null, "HUB", DateTime.Parse("2014-09-30 11:22:33.456"), "e87afea3-aa3f-40c5-9faf-fdf4dbf93682");
            AssertTransaction(transactions[6], DateTime.Parse("2014-09-30 11:22:44.777"), 1, "TSTENTRCV", null, null, "E2E", "E2E", "TSTENTSNT", "TypeKeyWhoseLengthLongerThan50____________________", null, null, null, "HUB", DateTime.Parse("2014-09-30 11:22:33.456"), "e87afea3-aa3f-40c5-9faf-fdf4dbf93682");
            AssertTransaction(transactions[7], DateTime.Parse("2014-09-30 11:22:44.777"), 1, "TSTENTRCV", null, null, "E2E", "E2T", "TSTENTSNT", "AccountingInvoiceAP INV 00001001", null, null, null, "HUB", DateTime.Parse("2014-09-30 11:22:33.456"), "e87afea3-aa3f-40c5-9faf-fdf4dbf93682");
		}
	}
}
