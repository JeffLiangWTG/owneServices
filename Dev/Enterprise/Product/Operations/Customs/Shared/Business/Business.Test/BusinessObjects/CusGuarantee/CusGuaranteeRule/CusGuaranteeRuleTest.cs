using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusGuaranteeRule))]
	sealed class CusGuaranteeRuleTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			var rule = header.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = "XXX";
			rule.CPR_ValueFrom = "1";
			rule.CPR_ValueTo = "2";
			return rule;
		}

		public void TestGuaranteeHeader()
		{
			var guarantee = Factory.New<BaseCusGuaranteeHeader>();
			var rule = guarantee.CusGuaranteeRules.AddNew();
			AssertSame(guarantee, rule.GuaranteeHeader);
		}

		public void TestValidationTypeVariesByRuleType()
		{
			var guarantee = Factory.New<BaseCusGuaranteeHeader>();
			var plainRule = guarantee.CusGuaranteeRules.AddNew();
			var accessCodeRule = guarantee.AdditionalAccessCodes.AddNew();
			guarantee.MainAccessCode = "ABC";
			var defaultPin = guarantee.MainAccessCodeRule;
			AssertType(typeof(CusGuaranteeRuleValidation), plainRule.Validation);
			AssertType(typeof(AccessCodePinRuleValidation), accessCodeRule.Validation);
			AssertType(typeof(AccessCodePinRuleValidation), defaultPin.Validation);
		}

		public void TestChangingValueFromWipesValueToForPinRules()
		{
			var guarantee = Factory.New<BaseCusGuaranteeHeader>();
			var plainRule = guarantee.CusGuaranteeRules.AddNew();
			var accessCodeRule = guarantee.AdditionalAccessCodes.AddNew();
			guarantee.MainAccessCode = "ABC";
			var defaultPin = guarantee.MainAccessCodeRule;

			var rules = new CusGuaranteeRule[] { plainRule, accessCodeRule, defaultPin }.ToList();
			foreach (var r in rules)
			{
				r.CPR_ValueFrom = "1";
				r.CPR_ValueFrom = "2";
			}
			rules.ForEach(r => Assert(!r.CPR_ValueToInfo.HasErrors()));
			AssertEquals("", accessCodeRule.CPR_ValueTo);
			AssertEquals("", defaultPin.CPR_ValueTo);
			AssertEquals("1", plainRule.CPR_ValueTo);
		}

		public void TestIsDefaultAccessCodeDefaultPin()
		{
			var guarantee = Factory.New<BaseCusGuaranteeHeader>();
			CombineAssertions(() =>
			{
				guarantee.MainAccessCode = "123";
				AssertEquals(ZBool.True, guarantee.MainAccessCodeRule.IsDefaultAccessCodeDefaultPin);

				var rule = guarantee.CusGuaranteeRules.AddNew();
				AssertEquals(ZBool.False, rule.IsDefaultAccessCodeDefaultPin);
			});
		}

		public void TestOnSaving()
		{
			var guarantee = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			guarantee.MainAccessCode = "123";
			var codeRulePK = guarantee.MainAccessCodeRule.PK;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNotNull("MainAccessCode isn't empty", Factory.Load<CusGuaranteeRule>(codeRulePK));

				guarantee.MainAccessCode = ZString.Empty;
				Factory.Save();
				AssertNull("MainAccessCode is empty", Factory.Load<CusGuaranteeRule>(codeRulePK));
			});
		}

		public void TestCPR_ValueFrom_IsPassword()
		{
			var guaranteeRule = Factory.New<CusGuaranteeRuleExposed>();
			guaranteeRule.CPR_RuleCode = "DIT";
			AssertEquals(false, guaranteeRule.CPR_ValueFrom_IsPassword_Exposed);

			guaranteeRule.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin;
			AssertEquals(true, guaranteeRule.CPR_ValueFrom_IsPassword_Exposed);

			guaranteeRule.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;
			AssertEquals(true, guaranteeRule.CPR_ValueFrom_IsPassword_Exposed);
		}

		sealed class CusGuaranteeRuleExposed : CusGuaranteeRule
		{
			public CusGuaranteeRuleExposed(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool CPR_ValueFrom_IsPassword_Exposed => base.CPR_ValueFrom_IsPassword;
		}
	}
}
