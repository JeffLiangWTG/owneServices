using System.Globalization;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module
{
	[TestedType(typeof(ValueAnalysisQuantityFilter))]
	class ValueAnalysisQuantityFilterTest : ModuleNumberRangeFilterTest
	{
		protected override ModuleNumberRangeFilter GetNewModuleFilter()
		{
			return new ValueAnalysisQuantityFilter("moo", OrgTradePeriodSchema.PAS_RepeatsMnth, true);
		}

		public void TestDeserializePropertiesFromToXml()
		{
			var filter = new ValueAnalysisQuantityFilter("moo", OrgTradePeriodSchema.PAS_RepeatsMnth, true);
			var filterStripBizO = new DummyFilterStripBusinessObject();
			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.Period = "test1234";
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;
			filter.Property2 = 13;

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = "";
			strip.Delete();

			var loadedFilter = filterStripBizO[filter.Description] as ValueAnalysisQuantityFilter;
			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals("test1234", loadedFilter.Period);
			AssertEquals(ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo, loadedFilter.PropertySearch);
			AssertEquals(13m, loadedFilter.Property2);
		}

		[TestDate(2016, 7, 8, 9, 10, 11)]
		public void TestFilterSQL_PA_Between_3()
		{
			var filter = new ValueAnalysisQuantityFilter("moo", OrgTradePeriodSchema.PAS_RepeatsMnth, true);
			filter.Period = SalesAnalysisPeriodList.Codes.Trailing3Months;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			filter.Property1 = 11;
			filter.Property2 = 22;
			string sql = filter.Query.GetAsWhereClause(true).Trim();
			AssertEquals("WHERE EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradePeriod JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 1 AND PAS_IsJobValue = 0 AND PAS_Period>=#2016-04-01 00:00:00.000# AND PAS_Period<#2016-07-01 00:00:00.000# GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAS_RepeatsMnth)>=11 AND SUM(PAS_RepeatsMnth)<=22) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)", sql);
		}

		[TestDate(2016, 7, 8, 9, 10, 11)]
		public void TestFilterSQL_PAV_GE_Current()
		{
			var filter = new ValueAnalysisQuantityFilter("moo", OrgTradeValueSchema.PAV_Revenue, true);
			filter.Period = SalesAnalysisPeriodList.Codes.CurrentMonth;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo;
			filter.Property1 = 5;
			string sql = filter.Query.GetAsWhereClause(true).Trim();
			AssertEquals("WHERE EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradeValue JOIN dbo.OrgTradePeriod ON PAS_PK=PAV_PAS JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 1 AND PAS_IsJobValue = 0 AND PAS_Period>=#2016-07-01 00:00:00.000# AND PAS_Period<#2016-08-01 00:00:00.000# GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAV_Revenue)>=5) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)", sql);
		}

		[TestDate(2016, 7, 8, 9, 10, 11)]
		public void TestFilterSQL_Profit_LE_12()
		{
			var filter = new ValueAnalysisQuantityFilter("moo", OrgTradeValueSchema.PAV_Revenue, true, ValueAnalysisQuantityFilter.Context.JobProfit);
			filter.Period = SalesAnalysisPeriodList.Codes.Last12Months;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;
			filter.Property2 = 13;
			string sql = filter.Query.GetAsWhereClause(true).Trim();
			AssertEquals(string.Format(CultureInfo.InvariantCulture,
				"WHERE EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradeValue JOIN dbo.OrgTradePeriod ON PAS_PK=PAV_PAS JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 1 AND PAV_GC = '878d7aca-ffc3-49fc-9710-969ca0c0f2ac' AND PAS_IsJobValue = 1 AND PAS_Period>=#2015-07-01 00:00:00.000# AND PAS_Period<#2016-08-01 00:00:00.000# GROUP BY PA_PK, PAS_OH_Client HAVING (SUM(PAV_Revenue)-SUM(PAV_Cost))<=13) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)",
				Env.CurrentCompanyPK.ToString()), sql);

			filter.Period = SalesAnalysisPeriodList.Codes.Trailing12Months;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;
			filter.Property2 = 15;
			sql = filter.Query.GetAsWhereClause(true).Trim();
			AssertEquals(string.Format(CultureInfo.InvariantCulture,
				"WHERE EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradeValue JOIN dbo.OrgTradePeriod ON PAS_PK=PAV_PAS JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 1 AND PAV_GC = '878d7aca-ffc3-49fc-9710-969ca0c0f2ac' AND PAS_IsJobValue = 1 AND PAS_Period>=#2015-07-01 00:00:00.000# AND PAS_Period<#2016-07-01 00:00:00.000# GROUP BY PA_PK, PAS_OH_Client HAVING (SUM(PAV_Revenue)-SUM(PAV_Cost))<=15) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)",
				Env.CurrentCompanyPK.ToString()), sql);
		}

		public void TestFilterSQL_PAV_EQ_ALL()
		{
			var filter = new ValueAnalysisQuantityFilter("moo", OrgTradeValueSchema.PAV_Revenue, true);
			filter.Period = SalesAnalysisPeriodList.Codes.TotalTradingLifetime;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			filter.Property1 = 17;
			string sql = filter.Query.GetAsWhereClause(true).Trim();
			AssertEquals("WHERE EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradeValue JOIN dbo.OrgTradePeriod ON PAS_PK=PAV_PAS JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 1 AND PAS_IsJobValue = 0 GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAV_Revenue)=17) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)", sql);
		}

		[TestDate(2016, 7, 8, 9, 10, 11)]
		public void TestProspectFilter()
		{
			var filter = new ValueAnalysisQuantityFilter("moo", OrgTradePeriodSchema.PAS_RepeatsMnth, isTradedFilter: false);
			filter.Period = SalesAnalysisPeriodList.Codes.Trailing3Months;
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			filter.Property1 = 11;
			filter.Property2 = 22;
			string sql = filter.Query.GetAsWhereClause(true).Trim();
			AssertEquals("WHERE EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradePeriod JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 0 AND PA_Status = 'SUC' AND PAS_Period>=#2016-04-01 00:00:00.000# AND PAS_Period<#2016-07-01 00:00:00.000# GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAS_RepeatsMnth)>=11 AND SUM(PAS_RepeatsMnth)<=22) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)", sql);
		}
	}
}
