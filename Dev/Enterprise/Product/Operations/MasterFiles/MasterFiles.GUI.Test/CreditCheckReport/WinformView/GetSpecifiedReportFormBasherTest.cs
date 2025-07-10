using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using WTG.ROPE.Model;
using static Enterprise.MasterFiles.GUI.Test.ImageBitmapTestHelper;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(GetSpecifiedReportForm))]
	public class GetSpecifiedReportFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var getSpecifiedReportForm = new GetSpecifiedReportForm(new GetSpecifiedReportModel(CreditReportType.ComprehensiveReport, null));

			return getSpecifiedReportForm;
		}

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
				using (var creditReportUserControl = new CreditReportUserControl())
				using (var getSpecifiedReportForm = new GetSpecifiedReportForm(new GetSpecifiedReportModel(creditReportType, creditReportUserControl)))
				{
					var org = Factory.NewWithValidTestData<OrgHeader>();
					org.OH_FullName = "Demo+Org";
					org.OH_RL_NKClosestPort = "AU";
					Factory.Save();

					creditReportUserControl.BindingSource.DataSource = org;
					getSpecifiedReportForm.Show();
					AssertCommonProperties(creditReportType, getSpecifiedReportForm);
					var viewSimpleReportLinkLabel = getSpecifiedReportForm.Controls.Find("ViewSimpleReportLinkLabel", true).Cast<ZLinkLabel>().First();
					WebUrlLauncher.ClearLastUrlLaunched();
					viewSimpleReportLinkLabel.PerformClick_ForTest();
					AssertWebUrlPath(creditReportType);
				}
			}
		}

		void AssertCommonProperties(CreditReportType reportType, GetSpecifiedReportForm form)
		{
			AssertFormSize(reportType, form);
			AssertFontStyle(form);
			AssertTopImage(reportType, form.Controls.Find("TopImage", true).Cast<ZPictureBox>().First().Image);
			AssertEquals(ResourceStringHelper.GetSpecifiedReport(reportType), form.Text);
			AssertEquals(ResourceStringHelper.GetReportCaption(reportType), form.Controls.Find("ReportCaptionLabel", true).Cast<ZLabel>().First().Text);
			AssertEquals(ResourceStringHelper.GetReportDescription(reportType), form.Controls.Find("ReportDescriptionLabel", true).Cast<ZLabel>().First().Text);
			AssertEquals(ResourceStringHelper.GetRiskDecisionsCaption(reportType), form.Controls.Find("RiskDecisionsCaptionLabel", true).Cast<ZLabel>().First().Text);
			AssertEquals(ResourceStringHelper.AddEDocDescription, form.Controls.Find("AddEdocDescriptionLabel", true).Cast<ZLabel>().First().Text);
			AssertEquals(ResourceStringHelper.ViewSampleReport, form.Controls.Find("ViewSimpleReportLinkLabel", true).Cast<ZLinkLabel>().First().Text);
			AssertEquals(ResourceStringHelper.GetReport, form.Controls.Find("GetReportButton", true).Cast<ZButton>().First().Text);
			AssertRiskDecisionDescriptions(reportType, form.Controls.Find("RiskDecisionsDescriptionTable", true).Cast<KTableLayoutPanel>().First());
		}

		void AssertRiskDecisionDescriptions(CreditReportType reportType, KTableLayoutPanel reportModel)
		{
			List<string> expectedRiskDecisionDescriptions = null;
			switch (reportType)
			{
				case CreditReportType.ComprehensiveReport:
					expectedRiskDecisionDescriptions = new List<string>() { ResourceStringHelper.RiskDecision1Description, ResourceStringHelper.RiskDecision2Description, ResourceStringHelper.RiskDecision3Description, ResourceStringHelper.RiskDecision4Description };
					break;
				case CreditReportType.FailureRisk:
					expectedRiskDecisionDescriptions = new List<string>() { ResourceStringHelper.RiskDecision1Description, ResourceStringHelper.RiskDecision8Description, ResourceStringHelper.RiskDecision9Description, ResourceStringHelper.RiskDecision0Description };
					break;
				case CreditReportType.LatePaymentRisk:
					expectedRiskDecisionDescriptions = new List<string>() { ResourceStringHelper.RiskDecision2Description, ResourceStringHelper.RiskDecision7Description };
					break;
				case CreditReportType.CommercialBureauEnquiry:
					expectedRiskDecisionDescriptions = new List<string>() { ResourceStringHelper.RiskDecision5Description, ResourceStringHelper.RiskDecision6Description };
					break;
				default:
					expectedRiskDecisionDescriptions = new List<string>() { ResourceStringHelper.UnknownReportType, ResourceStringHelper.UnknownReportType, ResourceStringHelper.UnknownReportType };
					break;
			}

			for (int i = 0; i < expectedRiskDecisionDescriptions.Count; i++)
			{
				var expectedDecisionLine = "●  " + expectedRiskDecisionDescriptions[i];
				var decisionLineLabel = reportModel.Controls[i] as ZLabel;
				AssertEquals(expectedDecisionLine, decisionLineLabel.Text);
			}
		}

		void AssertFormSize(CreditReportType reportType, GetSpecifiedReportForm form)
		{
			switch (reportType)
			{
				case CreditReportType.ComprehensiveReport:
					AssertEquals(ControlDpiScalingHelper.NewScaledSize(390, 538, true), form.Size);
					break;
				case CreditReportType.FailureRisk:
					AssertEquals(ControlDpiScalingHelper.NewScaledSize(390, 550, true), form.Size);
					break;
				case CreditReportType.LatePaymentRisk:
					AssertEquals(ControlDpiScalingHelper.NewScaledSize(390, 510, true), form.Size);
					break;
				case CreditReportType.CommercialBureauEnquiry:
					AssertEquals(ControlDpiScalingHelper.NewScaledSize(390, 475, true), form.Size);
					break;
			}
		}

		void AssertFontStyle(GetSpecifiedReportForm form)
		{
			AssertEquals("Segoe UI", form.Controls.Find("AddEdocDescriptionLabel", true).Cast<ZLabel>().First().Font.FontFamily.Name);
			AssertEquals(10, (int)form.Controls.Find("AddEdocDescriptionLabel", true).Cast<ZLabel>().First().Font.Size);
			AssertEquals("Segoe UI", form.Controls.Find("ReportDescriptionLabel", true).Cast<ZLabel>().First().Font.FontFamily.Name);
			AssertEquals(10, (int)form.Controls.Find("ReportDescriptionLabel", true).Cast<ZLabel>().First().Font.Size);
			AssertEquals("Segoe UI", form.Controls.Find("ViewSimpleReportLinkLabel", true).Cast<ZLinkLabel>().First().Font.FontFamily.Name);
			AssertEquals(10, (int)form.Controls.Find("ViewSimpleReportLinkLabel", true).Cast<ZLinkLabel>().First().Font.Size);
		}

		void AssertTopImage(CreditReportType reportType, Image topImage)
		{
			switch (reportType)
			{
				case CreditReportType.ComprehensiveReport:
					AssertImageBitsEquals(Properties.Resources.ComprehensiveReport_BG, topImage);
					break;
				case CreditReportType.FailureRisk:
					AssertImageBitsEquals(Properties.Resources.FailureRiskReport_BG, topImage);
					break;
				case CreditReportType.LatePaymentRisk:
					AssertImageBitsEquals(Properties.Resources.LatePaymentRiskReport_BG, topImage);
					break;
				case CreditReportType.CommercialBureauEnquiry:
					AssertImageBitsEquals(Properties.Resources.CommercialBureauInquiryReport_BG, topImage);
					break;
			}
		}

		void AssertWebUrlPath(CreditReportType reportType)
		{
			var uri = new Uri(new Uri(OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.Value.Cast<CodeDescriptionBool>().Single(u => u.Bool).Description), GetPath(reportType));
			AssertEquals(uri.AbsoluteUri, WebUrlLauncher.LastUrlLaunched);
		}

		string GetPath(CreditReportType reportType)
		{
			string reportPrefix = null;

			switch (reportType)
			{
				case CreditReportType.ComprehensiveReport:
					reportPrefix = "CR";
					break;
				case CreditReportType.FailureRisk:
					reportPrefix = "FR";
					break;
				case CreditReportType.LatePaymentRisk:
					reportPrefix = "LPR";
					break;
				case CreditReportType.CommercialBureauEnquiry:
					reportPrefix = "COM";
					break;
			}

			return $@"/demoreport/{reportPrefix}_Report.html?organization=Demo%2BOrg";
		}
	}
}
