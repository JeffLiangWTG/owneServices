using System;
using System.Drawing;
using Enterprise.ZArchitecture.Core;
using WTG.ROPE.Model;
using static WTG.CreditCheck.Constants;

namespace Enterprise.MasterFiles.GUI
{
	public static class ImageBitmapHelper
	{
		public static readonly Color CompanyInfoPanelBackColor = Color.FromArgb(255, 240, 240, 240);
		public static readonly Color CompanyInfoPanelInnerHoverColor = Color.FromArgb(255, 100, 182, 222);
		public static readonly Color CompanyInfoPanelInnerClickColor = Color.FromArgb(255, 73, 169, 215);
		public static readonly Color CompanyInfoPanelHoverColor = Color.FromArgb(255, 198, 217, 227);
		public static readonly Color CompanyInfoPanelClickColor = Color.FromArgb(255, 176, 209, 225);

		public static Bitmap GetReportTypeIcon(CreditReportType type)
		{
			switch (type)
			{
				case CreditReportType.ComprehensiveReport:
					return Properties.Resources.ComprehensiveReport;
				case CreditReportType.FailureRisk:
					return Properties.Resources.FailureRiskReport;
				case CreditReportType.LatePaymentRisk:
					return Properties.Resources.LatePaymentRiskReport;
				case CreditReportType.CommercialBureauEnquiry:
					return Properties.Resources.CommercialBureauInquiryReport;
				default:
					throw new NotImplementedException("Cannot find the image");
			}
		}

		public static Bitmap GetReportTypeBackground(CreditReportType type)
		{
			switch (type)
			{
				case CreditReportType.ComprehensiveReport:
					return Properties.Resources.ComprehensiveReport_BG;
				case CreditReportType.FailureRisk:
					return Properties.Resources.FailureRiskReport_BG;
				case CreditReportType.LatePaymentRisk:
					return Properties.Resources.LatePaymentRiskReport_BG;
				case CreditReportType.CommercialBureauEnquiry:
					return Properties.Resources.CommercialBureauInquiryReport_BG;
				default:
					throw new NotImplementedException("Cannot find the image");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Image and Color")]
		public static (Bitmap Icon, string Color) GetCreditEventIconInfo(CreditEventType type)
		{
			switch (type)
			{
				case CreditEventType.ScoreChange:
					return (Properties.Resources.WarningRed, (NoResString)"#EE443A");
				case CreditEventType.CollectionChange:
				case CreditEventType.CourtChange:
				case CreditEventType.PublicFilingChange:
					return (Properties.Resources.WarningOrange, (NoResString)"#FF6600");
				case CreditEventType.DirectorChange:
				case CreditEventType.ShareholderChange:
				case CreditEventType.FinancialChange:
				case CreditEventType.StatusChange:
					return (Properties.Resources.InfoGrey, "#71747C");
				default:
					throw new NotImplementedException("Cannot find the image");
			}
		}

		public static Color GetScoreLabelColor(int companyItemScore) => companyItemScore >= HighConfidenceStartPoint ? Color.Green : companyItemScore >= MediumConfidenceStartPoint ? Color.Orange : Color.Red;
	}
}
