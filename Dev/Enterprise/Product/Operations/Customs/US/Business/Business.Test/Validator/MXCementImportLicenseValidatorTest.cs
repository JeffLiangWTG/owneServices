using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class MXCementImportLicenseValidatorTest : PermitValidatorTest<MXCementImportLicenseValidator>
	{
		public void TestCementNo()
		{
			InvoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			CombineAssertions(() =>
			{
				var errorMsg = "The Mexican Cement Import License number is required.";
				InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
				AssertHasMessageError("MiscPermitNo is required for Mexico", InvoiceLine.US_MiscPermitNoInfo, errorMsg);
				InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Uzbekistan;
				InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
				AssertNoMessageError("MiscPermitNo is not required if CO other than Mexico", InvoiceLine.US_MiscPermitNoInfo, errorMsg);
			});
		}

		protected override string[] ValidNumbers => new string[] { "CEM123456" };

		protected override string[] InvalidNumbers => new string[] { "1", "1CEM12345", "CAM123456", "SEM123456", "CET123456", "CEM1234567", "CEM1A3456", "CEM12345A" };

		protected override string LicenseTypeCode => LicencePermitTypeList.Codes._09;

		protected override string LicenseTypeDescription => "Mexican Cement Import License";

		protected override string FormatMask => @"CEM\d{6}";

		protected override string FormatErrorText => "CEMNNNNNN where N is a numeric and CEM are the letters 'CEM'";

		protected override void SetupExtraTestDataForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			base.SetupExtraTestDataForInvoiceLine(invoiceLine);

			var tariff = invoiceLine.Factory.New<USCTariff>();
			tariff.UE_Tariff = "2523900000";
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.MexicanCementImportLicense;
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			invoiceLine.JI_Tariff = "2523900000";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Mexico;
		}
	}
}
