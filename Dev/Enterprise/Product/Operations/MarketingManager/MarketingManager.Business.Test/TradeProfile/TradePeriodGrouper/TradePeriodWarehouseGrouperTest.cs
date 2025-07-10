using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodWarehouseGrouperTest : TradePeriodGrouperTestCase
	{
		public void TestGetGroupings()
		{
			var whs1 = Factory.New<IWhsWarehouse>();
			whs1.WW_WarehouseName = "My 1st Warehouse";
			((BusinessObject)whs1).FillWithValidTestData();
			var whs2 = Factory.New<IWhsWarehouse>();
			whs2.WW_WarehouseName = "My 2nd Warehouse";
			((BusinessObject)whs2).FillWithValidTestData();
			var whs3 = Factory.New<IWhsWarehouse>();
			whs3.WW_WarehouseName = "My 3rd Warehouse";
			((BusinessObject)whs3).FillWithValidTestData();

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var period1 = NewTradePeriodForWarehouse(whs1.PK, org.PK);
			var period2 = NewTradePeriodForWarehouse(whs1.PK, org.PK);
			var period3 = NewTradePeriodForWarehouse(whs2.PK, org.PK);
			var period4 = NewTradePeriodForWarehouse(whs3.PK, org.PK);
			var period5 = NewTradePeriodForWarehouse(ZGuid.Empty, org.PK);

			Factory.Save();

			var grouper = new TradePeriodWarehouseGrouper();
			var anotherFactory = new BusinessObjectFactory();
			var tradePeriods = GetInAnotherFactory(anotherFactory, new[] { period1, period2, period3, period4, period5 });
			anotherFactory.ResetDatabaseLoadCount();
			grouper.AddFetchHints(tradePeriods);
			var groupings = grouper.GetGroupings(tradePeriods);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"My 1st Warehouse",
					"My 2nd Warehouse",
					"My 3rd Warehouse",
					"Unknown Warehouse",
				},
				groupings.Select(x => x.GroupKey));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period1, period2 },
				groupings.Single(x => x.GroupKey == "My 1st Warehouse").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period3 },
				groupings.Single(x => x.GroupKey == "My 2nd Warehouse").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period4 },
				groupings.Single(x => x.GroupKey == "My 3rd Warehouse").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period5 },
				groupings.Single(x => x.GroupKey == "Unknown Warehouse").GroupedTradePeriods);

			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgSalesSchema.Constants.TableName, 1 },
				{ OrgTradeDetailSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedDbHits, anotherFactory);
		}

		OrgTradePeriod NewTradePeriodForWarehouse(ZGuid warehousePk, ZGuid orgPk)
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_WW = warehousePk;
			var detail = sales.TradeDetails.AddNew();
			var period = detail.TradedPeriods.AddNew();
			period.PAS_OH_Client = orgPk;
			return period;
		}
	}
}
