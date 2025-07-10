using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.ServiceTask.Testing
{
	public class SalesTradeLanesSynchronisationOrgQueueTest : TestCaseWithFactory
	{
		public void TestInterfaces()
		{
			OrganisationRegistry.Instance.FullTradeLanesSyncDateTimeThreshold.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2000, 1, 1));

			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			var org5 = Factory.NewWithValidTestData<OrgHeader>();

			org1.Logs.AddNew(AutoEvents.CargoReportRecordsCreated, "aaaa", new ZDateTimeOffset(2001, 1, 1), null);
			org2.Logs.AddNew(AutoEvents.SalesTradeLanesSynchronised, TradeLinesSynchroniser.SynchronisedAllReference + " 199001", new ZDateTimeOffset(1990, 1, 1), null);
			org3.Logs.AddNew(AutoEvents.SalesTradeLanesSynchronised, TradeLinesSynchroniser.SynchronisedAllReference + " 200101", new ZDateTimeOffset(2001, 1, 1), null);
			org4.Logs.AddNew(AutoEvents.SalesTradeLanesSynchronised, TradeLinesSynchroniser.SynchronisedPartialReference + " 200101", new ZDateTimeOffset(2002, 1, 1), null);
			org5.Logs.AddNew(AutoEvents.SalesTradeLanesSynchronised, TradeLinesSynchroniser.SynchronisedPartialReference + " 200201", new ZDateTimeOffset(2002, 1, 1), null);
			org5.Logs.AddNew(AutoEvents.SalesTradeLanesSynchronised, TradeLinesSynchroniser.SynchronisedAllReference + " 200301", new ZDateTimeOffset(2003, 1, 1), null);

			Factory.Save();

			var sql =
$@"CREATE TABLE {CachedTradeLinesSummaryProvider.TradeLineCacheTableName} (MainOrg uniqueidentifier);
INSERT INTO {CachedTradeLinesSummaryProvider.TradeLineCacheTableName} (MainOrg) VALUES ('{org5.PK}');
INSERT INTO {CachedTradeLinesSummaryProvider.TradeLineCacheTableName} (MainOrg) VALUES ('{org4.PK}');
INSERT INTO {CachedTradeLinesSummaryProvider.TradeLineCacheTableName} (MainOrg) VALUES ('{org3.PK}');
INSERT INTO {CachedTradeLinesSummaryProvider.TradeLineCacheTableName} (MainOrg) VALUES ('{org2.PK}');
INSERT INTO {CachedTradeLinesSummaryProvider.TradeLineCacheTableName} (MainOrg) VALUES ('{org1.PK}');
";
			TestConnection.ExecuteNonQuery(sql);

			using (var provider = new CachedTradeLinesSummaryProvider(TestConnection, new ZDateTime(2001, 1, 1).Date, ZDateTime.MaxSmallDateTime.Date))
			{
				var queue = new SalesTradeLanesSynchronisationOrgQueue(provider);
				AssertEquals(5, queue.GetOrgsToProcessTotalCount(false));
				AssertEquals(4, queue.GetOrgsToProcessTotalCount(true));

				AssertEquals("not event at all", org1.PK.ToGuid(), queue.GetOrgPksToSync(true, 1)[0]);
				AssertEquals("event time under threshold", org2.PK.ToGuid(), queue.GetOrgPksToSync(true, 1)[0]);
				AssertEquals("not event 'ALL'", org4.PK.ToGuid(), queue.GetOrgPksToSync(true, 1)[0]);

				AssertEquals(2, queue.GetOrgsToProcessTotalCount(false));
				AssertEquals(1, queue.GetOrgsToProcessTotalCount(true));

				AssertEquals(org3.PK.ToGuid(), queue.GetOrgPksToSync(false, 1)[0]);
				AssertEquals(org5.PK.ToGuid(), queue.GetOrgPksToSync(false, 1)[0]);
			}
		}

		public void TestBuildQueueWithDefaultFullTradeLanesSyncDateTimeThresholdValue()
		{
			var thresholdRegistry = OrganisationRegistry.Instance.FullTradeLanesSyncDateTimeThreshold;
			thresholdRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, thresholdRegistry.DefaultValue);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			org1.Logs.AddNew(AutoEvents.SalesTradeLanesSynchronised, TradeLinesSynchroniser.SynchronisedAllReference + " 200101", new ZDateTimeOffset(2001, 1, 1), null);
			org2.Logs.AddNew(AutoEvents.SalesTradeLanesSynchronised, TradeLinesSynchroniser.SynchronisedPartialReference + " 200201", new ZDateTimeOffset(2002, 1, 1), null);

			Factory.Save();

			var sql =
