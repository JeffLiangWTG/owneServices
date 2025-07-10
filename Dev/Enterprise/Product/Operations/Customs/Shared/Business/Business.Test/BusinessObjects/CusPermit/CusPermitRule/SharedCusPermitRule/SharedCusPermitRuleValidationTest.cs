using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class SharedCusPermitRuleValidationTest<TSharedCusPermitRuleValidation, TSharedCusPermitRule> : CusPermitRuleValidationTest
			where TSharedCusPermitRuleValidation : SharedCusPermitRuleValidation
			where TSharedCusPermitRule : SharedCusPermitRule
	{
		public void TestCheckCPR_RuleCode_Mandatory()
		{
			PermitRule.CPR_RuleCode = ZString.Empty;
			AssertHasErrorContaining(PermitRule.CPR_RuleCodeInfo, MandatoryValidation.MustBeEntered);
			PermitRule.CPR_RuleCode = "TAR";
			AssertNoErrorContaining(PermitRule.CPR_RuleCodeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCPR_ValueFrom_Mandatory()
		{
			PermitRule.CPR_ValueFrom = ZString.Empty;
			AssertHasErrorContaining(PermitRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);
			PermitRule.CPR_ValueFrom = "1";
			AssertNoErrorContaining(PermitRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);
		}

		#region Implementation

		protected abstract TSharedCusPermitRule GetNewRule(BusinessObjectFactory factory);

		protected override void SetUp()
		{
			base.SetUp();
			PermitRule = GetNewRule(Factory);
		}

		protected TSharedCusPermitRule PermitRule { get; set; }

		protected SharedCusPermitHeader PermitHeader => PermitRule.PermitHeader;

		#endregion
	}
}
