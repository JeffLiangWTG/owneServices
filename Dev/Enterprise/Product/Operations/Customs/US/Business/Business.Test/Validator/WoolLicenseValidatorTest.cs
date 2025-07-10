using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class WoolLicenseValidatorTest : PermitValidatorTest<WoolLicenseValidator>
	{
		public void TestPermitNoRequired()
		{
			InvoiceLine.US_SupTariff = ZString.Empty;
			AssertEquals("Wool License number is not required when tariff is not entered.", ZString.Empty, Validator.GetErrorTextForRequirement(ZString.Empty));

			InvoiceLine.US_SupTariff = "9902511610";
			AssertEquals("Wool License number is required when tariff rule 'WLE' applies", "The Wool License number is required.", Validator.GetErrorTextForRequirement(ZString.Empty));
			AssertEquals("Wool License number is entered for tariff, no error should display.", ZString.Empty, Validator.GetErrorTextForRequirement("W22123ABC"));

			InvoiceLine.US_SupTariff = ZString.Empty;
			AssertEquals("Wool License number should NOT be entered.", WoolLicenseValidator.WoolLicenseShouldNotBeEnteredForTariff, Validator.GetErrorTextIfInvalidFormatOrNotRequired("W22123ABC"));
		}

		protected override string[] ValidNumbers => new[] { "W22123ABC", "W01ABC234" };

		protected override string[] InvalidNumbers => new[] { "M22123ABC", "WO1ABC2345", "W01 ABC234" };

		protected override string[] PermitRequiredEntryTypes => System.Array.Empty<string>();

		protected override string[] PermitNotRequiredEntryTypes => System.Array.Empty<string>();

		protected override string LicenseTypeCode => LicencePermitTypeList.Codes._17;

		protected override string LicenseTypeDescription => "Wool License";

		protected override string FormatMask => @"W[a-zA-Z0-9]{8}";

		protected override string FormatErrorText => "first character should be 'W', characters 2-3 must represent the year of issue (e.g. 2008=08), the final six characters should be alpha numeric.";

		protected override void SetupExtraTestDataForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			base.SetupExtraTestDataForInvoiceLine(invoiceLine);

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9902511610";
			tariff.UE_ShortDescription = "CASHMERE HAIR FM SBHD 5102";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);

			var tariffRule = Factory.New<USCTariffRule>();
			tariffRule.U1_RuleCode = TariffRuleList.Codes.WoolLicenseEligible;
			tariffRule.U1_Tariff = "99025115";
			tariffRule.U1_TariffTo = "99025116";
			tariffRule.U1_DateFrom = ZDateTime.BrettsBirthday;
			invoiceLine.US_SupTariff = tariff.UE_Tariff;
		}
	}
}
