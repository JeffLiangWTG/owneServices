using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPermitRuleExceptionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPE_ValueTo()
		{
			permitRuleException.CPE_ValueTo = ZString.Empty;
			AssertNoErrorContaining(permitRuleException.CPE_ValueToInfo, BaseCusPermitRuleValidation.ValueToMustBeGreaterThanOrEqualValueFrom);
			permitRuleException.CPE_ValueFrom = ZString.Empty;
			permitRuleException.CPE_ValueTo = "5";
			AssertNoErrorContaining(permitRuleException.CPE_ValueToInfo, BaseCusPermitRuleValidation.ValueToMustBeGreaterThanOrEqualValueFrom);
			permitRuleException.CPE_ValueFrom = "5";
			permitRuleException.CPE_ValueTo = "4";
			AssertHasErrorContaining(permitRuleException.CPE_ValueToInfo, BaseCusPermitRuleValidation.ValueToMustBeGreaterThanOrEqualValueFrom);
			permitRuleException.CPE_ValueTo = "5";
			AssertNoErrorContaining(permitRuleException.CPE_ValueToInfo, BaseCusPermitRuleValidation.ValueToMustBeGreaterThanOrEqualValueFrom);

			permitRuleException.PermitRule.CPR_ValueTo = ZString.Empty;
			permitRuleException.CPE_ValueFrom = ZString.Empty;
			permitRuleException.CPE_ValueTo = ZString.Empty;
			AssertNoErrorContaining(permitRuleException.CPE_ValueToInfo, CusPermitRuleExceptionValidation.ExceptionRangeMustBeContainedInRuleRequirement);
			permitRuleException.PermitRule.CPR_ValueTo = "5";
			permitRuleException.CPE_ValueTo = "6";
			AssertHasErrorContaining(permitRuleException.CPE_ValueToInfo, CusPermitRuleExceptionValidation.ExceptionRangeMustBeContainedInRuleRequirement);
			permitRuleException.PermitRule.CPR_ValueTo = "5";
			permitRuleException.CPE_ValueTo = "5";
			AssertNoErrorContaining(permitRuleException.CPE_ValueToInfo, CusPermitRuleExceptionValidation.ExceptionRangeMustBeContainedInRuleRequirement);
		}

		public void TestCheckCPE_ValueFrom()
		{
			permitRuleException.PermitRule.CPR_ValueFrom = ZString.Empty;
			permitRuleException.CPE_ValueFrom = ZString.Empty;
			AssertNoErrorContaining(permitRuleException.CPE_ValueFromInfo, CusPermitRuleExceptionValidation.ExceptionRangeMustBeContainedInRuleRequirement);
			AssertHasErrorContaining(permitRuleException.CPE_ValueFromInfo, MandatoryValidation.MustBeEntered);
			permitRuleException.PermitRule.CPR_ValueFrom = "5";
			permitRuleException.CPE_ValueFrom = "4";
			AssertHasErrorContaining(permitRuleException.CPE_ValueFromInfo, CusPermitRuleExceptionValidation.ExceptionRangeMustBeContainedInRuleRequirement);
			permitRuleException.PermitRule.CPR_ValueFrom = "5";
			permitRuleException.CPE_ValueFrom = "5";
			AssertNoErrorContaining(permitRuleException.CPE_ValueFromInfo, CusPermitRuleExceptionValidation.ExceptionRangeMustBeContainedInRuleRequirement);
		}

		#region Implementation

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
