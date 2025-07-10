using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class LinkedCusAuthorisationRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPR_RuleCode_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(linkedCusAuthorisationRule.CPR_RuleCodeInfo);
		}

		public void TestCheckCPR_RuleCode_List()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(linkedCusAuthorisationRule.CPR_RuleCodeInfo, "XYZ", LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice);
		}

		public void TestCheckCPR_ValueFrom_Mandatory()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(linkedCusAuthorisationRule.CPR_ValueFromInfo);
		}

		public void TestCheckCPR_ValueFrom_CustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ITR", "Rome Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			linkedCusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			linkedCusAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;

			ValidationTestHelper.AssertInvalidCodeMessageError(linkedCusAuthorisationRule.CPR_ValueFromInfo, "XYZ", "ITR");
		}

		public void TestCheckCPR_ValueFrom_ConditionalListValidation()
		{
			CombineAssertions(() =>
			{
				linkedCusAuthorisationRule.CPR_RuleCode = "XXX";
				linkedCusAuthorisationRule.CPR_ValueFrom = "ABC";
				AssertNoMessageErrorContaining(linkedCusAuthorisationRule.CPR_ValueFromInfo, ListValidation.InvalidCodeMessageError);

				linkedCusAuthorisationRule.CPR_RuleCode = LinkedCusAuthorisationRuleTypeList.Codes.CustomsOffice;
				linkedCusAuthorisationRule.CPR_ValueFrom = "ABC";
				AssertHasMessageErrorContaining(linkedCusAuthorisationRule.CPR_ValueFromInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			linkedCusAuthorisationRule = Factory.NewWithValidTestData<LinkedCusAuthorisationRule>();
		}
		LinkedCusAuthorisationRule linkedCusAuthorisationRule;
	}
}
