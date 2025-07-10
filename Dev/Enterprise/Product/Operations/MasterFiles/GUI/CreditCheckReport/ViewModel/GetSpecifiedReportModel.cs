using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using Enterprise.ZArchitecture.GUI;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI
{
	public class GetSpecifiedReportModel
	{
		public GetSpecifiedReportModel(CreditReportType reportType, CreditReportUserControl supportCreditCheck)
		{
			ReportType = reportType;
			this.supportCreditCheck = supportCreditCheck;
			mapping = new CreditCheckCodeMapping();
		}

		public void ShowSampleReport()
		{
			var url = string.Empty;
			var org = WebUtility.UrlEncode(supportCreditCheck.Header.OH_FullName);

			switch (ReportType)
			{
				case CreditReportType.ComprehensiveReport:
					url = $@"/demoreport/CR_Report.html?organization={org}";
					break;
				case CreditReportType.FailureRisk:
					url = $@"/demoreport/FR_Report.html?organization={org}";
					break;
				case CreditReportType.LatePaymentRisk:
					url = $@"/demoreport/LPR_Report.html?organization={org}";
					break;
				case CreditReportType.CommercialBureauEnquiry:
					url = $@"/demoreport/COM_Report.html?organization={org}";
					break;
			}

			if (!string.IsNullOrEmpty(url))
			{
				var uri = new Uri(new Uri(supportCreditCheck.ServiceWrapper.EndpointAddresses.FirstOrDefault(x => x.IsMain).Address), url);
				WebUrlLauncher.Launch(uri.AbsoluteUri);
			}
		}

		readonly CreditCheckCodeMapping mapping;

		public CreditReportType ReportType { get; }

		readonly CreditReportUserControl supportCreditCheck;

		public string FormTitle => ResourceStringHelper.GetSpecifiedReport(ReportType);

		public Image TopImage => ImageBitmapHelper.GetReportTypeBackground(ReportType);

		public string ReportCaption => ResourceStringHelper.GetReportCaption(ReportType);

		public string ReportDescription => ResourceStringHelper.GetReportDescription(ReportType);

		public string RiskDecisionsCaption => ResourceStringHelper.GetRiskDecisionsCaption(ReportType);

		public List<string> RiskDecisionDescriptions
		{
			get
			{
				switch (ReportType)
				{
					case CreditReportType.ComprehensiveReport:
						return new List<string>() { ResourceStringHelper.RiskDecision1Description, ResourceStringHelper.RiskDecision2Description, ResourceStringHelper.RiskDecision3Description, ResourceStringHelper.RiskDecision4Description };
					case CreditReportType.FailureRisk:
						return new List<string>() { ResourceStringHelper.RiskDecision1Description, ResourceStringHelper.RiskDecision8Description, ResourceStringHelper.RiskDecision9Description, ResourceStringHelper.RiskDecision0Description };
					case CreditReportType.LatePaymentRisk:
						return new List<string>() { ResourceStringHelper.RiskDecision2Description, ResourceStringHelper.RiskDecision7Description };
					case CreditReportType.CommercialBureauEnquiry:
						return new List<string>() { ResourceStringHelper.RiskDecision5Description, ResourceStringHelper.RiskDecision6Description };
					default:
						return new List<string>() { ResourceStringHelper.UnknownReportType, ResourceStringHelper.UnknownReportType, ResourceStringHelper.UnknownReportType };
				}
			}
		}

		public string AddEDocDescription => ResourceStringHelper.AddEDocDescription;

		public string ViewSampleReport => ResourceStringHelper.ViewSampleReport;

		public string PricingLevelCaption => ResourceStringHelper.PricingLevelCaption;

		public string PricingLevel => mapping.PriceItemCodeDescriptionMapping[mapping.ReportTypePriceItemCodeMapping[(ReportType, true)]];

		public string GetReportButtonCaption => ResourceStringHelper.GetReport;
	}
}
