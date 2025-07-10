using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusPermitHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPH_Type()
		{
			permitHeader.CPH_Type = PermitTypeList.Codes.FTZ;
			AssertHasError(permitHeader.CPH_TypeInfo, CusPermitHeaderValidation.TariffPermitRequiredForFTZ);
			var rule = permitHeader.CusPermitRules.AddNew();
			rule.CPR_RuleCode = USPermitRuleCodeList.Codes.COO;
			AssertHasError(permitHeader.CPH_TypeInfo, CusPermitHeaderValidation.TariffPermitRequiredForFTZ);
			rule.CPR_RuleCode = USPermitRuleCodeList.Codes.TAR;
			AssertNoError(permitHeader.CPH_TypeInfo, CusPermitHeaderValidation.TariffPermitRequiredForFTZ);
		}

		CusPermitHeader permitHeader;
		protected override void SetUp()
		{
			base.SetUp();
			permitHeader = Factory.New<CusPermitHeader>();
		}
	}
}
