using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusPermitRule))]
	sealed class CusPermitRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAvailableRuleCodes()
		{
			var permitHeader = Factory.New<CusPermitHeader>();
			permitHeader.CPH_Type = PermitTypeList.Codes.FTZ;
			var rule = permitHeader.CusPermitRules.AddNew();
			Assert("TAR Permit Rule Added for FTZ", rule.Lookups.PermitRuleCodes.ContainsCode("TAR"));
			Assert("COO Permit Rule Added for FTZ", rule.Lookups.PermitRuleCodes.ContainsCode("COO"));
			Assert("ZST Permit Rule Added for FTZ", rule.Lookups.PermitRuleCodes.ContainsCode("ZST"));
			Assert("PRD Permit Rule Added for FTZ", rule.Lookups.PermitRuleCodes.ContainsCode("PRD"));
		}

		public void TestCPR_ValueTo_ReadOnly()
		{
			permitRule.CPR_RuleCode = CusPermitRule.RuleCodes.Tariff;
			Assert("ValueTo is editable for Tariff", !permitRule.CPR_ValueToInfo.ReadOnly);
			permitRule.CPR_RuleCode = CusPermitRule.RuleCodes.CountryOfOrigin;
			Assert("ValueTo is readonly for CountryOfOrigin", permitRule.CPR_ValueToInfo.ReadOnly);
			permitRule.CPR_RuleCode = USPermitRuleCodeList.Codes.ZST;
			Assert("ValueTo is readonly for FTZ Zone Status", permitRule.CPR_ValueToInfo.ReadOnly);
			permitRule.CPR_RuleCode = USPermitRuleCodeList.Codes.PRD;
			Assert("ValueTo is readonly for Product", permitRule.CPR_ValueToInfo.ReadOnly);
		}

		public void TestCPR_ValueFrom_FieldType()
		{
			permitRule.CPR_RuleCode = BaseCusPermitRule.RuleCodes.Tariff;
			AssertEquals("Text", permitRule.CPR_ValueFrom_FieldType);
			permitRule.CPR_RuleCode = USPermitRuleCodeList.Codes.ZST;
			AssertEquals("TextDropEdit", permitRule.CPR_ValueFrom_FieldType);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(USPermitRuleCodeList.Codes.TAR, permitRule.CPR_RuleCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var permitHeader = Factory.New<CusPermitHeader>();
			return permitHeader.CusPermitRules.AddNew();
		}

		CusPermitRule permitRule;
		protected override void SetUp()
		{
			base.SetUp();
			var permitHeader = Factory.New<CusPermitHeader>();
			permitRule = (CusPermitRule)permitHeader.CusPermitRules.AddNew();
		}
	}
}
