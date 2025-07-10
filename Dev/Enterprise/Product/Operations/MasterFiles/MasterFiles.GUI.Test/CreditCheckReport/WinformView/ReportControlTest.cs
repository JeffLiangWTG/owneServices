using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Tests;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.ROPE.Model;
using static Enterprise.MasterFiles.GUI.Test.ImageBitmapTestHelper;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class ReportControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestAfterFirstBinding()
		{
			using (var form = new ZForm())
			using (var reportsControl = new ReportsControl())
			{
				var reportModel = new ReportsModel(new List<ReportItemModel> { new ReportItemModel(CreditReportType.ComprehensiveReport) }, null, null, null);
				form.Controls.Add(reportsControl);
				reportsControl.SetDataBinding(reportModel, string.Empty);
				form.Show();
				var reportItemControls = form.Controls.Find("ReportItemControl", true).Cast<ReportItemControl>();
				AssertEquals(1, reportItemControls.Count());
				var reportItemControl = reportItemControls.Single();
				AssertImageBitsEquals(Properties.Resources.ComprehensiveReport, reportItemControl.reportPictureBox.Image);
				AssertEquals(System.Windows.Forms.PictureBoxSizeMode.Zoom, reportItemControl.reportPictureBox.SizeMode);
				AssertEquals("Comprehensive Report", reportItemControl.reportNameLabel.Text);
				AssertEquals("More Info", reportItemControl.moreInfoLinkLabel.Text);
				AssertImageBitsEquals(Properties.Resources.WarningRed, reportItemControl.creditEventPictureBox.Image);
				AssertEquals(System.Windows.Forms.PictureBoxSizeMode.Zoom, reportItemControl.creditEventPictureBox.SizeMode);
				AssertEquals(false, reportItemControl.creditEventPictureBox.Visible);
				AssertEquals("Get Report", reportItemControl.getReportButton.Text);
			}
		}

		[RequiresSTA]
		public void TestReportItemControlDisposed_AfterRemovedOrParentControlDisposed()
		{
			using (var form = new ZForm())
			using (var reportsControl = new ReportsControl())
			{
				var reportModel = new ReportsModel(new List<ReportItemModel>
				{
					new ReportItemModel(CreditReportType.ComprehensiveReport),
					new ReportItemModel(CreditReportType.LatePaymentRisk)
				}, null, null, null);
				form.Controls.Add(reportsControl);
				reportsControl.SetDataBinding(reportModel, string.Empty);
				form.Show();
				var reportItemControls = form.Controls.Find("ReportItemControl", true).Cast<ReportItemControl>();
				AssertEquals(2, reportItemControls.Count());
				var reportItemControl = reportItemControls.FirstOrDefault();
				AssertEquals(false, reportItemControl.IsDisposed);
				var kFlowLayoutPanels = form.Controls.Find("flowLayoutPanel", true).Cast<KFlowLayoutPanel>();
				AssertEquals(1, kFlowLayoutPanels.Count());
				var kFlowLayoutPanel = kFlowLayoutPanels.FirstOrDefault();
				kFlowLayoutPanel.Controls.Remove(reportItemControl);
				AssertEquals(true, reportItemControl.IsDisposed);
				reportItemControls = form.Controls.Find("ReportItemControl", true).Cast<ReportItemControl>();
				AssertEquals(1, reportItemControls.Count());

				AssertEquals(false, kFlowLayoutPanel.IsDisposed);
				kFlowLayoutPanel.Dispose();
				AssertEquals(0, kFlowLayoutPanel.Controls.Count);
				AssertEquals(true, kFlowLayoutPanel.IsDisposed);
			}
		}

		[RequiresSTA]
		public void TestToolTipShow_WhenCreditEventHappened_AllReportItemEffected()
		{
			using (var form = new ZForm())
			using (var configForTest = new SupportCreditCheckForTest())
			using (var mainPageControl = new MainPageControl())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetCreditReportItemCollectionForTest()))
			{
				var dummyMainPageModel = new MainPageModel();

				var orgCusCode = Factory.New<OrgCusCode>();
				orgCusCode.OK_CustomsRegNo = "213";
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "I have events";
				org.OH_RL_NKClosestPort = "AU";
				org.CustomsCodes.Add(orgCusCode);
				Factory.Save();
				configForTest.BindingSource.DataSource = org;

				var client = new CreditCheckServiceForTest();
				dummyMainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				dummyMainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>() { new ReportItemModel(CreditReportType.ComprehensiveReport), new ReportItemModel(CreditReportType.FailureRisk) }, configForTest, client, dummyMainPageModel);
				dummyMainPageModel.EventsBannerModel = new EventsBannerModel(configForTest, client, dummyMainPageModel);
				creditReportItem.FailureRiskEnabled = true;

				mainPageControl.BindingSource.SetDataBinding(dummyMainPageModel, string.Empty);
				form.Controls.Add(mainPageControl);
				form.Show();

				var flowLayoutPanel = mainPageControl.Controls.Find("flowLayoutPanel", true);
				var reportItemControl1 = flowLayoutPanel[0].Controls.Find("ReportItemControl", true)[0] as ReportItemControl;
				var reportItemControl2 = flowLayoutPanel[0].Controls.Find("ReportItemControl", true)[1] as ReportItemControl;

				CombineAssertions(() =>
				{
					AssertNotNull(reportItemControl1);
					AssertNotNull(reportItemControl2);
					AssertImageBitsEquals(Properties.Resources.WarningOrange, reportItemControl1.creditEventPictureBox.Image);
					Assert(reportItemControl1.creditEventPictureBox.Visible);
					AssertImageBitsEquals(Properties.Resources.WarningOrange, reportItemControl2.creditEventPictureBox.Image);
					Assert(reportItemControl2.creditEventPictureBox.Visible);
#if !WINZOR
					AssertEquals(Color.FromArgb(255, 255, 102, 0), reportItemControl1.creditEventToolTip.BackColor);
					AssertEquals(Color.FromArgb(255, 255, 102, 0), reportItemControl2.creditEventToolTip.BackColor);
#endif
				});
			}
		}

		public void TestToolTipShow_WhenCreditEventHappened_ScoreChangeOnlyComprehensiveReportEffected()
		{
			using (var form = new ZForm())
			using (var configForTest = new SupportCreditCheckForTest())
			using (var mainPageControl = new MainPageControl())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetCreditReportItemCollectionForTest()))
			{
				var dummyMainPageModel = new MainPageModel();

				var orgCusCode = Factory.New<OrgCusCode>();
				orgCusCode.OK_CustomsRegNo = "213";
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "I have events";
				org.OH_RL_NKClosestPort = "AU";
				org.CustomsCodes.Add(orgCusCode);
				Factory.Save();
				configForTest.BindingSource.DataSource = org;

				var client = new CreditCheckServiceForTest();
				client.DummyEvents = new List<CreditEvent>()
				{
					new CreditEvent() { EventDate = DateTime.Today.AddMonths(-5), Type = CreditEventType.ScoreChange }
				};

				dummyMainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				dummyMainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>() { new ReportItemModel(CreditReportType.ComprehensiveReport), new ReportItemModel(CreditReportType.FailureRisk) }, configForTest, client, dummyMainPageModel);
				dummyMainPageModel.EventsBannerModel = new EventsBannerModel(configForTest, client, dummyMainPageModel);
				creditReportItem.FailureRiskEnabled = true;

				mainPageControl.BindingSource.SetDataBinding(dummyMainPageModel, string.Empty);
				form.Controls.Add(mainPageControl);
				form.Show();

				var flowLayoutPanel = mainPageControl.Controls.Find("flowLayoutPanel", true);
				var reportItemControl1 = flowLayoutPanel[0].Controls.Find("ReportItemControl", true)[0] as ReportItemControl;
				var reportItemControl2 = flowLayoutPanel[0].Controls.Find("ReportItemControl", true)[1] as ReportItemControl;

				CombineAssertions(() =>
				{
					AssertNotNull(reportItemControl1);
					AssertNotNull(reportItemControl2);
					AssertImageBitsEquals(Properties.Resources.WarningRed, reportItemControl1.creditEventPictureBox.Image);
					Assert(reportItemControl1.creditEventPictureBox.Visible);
					Assert(!reportItemControl2.creditEventPictureBox.Visible);
#if !WINZOR
					AssertEquals(Color.FromArgb(255, 238, 68, 58), reportItemControl1.creditEventToolTip.BackColor);
#endif
				});
			}
		}

		public void TestToolTipShow_WhenCreditEventHappened_FinancialChangeOnlyComprehensiveReportEffected()
		{
			using (var form = new ZForm())
			using (var configForTest = new SupportCreditCheckForTest())
			using (var mainPageControl = new MainPageControl())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetCreditReportItemCollectionForTest()))
			{
				var dummyMainPageModel = new MainPageModel();

				var orgCusCode = Factory.New<OrgCusCode>();
				orgCusCode.OK_CustomsRegNo = "213";
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "I have events";
				org.OH_RL_NKClosestPort = "AU";
				org.CustomsCodes.Add(orgCusCode);
				Factory.Save();
				configForTest.BindingSource.DataSource = org;

				var client = new CreditCheckServiceForTest();
				client.DummyEvents = new List<CreditEvent>()
				{
					new CreditEvent() { EventDate = DateTime.Today.AddMonths(-5), Type = CreditEventType.FinancialChange }
				};

				dummyMainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				dummyMainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>() { new ReportItemModel(CreditReportType.ComprehensiveReport), new ReportItemModel(CreditReportType.FailureRisk) }, configForTest, client, dummyMainPageModel);
				dummyMainPageModel.EventsBannerModel = new EventsBannerModel(configForTest, client, dummyMainPageModel);
				creditReportItem.FailureRiskEnabled = true;

				mainPageControl.BindingSource.SetDataBinding(dummyMainPageModel, string.Empty);
				form.Controls.Add(mainPageControl);
				form.Show();

				var flowLayoutPanel = mainPageControl.Controls.Find("flowLayoutPanel", true);
				var reportItemControl1 = flowLayoutPanel[0].Controls.Find("ReportItemControl", true)[0] as ReportItemControl;
				var reportItemControl2 = flowLayoutPanel[0].Controls.Find("ReportItemControl", true)[1] as ReportItemControl;

				CombineAssertions(() =>
				{
					AssertNotNull(reportItemControl1);
					AssertNotNull(reportItemControl2);
					AssertImageBitsEquals(Properties.Resources.InfoGrey, reportItemControl1.creditEventPictureBox.Image);
					Assert(reportItemControl1.creditEventPictureBox.Visible);
					Assert(!reportItemControl2.creditEventPictureBox.Visible);
#if !WINZOR
					AssertEquals(Color.FromArgb(255, 113, 116, 124), reportItemControl1.creditEventToolTip.BackColor);
#endif
				});
			}
		}

		CreditReportItemCollection GetCreditReportItemCollectionForTest()
		{
			creditReportItem = new CreditReportItem
			{
				CountryEnabledForCompany = true,
				CountryEnabledForOrganisation = true,
				CountryCode = "AU",
				CommercialBureauEnquiryEnabled = false,
				FailureRiskEnabled = false,
				ComprehensiveReportEnabled = true,
				LatePaymentRiskEnabled = false
			};

			var creditReportItemCollection = new CreditReportItemCollection() { creditReportItem };

			Factory.Save();

			return creditReportItemCollection;
		}

		CreditReportItem creditReportItem;
	}
}
