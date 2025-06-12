using System;
using System.Collections.Generic;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.WindowsService.Common.Tests;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.Collector.eHub.WindowsService.Tests
{
	class ZACustomsPluginTest : SqlBillingTransactionsPluginTest<Plugins.ZACustoms.Plugin>
	{
		protected override string[] QueryColumns
		{
			get { return new[] { "ClientID", "MessageName", "JobNumber", "CarrierCode", "FlightVoyage", "MasterBillNumber", "BillDate", "AM_ReceivedFromSenderUTC", "AM_ArchivedUTC", "MessageTrackingID" }; }
		}

		protected override IReadOnlyList<object[]> QueryResult
		{
			get
			{
				return new List<object[]>
				{
					new object[] { "SW1DURDUR", "HAB", "CAI17B303493", "235", "TK0044", "235-62857970", "20170314", DateTime.Parse("2014-10-20 11:31:33.013"), DateTime.Parse("2014-10-20 11:32:33.013"), new Guid("acc5e337-af74-473e-b367-7c91341ae7cd")},
					new object[] { "SW1DURDUR", "COH", "C00028163", "SAFM", "V87P", "SEABOLMANIFESTTES", "20170407", DateTime.Parse("2014-10-21 11:31:33.013"), DateTime.Parse("2014-10-21 11:32:33.013"), new Guid("1dce09cd-1d59-4a47-8b37-cd88b288ac17") },
					new object[] { "SW1DURDUR", "FFM", "C00028163", "SAFM", "V87P", "SEABOLMANIFESTTES", "20170407", DateTime.Parse("2014-10-22 11:31:33.013"), DateTime.Parse("2014-10-22 11:32:33.013"), new Guid("2dce09cd-1d59-4a47-8b37-cd88b288ac17") },
					new object[] { "SW1DURDUR", "ECL", "C00028163", "SAFM", "V87P", "SEABOLMANIFESTTES", "20170407", DateTime.Parse("2014-10-23 11:31:33.013"), DateTime.Parse("2014-10-23 11:32:33.013"), new Guid("3dce09cd-1d59-4a47-8b37-cd88b288ac17")},
					new object[] { "SW1DURDUR", "RFM", "C00028163", "SAFM", "V87P", "SEABOLMANIFESTTES", "20170407", DateTime.Parse("2014-10-24 11:31:33.013"), DateTime.Parse("2014-10-24 11:32:33.013"), new Guid("4dce09cd-1d59-4a47-8b37-cd88b288ac17")},
					new object[] { "SW1DURDUR", "BBB", "C00028163", "SAFM", "V87P", "SEABOLMANIFESTTES", "20170407", DateTime.Parse("2014-10-25 11:31:33.013"), DateTime.Parse("2014-10-25 11:32:33.013"), new Guid("5dce09cd-1d59-4a47-8b37-cd88b288ac17")},
					new object[] { "SW1DURDUR", "RMA", "C00028163", "SAFM", "V87P", "SEABOLMANIFESTTES", "20170407", DateTime.Parse("2014-10-26 11:31:33.013"), DateTime.Parse("2014-10-26 11:32:33.013"), new Guid("6dce09cd-1d59-4a47-8b37-cd88b288ac17")},
					new object[] { "SW1DURDUR", "COM", "C00028163", "SAFM", "V87P", "SEABOLMANIFESTTES", "20170407", DateTime.Parse("2014-10-27 11:31:33.013"), DateTime.Parse("2014-10-27 11:32:33.013"), new Guid("7dce09cd-1d59-4a47-8b37-cd88b288ac17")},
					new object[] { "SW1DURDUR", "FWB", "C00028163", "SAFM", "V87P", "SEABOLMANIFESTTES", "20170407", DateTime.Parse("2014-10-28 11:31:33.013"), DateTime.Parse("2014-10-28 11:32:33.013"), new Guid("8dce09cd-1d59-4a47-8b37-cd88b288ac17")},

					new object[] { "SW1DURDUR", "AQM", "C00028163", "SAFM", "V87P", "SEABOLMANIFESTTES", "20170407", DateTime.Parse("2018-12-01 11:31:33.013"), DateTime.Parse("2018-12-01 11:32:33.013"), new Guid("7347101b-457b-4bf2-9783-c070b6804792") },
					new object[] { "SW1DURDUR", "ALM", "C00028163", "SAFM", "V87P", "SEABOLMANIFESTTES", "20170407", DateTime.Parse("2018-12-02 11:31:33.013"), DateTime.Parse("2018-12-02 11:32:33.013"), new Guid("214e38e3-f684-45d7-bf9b-0d2c80669489") },
					new object[] { "SW1DURDUR", "ALH", "C00028163", "SAFM", "V87P", "SEABOLMANIFESTTES", "20170407", DateTime.Parse("2018-12-03 11:31:33.013"), DateTime.Parse("2018-12-03 11:32:33.013"), new Guid("a6c03c6e-0b48-4695-b4aa-00f0b7cd9c37") },
				};
			}
		}

		protected override void AssertTransactions(TimeStampedTransaction[] transactions)
		{
			Assert.That(transactions.Length, Is.EqualTo(12));
			AssertTransaction(transactions[0], DateTime.Parse("2014-10-20 11:32:33.013"), 1, "SW1DURDUR", null, null, "ZAC", "ZX1", "235" , "235-62857970"     , "TK0044", "CAI17B303493", "20170314", "HUB", DateTime.Parse("2014-10-20 11:31:33.013"), "acc5e337-af74-473e-b367-7c91341ae7cd");
			AssertTransaction(transactions[1], DateTime.Parse("2014-10-21 11:32:33.013"), 1, "SW1DURDUR", null, null, "ZAC", "ZX1", "SAFM", "SEABOLMANIFESTTES", "V87P"  , "C00028163"   , "20170407", "HUB", DateTime.Parse("2014-10-21 11:31:33.013"), "1dce09cd-1d59-4a47-8b37-cd88b288ac17");
			AssertTransaction(transactions[2], DateTime.Parse("2014-10-22 11:32:33.013"), 1, "SW1DURDUR", null, null, "ZAC", "ZX2", "SAFM", null               , "V87P"  , "C00028163"   , null      , "HUB", DateTime.Parse("2014-10-22 11:31:33.013"), "2dce09cd-1d59-4a47-8b37-cd88b288ac17");
			AssertTransaction(transactions[3], DateTime.Parse("2014-10-23 11:32:33.013"), 1, "SW1DURDUR", null, null, "ZAC", "ZX2", "SAFM", null               , "V87P"  , "C00028163"   , null      , "HUB", DateTime.Parse("2014-10-23 11:31:33.013"), "3dce09cd-1d59-4a47-8b37-cd88b288ac17");
			AssertTransaction(transactions[4], DateTime.Parse("2014-10-24 11:32:33.013"), 1, "SW1DURDUR", null, null, "ZAC", "ZX2", "SAFM", null               , "V87P"  , "C00028163"   , null      , "HUB", DateTime.Parse("2014-10-24 11:31:33.013"), "4dce09cd-1d59-4a47-8b37-cd88b288ac17");
			AssertTransaction(transactions[5], DateTime.Parse("2014-10-25 11:32:33.013"), 1, "SW1DURDUR", null, null, "ZAC", "ZX2", "SAFM", null               , "V87P"  , "C00028163"   , null      , "HUB", DateTime.Parse("2014-10-25 11:31:33.013"), "5dce09cd-1d59-4a47-8b37-cd88b288ac17");
			AssertTransaction(transactions[6], DateTime.Parse("2014-10-26 11:32:33.013"), 1, "SW1DURDUR", null, null, "ZAC", "ZX2", "SAFM", null               , "V87P"  , "C00028163"   , null      , "HUB", DateTime.Parse("2014-10-26 11:31:33.013"), "6dce09cd-1d59-4a47-8b37-cd88b288ac17");
			AssertTransaction(transactions[7], DateTime.Parse("2014-10-27 11:32:33.013"), 1, "SW1DURDUR", null, null, "ZAC", "ZX3", "SAFM", "20170407"         , "V87P"  , "C00028163"   , null      , "HUB", DateTime.Parse("2014-10-27 11:31:33.013"), "7dce09cd-1d59-4a47-8b37-cd88b288ac17");
			AssertTransaction(transactions[8], DateTime.Parse("2014-10-28 11:32:33.013"), 1, "SW1DURDUR", null, null, "ZAC", "ZX3", "SAFM", "20170407"         , "V87P"  , "C00028163"   , null      , "HUB", DateTime.Parse("2014-10-28 11:31:33.013"), "8dce09cd-1d59-4a47-8b37-cd88b288ac17");

			AssertTransaction(transactions[9], DateTime.Parse("2018-12-01 11:32:33.013"), 1, "SW1DURDUR", null, null, "ZAC", "ZX1", "SAFM", "SEABOLMANIFESTTES", "V87P", "C00028163", "20170407", "HUB", DateTime.Parse("2018-12-01 11:31:33.013"), "7347101b-457b-4bf2-9783-c070b6804792");
			AssertTransaction(transactions[10], DateTime.Parse("2018-12-02 11:32:33.013"), 1, "SW1DURDUR", null, null, "ZAC", "ZX1", "SAFM", "SEABOLMANIFESTTES", "V87P", "C00028163", "20170407", "HUB", DateTime.Parse("2018-12-02 11:31:33.013"), "214e38e3-f684-45d7-bf9b-0d2c80669489");
			AssertTransaction(transactions[11], DateTime.Parse("2018-12-03 11:32:33.013"), 1, "SW1DURDUR", null, null, "ZAC", "ZX1", "SAFM", "SEABOLMANIFESTTES", "V87P", "C00028163", "20170407", "HUB", DateTime.Parse("2018-12-03 11:31:33.013"), "a6c03c6e-0b48-4695-b4aa-00f0b7cd9c37");
		}
	}
}
