using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class NZCCPValidatorTest : TestCaseWithFactory
	{
		public void TestValidateCCPFormat()
		{
			OrgCusCode cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			cusCode.OK_CodeType = "CCP";
			cusCode.OK_CustomsRegNo = "ABCDE"; //invalid structure
			AssertHasError(cusCode.OK_CustomsRegNoInfo, NZCCPValidator.InvalidLocationCodeFormat);

			cusCode.OK_CustomsRegNo = "";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, NZCCPValidator.InvalidLocationCodeFormat);

			cusCode.OK_CustomsRegNo = "8624L";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, NZCCPValidator.InvalidLocationCodeFormat);

			cusCode.OK_CustomsRegNo = "10354D";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, NZCCPValidator.InvalidLocationCodeFormat);

			cusCode.OK_CustomsRegNo = "1035D7";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, NZCCPValidator.InvalidLocationCodeFormat);

			cusCode.OK_CustomsRegNo = "103D";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, NZCCPValidator.InvalidLocationCodeFormat);

			cusCode.OK_CustomsRegNo = "103456D";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, NZCCPValidator.InvalidLocationCodeFormat);
		}
	}
}
