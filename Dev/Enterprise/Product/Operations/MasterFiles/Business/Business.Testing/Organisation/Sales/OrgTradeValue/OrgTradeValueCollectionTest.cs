using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgTradeValueCollection))]
	sealed class OrgTradeValueCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgTradeValueCollection>
	{
		public void TestLoad()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var sales1 = org1.SalesCollection.AddNew();
			var tradeDetail1 = sales1.TradeDetails.AddNew();
			var tradePeriod1 = tradeDetail1.TradedPeriods.AddNew();
			tradePeriod1.PAS_IsTraded = true;
			tradePeriod1.PAS_OH_Client = org1.PK;
			var tradeValue1a = Factory.New<OrgTradeValue>();
			tradeValue1a.PAV_PAS = tradePeriod1.PK;
			tradeValue1a.PAV_GC = Env.CurrentCompanyPK;
			var tradeValue1b = Factory.New<OrgTradeValue>();
			tradeValue1b.PAV_PAS = tradePeriod1.PK;
			tradeValue1b.PAV_GC = company.PK;

			var sales2 = org2.SalesCollection.AddNew();
			var tradeDetail2 = sales2.TradeDetails.AddNew();
			var tradePeriod2 = tradeDetail2.TradedPeriods.AddNew();
			tradePeriod2.PAS_OH_Client = org2.PK;
			var tradeValue2 = Factory.New<OrgTradeValue>();
			tradeValue2.PAV_PAS = tradePeriod2.PK;
			tradeValue2.PAV_GC = Env.CurrentCompanyPK;

			Factory.Save();

			var collection1 = new OrgTradeValueCollection(tradePeriod1);
			AssertEquals(2, collection1.Count);

			var collection2 = new OrgTradeValueCollection(tradePeriod2);
			AssertEquals(1, collection2.Count);
		}

		#region Implementation

		protected override OrgTradeValueCollection GetCollectionToTest()
		{
			var sales = Factory.NewWithValidTestData<OrgHeader>().SalesCollection.AddNew();
			var tradeDetail = sales.TradeDetails.AddNew();
			var tradePeriod = tradeDetail.TradedPeriods.AddNew();
			return new OrgTradeValueCollection(tradePeriod);
		}

		#endregion
	}
}
