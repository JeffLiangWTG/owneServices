namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BeefCertificateValidatorTest : PermitValidatorTest<BeefCertificateValidator>
	{
		public void TestPermitNo_Beef()
		{
			CombineAssertions(() =>
			{
				var errorMsg = "The Beef Export Certificate number is required.";
				InvoiceLine.Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
				AssertHasMessageError("MiscPermitNo required for CO Uruguay", InvoiceLine.US_MiscPermitNoInfo, errorMsg);
				InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Uzbekistan;
				InvoiceLine.AddInfoValidation.ValidateUS_MiscPermitNo();
				AssertNoMessageError("MiscPermitNo not required if CO other than ARGENTINA (AR), AUSTRALIA (AU), NEW ZEALAND (NZ) AND URUGUAY (UY)", InvoiceLine.US_MiscPermitNoInfo, errorMsg);
			});
		}

		protected override string[] ValidNumbers => new string[] { "12345", "ABCDE", "1234ABCDE" };

		protected override string[] InvalidNumbers => new string[] { "1234567890", "12345ABCDE", "1234 ABCDE" };

		protected override string LicenseTypeCode => LicencePermitTypeList.Codes._05;

		protected override string LicenseTypeDescription => "Beef Export Certificate";

		protected override void SetupExtraTestDataForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			base.SetupExtraTestDataForInvoiceLine(invoiceLine);
			invoiceLine.JI_Tariff = "0202201000";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Uruguay;
		}
	}
}
