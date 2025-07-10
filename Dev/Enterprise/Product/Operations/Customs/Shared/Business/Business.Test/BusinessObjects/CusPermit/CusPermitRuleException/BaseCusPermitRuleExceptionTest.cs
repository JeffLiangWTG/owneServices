using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusPermitRuleException))]
	sealed class BaseCusPermitRuleExceptionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var permit = Factory.New<BaseCusPermitHeader>();
			var permitRule = permit.CusPermitRules.AddNew();
			var exception = permitRule.CusPermitRuleExceptions.AddNew();
			exception.CPE_ValueFrom = "1";
			exception.CPE_ValueTo = "2";

			AssertEquals("Permit Rule Exception : 1 - 2", exception.HumanReadableName);

			var guarantee = Factory.New<BaseCusGuaranteeHeader>();
			var guaranteeRule = guarantee.CusGuaranteeRules.AddNew();
			var guaranteeException = guaranteeRule.CusPermitRuleExceptions.AddNew();
			guaranteeException.CPE_ValueFrom = "3";
			guaranteeException.CPE_ValueTo = "4";

			AssertEquals("Guarantee Rule Exception : 3 - 4", guaranteeException.HumanReadableName);
		}

		public void TestCPE_ValueTo_ReadOnly()
		{
			permitRule.CPR_RuleCode = BaseCusPermitRule.RuleCodes.Tariff;
			Assert("ValueTo is editable for Tariff", !permitRuleException.CPE_ValueTo_ReadOnly);
			permitRule.CPR_RuleCode = BaseCusPermitRule.RuleCodes.CountryOfOrigin;
			Assert("ValueTo is readonly for CountryOfOrigin", permitRuleException.CPE_ValueTo_ReadOnly);
		}

		public void TestCPE_ValueFrom_FieldType()
		{
			permitRule.CPR_RuleCode = BaseCusPermitRule.RuleCodes.Tariff;
			AssertEquals("Text", permitRuleException.CPE_ValueFrom_FieldType);
			permitRule.CPR_RuleCode = BaseCusPermitRule.RuleCodes.CountryOfOrigin;
			AssertEquals("TextCodeFindBox", permitRuleException.CPE_ValueFrom_FieldType);
		}

		public void TestPermitHeader()
		{
			AssertType<BaseCusPermitRule>(permitRuleException.PermitRule);
			AssertEquals(permitRuleException.CPE_CPR_PermitRule, permitRuleException.PermitRule.PK);
		}

		public void TestDefaultCPE_ValueTo()
		{
			permitRuleException.CPE_ValueTo = ZString.Empty;
			AssertEquals(ZString.Empty, permitRuleException.CPE_ValueTo);
			permitRuleException.CPE_ValueFrom = "123";
			AssertEquals("123", permitRuleException.CPE_ValueTo);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var permitHeader = Factory.New<BaseCusPermitHeader>();
			var permitRule = permitHeader.CusPermitRules.AddNew();
			return permitRule.CusPermitRuleExceptions.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var permitHeader = Factory.New<BaseCusPermitHeader>();
			permitRule = permitHeader.CusPermitRules.AddNew();
			permitRuleException = permitRule.CusPermitRuleExceptions.AddNew();
		}

		BaseCusPermitRule permitRule;
		BaseCusPermitRuleException permitRuleException;

		#endregion
	}
}
