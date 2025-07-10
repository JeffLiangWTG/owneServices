using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCRuleFilterBusinessObject))]
	sealed class USCRuleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestGetTariffQuery()
		{
			var rule = Factory.New<USCRule>();
			rule.U0_Code = "AAA";
			var tariffRule = rule.Tariffs.AddNew();
			tariffRule.FormattedTariff = "0000.00";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			var rule2 = Factory.New<USCRule>();
			rule2.U0_Code = "BBB";
			var tariffRule2 = rule2.Tariffs.AddNew();
			tariffRule2.FormattedTariff = "0000.01";
			tariffRule2.U1_DateFrom = ZDateTime.BrettsBirthday;
			Factory.Save();
			var filterBizObj = new USCRuleFilterBusinessObject();
			var tariffNumberFilter = (ModuleTextFilter)filterBizObj[USCRuleFilterBusinessObject.Schema.Tariff];
			tariffNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			tariffNumberFilter.Property = "0000";
			tariffNumberFilter.IsActive = true;
			AssertEquals(true, rule.MatchesFilter(filterBizObj.Filter));
			AssertEquals(true, rule2.MatchesFilter(filterBizObj.Filter));
			tariffNumberFilter.Property = "0000.01";
			AssertEquals(false, rule.MatchesFilter(filterBizObj.Filter));
			AssertEquals(true, rule2.MatchesFilter(filterBizObj.Filter));
			var rule3 = Factory.New<USCRule>();
			rule.U0_Code = "CCC";
			var tariffRule3 = rule3.Tariffs.AddNew();
			tariffRule3.U1_Tariff = "2000";
			tariffRule3.U1_TariffTo = "3000";
			tariffRule3.U1_DateFrom = ZDateTime.BrettsBirthday;
			Factory.Save();
			tariffNumberFilter.Property = "2500";
			AssertEquals(false, rule.MatchesFilter(filterBizObj.Filter));
			AssertEquals(false, rule2.MatchesFilter(filterBizObj.Filter));
			AssertEquals(true, rule3.MatchesFilter(filterBizObj.Filter));
		}

		public void TestGetEffectiveDateQuery()
		{
			var rule = Factory.New<USCRule>();
			rule.U0_Code = "AAA";
			var tariffRule = rule.Tariffs.AddNew();
			tariffRule.FormattedTariff = "0000.00";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			var rule2 = Factory.New<USCRule>();
			rule2.U0_Code = "BBB";
			var tariffRule2 = rule2.Tariffs.AddNew();
			tariffRule2.FormattedTariff = "0000.01";
			tariffRule2.U1_DateFrom = ZDateTime.BrettsBirthday;
			tariffRule2.U1_DateTo = ZDateTime.BrettsBirthday.AddYears(1);
			var tariffRule3 = rule2.Tariffs.AddNew();
			tariffRule3.FormattedTariff = "0000.02";
			tariffRule3.U1_DateFrom = ZDateTime.BrettsBirthday.AddYears(1).AddDays(1);
			tariffRule3.U1_DateTo = ZDateTime.Empty;
			Factory.Save();
			var filterBizObj = new USCRuleFilterBusinessObject();
			var dateFilter = (ModuleDateFilter)filterBizObj[USCRuleFilterBusinessObject.Schema.EffectiveDates];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.BrettsBirthday;
			dateFilter.IsActive = true;
			AssertEquals(true, rule.MatchesFilter(filterBizObj.Filter));
			AssertEquals(true, rule2.MatchesFilter(filterBizObj.Filter));
			dateFilter.Property2 = ZDateTime.BrettsBirthday.AddYears(5);
			AssertEquals(true, rule.MatchesFilter(filterBizObj.Filter));
			AssertEquals(false, rule2.MatchesFilter(filterBizObj.Filter));
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today; //valid for today
			AssertEquals(true, rule.MatchesFilter(filterBizObj.Filter));
			AssertEquals(true, rule2.MatchesFilter(filterBizObj.Filter));
			dateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals(true, filterBizObj.Filter.IsNoResultQuery);
			dateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals(false, filterBizObj.Filter.IsNoResultQuery);
			AssertEquals(true, filterBizObj.Filter.IsEmpty);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCRuleFilterBusinessObject();
	}
}
