using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using OxyPlot;
using OxyPlot.Series;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class TrackingStatusChartViewModelTest : TestCaseWithFactory
	{
		public void TestUnHookEventsDoNotThrow()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			TrackingStatusChartViewModel statModel = new TrackingStatusChartViewModel(campaign);
			statModel.PopulatePieModel();

			AssertNoExceptionThrown(delegate
			{
				statModel.Dispose();
				statModel.Dispose();
			});
		}

		public void TestUnHookEventsDoNotThrowBeforePopulate()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			TrackingStatusChartViewModel statModel = new TrackingStatusChartViewModel(campaign);

			AssertNoExceptionThrown(delegate
			{
				statModel.Dispose();
				statModel.Dispose();
			});
		}

		public void TestLoadData()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ABC";
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_ContactName = "John Edwards";
			contact1.OC_Email = "john@abc.net";
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_ContactName = "Eddy Tan";
			contact2.OC_Email = "eddy@abc.net";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "XYZ";
			var contact3 = org2.Contacts.AddNew();
			contact3.OC_ContactName = "Sylvia Anne";
			contact3.OC_Email = "sylvia@abc.net";

			var contact4 = org2.Contacts.AddNew();
			contact4.OC_ContactName = "Richie";
			contact4.OC_Email = "richie@hotmail.com";

			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			TrackingStatusChartViewModel statModel = new TrackingStatusChartViewModel(campaign);
			statModel.PopulatePieModel();
			var list = statModel.ChartData;

			AssertEquals(0, list.Count);
			AssertEquals(0, statModel.EmailsTotal);
			AssertEquals(0, statModel.ClientsTotal);

			var unsubscribedContact = org2.Contacts.AddNew();
			unsubscribedContact.OC_Email = "unsubscribedContact@mail.com";
			var unsubscr = Factory.New<GlbCompanyCampaignSubscription>();
			unsubscr.GCS_G0 = campaign.PK;
			unsubscr.GCS_Email = unsubscribedContact.OC_Email;
			unsubscr.GCS_IsSubscribed = false;

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_TrackingStatus = "UNV";
			campaignItem1.G8_RecipientID = unsubscribedContact.PK;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_TrackingStatus = "VER";
			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;
			campaignItem3.G8_TrackingStatus = "NDR";

			Factory.Save();

			statModel.PopulatePieModel();
			list = statModel.ChartData;

			AssertEquals(3, list.Count);
			AssertEquals(3, statModel.EmailsTotal);
			AssertEquals(2, statModel.ClientsTotal);

			var campaignItem4 = campaign.CampaignsItemsSent.AddNew();
			campaignItem4.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem4.G8_RecipientID = contact4.PK;
			campaignItem4.G8_TrackingStatus = "QUE";
			Factory.Save();

			statModel.PopulatePieModel();
			list = statModel.ChartData;

			AssertEquals(4, list.Count);
			AssertEquals("Emails total should remail the same as 'QUE' is not included", 3, statModel.EmailsTotal);
			AssertEquals(2, statModel.ClientsTotal);

			var listUnsubscribed = statModel.UnsubscribedData;
			AssertNotNull("Unsubscribe Data Should Present", listUnsubscribed);
			AssertEquals(1, listUnsubscribed.Count);
			AssertEquals(1.0, listUnsubscribed[0].EmailsCount);
		}

		public void TestUnsubscribeRowVisibility()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "John Edwards";
			contact1.OC_Email = "john@abc.net";
			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "Eddy Tan";
			contact2.OC_Email = "eddy@abc.net";

			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var statModel = new TrackingStatusChartViewModel(campaign);

			statModel.PopulatePieModel();
			AssertEquals(0, statModel.ChartData.Count);
			AssertEquals(1, statModel.UnsubscribedData.Count);
			AssertEquals(0, statModel.EmailsTotal);
			AssertEquals(0, statModel.ClientsTotal);
			AssertEquals(1, statModel.PieModel.Series.Count);
			AssertEquals(false, statModel.UnsubscribedRowVisible);

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_TrackingStatus = "VER";
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_TrackingStatus = "VER";
			Factory.Save();

			statModel.PopulatePieModel();
			AssertEquals(1, statModel.ChartData.Count);
			AssertEquals(1, statModel.UnsubscribedData.Count);
			AssertEquals(2, statModel.EmailsTotal);
			AssertEquals(1, statModel.ClientsTotal);
			AssertEquals(1, statModel.PieModel.Series.Count);
			AssertEquals(false, statModel.UnsubscribedRowVisible);

			var unsubscr = Factory.New<GlbCompanyCampaignSubscription>();
			unsubscr.GCS_G0 = campaign.PK;
			unsubscr.GCS_Email = contact1.OC_Email;
			unsubscr.GCS_IsSubscribed = false;
			Factory.Save();

			statModel.PopulatePieModel();
			AssertEquals(1, statModel.ChartData.Count);
			AssertEquals(1, statModel.UnsubscribedData.Count);
			AssertEquals(2, statModel.EmailsTotal);
			AssertEquals(1, statModel.ClientsTotal);
			AssertEquals(2, statModel.PieModel.Series.Count);
			AssertEquals(true, statModel.UnsubscribedRowVisible);
		}

		public void TestPieSeriesDefaultValues()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ABC";

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "John Edwards";
			contact1.OC_Email = "john@abc.net";

			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "Eddy Tan";
			contact2.OC_Email = "eddy@abc.net";
			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var statModel = new TrackingStatusChartViewModel(campaign);

			statModel.PopulatePieModel();
			var pieSeries = (PieSeries)statModel.PieModel.Series.First();

			AssertEquals(0.8, pieSeries.Diameter);
			AssertEquals(0.5, pieSeries.InnerDiameter);
			AssertEquals(0d, pieSeries.ExplodedDistance);
			AssertEquals(OxyColors.White, pieSeries.Stroke);
			AssertEquals(360d, pieSeries.AngleSpan);
			AssertEquals(270d, pieSeries.StartAngle);
			AssertEquals(0d, pieSeries.TickRadialLength);
			AssertEquals(0d, pieSeries.TickHorizontalLength);
			AssertEquals(7d, pieSeries.TickDistance);
			AssertEquals(1000d, pieSeries.TickLabelDistance);
		}
	}
}
