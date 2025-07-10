using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class MexicoOrgCusCodeInfoTest : TestCaseWithFactory
	{
		string CountryCode => Core.Constants.CountryCodes.Mexico;

		public void TestCodesCannotCoexist()
		{
			var expectedErrorMessage = "The following (MX) Registration codes cannot coexist: RFC, RFG";
			var orgHeader = Factory.New<OrgHeader>();

			var orgCusCode1 = orgHeader.CustomsCodes.AddNew();
			orgCusCode1.OK_RN_NKCodeCountry = CountryCode;
			orgCusCode1.OK_CodeType = MexicoOrgCusCodeInfo.OrgCusCodes.RFC;
			AssertNoErrors(orgCusCode1.OK_CodeTypeInfo);

			var orgCusCode2 = orgHeader.CustomsCodes.AddNew();
			orgCusCode2.OK_RN_NKCodeCountry = CountryCode;
			orgCusCode2.OK_CodeType = MexicoOrgCusCodeInfo.OrgCusCodes.RFG;
			AssertHasError(orgCusCode2.OK_CodeTypeInfo, expectedErrorMessage);

			orgCusCode1.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			orgCusCode1.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AEO;
			AssertNoError(orgCusCode1.OK_CodeTypeInfo, expectedErrorMessage);
		}

		public void TestCodesCannotUnique()
		{
			GlbCompany.CurrentCompany.SetCountry(CountryCode);

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "MEXICO-1";
			var regNumber1 = new OrgRegistrationNumber(orgHeader1);
			regNumber1.NumberTypeForDisplay = MexicoOrgCusCodeInfo.OrgCusCodes.RFG;
			regNumber1.Number = "XAXX010101000";

			Factory.Save();

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "MEXICO-2";
			var regNumber2 = new OrgRegistrationNumber(orgHeader2);
			regNumber2.NumberTypeForDisplay = MexicoOrgCusCodeInfo.OrgCusCodes.RFG;
			regNumber2.Number = "XAXX010101000";

			Factory.Save();

			AssertNoWarnings(regNumber2.NumberInfo);
		}
	}
}
