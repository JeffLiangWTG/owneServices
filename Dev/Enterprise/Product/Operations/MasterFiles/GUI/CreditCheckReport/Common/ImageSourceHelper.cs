using System;
using System.Globalization;
using Enterprise.ZArchitecture.Core;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public static class ImageSourceHelper
	{
		public static string ComingSoonBackground => GetSource("ComingSoonBackground.png");

		public static string TickGreen => GetSource("TickGreen.png");

		public static string InfoGrey => GetSource("InfoGrey.png");

		public static string WarningRed => GetSource("WarningRed.png");

		public static string WarningOrange => GetSource("WarningOrange.png");

		public static string IonLogo => GetSource("ION-RGB-Logo.png");

		public static string CommercialBureauInquiryReportBackGround => GetSource("CommercialBureauInquiryReport-BG.png");

		public static string ComprehensiveReportBackGround => GetSource("ComprehensiveReport-BG.png");

		public static string FailureRiskReportBackGround => GetSource("FailureRiskReport-BG.png");

		public static string LatePaymentRiskReportBackGround => GetSource("LatePaymentRiskReport-BG.png");

		public static string CommercialBureauInquiryReport => GetSource("CommercialBureauInquiryReport.png");

		public static string ComprehensiveReport => GetSource("ComprehensiveReport.png");

		public static string FailureRiskReport => GetSource("FailureRiskReport.png");

		public static string LatePaymentRiskReport => GetSource("LatePaymentRiskReport.png");

		public static string GetReportIcon(CreditReportType type)
		{
			switch (type)
			{
				case CreditReportType.ComprehensiveReport:
					return ComprehensiveReport;
				case CreditReportType.FailureRisk:
					return FailureRiskReport;
				case CreditReportType.LatePaymentRisk:
					return LatePaymentRiskReport;
				case CreditReportType.CommercialBureauEnquiry:
					return CommercialBureauInquiryReport;
				default:
					throw new NotImplementedException("Cannot find the image");
			}
		}

		public static string GetReportBackground(CreditReportType type)
		{
			switch (type)
			{
				case CreditReportType.ComprehensiveReport:
					return ComprehensiveReportBackGround;
				case CreditReportType.FailureRisk:
					return FailureRiskReportBackGround;
				case CreditReportType.LatePaymentRisk:
					return LatePaymentRiskReportBackGround;
				case CreditReportType.CommercialBureauEnquiry:
					return CommercialBureauInquiryReportBackGround;
				default:
					throw new NotImplementedException("Cannot find the image");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Image and Color")]
		public static (string Icon, string Color) GetCreditEventIconInfo(CreditEventType type)
		{
			switch (type)
			{
				case CreditEventType.ScoreChange:
					return (WarningRed, (NoResString)"#EE443A");
				case CreditEventType.CollectionChange:
				case CreditEventType.CourtChange:
				case CreditEventType.PublicFilingChange:
					return (WarningOrange, (NoResString)"#FF6600");
				case CreditEventType.DirectorChange:
				case CreditEventType.ShareholderChange:
				case CreditEventType.FinancialChange:
				case CreditEventType.StatusChange:
					return (InfoGrey, "#71747C");
				default:
					throw new NotImplementedException("Cannot find the image");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File Path parameter")]
		static string GetSource(string fileName)
		{
			return string.Format(CultureInfo.InvariantCulture, "/Enterprise.MasterFiles.GUI;component/CreditCheckReport/Resource/Image/{0}", fileName);
		}
	}
}
