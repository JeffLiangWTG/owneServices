using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Tests;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class EventsBannerControlTest : TestCaseWithFactory
	{
		public void TestResetEventHandlerWhenSetDataBinding()
		{
			using (var control = new EventsBannerControl())
			using (var configForTest = new SupportCreditCheckForTest())
			{
				var mainPageModel = new MainPageModel();
				var eventsBannerModel = new EventsBannerModel(configForTest, new CreditCheckServiceForTest(), null);
				mainPageModel.EventsBannerModel = eventsBannerModel;
				control.SetDataBinding(mainPageModel, "EventsBannerModel");

				AssertEquals(1, eventsBannerModel.GetEventHandlerCount());
				control.SetDataBinding(null, string.Empty);
				AssertEquals(0, eventsBannerModel.GetEventHandlerCount());
			}
		}

		public void TestResetEventHandler_WhenControlDispose()
		{
			using (var configForTest = new SupportCreditCheckForTest())
			{
				var control = new EventsBannerControl();
				var dummyMainPageModel = new MainPageModel();
				var eventBannerModel = new EventsBannerModel(configForTest, new CreditCheckServiceForTest(), null);
				dummyMainPageModel.EventsBannerModel = eventBannerModel;
				control.SetDataBinding(dummyMainPageModel, "EventsBannerModel");

				AssertEquals(1, eventBannerModel.GetEventHandlerCount());
				control.Dispose();
				AssertEquals(0, eventBannerModel.GetEventHandlerCount());
			}
		}

		public void TestEventsBannerControlDefault()
		{
			using (var form = new ZForm())
			using (var configForTest = new SupportCreditCheckForTest())
			using (var mainPageControl = new MainPageControl())
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "Test org";
				Factory.Save();
				configForTest.BindingSource.DataSource = org;

				var mainPageModel = new MainPageModel();
				mainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "orgName");
				mainPageModel.EventsBannerModel = new EventsBannerModel(configForTest, new CreditCheckServiceForTest(), null);
				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>(), configForTest, null, mainPageModel);
				mainPageControl.BindingSource.SetDataBinding(mainPageModel, string.Empty);
				form.Controls.Add(mainPageControl);
				form.Show();
				var eventsBannerControls = mainPageControl.Controls.Find("eventsBannerControl", true);
				AssertEquals(1, eventsBannerControls.Length);
				var errorMessageControls = mainPageControl.Controls.Find("eventListUnavailableMessageControl", true);
				AssertEquals(1, errorMessageControls.Length);
				var messageLabel = mainPageControl.Controls.Find("messageLabel", true);
				AssertEquals(1, messageLabel.Length);
			}
		}

		[RequiresSTA]
		public void TestSubSplitterControlDisposed_WhenAddingToADisposedParent()
		{
			AssertNoExceptionThrown(() =>
			{
				using (var form = new ZForm())
				using (var configForTest = new SupportCreditCheckForTest())
				using (var mainPageControl = new MainPageControl())
				{
					var org = Factory.NewWithValidTestData<OrgHeader>();
					org.OH_FullName = "Test org";
					Factory.Save();
					configForTest.BindingSource.DataSource = org;

					var mainPageModel = new MainPageModel();
					mainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "orgName");
					mainPageModel.EventsBannerModel = new EventsBannerModel(configForTest, null, null);
					mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>(), configForTest, null, mainPageModel);
					mainPageControl.BindingSource.SetDataBinding(mainPageModel, string.Empty);
					mainPageModel.EventsBannerModel.ErrorMessage = null;

					form.Controls.Add(mainPageControl);
					form.Show();
					var eventsBannerControls = mainPageControl.Controls.Find("eventsBannerControl", true);
					AssertEquals(1, eventsBannerControls.Length);
					var eventsBannerControl = eventsBannerControls[0];
					var property = typeof(EventsBannerControl).GetField("EventsTableLayout", BindingFlags.Instance | BindingFlags.NonPublic);
					var eventsLayout = (KTableLayoutPanel)property.GetValue(eventsBannerControl);
					eventsLayout.ControlAdded += (sender, args) => { form.Dispose(); };
					mainPageModel.EventsBannerModel.SetEvents(new List<CreditEvent> { new CreditEvent() });
				}
			});
		}

		public void TestEventsBannerItemDisposed_AfterRemovedOrParentControlDisposed()
		{
			using (var form = new ZForm())
			using (var configForTest = new SupportCreditCheckForTest())
			using (var mainPageControl = new MainPageControl())
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "Test org";
				Factory.Save();
				configForTest.BindingSource.DataSource = org;

				var mainPageModel = new MainPageModel();
				mainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "orgName");
				mainPageModel.EventsBannerModel = new EventsBannerModel(configForTest, null, null);
				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>(), configForTest, null, mainPageModel);
				mainPageControl.BindingSource.SetDataBinding(mainPageModel, string.Empty);
				mainPageModel.EventsBannerModel.ErrorMessage = null;

				form.Controls.Add(mainPageControl);
				form.Show();
				mainPageModel.EventsBannerModel.SetEvents(new List<CreditEvent> { new CreditEvent() });

				var eventsBannerItemsControls = mainPageControl.Controls.Find("EventsBannerItemControl", true);
				AssertEquals(1, eventsBannerItemsControls.Length);
				var eventsBannerItemControl = eventsBannerItemsControls.FirstOrDefault();
				AssertEquals(false, eventsBannerItemControl.IsDisposed);
				var kTableLayoutPanelControls = mainPageControl.Controls.Find("EventsTableLayout", true);
				AssertEquals(1, kTableLayoutPanelControls.Length);
				var kTableLayoutPanel = kTableLayoutPanelControls.FirstOrDefault();
				kTableLayoutPanel.Controls.RemoveAt(0);
				AssertEquals(true, eventsBannerItemControl.IsDisposed);

				mainPageModel.EventsBannerModel.SetEvents(new List<CreditEvent> { new CreditEvent() });
				AssertEquals(false, kTableLayoutPanel.IsDisposed);
				mainPageControl.Dispose();
				AssertEquals(0, kTableLayoutPanel.Controls.Count);
				AssertEquals(true, kTableLayoutPanel.IsDisposed);
			}
		}

		public void TestEventsBannerShouldShowScrollBar_WhenManyElements()
		{
			using (var form = new ZForm())
			using (var configForTest = new SupportCreditCheckForTest())
			using (var mainPageControl = new MainPageControl())
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_FullName = "Test org";
				Factory.Save();
				configForTest.BindingSource.DataSource = org;

				var mainPageModel = new MainPageModel();
				mainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "orgName");
				mainPageModel.EventsBannerModel = new EventsBannerModel(configForTest, null, null);
				mainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>(), configForTest, null, mainPageModel);
				mainPageControl.BindingSource.SetDataBinding(mainPageModel, string.Empty);
				var eventItemModels = new List<CreditEvent>();
				for (var i = 0; i < 50; i++)
				{
					eventItemModels.Add(new CreditEvent());
				}

				form.Controls.Add(mainPageControl);
				form.Show();
				mainPageModel.EventsBannerModel.SetEvents(eventItemModels);

				var eventsBannerControls = mainPageControl.Controls.Find("eventsBannerControl", true);
				AssertEquals(1, eventsBannerControls.Length);
				var kTableLayoutPanelControls = mainPageControl.Controls.Find("eventsTableLayout", true);
				AssertEquals(1, kTableLayoutPanelControls.Length);
				var eventListPanelControls = mainPageControl.Controls.Find("eventListPanel", true);
				AssertEquals(1, kTableLayoutPanelControls.Length);
				var eventListPanel = eventListPanelControls[0] as ZPanel;
				Assert(eventListPanel.VerticalScroll.Visible);
			}
		}

		public void TestEventsBanner_WhenHasCreditEvents()
		{
			var creditReportItem = new CreditReportItem
			{
				CountryCode = "AU",
				CountryEnabledForCompany = true,
				CountryEnabledForOrganisation = true,
				CommercialBureauEnquiryEnabled = false,
				FailureRiskEnabled = false,
				ComprehensiveReportEnabled = true,
				LatePaymentRiskEnabled = false
			};

			var creditReportItemCollection = new CreditReportItemCollection();
			creditReportItemCollection.Add(creditReportItem);
			Factory.Save();

			using (var form = new ZForm())
			using (var configForTest = new SupportCreditCheckForTest())
			using (var mainPageControl = new MainPageControl())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
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
				DummyMainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				DummyMainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>() { new ReportItemModel(CreditReportType.ComprehensiveReport) }, configForTest, client, DummyMainPageModel);
				DummyMainPageModel.EventsBannerModel = new EventsBannerModel(configForTest, client, DummyMainPageModel);

				mainPageControl.BindingSource.SetDataBinding(DummyMainPageModel, string.Empty);
				form.Controls.Add(mainPageControl);
				form.Show();

				var eventsTableLayout = mainPageControl.Controls.Find("eventsTableLayout", true).Single();

				CombineAssertions(() =>
				{
					AssertEquals(DateTime.Today.AddMonths(-3).ToString("dd MMMM yyyy", CultureInfo.CurrentCulture), eventsTableLayout.Controls[0].Controls.Find("eventDateLabel", true).Cast<ZLabel>().Single().Text);
					AssertEquals("Collection Change", eventsTableLayout.Controls[0].Controls.Find("eventTypeLabel", true).Cast<ZLabel>().Single().Text);
					AssertEquals(DateTime.Today.AddMonths(-5).ToString("dd MMMM yyyy", CultureInfo.CurrentCulture), eventsTableLayout.Controls[2].Controls.Find("eventDateLabel", true).Cast<ZLabel>().Single().Text);
					AssertEquals("Status Change", eventsTableLayout.Controls[2].Controls.Find("eventTypeLabel", true).Cast<ZLabel>().Single().Text);
				});
			}
		}

		public void TestEventBanner_WhenIsLoading()
		{
			var creditReportItem = new CreditReportItem
			{
				CountryCode = "AU",
				CountryEnabledForCompany = true,
				CountryEnabledForOrganisation = true,
				CommercialBureauEnquiryEnabled = false,
				FailureRiskEnabled = false,
				ComprehensiveReportEnabled = true,
				LatePaymentRiskEnabled = false
			};

			var creditReportItemCollection = new CreditReportItemCollection();
			creditReportItemCollection.Add(creditReportItem);
			Factory.Save();

			using (var form = new ZForm())
			using (var configForTest = new SupportCreditCheckForTest())
			using (var mainPageControl = new MainPageControl())
			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				configForTest.BindingSource.DataSource = org;

				var client = new CreditCheckServiceForTest();
				DummyMainPageModel.TopBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				DummyMainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>(), configForTest, null, DummyMainPageModel);
				DummyMainPageModel.EventsBannerModel = new EventsBannerModel(configForTest, null, DummyMainPageModel);

				mainPageControl.BindingSource.SetDataBinding(DummyMainPageModel, string.Empty);
				form.Controls.Add(mainPageControl);
				form.Show();

				DummyMainPageModel.EventsBannerModel.IsLoading = true;
				var loadingPictureBox = mainPageControl.Controls.Find("loadingPictureBox", true)[0] as ZPictureBox;
				Assert(loadingPictureBox.Visible);

				DummyMainPageModel.EventsBannerModel.IsLoading = false;
				Assert(!loadingPictureBox.Visible);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			DummyMainPageModel = new MainPageModel();
		}

		MainPageModel DummyMainPageModel;
	}
}
