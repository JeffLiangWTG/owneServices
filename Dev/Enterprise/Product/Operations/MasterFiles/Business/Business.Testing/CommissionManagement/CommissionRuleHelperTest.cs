using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommissionRuleHelperTest : TestCaseWithFactory
	{
		public void TestSetCommissionTypeDefaults()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			var rule = new CommissionRateOverridableStub(Factory);
			rule.Company = GlbCompany.CurrentCompany;
			rule.CommissionPercentage = 10d;
			rule.CommissionCurrency = "USD";
			rule.CommissionAmount = 100d;

			rule.CommissionType = CommissionTypes.Codes.PCT;
			CommissionRuleHelper.SetCommissionTypeDefaults(rule);
			AssertEquals((ZDecimal)10d, rule.CommissionPercentage);
			AssertEquals("", rule.CommissionCurrency);
			AssertEquals(ZDecimal.Zero, rule.CommissionAmount);

			rule.CommissionType = CommissionTypes.Codes.FIX;
			CommissionRuleHelper.SetCommissionTypeDefaults(rule);
			AssertEquals(ZDecimal.Zero, rule.CommissionPercentage);
			AssertEquals("AUD", rule.CommissionCurrency);
			AssertEquals(ZDecimal.Zero, rule.CommissionAmount);
		}

		public void TestDefaultCommissionCurrency()
		{
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "AUD";
			var anotherCompany = Factory.New<GlbCompany>();
			anotherCompany.GC_RX_NKLocalCurrency = "USD";

			var globalTeamRule = new CommissionRateOverridableStub(Factory);
			globalTeamRule.Company = null;
			globalTeamRule.CommissionType = CommissionTypes.Codes.FIX;
			CommissionRuleHelper.DefaultCommissionCurrency(globalTeamRule);
			AssertEquals("AUD", globalTeamRule.CommissionCurrency);

			var companySpecificTeamRule = new CommissionRateOverridableStub(Factory);
			companySpecificTeamRule.Company = anotherCompany;
			companySpecificTeamRule.CommissionType = CommissionTypes.Codes.FIX;
			CommissionRuleHelper.DefaultCommissionCurrency(companySpecificTeamRule);
			AssertEquals("USD", companySpecificTeamRule.CommissionCurrency);
		}
	}
}
