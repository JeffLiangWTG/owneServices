using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module.Testing
{
	public class CusCalculationRulesFilterLookupsTest : TestCaseWithFactory
	{
		public virtual void TestRuleTypeList()
		{
			AssertEquals("RuleTypeList should equals to the lookups list", cusCalculationRule.Lookups.RuleTypeList, lookups.RuleTypeList);
			AssertEquals("RuleType of correct type", typeof(CodeDescriptionPairList), lookups.RuleTypeList.GetType());
		}

		public virtual void TestTransportModeList()
		{
			AssertEquals("TransportModeList should equals to the lookups list", cusCalculationRule.Lookups.TransportModeList, lookups.TransportModeList);
			AssertEquals("TransportMode of correct type", typeof(CodeDescriptionPairList), lookups.TransportModeList.GetType());
		}

		public void TestImporterList()
		{
			AssertEquals("ImporterList of correct type", typeof(OrgHeaderCollection), lookups.ImporterList.GetType());
		}

		CusCalculationRule cusCalculationRule;
		CusCalculationRulesFilterLookups lookups;
		CusCalculationRulesFilterBusinessObject filterBizObj;

		protected override void SetUp()
		{
			base.SetUp();
			cusCalculationRule = Factory.NewWithValidTestData<CusCalculationRule>();
			filterBizObj = new CusCalculationRulesFilterBusinessObject();
			lookups = new CusCalculationRulesFilterLookups(filterBizObj);
		}
	}
}
