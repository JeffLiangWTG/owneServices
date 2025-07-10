using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;

namespace Enterprise.Customs.US.Business.LicenseManager.Testing
{
	sealed class LicenseNumberValidationTest : TestCaseWithFactory
	{
		/// <summary>
		/// This table was taken from http://www.aesdirect.gov/support/odtc_required_fields.html on 22-Sep-08
		/// Updated 26-Mar-2012: http://www.cbp.gov/linkhandler/cgov/trade/automated/aes/tech_docs/aestir/ddtc_filing_matrix.ctt/ddtc_filing_matrix.doc
		/// 
		///                                                      License Type 
		///                                                      SAG SCA S00 S05 S61 S73 S85 S94	SAU	SGB
		///Export License Number ITX_9                            -   -   -   M   M   M   M   M		 M	 M
		///DDTC ITAR Exemption ODTCX_1                            M   M   M   -   -   -   -   -		 M	 M
		///DDTC Registration Number ODTCX_2                       M   M   M   M   M   M   M   M		 M	 M
		///DDTC Significant Military Equipment Indicator ODTCX_3  M   M   M   M   M   M   M   M		 M	 M
		///DDTC Eligible Party Cert. Indicator ODTCX_4            -   M   M   -   -   -   -   -		 M	 M
		///DDTC USML Category Code ODTCX_5                        M   M   M   M   M   M   M   M		 M	 M
		///DDTC Unit of Measure Code ODTCX_6                      M   M   M   M   M   M   M   M		 M	 M
		///DDTC Quantity ODTCX_7                                  M   M   M   M   M   M   M   M		 M	 M
		/// </summary>
		public void TestExportLicenseNumberForDDTC()
		{
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.SAG, ZString.Empty, Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.SCA, ZString.Empty, Factory, ZDateTime.Today));
			AssertEquals(ZString.Empty, LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.S00, ZString.Empty, Factory, ZDateTime.Today));
			AssertEquals(ZString.Format(LicenseNumberValidation.LicenseNumberIsRequiredMessage, USAESLicenseCode.Codes.S05), LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.S05, ZString.Empty, Factory, ZDateTime.Today));
			AssertEquals(ZString.Format(LicenseNumberValidation.LicenseNumberIsRequiredMessage, USAESLicenseCode.Codes.S61), LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.S61, ZString.Empty, Factory, ZDateTime.Today));
			AssertEquals(ZString.Format(LicenseNumberValidation.LicenseNumberIsRequiredMessage, USAESLicenseCode.Codes.S73), LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.S73, ZString.Empty, Factory, ZDateTime.Today));
			AssertEquals(ZString.Format(LicenseNumberValidation.LicenseNumberIsRequiredMessage, USAESLicenseCode.Codes.S85), LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.S85, ZString.Empty, Factory, ZDateTime.Today));
			AssertEquals(ZString.Format(LicenseNumberValidation.LicenseNumberIsRequiredMessage, USAESLicenseCode.Codes.S94), LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.S94, ZString.Empty, Factory, ZDateTime.Today));
			AssertEquals("License number is Mandatory for SAU", ZString.Format(LicenseNumberValidation.LicenseNumberIsRequiredMessage, USAESLicenseCode.Codes.SAU), LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.SAU, ZString.Empty, Factory, ZDateTime.Today));
			AssertEquals("License number is Mandatory for SGB", ZString.Format(LicenseNumberValidation.LicenseNumberIsRequiredMessage, USAESLicenseCode.Codes.SGB), LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.SGB, ZString.Empty, Factory, ZDateTime.Today));
		}

		public void TestGetLicenseNumberError()
		{
			var licenseNumber = ZString.Empty;
			AssertEquals("GetLicenseNumberError", "", LicenseValidationHelper.GetLicenseNumberError("ZZ", licenseNumber, Factory, ZDateTime.Today));

			var mesage = string.Format(LicenseNumberValidation.LicenseNumberIsRequiredMessage, USAESLicenseCode.Codes.C30);
			AssertEquals("GetLicenseNumberError", mesage, LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C30, licenseNumber, Factory, ZDateTime.Today));
			licenseNumber = "LIC234";
			AssertNotEquals("GetLicenseNumberError", mesage, LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C30, licenseNumber, Factory, ZDateTime.Today));

			AssertEquals("", LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.S94, "AA-A-BBB", Factory, ZDateTime.Today));
			AssertEquals(LicenseNumberValidation.S94LicenseNoFormat, LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.S94, "AAA-BB-B", Factory, ZDateTime.Today));
		}

		public void TestNoExceptionWithInvalidDate()
		{
			AssertEquals(false, LicenseNumberValidation.IsLicenseNumberRequired(Factory, USAESLicenseCode.Codes.C31, ZDateTime.Invalid));
		}

		public void TestLicenseNumberErrorForC31()
		{
			var licenseNumber = "F49020";
			AssertEquals("GetLicenseNumberError For C31", LicenseNumberValidation.C31LicenseInvalid, LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C31, licenseNumber, Factory, ZDateTime.Today));

			licenseNumber = "S49020";
			AssertEquals("GetLicenseNumberError For C31", "", LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C31, licenseNumber, Factory, ZDateTime.Today));
			licenseNumber = "VIC234";
			AssertEquals("GetLicenseNumberError For C31", "", LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C31, licenseNumber, Factory, ZDateTime.Today));
		}

		public void TestLicenseNumberForC60()
		{
			AssertEquals("GetLicenseNumberError For C60 -1", "License Number is required when License Type is 'C60'.", LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C60, string.Empty, Factory, ZDateTime.Today));
			AssertEquals("GetLicenseNumberError For C60 -2", LicenseNumberValidation.C60LicenseInvalid, LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C60, "APP", Factory, ZDateTime.Today));
			AssertNotEquals("GetLicenseNumberError For C60 -3", LicenseNumberValidation.C60LicenseInvalid, LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C60, LicenseExemptionTypeList.Codes.DY6, Factory, ZDateTime.Today));
		}

		public void TestLicenseNumberForT10()
		{
			AssertEquals("T10 -1", "License Number is required when License Type is 'T10'.", LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.T10, string.Empty, Factory, ZDateTime.Today));
			AssertEquals("T10 -2", LicenseNumberValidation.T10LicenseInvalid, LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.T10, "APP", Factory, ZDateTime.Today));
			AssertEquals("T10 -3", LicenseNumberValidation.T10LicenseInvalid, LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.T10, "AA-5467-4", Factory, ZDateTime.Today));
			AssertEquals("T10 -4-", LicenseNumberValidation.T10LicenseInvalid, LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.T10, "AA-5467-4", Factory, ZDateTime.Today));
			AssertNotEquals("T10 -4", LicenseNumberValidation.T10LicenseInvalid, LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.T10, "AA54674", Factory, ZDateTime.Today));
		}

		public void TestLicenseNumberForC62()
		{
			AssertNotEquals(LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C62, string.Empty, Factory, ZDateTime.Today), string.Empty);
			AssertNotEquals(LicenseValidationHelper.GetLicenseNumberError(USAESLicenseCode.Codes.C62, "APP", Factory, ZDateTime.Today), string.Empty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] {
				USAESLicenseCode.Codes.C30, USAESLicenseCode.Codes.C31,USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.SAG, USAESLicenseCode.Codes.SAU,
				USAESLicenseCode.Codes.SCA, USAESLicenseCode.Codes.SGB, USAESLicenseCode.Codes.S00, USAESLicenseCode.Codes.S05, USAESLicenseCode.Codes.S61,
				USAESLicenseCode.Codes.S73, USAESLicenseCode.Codes.S85, USAESLicenseCode.Codes.S94, USAESLicenseCode.Codes.T10, USAESLicenseCode.Codes.VDS });
		}
	}
}
