using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CanadaExportSugarCertificateValidatorTest : PermitValidatorTest<CanadaExportSugarCertificateValidator>
	{
		public void TestCASugarCertCountryOfExport()
		{
			InvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Australia;
			AssertEquals(CanadaExportSugarCertificateValidator.CASugarCertCountryOfExport, Validator.GetErrorTextIfInvalidFormatOrNotRequired("1234"));

			InvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			AssertEquals(ZString.Empty, Validator.GetErrorTextIfInvalidFormatOrNotRequired("1234"));
		}

		public void TestCASugarCertCountryOfOrigin()
		{
			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals(CanadaExportSugarCertificateValidator.CASugarCertCountryOfOrigin, Validator.GetErrorTextIfInvalidFormatOrNotRequired("1234"));

			InvoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XA;
			AssertEquals(ZString.Empty, Validator.GetErrorTextIfInvalidFormatOrNotRequired("1234"));

			InvoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XD;
			AssertEquals(ZString.Empty, Validator.GetErrorTextIfInvalidFormatOrNotRequired("1234"));

			InvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals(ZString.Empty, Validator.GetErrorTextIfInvalidFormatOrNotRequired("1234"));
		}

		protected override string[] ValidNumbers => new[] { "1234", "ABCD", "1234ABCD" };

		protected override string[] InvalidNumbers => new[] { "123456789", "ABCDEFGHI", "1234-ABCD" };

		protected override string LicenseTypeCode => LicencePermitTypeList.Codes._16;

		protected override string LicenseTypeDescription => "Canadian Export Sugar Certificate";

		protected override string FormatMask => @"\w{1,8}";

		protected override string FormatErrorText => "1 to 8 alpha-numeric characters";

		protected override void SetupExtraTestDataForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			base.SetupExtraTestDataForInvoiceLine(invoiceLine);
			InvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Canada;
			InvoiceLine.US_UC_NKCountryOfOrigin = CanadaProvinceTerritoryCodes.Codes.XO;
		}
	}
}
