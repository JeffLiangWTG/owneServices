using NUnit.Framework;
using WTG.CreditCheck;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class CreditCheckCodeMappingTest : TestCase
	{
		public void TestConstants()
		{
			AssertEquals("CRD", Constants.BillingCategory);
			AssertEquals("CW1", Constants.ReportingSource);
		}

		public void TestMapping()
		{
			var mapping = new CreditCheckCodeMapping();

			CombineAssertions(() =>
			{
				AssertEquals(Constants.BusinessVerification, mapping.ReportTypePriceItemCodeMapping[(CreditReportType.CommercialBureauEnquiry, true)]);
				AssertEquals(Constants.BusinessVerificationUnpaid, mapping.ReportTypePriceItemCodeMapping[(CreditReportType.CommercialBureauEnquiry, false)]);

				AssertEquals(Constants.DecisionSupport, mapping.ReportTypePriceItemCodeMapping[(CreditReportType.LatePaymentRisk, true)]);
				AssertEquals(Constants.DecisionSupportUnpaid, mapping.ReportTypePriceItemCodeMapping[(CreditReportType.LatePaymentRisk, false)]);

				AssertEquals(Constants.DelinquencyScore, mapping.ReportTypePriceItemCodeMapping[(CreditReportType.FailureRisk, true)]);
				AssertEquals(Constants.DelinquencyScoreUnpaid, mapping.ReportTypePriceItemCodeMapping[(CreditReportType.FailureRisk, false)]);

				AssertEquals(Constants.EnterpriseManagement, mapping.ReportTypePriceItemCodeMapping[(CreditReportType.ComprehensiveReport, true)]);
				AssertEquals(Constants.EnterpriseManagementUnpaid, mapping.ReportTypePriceItemCodeMapping[(CreditReportType.ComprehensiveReport, false)]);

				AssertEquals(ResourceStringHelper.BusinessVerification, mapping.PriceItemCodeDescriptionMapping[Constants.BusinessVerification]);
				AssertEquals(ResourceStringHelper.DecisionSupport, mapping.PriceItemCodeDescriptionMapping[Constants.DecisionSupport]);
				AssertEquals(ResourceStringHelper.DelinquencyScore, mapping.PriceItemCodeDescriptionMapping[Constants.DelinquencyScore]);
				AssertEquals(ResourceStringHelper.EnterpriseManagement, mapping.PriceItemCodeDescriptionMapping[Constants.EnterpriseManagement]);
			});
		}
	}
}
