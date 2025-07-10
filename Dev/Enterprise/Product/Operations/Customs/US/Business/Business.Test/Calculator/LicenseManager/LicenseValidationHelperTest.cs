using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;

namespace Enterprise.Customs.US.Business.LicenseManager.Testing
{
	sealed class LicenseValidationHelperTest : TestCaseWithFactory
	{
		public void TestValidationE01Licence()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.E01 });

			AssertEquals("License Number is required when License Type is 'E01'.", LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.E01, "", Factory, ZDateTime.Today));
			AssertEquals("", LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.E01, LicenseExemptionTypeList.Codes.AEA, Factory, ZDateTime.Today));

			AssertEquals("Export Code entered is invalid; valid Export Code is OS.", LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.E01, ""));
			AssertEquals("", LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.E01, ExportInformationCodeList.Codes.OS));
			AssertEquals("Export Code entered is invalid; valid Export Code is OS.", LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.E01, ExportInformationCodeList.Codes.MS));

			AssertEquals("License Type entered is not allowed for the following Transport Modes: FIX,RAI,BWB.", LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.E01, TransportTypeList.Codes.FixedTransportInstallations, Factory, ZDateTime.Today));
			AssertEquals("", LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.E01, TransportTypeList.Codes.Sea, Factory, ZDateTime.Today));
			AssertEquals("License Type entered is not allowed for the following Transport Modes: FIX,RAI,BWB.", LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.E01, TransportTypeList.Codes.BorderWaterBorne, Factory, ZDateTime.Today));
			AssertEquals("", LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.E01, TransportTypeList.Codes.Sea, Factory, ZDateTime.Today));
			AssertEquals("License Type entered is not allowed for the following Transport Modes: FIX,RAI,BWB.", LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.E01, TransportTypeList.Codes.Rail, Factory, ZDateTime.Today));
			AssertEquals("", LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.E01, TransportTypeList.Codes.Sea, Factory, ZDateTime.Today));
		}

		public void TestAddLicenseTypeValidationErrorMessage()
		{
			AssertEquals("", LicenseValidationHelper.AddLicenseTypeValidationErrorMessage(Core.Constants.CountryCodes.Cuba, USAESLicenseCode.Codes.C30));
			AssertEquals("", LicenseValidationHelper.AddLicenseTypeValidationErrorMessage(Core.Constants.CountryCodes.Cuba, USAESLicenseCode.Codes.T10));
			AssertEquals(LicenseValidationHelper.LicenseTypeForCuba, LicenseValidationHelper.AddLicenseTypeValidationErrorMessage(Core.Constants.CountryCodes.Cuba, USAESLicenseCode.Codes.C50));
		}
	}
}
