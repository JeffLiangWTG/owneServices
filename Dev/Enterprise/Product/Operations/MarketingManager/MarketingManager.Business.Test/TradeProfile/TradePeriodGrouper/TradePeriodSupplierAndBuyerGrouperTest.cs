using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodSupplierAndBuyerGrouperTest : TradePeriodGrouperTestCase
	{
		public void TestGetGroupings()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "111";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "222";
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "333";

			var period1 = NewTradePeriodForSupplierAndBuyer(org1.PK, org1.PK, org1.PK);
			var period2 = NewTradePeriodForSupplierAndBuyer(org1.PK, org1.PK, org1.PK);
			var period3 = NewTradePeriodForSupplierAndBuyer(org1.PK, org1.PK, org2.PK);
			var period4 = NewTradePeriodForSupplierAndBuyer(org1.PK, org2.PK, org1.PK);
			var period5 = NewTradePeriodForSupplierAndBuyer(org1.PK, org1.PK, ZGuid.Empty);
			var period6 = NewTradePeriodForSupplierAndBuyer(org1.PK, ZGuid.Empty, org2.PK);

			Factory.Save();

			var grouper = new TradePeriodSupplierAndBuyerGrouper();
			var anotherFactory = new BusinessObjectFactory();
			var tradeDetails = GetInAnotherFactory(anotherFactory, new[] { period1, period2, period3, period4, period5, period6 });
			anotherFactory.ResetDatabaseLoadCount();
			grouper.AddFetchHints(tradeDetails);
			var groupings = grouper.GetGroupings(tradeDetails);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"Buyer:111  Supplier:111",
					"Buyer:111  Supplier:222",
					"Buyer:222  Supplier:111",
					"Buyer:111  Supplier:(Unknown)",
					"Buyer:(Unknown)  Supplier:222",
				},
				groupings.Select(x => x.GroupKey));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period1, period2 },
				groupings.Single(x => x.GroupKey == "Buyer:111  Supplier:111").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period3 },
				groupings.Single(x => x.GroupKey == "Buyer:111  Supplier:222").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period4 },
				groupings.Single(x => x.GroupKey == "Buyer:222  Supplier:111").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period5 },
				groupings.Single(x => x.GroupKey == "Buyer:111  Supplier:(Unknown)").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period6 },
				groupings.Single(x => x.GroupKey == "Buyer:(Unknown)  Supplier:222").GroupedTradePeriods);

			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgSalesSchema.Constants.TableName, 1 },
				{ OrgTradeDetailSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedDbHits, anotherFactory);
		}

		public void TestGetGroupings_WarehouseStorage()
		{
			var tradeDetail1 = NewWarehouseStorageDetail();
			var tradeDetail2 = NewWarehouseStorageDetail();
			var tradeDetail3 = NewWarehouseStorageDetail();

			var period1 = tradeDetail1.TradedPeriods.AddNew();
			var period2 = tradeDetail1.TradedPeriods.AddNew();
			var period3 = tradeDetail1.TradedPeriods.AddNew();

			var grouper = new TradePeriodSupplierAndBuyerGrouper();
			var groupings = grouper.GetGroupings(new[] { period1, period2, period3 });

			AssertContainsExactElementsInAnyOrder(
				System.Array.Empty<ZString>(),
				groupings.Select(x => x.GroupKey));
		}

		OrgTradeDetail NewWarehouseStorageDetail()
		{
			var sales = Factory.New<OrgSales>();
			sales.OW_MP_Product = WarehouseProduct.PK;
			sales.OW_Service = OrgSalesWarehouseServiceTypesList.Codes.Storage;
			return sales.TradeDetails.AddNew();
		}

		OrgSalesProduct WarehouseProduct
		{
			get { return warehouseProduct ?? (warehouseProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.Warehouse)); }
		}
		OrgSalesProduct warehouseProduct;
	}
}
