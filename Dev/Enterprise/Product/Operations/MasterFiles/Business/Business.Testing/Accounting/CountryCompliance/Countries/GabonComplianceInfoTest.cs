using Enterprise.Core;
using Enterprise.Integration.Compliance;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(GabonComplianceInfo))]
	sealed class GabonComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Constants.CountryCodes.Gabon;

		protected override bool? ExpectedDefaultValueForDisplayRecipientTaxIDRegistry => true;

		public void TestGetConsumptionTaxRegistrationOrgCusCode()
		{
			var result = Country.GetConsumptionTaxRegistrationOrgCusCode(CountryCode);
			var expected = "TVA";

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
			AssertEquals("CSS", complianceInfo.GetExtraTaxDescription("QCT"));
		}

		public override void TestGetExtraTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			var caption = complianceInfo.GetExtraTaxOSAmountCaption();
			AssertEquals("CSS Amount", caption.Caption);
			AssertEquals("CSS Amt", caption.ShortCaption);
		}

		public override void TestGetExtraTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			var caption = complianceInfo.GetExtraTaxLocalAmountCaption();
			AssertEquals("CSS Local", caption.Caption);
		}
	}
}
