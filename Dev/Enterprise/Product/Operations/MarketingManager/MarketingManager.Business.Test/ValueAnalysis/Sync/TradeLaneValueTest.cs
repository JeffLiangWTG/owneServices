using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class TradeLaneValueTest : TestCaseWithFactory
	{
		public void TestAddData()
		{
			var org1Pk = ZGuid.NewZGuid();
			var org2Pk = ZGuid.NewZGuid();
			var org3Pk = ZGuid.NewZGuid();
			var companyPk = ZGuid.NewZGuid();

			var product = Factory.NewWithValidTestData<OrgSalesProduct>();
			var tradeLine1Org1 = new TradeLineForTest(
				"SEA", "FCL", ZGuid.Empty, 3,
				new ZDate(2017, 3, 1),
				org1Pk, org2Pk, org3Pk,
				new ZDateTime(2017, 3, 4), 2, 2, 2, 2, 2, 2, 2, "KG", 2,
				"AUD", companyPk,
				60, -30, 10, -5,
				ZGuid.Empty, product
			);

			var tradeLaneValue1 = new TradeLaneValue();

			tradeLaneValue1.AddData(
				org1Pk,
				tradeLine1Org1
			);

			var tradeLine2Org1 = new TradeLineForTest(
				"SEA", "FCL", ZGuid.Empty, 3,
				new ZDate(2017, 3, 1),
				org1Pk, org2Pk, org3Pk,
				new ZDateTime(2017, 3, 4), 2, 2, 2, 2, 2, 2, 2, "KG", 2,
				"AUD", companyPk,
				150, -75, 40, -20,
				ZGuid.Empty, product
			);

			tradeLaneValue1.AddData(
				org1Pk,
				tradeLine2Org1
			);

			AssertEquals(1, tradeLaneValue1.TradeDetails.Count);

			var tradeDetail = tradeLaneValue1.TradeDetails.Values.First();
			AssertEquals(2, tradeDetail.TradePeriods.Count);

			var jobPeriod = tradeDetail.TradePeriods.Single(x => x.Key.IsJobValue && x.Key.OrgPk == org1Pk).Value;
			var org1Period = tradeDetail.TradePeriods.Single(x => !x.Key.IsJobValue && x.Key.OrgPk == org1Pk).Value;

			AssertEquals(2, jobPeriod.NumberOfJobs);

			var jobValue = jobPeriod.TradeValues.Single(x => x.Key.Currency == "AUD" && x.Key.CompanyPk == companyPk).Value;
			var org1Value = org1Period.TradeValues.Single(x => x.Key.Currency == "AUD" && x.Key.CompanyPk == companyPk).Value;

			AssertEquals(210m, jobValue.Revenue);
			AssertEquals(50m, org1Value.Revenue);

			AssertEquals(-105m, jobValue.Cost);
			AssertEquals(-25m, org1Value.Cost);
		}
	}
}
