using CargoWise.EntityFramework.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	class FRALTCodeValidatorTest : BusinessObjectValidationTestCase
	{
		public void TestValidationALT_France()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_RL_NKClosestPort = "FRANG";

			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.FranceCodeTypes.ALT;
			var expectedError = "ALT code should be in the format: FRNNNNNNNNN, where N is a number.";

			cusCode.OK_CustomsRegNo = "FR123456789";
			AssertNoMessageErrors(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "FR123456789/20190101";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "fr123456789";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "FR123456789/";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "/20190101";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "20190101";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "FR12345678";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "FR12345678/20190101";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "FR1234567890";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "FR1234567890/20190101";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "123456789";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, expectedError);
		}
	}
}
