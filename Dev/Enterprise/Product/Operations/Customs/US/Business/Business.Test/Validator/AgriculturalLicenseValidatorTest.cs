using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AgriculturalLicenseValidatorTest : PermitValidatorTest<AgriculturalLicenseValidator>
	{
		public void TestAgricultureLicenseNoInvalidEntryType()
		{
			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals(AgriculturalLicenseValidator.AgricultureLicenseNoInvalidEntryType, Validator.GetErrorTextIfInvalidFormatOrNotRequired("1-AB-234-5"));

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
			AssertEquals(ZString.Empty, Validator.GetErrorTextIfInvalidFormatOrNotRequired("1-AB-234-5"));
		}

		public void TestAgricultureLicenseNoCannotBeEnteredIfTextileInfoEntered()
		{
			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(ZString.Empty, Validator.GetErrorTextIfInvalidFormatOrNotRequired("1-AB-234-5"));

			InvoiceLine.US_VisaNo = "~";
			AssertEquals(AgriculturalLicenseValidator.AgricultureLicenseNoCannotBeEnteredIfTextileInfoEntered, Validator.GetErrorTextIfInvalidFormatOrNotRequired("1-AB-234-5"));

			InvoiceLine.US_VisaNo = ZString.Empty;
			InvoiceLine.US_TextileCategoryNo = "~";
			AssertEquals(AgriculturalLicenseValidator.AgricultureLicenseNoCannotBeEnteredIfTextileInfoEntered, Validator.GetErrorTextIfInvalidFormatOrNotRequired("1-AB-234-5"));

			InvoiceLine.US_TextileCategoryNo = ZString.Empty;
			InvoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Today;
			AssertEquals(AgriculturalLicenseValidator.AgricultureLicenseNoCannotBeEnteredIfTextileInfoEntered, Validator.GetErrorTextIfInvalidFormatOrNotRequired("1-AB-234-5"));

			InvoiceLine.US_DateOfExportFromCountryOfOrigin = ZDateTime.Empty;
			AssertEquals(ZString.Empty, Validator.GetErrorTextIfInvalidFormatOrNotRequired("1-AB-234-5"));
		}

		protected override string[] ValidNumbers => new[] { "1-AB-234-5", "6-N -789-0" };

		protected override string[] InvalidNumbers => new[] { "123", "ABC" };

		protected override string LicenseTypeCode => LicencePermitTypeList.Codes._14;

		protected override string LicenseTypeDescription => "Agricultural License";

		protected override string FormatMask => @"[0-9]{1}-([a-z]{2}|[a-z]{1}\s)-[0-9]{3}-[0-9]{1}";

		protected override string FormatErrorText => "'N-AA-NNN-N' or 'N-AB-NNN-N' where 'N'-numeric, 'A'-alphabetic, AND 'B'-space.";

		protected override void SetupExtraTestDataForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			base.SetupExtraTestDataForInvoiceLine(invoiceLine);
			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
		}
	}
}
