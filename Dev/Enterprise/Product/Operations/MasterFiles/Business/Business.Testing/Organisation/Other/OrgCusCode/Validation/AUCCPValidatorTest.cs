using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AUCCPValidatorTest : TestCaseWithFactory
	{
		public void TestCheckingCCPValidity()
		{
			CheckValidCode("1234D", "1234A");
			CheckValidCode("B123B", "B123C");
			CheckValidCode("CB12E", "CB12B");
		}

		public void TestValidateCCPFormat()
		{
			OrgCusCode cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			cusCode.OK_CodeType = "CCP";
			cusCode.OK_CustomsRegNo = "ABCDE"; //invalid structure
			AssertHasError(cusCode.OK_CustomsRegNoInfo, AUCCPValidator.InvalidEstablishmentCodeFormat);

			cusCode.OK_CustomsRegNo = "";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, AUCCPValidator.InvalidEstablishmentCodeFormat);
		}

		void CheckValidCode(ZString validCode, ZString invalidCode)
		{
			OrgCusCode cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			cusCode.OK_CodeType = "CCP";
			AssertHasError("Empty not allowed", cusCode.OK_CustomsRegNoInfo, "Please enter a " + cusCode.OK_CustomsRegNoInfo.Description + ".");

			cusCode.OK_CustomsRegNo = validCode;
			AssertNoNotifications(cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = invalidCode;
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, AUCCPValidator.InvalidEstablishmentCode + validCode.Right(1));
		}
	}
}
