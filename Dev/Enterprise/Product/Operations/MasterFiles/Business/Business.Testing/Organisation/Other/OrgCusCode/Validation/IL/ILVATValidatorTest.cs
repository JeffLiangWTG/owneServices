using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ILVATValidatorTest : TestCaseWithFactory
	{
		public void TestValidateVATFormat()
		{
			OrgCusCode cusCode = Factory.New<OrgCusCode>();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Israel;
			cusCode.OK_CodeType = "VAT";
			var expectedErrorMesasge = "Israel VAT Business Registration Number should be 9 digits NNNNNNNNN and compiled with last digit checksum calculation to ensure its validity. An error in this field indicates that the entered value is invalid - please check you have input the correct value.";

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDefaultValue());

			cusCode.OK_CustomsRegNo = "ABCDE";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, expectedErrorMesasge);

			cusCode.OK_CustomsRegNo = "12346";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, expectedErrorMesasge);

			cusCode.OK_CustomsRegNo = "123456789";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, expectedErrorMesasge);

			cusCode.OK_CustomsRegNo = "";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, expectedErrorMesasge);

			cusCode.OK_CustomsRegNo = "777777731";
			AssertNoError(cusCode.OK_CustomsRegNoInfo, expectedErrorMesasge);

			OrganisationRegistry.Instance.StrictEnforcementOfRegistrationNumberFormats.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OrganisationRegistry.Instance.GetStrictEnforcementOfRegistryNumberFormatsDisabledValue());

			cusCode.OK_CustomsRegNo = "ABCDE";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, expectedErrorMesasge);

			cusCode.OK_CustomsRegNo = "12346";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, expectedErrorMesasge);

			cusCode.OK_CustomsRegNo = "123456789";
			AssertHasWarning(cusCode.OK_CustomsRegNoInfo, expectedErrorMesasge);

			cusCode.OK_CustomsRegNo = "";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, expectedErrorMesasge);

			cusCode.OK_CustomsRegNo = "777777731";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, expectedErrorMesasge);

			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
			cusCode.OK_CustomsRegNo = "ABCDE";
			AssertNoWarning(cusCode.OK_CustomsRegNoInfo, expectedErrorMesasge);
			AssertNoError(cusCode.OK_CustomsRegNoInfo, expectedErrorMesasge);
		}
	}
}
