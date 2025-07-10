using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CaribbeanBasinTradePartnershipActCertificateValidatorTest : PermitValidatorTest<CaribbeanBasinTradePartnershipActCertificateValidator>
	{
		public void TestPermitNoRequired()
		{
			AssertContains("The Caribbean Basin Trade Partnership Act (CBTPA) Certification number is required.", Validator.GetErrorTextForRequirement(ZString.Empty));

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertNotContains("The Caribbean Basin Trade Partnership Act (CBTPA) Certification number is required.", Validator.GetErrorTextForRequirement(ZString.Empty));

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			AssertNotContains("The Caribbean Basin Trade Partnership Act (CBTPA) Certification number is required.", Validator.GetErrorTextForRequirement(ZString.Empty));

			InvoiceLine.Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
			AssertContains("The Caribbean Basin Trade Partnership Act (CBTPA) Certification number is required.", Validator.GetErrorTextForRequirement(ZString.Empty));
		}

		public void TestPermitNoMayOnlyBeEnteredForTariff9820115()
		{
			AssertEquals(ZString.Empty, Validator.GetErrorTextIfInvalidFormatOrNotRequired("9CB999999"));

			InvoiceLine.US_SupTariff = ZString.Empty;
			AssertEquals(CaribbeanBasinTradePartnershipActCertificateValidator.CBTPACertificateNoMayOnlyBeEnteredForTariff9820115, Validator.GetErrorTextIfInvalidFormatOrNotRequired("9CB999999"));
		}

		protected override string[] ValidNumbers => new[] { "9CB999999", "1CB234567" };

		protected override string[] InvalidNumbers => new[] { "9BC999999", "1CB2345678", "0CBA123456" };

		protected override string[] PermitRequiredEntryTypes => System.Array.Empty<string>();

		protected override string[] PermitNotRequiredEntryTypes => System.Array.Empty<string>();

		protected override string LicenseTypeCode => LicencePermitTypeList.Codes._18;

		protected override string LicenseTypeDescription => "Caribbean Basin Trade Partnership Act (CBTPA) Certification";

		protected override string FormatMask => @"[0-9]{1}CB[0-9]{6}";

		protected override string FormatErrorText => "character 1 & 3-9 must be numeric, characters 2-3 must be 'CB' - e.g. 9CB999999.";

		protected override void SetupExtraTestDataForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			base.SetupExtraTestDataForInvoiceLine(invoiceLine);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "98201115";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;

			invoiceLine.US_SupTariff = tariff.UE_Tariff;
		}
	}
}
