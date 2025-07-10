using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CountryOfOriginRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPR_ValueFrom()
		{
			permitRule.CPR_ValueFrom = ZString.Empty;
			AssertHasErrorContaining(permitRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);
			permitRule.CPR_ValueFrom = "1";
			AssertNoErrorContaining(permitRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(permitRule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);
			permitRule.CPR_ValueFrom = Core.Constants.CountryCodes.SouthAfrica;
			AssertNoErrorContaining(permitRule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			permitHeader = Factory.New<BaseCusPermitHeader>();
			permitHeader.CPH_RN_NKCountryCode = "ZA";
			permitRule = permitHeader.CusPermitRules.AddNew();
			permitRule.CPR_RuleCode = BaseCusPermitRule.RuleCodes.CountryOfOrigin;
		}

		BaseCusPermitHeader permitHeader;
		BaseCusPermitRule permitRule;

		#endregion
	}
}
