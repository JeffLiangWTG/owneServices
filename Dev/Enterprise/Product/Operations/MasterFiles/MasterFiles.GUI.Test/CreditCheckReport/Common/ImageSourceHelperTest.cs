using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class ImageSourceHelperTest : TestCase
	{
		public void TestImageSource()
		{
			CombineAssertions(() =>
			{
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/ComingSoonBackground.png", ImageSourceHelper.ComingSoonBackground);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/TickGreen.png", ImageSourceHelper.TickGreen);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/InfoGrey.png", ImageSourceHelper.InfoGrey);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/WarningRed.png", ImageSourceHelper.WarningRed);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/WarningOrange.png", ImageSourceHelper.WarningOrange);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/ION-RGB-Logo.png", ImageSourceHelper.IonLogo);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/CommercialBureauInquiryReport-BG.png", ImageSourceHelper.CommercialBureauInquiryReportBackGround);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/LatePaymentRiskReport-BG.png", ImageSourceHelper.LatePaymentRiskReportBackGround);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/ComprehensiveReport-BG.png", ImageSourceHelper.ComprehensiveReportBackGround);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/FailureRiskReport-BG.png", ImageSourceHelper.FailureRiskReportBackGround);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/LatePaymentRiskReport.png", ImageSourceHelper.LatePaymentRiskReport);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/CommercialBureauInquiryReport.png", ImageSourceHelper.CommercialBureauInquiryReport);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/ComprehensiveReport.png", ImageSourceHelper.ComprehensiveReport);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/FailureRiskReport.png", ImageSourceHelper.FailureRiskReport);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/LatePaymentRiskReport.png", ImageSourceHelper.LatePaymentRiskReport);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/CommercialBureauInquiryReport.png", ImageSourceHelper.GetReportIcon(CreditReportType.CommercialBureauEnquiry));
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/ComprehensiveReport.png", ImageSourceHelper.GetReportIcon(CreditReportType.ComprehensiveReport));
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/FailureRiskReport.png", ImageSourceHelper.GetReportIcon(CreditReportType.FailureRisk));
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/LatePaymentRiskReport.png", ImageSourceHelper.GetReportIcon(CreditReportType.LatePaymentRisk));
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/WarningRed.png", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.ScoreChange).Icon);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/WarningOrange.png", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.CollectionChange).Icon);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/WarningOrange.png", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.CourtChange).Icon);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/WarningOrange.png", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.PublicFilingChange).Icon);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/InfoGrey.png", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.DirectorChange).Icon);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/InfoGrey.png", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.ShareholderChange).Icon);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/InfoGrey.png", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.FinancialChange).Icon);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/InfoGrey.png", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.StatusChange).Icon);
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/ComprehensiveReport-BG.png", ImageSourceHelper.GetReportBackground(CreditReportType.ComprehensiveReport));
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/FailureRiskReport-BG.png", ImageSourceHelper.GetReportBackground(CreditReportType.FailureRisk));
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/LatePaymentRiskReport-BG.png", ImageSourceHelper.GetReportBackground(CreditReportType.LatePaymentRisk));
				AssertEquals("/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/CommercialBureauInquiryReport-BG.png", ImageSourceHelper.GetReportBackground(CreditReportType.CommercialBureauEnquiry));
				AssertEquals("#EE443A", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.ScoreChange).Color);
				AssertEquals("#FF6600", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.CollectionChange).Color);
				AssertEquals("#FF6600", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.CourtChange).Color);
				AssertEquals("#FF6600", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.PublicFilingChange).Color);
				AssertEquals("#71747C", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.DirectorChange).Color);
				AssertEquals("#71747C", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.ShareholderChange).Color);
				AssertEquals("#71747C", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.FinancialChange).Color);
				AssertEquals("#71747C", ImageSourceHelper.GetCreditEventIconInfo(CreditEventType.StatusChange).Color);
			});
		}
	}
}
