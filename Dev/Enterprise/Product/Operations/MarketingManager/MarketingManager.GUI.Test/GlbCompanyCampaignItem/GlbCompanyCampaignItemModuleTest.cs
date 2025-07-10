using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GlbCompanyCampaignItemModule))]
	public class GlbCompanyCampaignItemModuleTest : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportWorkflow()
		{
			using (GlbCompanyCampaignItemModule module = new GlbCompanyCampaignItemModule())
			{
				AssertEquals(ModuleIDs.GlbCompanyCampaignItem, module.ID);
				AssertEquals(false, module.SupportsWorkflow);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbCompanyCampaignItem;
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		public void TestShowRecent()
		{
			using (var module = new GlbCompanyCampaignItemModuleForTest())
			{
				Assert("Should not show recent", !module.ShowRecentExposed);
			}
		}

		public void TestCampaignsItemsSentResultOnPerformSearch()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "contact1";
			contact.OC_Email = "andy@search.com";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "contact2";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "contact3";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;

			Factory.Save();

			using (GlbCompanyCampaignItemModule module = new GlbCompanyCampaignItemModule())
			{
				GlbCompanyCampaignItemCampaignDependentCollection collection = new GlbCompanyCampaignItemCampaignDependentCollection(campaign);
				module.Campaign = campaign;
				GlbCompanyCampaignItemFilterBusinessObject filterBusinessObject = module.FilterBusinessObject as GlbCompanyCampaignItemFilterBusinessObject;

				AssertEquals("There should be 3 items in campaign.CampaignsItemsSent", 3, campaign.CampaignsItemsSent.Count);
				AssertEquals(0, ((GlbCompanyCampaignItemFilterControl)module.EmbeddedControl).GridCollection.Count);

				var result = (ModuleTextFilter)filterBusinessObject["Email Address"];
				result.Property = "andy@search.com";
				result.SqlComparisonOperator = SQLComparisonOperator.Equal;
				result.IsActive = true;

				((GlbCompanyCampaignItemFilterControl)module.EmbeddedControl).Find();
				AssertEquals("There should still be 3 items in campaign.CampaignsItemsSent", 3, campaign.CampaignsItemsSent.Count);
				AssertEquals(1, ((GlbCompanyCampaignItemFilterControl)module.EmbeddedControl).GridCollection.Count);
			}
		}

		public void TestUpdatedResultSetOnFind()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "contact1";
			contact.OC_Email = "edward@search.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			campaignItem.G8_TrackingStatus = "UNV";

			Factory.Save();

			using (GlbCompanyCampaignItemModule module = new GlbCompanyCampaignItemModule())
			{
				GlbCompanyCampaignItemCampaignDependentCollection collection = new GlbCompanyCampaignItemCampaignDependentCollection(campaign);
				module.Campaign = campaign;
				GlbCompanyCampaignItemFilterBusinessObject filterBusinessObject = module.FilterBusinessObject as GlbCompanyCampaignItemFilterBusinessObject;

				AssertEquals("There should be 1 items in campaign.CampaignsItemsSent", 1, campaign.CampaignsItemsSent.Count);
				AssertEquals(0, ((GlbCompanyCampaignItemFilterControl)module.EmbeddedControl).GridCollection.Count);

				var result = (ModuleTextFilter)filterBusinessObject["Email Address"];
				result.Property = "edward@search.com";
				result.SqlComparisonOperator = SQLComparisonOperator.Equal;
				result.IsActive = true;

				((GlbCompanyCampaignItemFilterControl)module.EmbeddedControl).Find();
				AssertEquals("There should still be 1 items in campaign.CampaignsItemsSent", 1, campaign.CampaignsItemsSent.Count);
				AssertEquals(1, ((GlbCompanyCampaignItemFilterControl)module.EmbeddedControl).GridCollection.Count);

				var trackingStatus = ((GlbCompanyCampaignItemCampaignDependentCollection)((GlbCompanyCampaignItemFilterControl)module.EmbeddedControl).GridCollection)[0].G8_TrackingStatus;
				AssertEquals("UNV", trackingStatus);

				var externalFactory = new BusinessObjectFactory();
				using (GetFactoryIsolater(externalFactory))
				{
					var dbCampaignItem = externalFactory.Load<GlbCompanyCampaignItem>(campaignItem.PK);
					dbCampaignItem.G8_TrackingStatus = "VER";
					externalFactory.Save();
				}

				((GlbCompanyCampaignItemFilterControl)module.EmbeddedControl).Find();
				AssertEquals("There should still be 1 items in campaign.CampaignsItemsSent", 1, campaign.CampaignsItemsSent.Count);
				AssertEquals(1, ((GlbCompanyCampaignItemFilterControl)module.EmbeddedControl).GridCollection.Count);

				trackingStatus = ((GlbCompanyCampaignItemCampaignDependentCollection)((GlbCompanyCampaignItemFilterControl)module.EmbeddedControl).GridCollection)[0].G8_TrackingStatus;
				AssertEquals("VER", trackingStatus);
			}
		}

		public void TestAllowDelete()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;

			using (var module = new GlbCompanyCampaignItemModule())
			{
				Assert("Deletion should not be allowed if no campaign is set", !module.AllowDelete);

				module.Campaign = campaign;
				Assert("Deletion should be allowed for target list campaign", module.AllowDelete);

				foreach (var code in new CampaignTypeList().GetAllCodes().Where(c => c != CampaignTypeList.Codes.TargetList))
				{
					campaign.G0_BroadcastVoteSurveyExam = code;
					Assert("Deletion should not be allowed if campaign is not target list", !module.AllowDelete);
				}
			}
		}

		public void TestDelete()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();

			using (var module = new GlbCompanyCampaignItemModuleForTest())
			using (var form = new ZForm())
			{
				module.Campaign = campaign;
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch_ForTest();
				Assert("Precondition: Deletion should be allowed for target list campaign", module.AllowDelete);
				AssertEquals("Precondition: Grid should have 3 records", 3, module.DisplayGrid.VisibleRowCount);

				module.DeleteMenuItem.PerformClick();
				AssertEquals("Warning should be shown when there are no selected items", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Campaign Item 1 should not be deleted after warning", !campaignItem1.IsDeleted);
				Assert("Campaign Item 2 should not be deleted after warning", !campaignItem2.IsDeleted);
				Assert("Campaign Item 3 should not be deleted after warning", !campaignItem3.IsDeleted);

				module.DisplayGrid.Select(0);
				module.DisplayGrid.Select(2);
				AssertArrayEqualsByElements("Precondition: Grid selected elements are incorrect", new[] { campaignItem1, campaignItem3 }, module.GetSelectedBusinessObjects());

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				module.DeleteMenuItem.PerformClick();
				AssertEquals("Confirmation should be shown when there are selected items", "You are about to remove the selected contact(s) from the Target List. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Campaign Item 1 should not be deleted after selecting No", !campaignItem1.IsDeleted);
				Assert("Campaign Item 2 should not be deleted after selecting No", !campaignItem2.IsDeleted);
				Assert("Campaign Item 3 should nor be deleted after selecting No", !campaignItem3.IsDeleted);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				module.DeleteMenuItem.PerformClick();
				AssertEquals("Confirmation should be shown when there are selected items", "You are about to remove the selected contact(s) from the Target List. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Campaign Item 1 should be deleted after selecting Yes", campaignItem1.IsDeleted);
				Assert("Campaign Item 2 should not be deleted after selecting Yes", !campaignItem2.IsDeleted);
				Assert("Campaign Item 3 should be deleted after selecting Yes", campaignItem3.IsDeleted);
			}
		}

		public void TestDelete_NotAllowed()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;

			using (var module = new GlbCompanyCampaignItemModuleForTest())
			using (var form = new ZForm())
			{
				module.Campaign = campaign;
				Assert("Precondition: Deletion should be allowed for target list campaign. Need to allow initially so that DeleteMenuItem is set when showing form.", module.AllowDelete);
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
				var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
				var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
				var campaignItem3 = campaign.CampaignsItemsSent.AddNew();

				module.PerformSearch_ForTest();
				module.DisplayGrid.Select(1);
				Assert("Precondition: Deletion should not be allowed for broadcast campaign", !module.AllowDelete);
				AssertEquals("Precondition: Grid should have 3 records", 3, module.DisplayGrid.VisibleRowCount);
				AssertArrayEqualsByElements("Precondition: Grid selected elements are incorrect", new[] { campaignItem2 }, module.GetSelectedBusinessObjects());

				module.DeleteMenuItem.PerformClick();
				AssertNull("Message should not be shown when deletion is not allowed", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Campaign Item 1 should not be deleted", !campaignItem1.IsDeleted);
				Assert("Campaign Item 2 should not be deleted", !campaignItem2.IsDeleted);
				Assert("Campaign Item 3 should not be deleted", !campaignItem3.IsDeleted);
			}
		}
	}
}
