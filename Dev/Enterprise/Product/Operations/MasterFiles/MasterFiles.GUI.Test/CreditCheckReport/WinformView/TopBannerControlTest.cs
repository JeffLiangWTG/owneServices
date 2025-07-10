using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Tests;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using WTG.ROPE.Model;
using static Enterprise.MasterFiles.GUI.Test.ImageBitmapTestHelper;

namespace Enterprise.MasterFiles.GUI.Test
{
	public class TopBannerControlTest : TestCaseWithFactory
	{
		public void TestResetEventHandler_WhenSetDataBinding()
		{
			using (var control = new TopBannerControl())
			{
				var dummyMainPageModel = new MainPageModel();
				var topBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
				dummyMainPageModel.TopBannerModel = topBannerModel;
				control.SetDataBinding(dummyMainPageModel, "TopBannerModel");

				AssertEquals(1, topBannerModel.GetEventHandlerCount());
				control.SetDataBinding(null,string.Empty);
				AssertEquals(0, topBannerModel.GetEventHandlerCount());
			}
		}

		public void TestResetEventHandler_WhenControlDispose()
		{
			var control = new TopBannerControl();
			var dummyMainPageModel = new MainPageModel();
			var topBannerModel = new TopBannerModel(CreditReportStatusType.NoEvent, 0, "WiseTech Global Limited");
			dummyMainPageModel.TopBannerModel = topBannerModel;
			control.SetDataBinding(dummyMainPageModel, "TopBannerModel");

			AssertEquals(1, topBannerModel.GetEventHandlerCount());
			control.Dispose();
			AssertEquals(0, topBannerModel.GetEventHandlerCount());
		}

		public void TestStatusChanged_WhenCreditEventHappened()
		{
			using (var form = new ZForm())
			using (var mainPageControl = new MainPageControl())
			using (var configForTest = new SupportCreditCheckForTest())
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
				dummyMainPageModel.ReportsModel = new ReportsModel(new List<ReportItemModel>() { new ReportItemModel(CreditReportType.ComprehensiveReport) }, configForTest, client, dummyMainPageModel);
				dummyMainPageModel.EventsBannerModel = new EventsBannerModel(configForTest, client, dummyMainPageModel);

				mainPageControl.BindingSource.SetDataBinding(dummyMainPageModel, string.Empty);
				form.Controls.Add(mainPageControl);
				form.Show();

				var statusImage = mainPageControl.Controls.Find("statusImage", true)[0] as ZPictureBox;
				var statusDetailInfo = mainPageControl.Controls.Find("statusDetailInfo", true)[0] as ZLabel;
				var statusDetail = mainPageControl.Controls.Find("statusDetail", true)[0] as ZLabel;

				CombineAssertions(() =>
				{
					AssertImageBitsEquals(Properties.Resources.WarningRed, statusImage.Image);
					Assert(statusImage.Visible);
					AssertEquals("2 events.", statusDetailInfo.Text);
					Assert(statusDetailInfo.Visible);
					AssertEquals("Since your last report WiseTech Global Limited has new events recorded.", statusDetail.Text);
				});
			}
		}

		CreditReportItemCollection GetCreditReportItemCollectionForTest()
		{
			var creditReportItem = new CreditReportItem
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
	}
}