$@"CREATE TABLE {CachedTradeLinesSummaryProvider.TradeLineCacheTableName} (MainOrg uniqueidentifier);
INSERT INTO {CachedTradeLinesSummaryProvider.TradeLineCacheTableName} (MainOrg) VALUES ('{org1.PK}');
INSERT INTO {CachedTradeLinesSummaryProvider.TradeLineCacheTableName} (MainOrg) VALUES ('{org2.PK}');
";
			TestConnection.ExecuteNonQuery(sql);

			using (var provider = new CachedTradeLinesSummaryProvider(TestConnection, new ZDateTime(2001, 1, 1).Date, ZDateTime.MaxSmallDateTime.Date))
			{
				var queue = new SalesTradeLanesSynchronisationOrgQueue(provider);
				AssertEquals(2, queue.GetOrgsToProcessTotalCount(false));
				AssertEquals(1, queue.GetOrgsToProcessTotalCount(true));
			}
		}

		public void TestQueueForActualValuesOnly()
		{
			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var countries = Factory.Load<RefCountry>(new ZQuery() { MaximumRows = 10, OrderBy = RefCountrySchema.Constants.RN_Code });

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var org4 = Factory.NewWithValidTestData<OrgHeader>();

			var actualSales1 = org1.SalesCollection.AddNew();
			actualSales1.OW_IsTraded = true;
			actualSales1.OW_MP_Product = product.PK;
			actualSales1.OW_OriginID = countries[0].PK;
			actualSales1.OW_OriginTableCode = countries[0].TablePrefix;
			actualSales1.OW_DestinationID = countries[1].PK;
			actualSales1.OW_DestinationTableCode = countries[1].TablePrefix;
			actualSales1.OW_OH_Buyer = org2.PK;
			actualSales1.OW_OH_Supplier = org3.PK;

			var detail1 = actualSales1.TradeDetails.AddNew();
			var period1 = detail1.TradedPeriods.AddNew();
			period1.PAS_Period = new ZDate(2022, 1, 1);
			period1.PAS_LastTraded = new ZDate(2022, 1, 1);
			period1.PAS_OH_Client = org1.PK;

			var actualSales2 = org4.SalesCollection.AddNew();
			actualSales2.OW_IsTraded = true;
			actualSales2.OW_MP_Product = product.PK;
			actualSales2.OW_OriginID = countries[0].PK;
			actualSales2.OW_OriginTableCode = countries[0].TablePrefix;
			actualSales2.OW_DestinationID = countries[1].PK;
			actualSales2.OW_DestinationTableCode = countries[1].TablePrefix;

			var detail2 = actualSales2.TradeDetails.AddNew();
			var period2 = detail2.TradedPeriods.AddNew();
			period2.PAS_Period = new ZDate(2022, 2, 1);
			period2.PAS_LastTraded = new ZDate(2022, 2, 1);
			period2.PAS_OH_Client = org4.PK;

			Factory.Save();

			var sql =
$@"CREATE TABLE {CachedTradeLinesSummaryProvider.TradeLineCacheTableName} (MainOrg uniqueidentifier);
";
			TestConnection.ExecuteNonQuery(sql);

			using (var provider = new CachedTradeLinesSummaryProvider(TestConnection, new ZDateTime(2022, 1, 1).Date, new ZDateTime(2022, 2, 1).Date))
			{
				var queue = new SalesTradeLanesSynchronisationOrgQueue(provider);
				AssertEquals(3, queue.GetOrgsToProcessTotalCount(false));
				AssertEquals(3, queue.GetOrgsToProcessTotalCount(true));
			}
		}

		protected override void TearDown()
		{
			base.TearDown();

			var sql = $"IF OBJECT_ID('tempdb..{CachedTradeLinesSummaryProvider.TradeLineCacheTableName}') IS NOT NULL DROP TABLE {CachedTradeLinesSummaryProvider.TradeLineCacheTableName}";
			TestConnection.ExecuteNonQuery(sql);
		}
	}
}
