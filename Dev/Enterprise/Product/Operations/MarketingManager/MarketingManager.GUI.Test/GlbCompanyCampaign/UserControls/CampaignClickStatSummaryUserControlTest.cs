using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class CampaignClickStatSummaryUserControlTest : TestCaseWithFactory
	{
		#region TrackingStatButton

		public void TestTrackingStatButton()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			var clickStatModel = new ClickStatModel(campaign);

			using (var form = new ZForm(clickStatModel))
			using (var control = new CampaignClickStatSummaryUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.TrackingStatButton.PerformClick();
				using (var shownForm = ZFormModaliser.LastFormShownForTest)
				{
					AssertType(typeof(TrackingStatForm), shownForm);
					AssertEquals(form, shownForm.Owner);
				}
			}
		}

		public void TestTrackingStatButton_WithoutDataSource()
		{
			using (var form = new ZForm())
			using (var control = new CampaignClickStatSummaryUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.TrackingStatButton.PerformClick();
				using (var shownForm = ZFormModaliser.LastFormShownForTest)
				{
					AssertNull(shownForm);
				}
			}
		}

		#endregion

		[TestDate(2015, 5, 21, 12, 12, 4)]
		public void TestLinksGrid_DoubleClick()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "AAA";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "BBB";

			var item1 = campaign.CampaignsItemsSent.AddNew();
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = contact1.PK;
			var item2 = campaign.CampaignsItemsSent.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;

			var link1 = campaign.TrackedLinks.AddNew();
			link1.GCL_Context = "AAA";
			link1.GCL_URL = "http://webtest.net";
			var link2 = campaign.TrackedLinks.AddNew();
			link2.GCL_Context = "BBB";
			link2.GCL_URL = "http://webtest.com";
			var link3 = campaign.TrackedLinks.AddNew();
			link3.GCL_Context = "BBB [Image Link]";
			link3.GCL_URL = "http://webtest.com";

			var clickTime = ZDateTime.UtcNow;

			var click11 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click11.GCC_GCL = link1.PK;
			click11.GCC_ClickTimeUtc = clickTime;
			click11.GCC_G8_Recipient = item1.PK;

			var click12 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click12.GCC_GCL = link2.PK;
			click12.GCC_ClickTimeUtc = clickTime.AddHours(4);
			click12.GCC_G8_Recipient = item1.PK;

			var click13 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click13.GCC_GCL = link2.PK;
			click13.GCC_ClickTimeUtc = clickTime;
			click13.GCC_G8_Recipient = item2.PK;

			var click14 = Factory.NewWithValidTestData<GlbCompanyCampaignClick>();
			click14.GCC_GCL = link3.PK;
			click14.GCC_ClickTimeUtc = clickTime;
			click14.GCC_G8_Recipient = item1.PK;

			Factory.Save();

			ClickStatModel statModel = new ClickStatModel(campaign);
			statModel.ReportBy = ReportByList.Codes.Context;
			statModel.LoadClicks();

			AssertEquals(3, statModel.LinkClicks.Count);

			using (var form = new GlbCompanyCampaignForm(campaign))
			{
				var clickStatSummaryUserControl = new CampaignClickStatSummaryUserControlForTest();
				clickStatSummaryUserControl.SetDataBinding(statModel, "");
				form.Show();
				form.Controls.Add(clickStatSummaryUserControl);

				clickStatSummaryUserControl.LinksGridExposed.ListManager.List.Cast<ClickStatData>().OrderBy(s => s.Context);
				clickStatSummaryUserControl.LinksGridExposed.ListManager.Position = 2;
				clickStatSummaryUserControl.LinksGridExposed.PerformDoubleClickForTest();

				AssertEquals(1, form.CampaignTrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item1, form.CampaignTrackingControl.FilterStripControl.GridCollection);
			}

			statModel.ReportBy = ReportByList.Codes.Url;

			AssertEquals(2, statModel.LinkClicks.Count);

			using (var form = new GlbCompanyCampaignForm(campaign))
			{
				var clickStatSummaryUserControl = new CampaignClickStatSummaryUserControlForTest();
				clickStatSummaryUserControl.SetDataBinding(statModel, "");
				form.Show();
				form.Controls.Add(clickStatSummaryUserControl);

				clickStatSummaryUserControl.LinksGridExposed.ListManager.List.Cast<ClickStatData>().OrderBy(s => s.URL);
				clickStatSummaryUserControl.LinksGridExposed.ListManager.Position = 0;
				clickStatSummaryUserControl.LinksGridExposed.PerformDoubleClickForTest();

				AssertEquals(2, form.CampaignTrackingControl.FilterStripControl.GridCollection.Count);
			}
		}
	}
}
