using CargoWise.EntityFramework.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	class FRISTCodeValidatorTest : BusinessObjectValidationTestCase
	{
		public void TestValidationIST_France()
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_RL_NKClosestPort = "FRANG";

			var cusCode = newOrg.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = CountryCodes.France;
			cusCode.OK_CodeType = OrgCusCode.FranceCodeTypes.IST;
			var expectedError = "The registration number for IST must be exactly 8 alphanumeric characters.";

			cusCode.OK_CustomsRegNo = "1234abc";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "1234abc!";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "1234abcde";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, expectedError);

			cusCode.OK_CustomsRegNo = "1234abcd";
			AssertNoMessageErrors(cusCode.OK_CustomsRegNoInfo);
		}
	}
}
