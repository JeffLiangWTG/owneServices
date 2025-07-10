using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class GetSpecifiedReportModelTest : TestCaseWithFactory
	{
		public void TestShowSpecifiedReportForm()
		{
			var allReportTypes = new List<CreditReportType>()
			{
				CreditReportType.ComprehensiveReport,
				CreditReportType.CommercialBureauEnquiry,
				CreditReportType.LatePaymentRisk,
				CreditReportType.FailureRisk,
			};

			foreach (var creditReportType in allReportTypes)
			{
				using (var configForTest = new SupportCreditCheckForTest())
				{
					var getSpecifiedReportModel = new GetSpecifiedReportModel(creditReportType, configForTest);
					var org = Factory.NewWithValidTestData<OrgHeader>();
					org.OH_FullName = "Demo+Org";
					org.OH_RL_NKClosestPort = "AU";
					Factory.Save();

					configForTest.BindingSource.DataSource = org;
					AssertCommonProperties(creditReportType, getSpecifiedReportModel);
				}
			}
		}

		void AssertCommonProperties(CreditReportType reportType, GetSpecifiedReportModel reportModel)
		{
			AssertEquals(ResourceStringHelper.GetSpecifiedReport(reportType), reportModel.FormTitle);
			AssertEquals(ResourceStringHelper.GetReportCaption(reportType), reportModel.ReportCaption);
			AssertEquals(ResourceStringHelper.GetReportDescription(reportType), reportModel.ReportDescription);
			AssertEquals(ResourceStringHelper.GetRiskDecisionsCaption(reportType), reportModel.RiskDecisionsCaption);
			AssertEquals(ResourceStringHelper.AddEDocDescription, reportModel.AddEDocDescription);
			AssertEquals(ResourceStringHelper.ViewSampleReport, reportModel.ViewSampleReport);
			AssertEquals(ResourceStringHelper.PricingLevelCaption, reportModel.PricingLevelCaption);
			AssertEquals(ResourceStringHelper.GetReport, reportModel.GetReportButtonCaption);
			AssertTypeRelatedProperties(reportType, reportModel);
		}

		void AssertTypeRelatedProperties(CreditReportType reportType, GetSpecifiedReportModel reportModel)
		{
			List<string> expectedRiskDecisionDescriptions = null;
			var expectedPricingLevel = string.Empty;
			var reportPrefix = string.Empty;
			switch (reportType)
			{
				case CreditReportType.ComprehensiveReport:
					expectedRiskDecisionDescriptions = new List<string>() { ResourceStringHelper.RiskDecision1Description, ResourceStringHelper.RiskDecision2Description, ResourceStringHelper.RiskDecision3Description, ResourceStringHelper.RiskDecision4Description };
					expectedPricingLevel = ResourceStringHelper.EnterpriseManagement;
					reportPrefix = "CR";
					break;
				case CreditReportType.FailureRisk:
					expectedRiskDecisionDescriptions = new List<string>() { ResourceStringHelper.RiskDecision1Description, ResourceStringHelper.RiskDecision8Description, ResourceStringHelper.RiskDecision9Description, ResourceStringHelper.RiskDecision0Description };
					expectedPricingLevel = ResourceStringHelper.DelinquencyScore;
					reportPrefix = "FR";
					break;
				case CreditReportType.LatePaymentRisk:
					expectedRiskDecisionDescriptions = new List<string>() { ResourceStringHelper.RiskDecision2Description, ResourceStringHelper.RiskDecision7Description };
					expectedPricingLevel = ResourceStringHelper.DecisionSupport;
					reportPrefix = "LPR";
					break;
				case CreditReportType.CommercialBureauEnquiry:
					expectedRiskDecisionDescriptions = new List<string>() { ResourceStringHelper.RiskDecision5Description, ResourceStringHelper.RiskDecision6Description };
					expectedPricingLevel = ResourceStringHelper.BusinessVerification;
					reportPrefix = "COM";
					break;
				default:
					expectedRiskDecisionDescriptions = new List<string>() { ResourceStringHelper.UnknownReportType, ResourceStringHelper.UnknownReportType, ResourceStringHelper.UnknownReportType };
					break;
			}

			var uriString = $@"/demoreport/{reportPrefix}_Report.html?organization=Demo+Org";
			WebUrlLauncher.ClearLastUrlLaunched();
			var expectedUri = new Uri(new Uri(OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.Value.Cast<CodeDescriptionBool>().Single(u => u.Bool).Description), uriString);
			reportModel.ShowSampleReport();
			AssertEquals(expectedUri.AbsoluteUri, HttpUtility.UrlDecode(WebUrlLauncher.LastUrlLaunched));
			AssertEquals(expectedPricingLevel, reportModel.PricingLevel);
			AssertEquals(expectedRiskDecisionDescriptions.Count, reportModel.RiskDecisionDescriptions.Count);
			AssertArrayEqualsByElements(expectedRiskDecisionDescriptions.ToArray(), reportModel.RiskDecisionDescriptions.ToArray());
		}
	}
}
