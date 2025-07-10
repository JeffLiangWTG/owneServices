using Enterprise.Integration.Compliance;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing
{
	[TestedType(typeof(GhanaComplianceInfo))]
	sealed class GhanaComplianceInfoTest : CountryComplianceInfoTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Ghana;

		public override void TestGetExtraTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("NHIL/GETFL Amt", complianceInfo.GetExtraTaxOSAmountCaption().ShortCaption);
			AssertEquals("NHIL/GETFL Amount", complianceInfo.GetExtraTaxOSAmountCaption().FullDescription);
		}

		public override void TestGetExtraTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("NHIL/GETFL Local", complianceInfo.GetExtraTaxLocalAmountCaption().Caption);
		}

		public override void TestGetTaxOSAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("VAT Amt", complianceInfo.GetTaxOSAmountCaption().ShortCaption);
			AssertEquals("VAT Amount", complianceInfo.GetTaxOSAmountCaption().Caption);
		}

		public override void TestGetTaxLocalAmountCaption()
		{
			var complianceInfo = CountryComplianceFactory.GetCountryComplianceInfo(CountryCode) as ICountryComplianceInfo;
			AssertEquals("VAT Local", complianceInfo.GetTaxLocalAmountCaption().Caption);
		}
	}
}
