using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UgandaOrgCusCodeInfoTest : TestCaseWithFactory
	{
		string CountryCode => Core.Constants.CountryCodes.Uganda;

		public void TestIOrgCusCodeCustomsRegNoValidationProvider_Validate()
		{
			var invalidCodeMessage = "UG AEO number should consist of 10 numeric characters.";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "Org-2";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_RN_NKCodeCountry = CountryCode;
			orgCusCode.OK_CodeType = UgandaOrgCusCodeInfo.OrgCusCodes.AuthorizedEconomicOperator;

			orgCusCode.OK_CustomsRegNo = "12345678";
			AssertHasError("Invalid Length", orgCusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			orgCusCode.OK_CustomsRegNo = "12345678901";
			AssertHasError("Invalid Length", orgCusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			orgCusCode.OK_CustomsRegNo = "123456789X";
			AssertHasError("Invalid character", orgCusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			orgCusCode.OK_CustomsRegNo = "1234567890";
			AssertNoError("Valid Code", orgCusCode.OK_CustomsRegNoInfo, invalidCodeMessage);
		}
	}
}
