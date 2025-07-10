using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module
{
	[TestedType(typeof(ValueAnalysisPipelineFilter))]
	class ValueAnalysisPipelineFilterTest : ModuleNumberRangeFilterTest
	{
		protected override ModuleNumberRangeFilter GetNewModuleFilter()
		{
			return new ValueAnalysisPipelineFilter("moo", OrgTradePeriodSchema.PAS_RepeatsMnth);
		}

		[TestDate(2016, 7, 8, 9, 10, 11)]
		public void TestPAS_RepeatsMnthFilter()
		{
			var filter = new ValueAnalysisPipelineFilter("moo", OrgTradePeriodSchema.PAS_RepeatsMnth);
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			filter.Property1 = 11;
			filter.Property2 = 22;
			string sql = filter.Query.GetAsWhereClause(true).Trim();
			AssertEquals(@"WHERE EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradePeriod JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgTradeProspect ON PAP_PA=PA_PK JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 0 AND PAS_Period IS NULL GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAS_RepeatsMnth *
			(CASE
				WHEN PAP_RecurrenceType = 'ONE' THEN 1
				WHEN PAP_RecurrenceType = 'YR' THEN 1
				WHEN PAP_RecurrenceType = 'MTH' THEN 12
				WHEN PAP_RecurrenceType = 'WK' THEN 52
			END))>=11 AND SUM(PAS_RepeatsMnth *
			(CASE
				WHEN PAP_RecurrenceType = 'ONE' THEN 1
				WHEN PAP_RecurrenceType = 'YR' THEN 1
				WHEN PAP_RecurrenceType = 'MTH' THEN 12
				WHEN PAP_RecurrenceType = 'WK' THEN 52
			END))<=22) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)", sql);
		}

		[TestDate(2016, 7, 8, 9, 10, 11)]
		public void TestPAS_TEUQuantityFilter()
		{
			var filter = new ValueAnalysisPipelineFilter("moo", OrgTradePeriodSchema.PAS_TEUQuantity);
			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			filter.Property1 = 11;
			filter.Property2 = 22;
			string sql = filter.Query.GetAsWhereClause(true).Trim();
			AssertEquals(@"WHERE EXISTS (SELECT 1 FROM (SELECT PA_PK, PAS_OH_Client FROM dbo.OrgTradePeriod JOIN dbo.OrgTradeDetail ON PA_PK=PAS_PA JOIN dbo.OrgTradeProspect ON PAP_PA=PA_PK JOIN dbo.OrgSales ON PA_OW=OW_PK AND OW_IsTraded = 0 AND PAS_Period IS NULL GROUP BY PA_PK, PAS_OH_Client HAVING SUM(PAS_TEUQuantity * (CASE WHEN PAS_RepeatsMnth = 0 THEN 1 ELSE PAS_RepeatsMnth END) *
			(CASE
				WHEN PAP_RecurrenceType = 'ONE' THEN 1
				WHEN PAP_RecurrenceType = 'YR' THEN 1
				WHEN PAP_RecurrenceType = 'MTH' THEN 12
				WHEN PAP_RecurrenceType = 'WK' THEN 52
			END))>=11 AND SUM(PAS_TEUQuantity * (CASE WHEN PAS_RepeatsMnth = 0 THEN 1 ELSE PAS_RepeatsMnth END) *
			(CASE
				WHEN PAP_RecurrenceType = 'ONE' THEN 1
				WHEN PAP_RecurrenceType = 'YR' THEN 1
				WHEN PAP_RecurrenceType = 'MTH' THEN 12
				WHEN PAP_RecurrenceType = 'WK' THEN 52
			END))<=22) a WHERE a.PA_PK = VVA_PA AND a.PAS_OH_Client = VVA_OH_Primary)", sql);
		}
	}
}
