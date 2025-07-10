using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradePeriodSupplierPartGrouperTest : TradePeriodGrouperTestCase
	{
		public void TestGetGroupings()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var supplierPartA = Factory.New<OrgSupplierPart>();
			supplierPartA.OP_PartNum = "NumA";
			supplierPartA.OP_Desc = "AAA";
			var supplierPartB = Factory.New<OrgSupplierPart>();
			supplierPartB.OP_Desc = "BBB";
			supplierPartB.OP_PartNum = "NumB";
			var supplierPartC = Factory.New<OrgSupplierPart>();
			supplierPartC.OP_Desc = "CCC";
			supplierPartC.OP_PartNum = "NumC";

			var period1 = NewTradePeriodForSupplierPart(org.PK, supplierPartA.PK);
			var period2 = NewTradePeriodForSupplierPart(org.PK, supplierPartA.PK);
			var period3 = NewTradePeriodForSupplierPart(org.PK, supplierPartB.PK);
			var period4 = NewTradePeriodForSupplierPart(org.PK, supplierPartB.PK);
			var period5 = NewTradePeriodForSupplierPart(org.PK, supplierPartC.PK);
			var period6 = NewTradePeriodForSupplierPart(org.PK, ZGuid.Empty);

			Factory.Save();

			var grouper = new TradePeriodSupplierPartGrouper();
			var anotherFactory = new BusinessObjectFactory();
			var tradeDetails = GetInAnotherFactory(anotherFactory, new[] { period1, period2, period3, period4, period5, period6 });
			anotherFactory.ResetDatabaseLoadCount();
			grouper.AddFetchHints(tradeDetails);
			var groupings = grouper.GetGroupings(tradeDetails);

			AssertContainsExactElementsInAnyOrder(
				new ZString[]
				{
					"AAA",
					"BBB",
					"CCC",
					"No Product"
				},
				groupings.Select(x => x.GroupKey));

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period1, period2 },
				groupings.Single(x => x.GroupKey == "AAA").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period3, period4 },
				groupings.Single(x => x.GroupKey == "BBB").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period5 },
				groupings.Single(x => x.GroupKey == "CCC").GroupedTradePeriods);

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgTradePeriod>.PKOnlyComparer,
				new[] { period6 },
				groupings.Single(x => x.GroupKey == "No Product").GroupedTradePeriods);

			var expectedDbHits = new Dictionary<string, int>
			{
				{ OrgSupplierPartSchema.Constants.TableName, 1 },
				{ OrgTradeDetailSchema.Constants.TableName, 1 }
			};
			AssertDbHits(expectedDbHits, anotherFactory);
		}

		OrgTradePeriod NewTradePeriodForSupplierPart(ZGuid orgPk, ZGuid supplierPartPk)
		{
			var sales = Factory.New<OrgSales>();
			var tradeDetail = sales.TradeDetails.AddNew();
			tradeDetail.PA_OP = supplierPartPk;
			var period = tradeDetail.TradedPeriods.AddNew();
			period.PAS_OH_Client = orgPk;
			return period;
		}
	}
}
