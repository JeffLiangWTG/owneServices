using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusCalculationRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRuleTypeList()
		{
			var cusCalculationRule = Factory.NewWithValidTestData<CusCalculationRule>();
			var lookups = cusCalculationRule.Lookups;
			CombineAssertions(() =>
			{
				AssertType<CodeDescriptionPairList>("Empty RuleTypeList: type", lookups.RuleTypeList);
				AssertEquals("Empty RuleTypeList: count", 0, lookups.RuleTypeList.Count);
			});
		}

		public void TestTransportModeList()
		{
			var cusCalculationRule = Factory.NewWithValidTestData<CusCalculationRule>();
			var lookups = cusCalculationRule.Lookups;
			CombineAssertions(() =>
			{
				AssertType<CodeDescriptionPairList>("Empty TransportModeList: type", lookups.TransportModeList);
				AssertEquals("Empty TransportModeList: count", 0, lookups.TransportModeList.Count);
			});
		}
	}
}
