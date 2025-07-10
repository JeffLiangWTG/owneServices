using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Tests;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Test
{
	class MainPageControlTest : TestCaseWithFactory
	{
		public void TestMainPageControl()
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
				var topBannerControls = form.Controls.Find("TopBannerControl", true);
				AssertEquals(1, topBannerControls.Length);
				var reportsControls = form.Controls.Find("ReportsControl", true);
				AssertEquals(1, reportsControls.Length);
				var eventsBannerControls = form.Controls.Find("EventsBannerControl", true);
				AssertEquals(1, eventsBannerControls.Length);
			}
		}
	}
}
