using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusCalculationRule))]
	sealed class CusCalculationRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var rule = Factory.New<CusCalculationRule>();
			AssertType<CusCalculationRuleLookups>(rule.Lookups);
		}

		public void TestValidation()
		{
			var rule = Factory.New<CusCalculationRule>();
			AssertType<CusCalculationRuleValidation>(rule.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var calcRule = factory.New<CusCalculationRule>();
			calcRule.CCR_RuleType = "INS";
			calcRule.CCR_TransportMode = "AIR";
			calcRule.CCR_Formula = "F";
			calcRule.CCR_RX_NKCurrency = "AUD";
			calcRule.CCR_StartDate = ZDateTimeOffset.Today;
			calcRule.CCR_EndDate = ZDateTimeOffset.Today;
			calcRule.CCR_GC_Company = GlbCompany.CurrentCompany.PK;
			return calcRule;
		}
	}
}
