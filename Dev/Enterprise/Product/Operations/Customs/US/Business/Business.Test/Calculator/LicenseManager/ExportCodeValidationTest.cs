using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.LicenseManager.Testing
{
	sealed class ExportCodeValidationTest : TestCaseWithFactory
	{
		public void TestGetExportCodeError()
		{
			AssertEquals("GetExportCodeError", string.Format(ExportCodeValidation.ExportCodeIsRequired, "ZZ"), LicenseValidationHelper.GetExportCodeError("ZZ", ""));
			AssertEquals("GetExportCodeError", "", LicenseValidationHelper.GetExportCodeError("ZZ", ExportInformationCodeList.Codes.OS));

			string message = string.Format(ExportCodeValidation.ExportCodeIsInvalidMessage, "is", ExportInformationCodeList.Codes.OS);
			AssertEquals("GetExportCodeError", message, LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.E01, ""));
			AssertEquals("GetExportCodeError", message, LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.E01, ExportInformationCodeList.Codes.TL));
			AssertEquals("GetExportCodeError", "", LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.E01, ExportInformationCodeList.Codes.OS));

			message = string.Format(ExportCodeValidation.ExportCodeIsInvalidMessage, "are", ExportInformationCodeList.Codes.OS + "," + ExportInformationCodeList.Codes.OI + "," + ExportInformationCodeList.Codes.TL + "," + ExportInformationCodeList.Codes.IW + "," + ExportInformationCodeList.Codes.CH);
			AssertEquals("GetExportCodeError", message, LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C30, ""));
			AssertEquals("GetExportCodeError", message, LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C30, ExportInformationCodeList.Codes.MS));
			AssertEquals("GetExportCodeError", "", LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C30, ExportInformationCodeList.Codes.OS));
			AssertEquals("GetExportCodeError", "", LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C30, ExportInformationCodeList.Codes.OI));

			message = string.Format(ExportCodeValidation.ExportCodeIsInvalidMessage, "are", ExportInformationCodeList.Codes.OI + "," + ExportInformationCodeList.Codes.OS + "," + ExportInformationCodeList.Codes.CH + "," + ExportInformationCodeList.Codes.CI);
			AssertEquals("GetExportCodeError C60 1", message, LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C60, ""));
			AssertEquals("GetExportCodeError C60 2", message, LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C60, ExportInformationCodeList.Codes.MS));
			AssertEquals("GetExportCodeError C60 3", "", LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C60, ExportInformationCodeList.Codes.OS));
			AssertEquals("GetExportCodeError C60 4", "", LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C60, ExportInformationCodeList.Codes.OI));

			AssertEquals("GetExportCodeError", string.Format(ExportCodeValidation.ExportCodeIsRequired, USAESLicenseCode.Codes.C33), LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C33, ""));
			AssertEquals("GetExportCodeError", string.Format(ExportCodeValidation.ExportCodeIsNotAllowedMessage, ExportInformationCodeList.Codes.UG + "," + ExportInformationCodeList.Codes.FS + "," + ExportInformationCodeList.Codes.FI), LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C33, ExportInformationCodeList.Codes.UG));
			AssertEquals("GetExportCodeError", string.Format(ExportCodeValidation.ExportCodeIsNotAllowedMessage, ExportInformationCodeList.Codes.UG + "," + ExportInformationCodeList.Codes.FS + "," + ExportInformationCodeList.Codes.FI), LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C62, ExportInformationCodeList.Codes.UG));
			AssertEquals("GetExportCodeError", string.Format(ExportCodeValidation.ExportCodeIsNotAllowedMessage, ExportInformationCodeList.Codes.UG + "," + ExportInformationCodeList.Codes.FS + "," + ExportInformationCodeList.Codes.FI), LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C63, ExportInformationCodeList.Codes.UG));
			AssertEquals("GetExportCodeError", "", LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.C33, ExportInformationCodeList.Codes.OS));
			AssertEquals("GetExportCodeError", string.Format(ExportCodeValidation.ExportCodeIsNotAllowedMessage, ExportInformationCodeList.Codes.UG + "," + ExportInformationCodeList.Codes.FS + "," + ExportInformationCodeList.Codes.FI + "," + ExportInformationCodeList.Codes.IW), LicenseValidationHelper.GetExportCodeError(USAESLicenseCode.Codes.OPA, ExportInformationCodeList.Codes.UG));
		}
	}
}
