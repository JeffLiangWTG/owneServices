using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class OrgTradePeriodAndValuePartitionBuilderTest : TestCaseWithFactory
	{
		public void TestBuild_ShouldUseTableValuedParameters()
		{
			using (Db.Connection.TrackExecutedCommands())
			{
				using (var settings = TestEntityFrameworkSettings.Get())
				{
					settings.TVPRule = new TVPRule("0");

					var org = Factory.NewWithValidTestData<OrgHeader>();

					var detail1 = NewOrgTradeDetail();
					var detail2 = NewOrgTradeDetail();
					var detail3 = NewOrgTradeDetail();

					var partition1 = new OrgTradePeriodAndValuePartitionBuilder(Factory, org, new ZDate(2017, 8, 1), new ZDate(2017, 8, 1), new ZDate(2017, 10, 1), new OrgTradeDetail[] { detail1, detail2, detail3 });

					var executedCommand = Db.Connection.ExecutedCommands.FirstOrDefault(c => c.Contains("FROM dbo.OrgTradePeriod"));
					AssertNotNull(executedCommand);
					AssertContains("Should use TVPs", "(PA_OW in (SELECT Value FROM", executedCommand);
					Assert("Should not use explicit values", !Regex.IsMatch(executedCommand, @"\(PA_OW in \((@(.*?),)*?@(.*?)\)"));
				}
			}
		}

		public void TestBuild()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var uslax = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
			var nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");

			var actualSales1 = Factory.New<OrgSales>();
			actualSales1.OW_IsTraded = true;
			actualSales1.OW_OriginID = ausyd.PK;
			actualSales1.OW_OriginTableCode = "RL";
			actualSales1.OW_DestinationID = uslax.PK;
			actualSales1.OW_DestinationTableCode = "RL";
			actualSales1.OW_OH_Buyer = org.PK;
			var detail1 = actualSales1.TradeDetails.AddNew();
			detail1.PA_TradeMode = "AIR";
			detail1.PA_TradeType = "LSE";

			var period1a = detail1.TradedPeriods.AddNew();
			period1a.PAS_Period = new ZDate(2017, 8, 1);
			period1a.PAS_OH_Client = org.PK;
			period1a.PAS_IsJobValue = true;
			var value1a = period1a.TradeValues.AddNew();
			value1a.PAV_GC = Env.CurrentCompanyPK;
			value1a.PAV_RX_NKCurrency = "AUD";

			var period1b = detail1.TradedPeriods.AddNew();
			period1b.PAS_Period = new ZDate(2017, 8, 1);
			period1b.PAS_OH_Client = org.PK;
			var value1b = period1b.TradeValues.AddNew();
			value1b.PAV_GC = Env.CurrentCompanyPK;
			value1b.PAV_RX_NKCurrency = "AUD";

			var actualSales2 = Factory.New<OrgSales>();
			actualSales2.OW_IsTraded = true;
			actualSales2.OW_OriginID = uslax.PK;
			actualSales2.OW_OriginTableCode = "RL";
			actualSales2.OW_DestinationID = nzakl.PK;
			actualSales2.OW_DestinationTableCode = "RL";
			actualSales2.OW_OH_Supplier = org.PK;
			var detail2a = actualSales2.TradeDetails.AddNew();
			detail2a.PA_TradeMode = "SEA";
			detail2a.PA_TradeType = "FCL";
			var detail2b = actualSales2.TradeDetails.AddNew();
			detail2b.PA_TradeMode = "SEA";
			detail2b.PA_TradeType = "LCL";

			var period2a = detail2a.TradedPeriods.AddNew();
			period2a.PAS_Period = new ZDate(2017, 8, 1);
			period2a.PAS_OH_Client = org.PK;
			period2a.PAS_IsJobValue = true;
			var value2a = period2a.TradeValues.AddNew();
			value2a.PAV_GC = Env.CurrentCompanyPK;
			value2a.PAV_RX_NKCurrency = "USD";

			var period2b = detail2a.TradedPeriods.AddNew();
			period2b.PAS_Period = new ZDate(2017, 9, 1);
			period2b.PAS_OH_Client = org.PK;
			period2b.PAS_IsJobValue = true;
			var value2b = period2b.TradeValues.AddNew();
			value2b.PAV_GC = Env.CurrentCompanyPK;
			value2b.PAV_RX_NKCurrency = "USD";

			var period2c = detail2a.TradedPeriods.AddNew();
			period2c.PAS_Period = new ZDate(2017, 11, 1);
			period2c.PAS_OH_Client = org.PK;
			period2c.PAS_IsJobValue = true;

			Factory.Save();

			var partition1 = new OrgTradePeriodAndValuePartitionBuilder(Factory, org, new ZDate(2017, 8, 1), new ZDate(2017, 8, 1), new ZDate(2017, 10, 1), new OrgTradeDetail[] { detail1, detail2a, detail2b });

			AssertEquals(new ZDate(2017, 8, 1), partition1.PartitionPeriod);
			AssertEquals(3, partition1.TradePeriods.Count);
			AssertEquals(2, partition1.TradePeriodsMap.Count);
			AssertEquals(2, partition1.TradePeriodsMap[detail1.PK].Count);
			AssertEquals(1, partition1.TradePeriodsMap[detail2a.PK].Count);
			AssertEquals(3, partition1.TradeValuesMap.Count);
			AssertEquals(1, partition1.TradeValuesMap[period1a.PK].Count);
			AssertEquals(1, partition1.TradeValuesMap[period1b.PK].Count);
			AssertEquals(1, partition1.TradeValuesMap[period2a.PK].Count);

			var partition2 = new OrgTradePeriodAndValuePartitionBuilder(Factory, org, ZDate.Empty, new ZDate(2017, 8, 1), new ZDate(2017, 10, 1), new OrgTradeDetail[] { detail1, detail2a, detail2b });

			AssertEquals(ZDate.Empty, partition2.PartitionPeriod);
			AssertEquals(4, partition2.TradePeriods.Count);
			AssertEquals(2, partition2.TradePeriodsMap.Count);
			AssertEquals(2, partition2.TradePeriodsMap[detail1.PK].Count);
			AssertEquals(2, partition2.TradePeriodsMap[detail2a.PK].Count);
			AssertEquals(4, partition2.TradeValuesMap.Count);
			AssertEquals(1, partition2.TradeValuesMap[period1a.PK].Count);
			AssertEquals(1, partition2.TradeValuesMap[period1b.PK].Count);
			AssertEquals(1, partition2.TradeValuesMap[period2a.PK].Count);
			AssertEquals(1, partition2.TradeValuesMap[period2b.PK].Count);
		}

		OrgTradeDetail NewOrgTradeDetail()
		{
			var orgSale = Factory.New<OrgSales>();

			orgSale.OW_IsTraded = true;
			orgSale.OW_OriginTableCode = "RL";
			orgSale.OW_DestinationTableCode = "RL";
			orgSale.OW_OriginID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD").PK;
			orgSale.OW_DestinationID = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL").PK;

			var orgTradeDetail = orgSale.TradeDetails.AddNew();
			orgTradeDetail.PA_TradeMode = "AIR";
			orgTradeDetail.PA_TradeType = "LSE";

			return orgTradeDetail;
		}
	}
}
