using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZZoneStatusPermitRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPR_ValueFrom()
		{
			permitRule.CPR_ValueFrom = ZString.Empty;
			AssertHasErrorContaining(permitRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);
			permitRule.CPR_ValueFrom = "1";
			AssertNoErrorContaining(permitRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(permitRule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);
			permitRule.CPR_ValueFrom = ZoneStatusList.Codes.Domestic;
			AssertNoErrorContaining(permitRule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			permitHeader = Factory.New<BaseCusPermitHeader>();
			permitHeader.CPH_RN_NKCountryCode = "US";
			permitRule = permitHeader.CusPermitRules.AddNew();
			permitRule.CPR_RuleCode = USPermitRuleCodeList.Codes.ZST;
		}

		BaseCusPermitHeader permitHeader;
		BaseCusPermitRule permitRule;
	}
}
