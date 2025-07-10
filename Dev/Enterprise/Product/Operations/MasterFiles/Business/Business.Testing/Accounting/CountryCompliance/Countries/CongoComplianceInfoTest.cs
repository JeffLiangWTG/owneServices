using Enterprise.Integration.Compliance;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(CongoComplianceInfo))]
	sealed class CongoComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Congo;

		protected override string ExpectedRecipientLocalBusinessRegNumberCodeType => "NIU";

		protected override string ExpectedRecipientLocalBusinessRegHeading => "CLIENT NIU #";

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => false;

		public void TestGetConsumptionTaxRegistrationOrgCusCode()
		{
			var result = Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCode);
			var expected = "NIU";

			AssertEquals(expected, result);
		}

		public void TestGetConsumptionTaxDescription()
		{
			var result = Country.GetConsumptionTaxDescription(CountryCode);
			var expected = "TVA";

			AssertEquals(expected, result);
		}

		public void TestGetIsGSTRegistered()
		{
			var result = Country.GetIsGSTRegistered(CountryCode);
			var expected = true;

			AssertEquals(expected, result);
		}

		public void TestIsCashBasisVAT()
		{
			var result = Country.IsGSTCashBasis(CountryCode);
			var expected = false;

			AssertEquals(expected, result);
		}

		public override void TestHasExtraTaxInfo()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals(true, complianceInfo.HasExtraTaxInfo());
		}

		public override void TestGetExtraTax()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("SURTAX", complianceInfo.GetExtraTaxDescription("QCT"));
		}

		public override void TestGetExtraTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			var caption = complianceInfo.GetExtraTaxOSAmountCaption();
			AssertEquals("SURTAX Amount", caption.Caption);
			AssertEquals("SURTAX Amt", caption.ShortCaption);
		}

		public override void TestGetExtraTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			var caption = complianceInfo.GetExtraTaxLocalAmountCaption();
			AssertEquals("SURTAX Local", caption.Caption);
		}

		public override void TestGetTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("TVA Amt", complianceInfo.GetTaxOSAmountCaption().ShortCaption);
			AssertEquals("TVA Amount", complianceInfo.GetTaxOSAmountCaption().Caption);
		}

		public override void TestGetTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("TVA Local", complianceInfo.GetTaxLocalAmountCaption().Caption);
		}
	}
}
