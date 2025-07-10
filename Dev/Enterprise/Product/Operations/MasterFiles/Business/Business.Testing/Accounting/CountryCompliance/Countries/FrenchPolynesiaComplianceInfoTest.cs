using Enterprise.Integration.Compliance;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(FrenchPolynesiaComplianceInfo))]
	sealed class FrenchPolynesiaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.FrenchPolynesia;

		public override void TestHasExtraTaxInfo()
		{
			AssertEquals(true, ComplianceInfo.HasExtraTaxInfo());
		}

		public override void TestGetExtraTaxOSAmountCaption()
		{
			var caption = ComplianceInfo.GetExtraTaxOSAmountCaption();
			AssertEquals("CPS Amount", caption.Caption);
			AssertEquals("CPS Amt", caption.ShortCaption);
		}

		public override void TestGetExtraTaxLocalAmountCaption()
		{
			var caption = ComplianceInfo.GetExtraTaxLocalAmountCaption();
			AssertEquals("CPS Local", caption.Caption);
		}

		public override void TestGetTaxOSAmountCaption()
		{
			var caption = ComplianceInfo.GetTaxOSAmountCaption();
			AssertEquals("TVA Amount", caption.Caption);
			AssertEquals("TVA Amt", caption.ShortCaption);
		}

		public override void TestGetTaxLocalAmountCaption()
		{
			var caption = ComplianceInfo.GetTaxLocalAmountCaption();
			AssertEquals("TVA Local", caption.Caption);
		}

		public override void TestGetExtraTax()
		{
			AssertEquals("CPS", ComplianceInfo.GetExtraTaxDescription(AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase));
		}

		ICountryComplianceInfo ComplianceInfo => complianceInfo ?? (complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode));
		ICountryComplianceInfo complianceInfo;
	}
}
