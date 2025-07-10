using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodWarehouseCountryGrouperTest : TradePeriodGrouperTestCase
	{
		public void TestGetGroupings()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var ausydBranch = Factory.NewWithValidTestData<GlbBranch>();
			ausydBranch.GB_RL_NKHomePort = "AUSYD";
			var aumelBranch = Factory.NewWithValidTestData<GlbBranch>();
			aumelBranch.GB_RL_NKHomePort = "AUMEL";
			var usnycBranch = Factory.NewWithValidTestData<GlbBranch>();
			usnycBranch.GB_RL_NKHomePort = "USNYC";

			var whs1 = Factory.New<IWhsWarehouse>();
			whs1.WW_WarehouseName = "My 1st Warehouse";
			whs1.WW_GB_RelatedCompanyBranch = ausydBranch.PK;
			((BusinessObject)whs1).FillWithValidTestData();
			var whs2 = Factory.New<IWhsWarehouse>();
			whs2.WW_WarehouseName = "My 2nd Warehouse";
			whs2.WW_GB_RelatedCompanyBranch = aumelBranch.PK;
			((BusinessObject)whs2).FillWithValidTestData();
			var whs3 = Factory.New<IWhsWarehouse>();
			whs3.WW_WarehouseName = "My 3rd Warehouse";
			whs3.WW_GB_RelatedCompanyBranch = usnycBranch.PK;
			((BusinessObject)whs3).FillWithValidTestData();

			var period1 = NewTradePeriodForWarehouse(org.PK, whs1.PK);
			var period2 = NewTradePeriodForWarehouse(org.PK, whs1.PK);
			var period3 = NewTradePeriodForWarehouse(org.PK, whs2.PK);
			var period4 = NewTradePeriodForWarehouse(org.PK, whs3.PK);
			var period5 = NewTradePeriodForWarehouse(org.PK, ZGuid.Empty);

			Factory.Save();

			var grouper = new TradePeriodWarehouseCountryGrouper();
			var anotherFactory = new BusinessObjectFactory();
			var tradeDetails = GetInAnotherFactory(anotherFactory, new[] { period1, period2, period3, period4, period5 });
			anotherFactory.ResetDatabaseLoadCount();
			grouper.AddFetchHints(tradeDetails);
			var groupings = grouper.GetGroupings(tradeDetails);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"AU (Australia)",
					"US (United States)",
					"(Unknown)",
				},
				groupings.Select(x => x.GroupKey));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period1, period2, period3 },
				groupings.Single(x => x.GroupKey == "AU (Australia)").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period4 },
				groupings.Single(x => x.GroupKey == "US (United States)").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period5 },
				groupings.Single(x => x.GroupKey == "(Unknown)").GroupedTradePeriods);

			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgSalesSchema.Constants.TableName, 1 },
				{ OrgTradeDetailSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ RefCountrySchema.Constants.TableName, 1 },
				{ RefUNLOCOSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedDbHits, anotherFactory);
		}

		OrgTradePeriod NewTradePeriodForWarehouse(ZGuid orgPk, ZGuid warehousePk)
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
