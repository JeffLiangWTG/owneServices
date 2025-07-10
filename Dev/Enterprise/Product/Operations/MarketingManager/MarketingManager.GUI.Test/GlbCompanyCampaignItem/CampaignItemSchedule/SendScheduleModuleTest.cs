using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(SendScheduleModule))]
	public class SendScheduleModuleTest : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (SendScheduleModule module = new SendScheduleModule())
			{
				AssertEquals(ModuleIDs.GlbCompanyCampaignItemSchedule, module.ID);
				AssertEquals(false, module.SupportsWorkflow);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbCompanyCampaignItemSchedule;
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		public void TestShowRecent()
		{
			using (var module = new SendScheduleModuleForTest())
			{
				Assert("Should not show recent", !module.ShowRecentExposed);
			}
		}

		[StressTest]
		public override void TestModuleShowsAndCanSearch()
		{
			base.TestModuleShowsAndCanSearch();
		}

		public void TestEditCampaignSchedule_InactiveRecipient()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact A";
			contact1.OC_Email = "contacta@gmail.com";
			contact1.OC_IsActive = false;
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Contact B";
			contact2.OC_Email = "contactb@gmail.com";
			contact2.OC_IsActive = false;

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			using (FormForTest form = new FormForTest(campaign))
			{
				form.Show();

				campaign.CampaignItemSchedule.SelectedScheduleItems.UnionWith(new[] { campaignItem1, campaignItem2 });
				((GlbCompanyCampaignItemScheduleItemsCollection)form.SendScheduleControl.FilterItemModule.GridCollection).LoadData(campaign.CampaignItemSchedule, ZDateTime.Empty);

				form.SendScheduleControl.ToolStripExposed.Items[1].PerformClick();  // Edit Button
				AssertEquals("Last message should be", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.SendScheduleControl.FilterItemModule.DisplayGrid.SelectAllElements();
				form.SendScheduleControl.ToolStripExposed.Items[1].PerformClick();  // Edit Button
				AssertNull(ZFormModaliser.LastFormShownForTest);
				ZFormModaliser.LastFormShownDialogForTest = null;
			}
		}

		public void TestEditCampaignSchedule()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact A";
			contact1.OC_Email = "contacta@gmail.com";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Contact B";
			contact2.OC_Email = "contactb@gmail.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			using (FormForTest form = new FormForTest(campaign))
			{
				form.Show();

				campaign.CampaignItemSchedule.SelectedScheduleItems.UnionWith(new[] { campaignItem1, campaignItem2 });
				((GlbCompanyCampaignItemScheduleItemsCollection)form.SendScheduleControl.FilterItemModule.GridCollection).LoadData(campaign.CampaignItemSchedule, ZDateTime.Empty);

				form.SendScheduleControl.ToolStripExposed.Items[1].PerformClick();  // Edit Button
				AssertEquals("Last message should be", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.SendScheduleControl.FilterItemModule.DisplayGrid.SelectAllElements();
				form.SendScheduleControl.ToolStripExposed.Items[1].PerformClick();  // Edit Button
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("CampaignItemScheduleForm should popup", typeof(CampaignItemScheduleForm), ZFormModaliser.LastFormShownForTest.GetType());
				ZFormModaliser.LastFormShownDialogForTest = null;
			}
		}

		public void TestViewCampaignSchedule()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact A";
			contact1.OC_Email = "contacta@gmail.com";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Contact B";
			contact2.OC_Email = "contactb@gmail.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow;

			Factory.Save();

			using (FormForTest form = new FormForTest(campaign))
			{
				form.Show();

				campaign.CampaignItemSchedule.SelectedScheduleItems.UnionWith(new[] { campaignItem1, campaignItem2 });
				((GlbCompanyCampaignItemScheduleItemsCollection)form.SendScheduleControl.FilterItemModule.GridCollection).LoadData(campaign.CampaignItemSchedule, ZDateTime.Empty);

				form.SendScheduleControl.ToolStripExposed.Items[0].PerformClick();  // View Button
				AssertEquals("Last message should be", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.SendScheduleControl.FilterItemModule.DisplayGrid.SelectAllElements();
				form.SendScheduleControl.ToolStripExposed.Items[0].PerformClick();  // View Button
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("CampaignItemScheduleForm should popup", typeof(CampaignItemScheduleForm), ZFormModaliser.LastFormShownForTest.GetType());
				Assert(((CampaignItemScheduleForm)ZFormModaliser.LastFormShownForTest).DisplayMode == ODisplayMode.ReadOnly);
				ZFormModaliser.LastFormShownDialogForTest = null;
			}
		}

		[TestDate(2016, 3, 3)]
		public void TestDeleteCampaignSchedule()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XWY";
			staff.GS_IsController = true;
			staff.GS_EmailAddress = "winter@test.com";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			org.OH_RL_NKClosestPort = "AUBNE";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact A";
			contact1.OC_Email = "contacta@gmail.com";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Contact B";
			contact2.OC_Email = "contactb@gmail.com";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Contact C";
			contact3.OC_Email = "contactc@gmail.com";
			var contact4 = org.Contacts.AddNew();
			contact4.OC_ContactName = "Contact D";
			contact4.OC_Email = "contactd@gmail.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Campaign";
			campaign.G0_Category = "PRINT";
			campaign.G0_GS_NKCampaignManager = staff.GS_Code;
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_Type = "PREAP";
			campaign.G0_EstimatedStartedDate = ZDateTime.Now;

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(2);
			var campaignItem3 = campaign.CampaignsItemsSent.AddNew();
			campaignItem3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3.G8_RecipientID = contact3.PK;
			campaignItem3.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(3);

			var campaignItem4 = campaign.CampaignsItemsSent.AddNew();
			campaignItem4.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem4.G8_RecipientID = contact4.PK;
			campaignItem4.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(4);
			campaignItem4.G8_TrackingStatus = TrackingStatusCodes.Codes.UNV;

			Factory.Save();

			using (FormForTest form = new FormForTest(campaign))
			{
				form.Show();

				campaign.CampaignItemSchedule.SelectedScheduleItems.UnionWith(new[] { campaignItem1, campaignItem2, campaignItem3, campaignItem4 });
				((GlbCompanyCampaignItemScheduleItemsCollection)form.SendScheduleControl.FilterItemModule.GridCollection).LoadData(campaign.CampaignItemSchedule, ZDateTime.Empty);
				AssertEquals("There should be 4 element in grid", 4, form.SendScheduleControl.FilterItemModule.DisplayGrid.ListManager.List.Count);

				form.SendScheduleControl.ToolStripExposed.Items[2].PerformClick();  // Delete Button
				AssertEquals("Last message should be", "Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendScheduleControl.FilterItemModule.DisplayGrid.Select(0);
				form.SendScheduleControl.FilterItemModule.DisplayGrid.Select(1);
				form.SendScheduleControl.ToolStripExposed.Items[2].PerformClick();  // Delete Button
				AssertNull("Extra confirmation dialog should not be shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Last message should be", "You are about to delete the 'Scheduled delivery' for the selected contact(s). Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("CampaignItemScheduleForm should not popup", ZFormModaliser.LastFormShownForTest);
				AssertEquals("There should be 2 element in grid", 2, form.SendScheduleControl.FilterItemModule.DisplayGrid.ListManager.List.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendScheduleControl.FilterItemModule.DisplayGrid.Select(0);
				form.SendScheduleControl.ToolStripExposed.Items[2].PerformClick();  // Delete Button
				AssertEquals("Last message should be", "You are about to delete the 'Scheduled delivery' for the selected contact(s). Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("CampaignItemScheduleForm should not popup", ZFormModaliser.LastFormShownForTest);
				AssertEquals("There should be 1 element in grid", 1, form.SendScheduleControl.FilterItemModule.DisplayGrid.ListManager.List.Count);

				form.SendScheduleControl.FilterItemModule.DisplayGrid.Select(0);
				form.SendScheduleControl.ToolStripExposed.Items[2].PerformClick();  // Delete Button
				AssertEquals("Last message should be", "Only Queued Schedules can be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2016, 3, 3)]
		public void TestDeleteCampaignSchedule_DeleteAllItems()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XWY";
			staff.GS_IsController = true;
			staff.GS_EmailAddress = "winter@test.com";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABC";
			org.OH_RL_NKClosestPort = "AUBNE";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact A";
			contact1.OC_Email = "contacta@gmail.com";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Contact B";
			contact2.OC_Email = "contactb@gmail.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_CampaignName = "Test Campaign";
			campaign.G0_Category = "PRINT";
			campaign.G0_GS_NKCampaignManager = staff.GS_Code;
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_Type = "PREAP";
			campaign.G0_EstimatedStartedDate = ZDateTime.Now;

			var campaignItem1 = campaign.CampaignsItemsSent.AddNew();
			campaignItem1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1.G8_RecipientID = contact1.PK;
			campaignItem1.G8_ScheduleTimeUtc = ZDateTime.UtcNow;
			var campaignItem2 = campaign.CampaignsItemsSent.AddNew();
			campaignItem2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2.G8_RecipientID = contact2.PK;
			campaignItem2.G8_ScheduleTimeUtc = ZDateTime.UtcNow.AddDays(2);

			Factory.Save();

			using (var form = new FormForTest(campaign))
			{
				form.Show();

				campaign.CampaignItemSchedule.SelectedScheduleItems.UnionWith(new[] { campaignItem1, campaignItem2 });
				((GlbCompanyCampaignItemScheduleItemsCollection)form.SendScheduleControl.FilterItemModule.GridCollection).LoadData(campaign.CampaignItemSchedule, ZDateTime.Empty);
				AssertEquals("There should be 2 elements in grid", 2, form.SendScheduleControl.FilterItemModule.DisplayGrid.ListManager.List.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.SendScheduleControl.FilterItemModule.DisplayGrid.Select(0);
				form.SendScheduleControl.FilterItemModule.DisplayGrid.Select(1);
				form.SendScheduleControl.ToolStripExposed.Items[2].PerformClick();  // Delete Button
				AssertNull("Extra confirmation dialog should not be shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Last message should be", "You are about to delete the 'Scheduled delivery' for the selected contact(s). Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Test Objects

		class FormForTest : ZForm
		{
			public FormForTest(GlbCompanyCampaign businessEntity)
				: base(businessEntity)
			{
				SendScheduleControl = new SendScheduleUserControlForTest();
				Controls.Add(SendScheduleControl);
				SendScheduleControl.SetDataBinding(businessEntity, "CampaignItemSchedule");
			}

			public SendScheduleUserControlForTest SendScheduleControl;

			void BusinessEntity_MessageOnCampaignSending(object sender, GlbCompanyCampaignSender.MessageOnCampaignSendingEventArgs e)
			{
				LastEventArgs = e;
			}

			public GlbCompanyCampaignSender.MessageOnCampaignSendingEventArgs LastEventArgs;
		}

		class SendScheduleUserControlForTest : SendScheduleUserControl
		{
			public ZToolStrip ToolStripExposed { get { return base.ToolStrip; } }
		}

		#endregion

		#region Implementation

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			ScheduleCampaignItems scheduled1 = new ScheduleCampaignItems(campaign);
			scheduled1.UtcOffset = new ZShort(600);
			scheduled1.Status = "QUE";
			scheduled1.StandardTimeZoneCode = "EST";
			scheduled1.TimeZone = "Australia/Brisbane";
			scheduled1.ScheduleSendTimeUTC = ZDateTime.UtcNow;

			collection.Add(scheduled1);
		}

		#endregion
	}
}
