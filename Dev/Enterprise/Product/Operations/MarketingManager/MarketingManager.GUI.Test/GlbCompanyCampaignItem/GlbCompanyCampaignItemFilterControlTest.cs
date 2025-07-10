using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class GlbCompanyCampaignItemFilterControlTest : TestCaseWithFactory
	{
		public void TestTouchColumns()
		{
			var helper = new GlbCompanyCampaignTestHelper(Factory);
			helper.SetupDripCampaign();

			using (var form = new CampaignTrackingControlTest.DummyCampaignForm(helper.Master))
			{
				form.Show();

				foreach (var horizontal in helper.Master.Horizontals)
				{
					ZGridColumn foundColumn = null;
					foreach (var column in form.TrackingControl.FilterStripControl.Grid.Columns)
					{
						if (column.ColumnName == horizontal.Name)
						{
							foundColumn = column;
							break;
						}
					}

					AssertNotNull("Should contain touch column: " + horizontal.Name, foundColumn);
					AssertEquals(horizontal.Name, foundColumn.ColumnName);
					AssertEquals(true, foundColumn.IsVisible);
					AssertEquals(true, foundColumn.ColumnStyle.ReadOnly);
				}
			}
		}

		public void TestGetValueOpportunityCreatedTouchItem()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "thomas@test.com";
			contact.OC_ContactName = "Thomas";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "gordon@test.com";
			contact2.OC_ContactName = "Gordon";

			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			var touch = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch.G0_CampaignName = "Touch 1A";
			touch.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			touch.G0_HorizontalId = 1;
			touch.G0_VerticalId = "A";
			touch.G0_G0_Master = master.PK;
			var item = touch.CampaignsItemsSent.AddNew();
			item.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item.G8_RecipientID = contact.PK;
			item.G8_TrackingStatus = TrackingStatusCodes.Codes.OPQ;

			var touch2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch2.G0_CampaignName = "Touch 1A";
			touch2.G0_EstimatedStartedDate = ZDateTime.Today.AddDays(1);
			touch2.G0_HorizontalId = 2;
			touch2.G0_VerticalId = "B";
			touch2.G0_G0_Master = master.PK;
			var item2 = touch2.CampaignsItemsSent.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;
			item2.G8_TrackingStatus = TrackingStatusCodes.Codes.OPC;

			GlbCompanyCampaignTestHelper.PopulateCampaign(master, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch, Factory);
			Factory.Save();

			var horizontalList = master.Horizontals.ToList();
			var propertyDescriptor = new GlbCompanyCampaignItemFilterControl.TouchIdPropertyDescriptor(horizontalList[0].Id, master);
			AssertEquals(propertyDescriptor.GetValue(item), "OPP(OPQ) - A");

			propertyDescriptor = new GlbCompanyCampaignItemFilterControl.TouchIdPropertyDescriptor(horizontalList[1].Id, master);
			AssertEquals(propertyDescriptor.GetValue(item2), "OPP(OPC) - B");
		}

		#region Find Campaign Item On Tracking Tab

		public void TestFindCampaignItemOnTrackingTab()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Blob";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Plonk";
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var item1 = campaign.CampaignsItemsSent.AddNew();
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = contact1.PK;
			var item2 = campaign.CampaignsItemsSent.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;

			Factory.Save();

			using (var form = new CampaignTrackingControlTest.DummyCampaignForm(campaign))
			{
				form.Show();
				AssertEquals("Precondition", 0, form.TrackingControl.FilterStripControl.GridCollection.Count);
				form.TrackingControl.FindCampaignItemOnTrackingTab(item1);
				AssertEquals(1, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item1, form.TrackingControl.FilterStripControl.GridCollection);
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals(2, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item1, form.TrackingControl.FilterStripControl.GridCollection);
				AssertCollectionContains(item2, form.TrackingControl.FilterStripControl.GridCollection);
			}
		}

		public void TestFindCampaignItemOnTrackingTab_WithDeliveryStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Blob";
			contact1.OC_Email = "blob@gmail.com";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Plonk";
			contact2.OC_Email = "plonk@gmail.com";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Berby";
			contact3.OC_Email = "berby@gmail.com";
			var contact4 = org.Contacts.AddNew();
			contact4.OC_ContactName = "Rosgard";
			contact4.OC_Email = "rosgard@gmail.com";
			var contact5 = org.Contacts.AddNew();
			contact5.OC_ContactName = "Keria";
			contact5.OC_Email = "keria@gmail.com";
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var item1 = campaign.CampaignsItemsSent.AddNew();
			item1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item1.G8_RecipientID = contact1.PK;
			item1.G8_TrackingStatus = "VER";
			var item2 = campaign.CampaignsItemsSent.AddNew();
			item2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item2.G8_RecipientID = contact2.PK;
			item2.G8_TrackingStatus = "UNV";
			var item3 = campaign.CampaignsItemsSent.AddNew();
			item3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item3.G8_RecipientID = contact3.PK;
			item3.G8_TrackingStatus = "VER";
			var item4 = campaign.CampaignsItemsSent.AddNew();
			item4.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item4.G8_RecipientID = contact4.PK;
			item4.G8_TrackingStatus = "QUE";
			item4.G8_ScheduleTimeUtc = ZDateTime.Now;
			var item5 = campaign.CampaignsItemsSent.AddNew();
			item5.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			item5.G8_RecipientID = contact5.PK;
			item5.G8_TrackingStatus = "QUE";

			Factory.Save();

			using (var form = new CampaignTrackingControlTest.DummyCampaignForm(campaign))
			{
				form.Show();
				AssertEquals("Precondition", 0, form.TrackingControl.FilterStripControl.GridCollection.Count);
				form.TrackingControl.FindCampaignItemWithDeliveryStatus(TrackingStatusCodes.Codes.VER);
				AssertEquals(2, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item1, form.TrackingControl.FilterStripControl.GridCollection);
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals(2, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item1, form.TrackingControl.FilterStripControl.GridCollection);
				AssertCollectionContains(item3, form.TrackingControl.FilterStripControl.GridCollection);

				form.TrackingControl.FindCampaignItemWithDeliveryStatus(TrackingStatusCodes.Codes.UNV);
				AssertEquals(1, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item2, form.TrackingControl.FilterStripControl.GridCollection);
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals(1, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item2, form.TrackingControl.FilterStripControl.GridCollection);

				form.TrackingControl.FindCampaignItemWithDeliveryStatus(TrackingStatusCodes.Codes.SCH);
				AssertEquals(1, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item4, form.TrackingControl.FilterStripControl.GridCollection);
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals(1, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item4, form.TrackingControl.FilterStripControl.GridCollection);

				form.TrackingControl.FindCampaignItemWithDeliveryStatus(TrackingStatusCodes.Codes.UNS);
				AssertEquals(1, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item5, form.TrackingControl.FilterStripControl.GridCollection);
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals(1, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(item5, form.TrackingControl.FilterStripControl.GridCollection);
			}
		}

		public void TestFindCampaignItemOnTrackingTab_WithUnsubscribeStatus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.OC_Email = "contact1@email.em";
			org.Contacts.Add(contact1);

			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_Email = "contact2@email.em";
			org.Contacts.Add(contact2);

			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_Email = "contact3@email.em";
			org.Contacts.Add(contact3);

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;

			var subscription = Factory.New<GlbCompanyCampaignSubscription>();
			subscription.GCS_Email = contact1.OC_Email;
			subscription.GCS_IsSubscribed = false;
			subscription.GCS_G0 = campaign.PK;

			Factory.Save();

			using (var form = new CampaignTrackingControlTest.DummyCampaignForm(campaign))
			{
				form.Show();
				AssertEquals("Precondition", 0, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertEquals(0, form.TrackingControl.FilterStripControl.GridCollection.Count);

				form.TrackingControl.FindCampaignItemWithUnsubscribeStatus(true);
				AssertEquals(1, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(campaignItem1, form.TrackingControl.FilterStripControl.GridCollection);
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals(1, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(campaignItem1, form.TrackingControl.FilterStripControl.GridCollection);

				form.TrackingControl.FindCampaignItemWithUnsubscribeStatus(false);
				AssertEquals(2, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(campaignItem2, form.TrackingControl.FilterStripControl.GridCollection);
				AssertCollectionContains(campaignItem3, form.TrackingControl.FilterStripControl.GridCollection);
				form.TrackingControl.FilterStripControl.FirePerformSearch();
				AssertEquals(2, form.TrackingControl.FilterStripControl.GridCollection.Count);
				AssertCollectionContains(campaignItem2, form.TrackingControl.FilterStripControl.GridCollection);
				AssertCollectionContains(campaignItem3, form.TrackingControl.FilterStripControl.GridCollection);
			}
		}

		#endregion
	}
}
