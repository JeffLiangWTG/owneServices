using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCTariffRuleFilterBusinessObject))]
	sealed class USCTariffRuleFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestGetTariffQuery()
		{
			var filterBizObj = new USCTariffRuleFilterBusinessObject();
			var rule1 = Factory.New<USCTariffRule>();
			rule1.U1_RuleCode = TariffRuleList.Codes.AdditionalTariffs;
			rule1.U1_Tariff = "000000";
			rule1.U1_DateFrom = ZDateTime.BrettsBirthday;
			var rule2 = Factory.New<USCTariffRule>();
			rule2.U1_RuleCode = TariffRuleList.Codes.AdditionalTariffs;
			rule2.U1_Tariff = "000100";
			rule2.U1_DateFrom = ZDateTime.BrettsBirthday;
			var rule3 = Factory.New<USCTariffRule>();
			rule3.U1_RuleCode = TariffRuleList.Codes.AssembledAbroadOfUSProducts;
			rule3.U1_Tariff = "000100";
			rule3.U1_DateFrom = ZDateTime.BrettsBirthday;
			var rule4 = Factory.New<USCTariffRule>();
			rule4.U1_RuleCode = TariffRuleList.Codes.RequiresFormalEntryRegardlessOfValue;
			rule4.U1_Tariff = "000200";
			rule4.U1_TariffTo = "000300";
			rule4.U1_DateFrom = ZDateTime.BrettsBirthday;
			var tariffNumberFilter = (ModuleTextFilter)filterBizObj[USCTariffRuleFilterBusinessObject.Schema.Tariff];
			tariffNumberFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			tariffNumberFilter.Property = "0001.00";
			tariffNumberFilter.IsActive = true;
			AssertEquals(12, tariffNumberFilter.MaxLength);
			Assert("Rule1 does not match filter", !rule1.MatchesFilter(filterBizObj.Filter));
			Assert("Rule2 matches filter", rule2.MatchesFilter(filterBizObj.Filter));
			Assert("Rule3 matches filter", rule3.MatchesFilter(filterBizObj.Filter));
			tariffNumberFilter.Property = "000000";
			Assert("Rule1 matches filter", rule1.MatchesFilter(filterBizObj.Filter));
			Assert("Rule2 does not match filter", !rule2.MatchesFilter(filterBizObj.Filter));
			Assert("Rule3 does not match filter", !rule3.MatchesFilter(filterBizObj.Filter));
			tariffNumberFilter.Property = "000250";
			Assert("Rule1 does not match filter", !rule1.MatchesFilter(filterBizObj.Filter));
			Assert("Rule2 does not match filter", !rule2.MatchesFilter(filterBizObj.Filter));
			Assert("Rule3 does not match filter", !rule3.MatchesFilter(filterBizObj.Filter));
			Assert("Rule4 matches filter", rule4.MatchesFilter(filterBizObj.Filter));
		}

		public void TestGetEffectiveDateQuery()
		{
			var filterBizObj = new USCTariffRuleFilterBusinessObject();
			var rule1 = Factory.New<USCTariffRule>();
			rule1.U1_RuleCode = TariffRuleList.Codes.AdditionalTariffs;
			rule1.U1_Tariff = "000000";
			rule1.U1_DateFrom = ZDateTime.BrettsBirthday;
			var rule2 = Factory.New<USCTariffRule>();
			rule2.U1_RuleCode = TariffRuleList.Codes.AdditionalTariffs;
			rule2.U1_Tariff = "000100";
			rule2.U1_DateFrom = ZDateTime.BrettsBirthday;
			rule2.U1_DateTo = ZDateTime.BrettsBirthday.AddYears(10);
			var rule3 = Factory.New<USCTariffRule>();
			rule3.U1_RuleCode = TariffRuleList.Codes.AdditionalTariffs;
			rule3.U1_Tariff = "000100";
			rule3.U1_DateFrom = ZDateTime.BrettsBirthday.AddYears(10).AddDays(1);
			var dateFilter = (ModuleDateFilter)filterBizObj[USCTariffRuleFilterBusinessObject.Schema.EffectiveDates];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = ZDateTime.BrettsBirthday;
			dateFilter.IsActive = true;
			Assert("Rule1 matches filter", rule1.MatchesFilter(filterBizObj.Filter));
			Assert("Rule2 matches filter", rule2.MatchesFilter(filterBizObj.Filter));
			Assert("Rule3 does not match filter", !rule3.MatchesFilter(filterBizObj.Filter));
			dateFilter.Property2 = ZDateTime.BrettsBirthday.AddYears(9);
			Assert("Rule1 matches filter", rule1.MatchesFilter(filterBizObj.Filter));
			Assert("Rule2 matches filter", rule2.MatchesFilter(filterBizObj.Filter));
			Assert("Rule3 does not match filter", !rule3.MatchesFilter(filterBizObj.Filter));
			dateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			Assert("Rule1 matches filter", rule1.MatchesFilter(filterBizObj.Filter));
			Assert("Rule2 does not match filter", !rule2.MatchesFilter(filterBizObj.Filter));
			Assert("Rule3 matches filter", rule3.MatchesFilter(filterBizObj.Filter));
			dateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals(true, filterBizObj.Filter.IsNoResultQuery);
			dateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals(false, filterBizObj.Filter.IsNoResultQuery);
			AssertEquals(true, filterBizObj.Filter.IsEmpty);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCTariffRuleFilterBusinessObject();
	}
}
