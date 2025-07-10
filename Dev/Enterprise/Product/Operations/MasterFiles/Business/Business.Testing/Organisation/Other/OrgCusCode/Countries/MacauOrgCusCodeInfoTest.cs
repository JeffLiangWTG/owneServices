using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MacauOrgCusCodeInfoTest : TestCaseWithFactory
	{
		string CountryCode => Core.Constants.CountryCodes.Macau;

		public void TestIOrgCusCodeCustomsRegNoValidationProvider_Validate()
		{
			var invalidCodeMessage = "MO AEO number should consist of 9 numeric characters.";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "Org-2";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_RN_NKCodeCountry = CountryCode;
			orgCusCode.OK_CodeType = MacauOrgCusCodeInfo.OrgCusCodes.AuthorizedEconomicOperator;

			orgCusCode.OK_CustomsRegNo = "12345678";
			AssertHasError("Invalid Length", orgCusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			orgCusCode.OK_CustomsRegNo = "1234567890";
			AssertHasError("Invalid Length", orgCusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			orgCusCode.OK_CustomsRegNo = "12345678X";
			AssertHasError("Invalid character", orgCusCode.OK_CustomsRegNoInfo, invalidCodeMessage);

			orgCusCode.OK_CustomsRegNo = "123456789";
			AssertNoError("Valid Code", orgCusCode.OK_CustomsRegNoInfo, invalidCodeMessage);
		}
	}
}
