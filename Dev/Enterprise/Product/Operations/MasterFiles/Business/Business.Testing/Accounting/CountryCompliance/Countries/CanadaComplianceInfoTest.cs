using Enterprise.Integration.Compliance;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(CanadaComplianceInfo))]
	sealed class CanadaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Canada;

		public override void TestGetExtraTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("QST Amt", complianceInfo.GetExtraTaxOSAmountCaption().ShortCaption);
			AssertEquals("QST Amount", complianceInfo.GetExtraTaxOSAmountCaption().Caption);
		}

		public override void TestGetExtraTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("QST Local", complianceInfo.GetExtraTaxLocalAmountCaption().Caption);
		}

		public override void TestGetTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("GST Amt", complianceInfo.GetTaxOSAmountCaption().ShortCaption);
			AssertEquals("GST Amount", complianceInfo.GetTaxOSAmountCaption().Caption);
		}

		public override void TestGetTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("GST Local", complianceInfo.GetTaxLocalAmountCaption().Caption);
		}
	}
}
