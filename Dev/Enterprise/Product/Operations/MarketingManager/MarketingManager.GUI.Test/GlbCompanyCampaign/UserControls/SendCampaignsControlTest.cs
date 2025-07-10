using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class SendCampaignsControlTest : TestCaseWithFactory
	{
		#region Test Objects

		public class FormForTest : ZForm
		{
			public FormForTest(GlbCompanyCampaign businessEntity)
					: base(businessEntity)
			{
				CampaignsControl = new SendCampaignsControl();
				Controls.Add(CampaignsControl);
				CampaignsControl.SetDataBinding(businessEntity, null);
			}

			public SendCampaignsControl CampaignsControl;

			void BusinessEntity_MessageOnCampaignSending(object sender, GlbCompanyCampaignSender.MessageOnCampaignSendingEventArgs e)
			{
				LastEventArgs = e;
			}

			public GlbCompanyCampaignSender.MessageOnCampaignSendingEventArgs LastEventArgs;
		}

		#endregion

		public void TestDripMarketingMode()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			using (FormForTest form = new FormForTest(campaign))
			{
				form.Show();
				form.CampaignsControl.IsDripMarketingMode = true;

				AssertEquals(false, form.CampaignsControl.FindRecipientContactsPanel.Visible);
				AssertEquals(false, form.CampaignsControl.SendButton.Visible);
				AssertEquals(false, form.CampaignsControl.ScheduleSendButton.Visible);
				AssertEquals(false, form.CampaignsControl.dropDownButtonsToolStrip.Visible);
			}
		}

		public void TestSourceCampaignPKGuidFindBoxVisibility()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			using (FormForTest form = new FormForTest(campaign))
			{
				form.Show();
				AssertEquals("Precondition", false, campaign.IsUsingCampaignTrackingDataSource);
				AssertEquals(false, form.CampaignsControl.SourceCampaignPKGuidFindBox.Visible);

				campaign.ContactDataSource = ContactDataSourceList.Codes.CampaignTracking;
				AssertEquals("Precondition", true, campaign.IsUsingCampaignTrackingDataSource);
				AssertEquals(true, form.CampaignsControl.SourceCampaignPKGuidFindBox.Visible);

				campaign.ContactDataSource = ContactDataSourceList.Codes.Inquiries;
				AssertEquals("Precondition", false, campaign.IsUsingCampaignTrackingDataSource);
				AssertEquals(false, form.CampaignsControl.SourceCampaignPKGuidFindBox.Visible);
			}
		}

		#region Messages

		#region Campaign Sending Messages

		public void TestSetupControlsForTargetListCampaign_DoesntRegisterOnClickMultipleTimes()
		{
			var campaignStaff = Factory.NewWithValidTestData<GlbStaff>();
			campaignStaff.GS_Code = "TST";
			campaignStaff.GS_EmailAddress = "unit.test@cw1.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
			campaign.G0_Category = "PRINT";
			campaign.G0_EstimatedStartedDate = ZDateTime.Today;
			campaign.G0_GS_NKCampaignCoordinator = campaignStaff.GS_Code;
			campaign.G0_GS_NKCampaignManager = campaignStaff.GS_Code;
			campaign.G0_Type = "EXIST";

			Factory.Save();

			using (var form = new FormForTest(campaign))
			{
				Enumerable.Range(0, 3).ForEach(x => form.CampaignsControl.SetupControlsForTargetListCampaign());
				form.CampaignsControl.dropDownButtonsToolStrip.PerformClick();
				AssertEquals("2 messages (1 default + 1 NoSelectedRecipientsMessage) should be shown.", 2, UnitTestUserNotification.Instance.PreviousMessages.Length);
			}
		}

		public void TestGlbCompanyCampaignForm_ShouldContinueWithSending()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			using (FormForTest form = new FormForTest(campaign))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				bool result = form.CampaignsControl.GlbCompanyCampaignForm_ShouldContinueWithSending(6, null);
				AssertEquals("Last message should be", "About to send campaigns to 6 contacts. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Answer was Yes, should return true", result);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				result = form.CampaignsControl.GlbCompanyCampaignForm_ShouldContinueWithSending(6, null);
				AssertEquals("Last message should be", "About to send campaigns to 6 contacts. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Answer was No, should return false", !result);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				result = form.CampaignsControl.GlbCompanyCampaignForm_ShouldContinueWithSending(6, null);
				AssertEquals("Last message should be", "About to send campaigns to 6 contacts. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Answer was Cancel, should return false", !result);
			}
		}

		public void TestGlbCompanyCampaignForm_ShouldContinueMessageOnTargetList()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;

			using (var form = new FormForTest(campaign))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var result = form.CampaignsControl.GlbCompanyCampaignForm_ShouldContinueWithSending(6, null);
				AssertEquals("Last message should be", "6 contact(s) will be added to the Target List. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Answer was Yes, should return true", result);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				result = form.CampaignsControl.GlbCompanyCampaignForm_ShouldContinueWithSending(6, null);
				AssertEquals("Last message should be", "6 contact(s) will be added to the Target List. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Answer was No, should return false", !result);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				result = form.CampaignsControl.GlbCompanyCampaignForm_ShouldContinueWithSending(6, null);
				AssertEquals("Last message should be", "6 contact(s) will be added to the Target List. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Answer was Cancel, should return false", !result);
			}
		}

		public void TestGlbCompanyCampaignForm_ShouldContinueMessageOnInsideSales()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.InsideSales;

			using (var form = new FormForTest(campaign))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var result = form.CampaignsControl.GlbCompanyCampaignForm_ShouldContinueWithSending(6, null);
				AssertEquals("Last message should be", "6 contact(s) will be added to the Master List. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Answer was Yes, should return true", result);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				result = form.CampaignsControl.GlbCompanyCampaignForm_ShouldContinueWithSending(6, null);
				AssertEquals("Last message should be", "6 contact(s) will be added to the Master List. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Answer was No, should return false", !result);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				result = form.CampaignsControl.GlbCompanyCampaignForm_ShouldContinueWithSending(6, null);
				AssertEquals("Last message should be", "6 contact(s) will be added to the Master List. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Answer was Cancel, should return false", !result);
			}
		}

		public void TestGlbCompanyCampaignForm_MessageOnCampaignSending()
		{
			var campaign = Factory.New<GlbCompanyCampaign>();
			using (FormForTest form = new FormForTest(campaign))
			{
				form.CampaignsControl.GlbCompanyCampaignForm_MessageOnCampaignSending(null, new GlbCompanyCampaignSender.MessageOnCampaignSendingEventArgs("Message", "Summary", true));
				Assert("Should have shown error message", UnitTestUserNotification.Instance.LastMessage.WasError);

				form.CampaignsControl.CurrentDataItem.G0_EstimatedStartedDate = ZDateTime.Invalid;
				form.CampaignsControl.GlbCompanyCampaignForm_MessageOnCampaignSending(null, new GlbCompanyCampaignSender.MessageOnCampaignSendingEventArgs("Message", "Summary", true));
				AssertEquals("Should always show error message", "Message", UnitTestUserNotification.Instance.LastMessage.Text);

				form.CampaignsControl.GlbCompanyCampaignForm_MessageOnCampaignSending(null, new GlbCompanyCampaignSender.MessageOnCampaignSendingEventArgs("Message", "Summary", false));
				Assert("Should have shown information", UnitTestUserNotification.Instance.LastMessage.WasInformation);
			}
		}

		public void TestGlbCompanyCampaignForm_ContactsNotSentCampaign()
		{
			List<CampaignContact> contacts = new List<CampaignContact>();
			var campaign = Factory.New<GlbCompanyCampaign>();
			using (FormForTest form = new FormForTest(campaign))
			{
				form.CampaignsControl.GlbCompanyCampaignForm_ContactsNotSentCampaign(2, contacts.AsReadOnly());
				AssertEquals("UpdateContactForm shown", typeof(UpdateContactsForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}

			if (ZFormModaliser.LastFormShownDialogForTest != null)
			{
				ZFormModaliser.LastFormShownDialogForTest.Dispose();
				ZFormModaliser.LastFormShownDialogForTest = null;
			}
		}

		public void TestGlbCompanyCampaignForm_SelectedCampaignTrackedLink()
		{
			GlbCompanyCampaign campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			using (FormForTest form = new FormForTest(campaign))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.CampaignsControl.PromptNoTrackedLinksAndSend(4, ActionCallback);
				AssertEquals(false, ActionCalled);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.CampaignsControl.PromptNoTrackedLinksAndSend(4, ActionCallback);
				Assert(ActionCalled);

				ActionCalled = false;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.CampaignsControl.PromptNoTrackedLinksAndSend(4, ActionCallback);
				AssertEquals(false, ActionCalled);
			}
		}

		bool ActionCalled;

		void ActionCallback()
		{
			ActionCalled = true;
		}

		#endregion

		#endregion

		#region Test Send Campaigns

		public void TestSendShowsProgressForm()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "zubin.appoo@cargowise.com";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test@cargowise.com";
			contact.OC_ContactName = "Test person";
			contact.OC_OH = org.PK;

			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaignTest.GlbCompanyCampaignForTest>();

			using (FormForTest form = new FormForTest(campaign))
			{
				AssertNull(form.CampaignsControl.LastSendProgressForm);
				form.CampaignsControl.SendToSelectedButton_Click(null, EventArgs.Empty);
				AssertNull("Errors so progress form should not get shown", form.CampaignsControl.LastSendProgressForm);
			}

			campaign.G0_CampaignName = "test asf asf asf asf as fas fas f";
			campaign.HtmlDocumentBlob = ZBlob.FromAscii("blah blah");
			campaign.G0_Category = campaign.Lookups.MediaCategoryList[0].Code;
			campaign.G0_Type = campaign.Lookups.ActiveMediaTypesList[0].Code;
			campaign.G0_EmailSubject = "hasda asfdjas n fan oasdnf oif";
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_GS_NKCampaignManager = staff.GS_Code;
			campaign.G0_EstimatedStartedDate = ZDateTime.Today;
			Factory.Save();

			using (FormForTest form = new FormForTest(campaign))
			{
				form.Show();
				AssertNull(form.CampaignsControl.LastSendProgressForm);

				var campaignItem = campaign.CampaignsItemsSent.AddNew();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				((GlbCampaignContactCollection)form.CampaignsControl.FilterItemModule.GridCollection).Load(new ZQuery(ViewCampaignContactSchema.PK, contact.PK));
				SelectContactInDisplayGrid(form, contact);
				form.CampaignsControl.SendToSelectedButton_Click(null, EventArgs.Empty);

				AssertNotNull(form.CampaignsControl.LastSendProgressForm);
				Assert(form.CampaignsControl.LastSendProgressForm.IsDisposed);
			}
		}

		static void SelectContactInDisplayGrid(FormForTest form, OrgContact contact)
		{
			if (form.CampaignsControl.FilterItemModule.DisplayGrid.ListManager == null)
			{
				throw new DeveloperNotificationException("form has to be shown so ListManager can be initialised");
			}
			form.CampaignsControl.FilterItemModule.DisplayGrid.Select(form.CampaignsControl.FilterItemModule.DisplayGrid.ListManager.List.IndexOf(contact));
		}

		public void TestSendCampaigns()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "CargoWise";
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Samuel";
			contact1.OC_Email = "nobody@nowhere.com";
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Jenny";
			contact2.OC_Email = "anybody@anywhere.com";
			GlbCompanyCampaign campaign = new GlbCompanyCampaignTestHelper(Factory).GetCampaignWithoutErrors();

			Factory.Save();

			ZQuery queryFilter = new ZQuery(StmModuleFilterSchema.S9_FilterName, "");
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_ModuleID, "GlbCompanyCampaignContact");
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_IsPublished, false);
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, campaign.PK);
			var reloadedFilter = Factory.LoadTop1<StmModuleFilter>(queryFilter);
			AssertNull("Precondition", reloadedFilter);

			using (FormForTest form = new FormForTest(campaign))
			{
				form.Show();
				form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory = FilterOrCategory.Green;

				campaign.TemplateEditor.TrackedLinks.AddNew();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				((GlbCampaignContactCollection)form.CampaignsControl.FilterItemModule.GridCollection).Load(new ZQuery(ViewCampaignContactSchema.VCC_OrgFullName, org.OH_FullName));
				SelectContactInDisplayGrid(form, contact1);
				form.CampaignsControl.SendToSelectedButton_Click(null, EventArgs.Empty);
				AssertEquals("Last message should be", "You are about to send this campaign to 1 contact(s). However no hyperlinks/images have been set for monitoring.\r\nIn order to capture a read receipt, destination URL's and/or images must be set for tracking. Would you like to do so?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Campaign should not be sent", 0, campaign.CampaignsItemsSent.Count);
				AssertEquals("Should not clear any grid collection items", 2, form.CampaignsControl.FilterItemModule.GridCollection.Count);
				AssertEquals("Should not clear selection", 1, form.CampaignsControl.FilterItemModule.GetSelectedBusinessObjects().Length);

				reloadedFilter = Factory.LoadTop1<StmModuleFilter>(queryFilter);
				AssertNotNull(reloadedFilter);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				((GlbCampaignContactCollection)form.CampaignsControl.FilterItemModule.GridCollection).Load(new ZQuery(ViewCampaignContactSchema.VCC_OrgFullName, org.OH_FullName));
				SelectContactInDisplayGrid(form, contact1);
				form.CampaignsControl.SendToSelectedButton_Click(null, EventArgs.Empty);
				AssertEquals("Last message should be", "1 campaigns were successfully sent", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Campaign should be sent", 1, campaign.CampaignsItemsSent.Count);
				AssertEquals("Should clear selection", 0, form.CampaignsControl.FilterItemModule.GetSelectedBusinessObjects().Length);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				((GlbCampaignContactCollection)form.CampaignsControl.FilterItemModule.GridCollection).Load(new ZQuery(ViewCampaignContactSchema.PK, contact2.PK));
				SelectContactInDisplayGrid(form, contact2);
				form.CampaignsControl.SendToSelectedButton_Click(null, EventArgs.Empty);
				AssertEquals("Last message should be", "About to send campaigns to 1 contacts. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("There should be 1 campaign item sent", 1, campaign.CampaignsItemsSent.Count);
				AssertEquals("Should not clear any grid collection items", 1, form.CampaignsControl.FilterItemModule.GridCollection.Count);
				AssertEquals("Should not clear selection", 1, form.CampaignsControl.FilterItemModule.GetSelectedBusinessObjects().Length);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.CampaignsControl.SendToSelectedButton_Click(null, EventArgs.Empty);
				AssertEquals("Campaign should be sent", 2, campaign.CampaignsItemsSent.Count);
				AssertEquals("Should clear selection", 0, form.CampaignsControl.FilterItemModule.GetSelectedBusinessObjects().Length);
			}
		}

		public void TestSendScheduleCampaigns()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "CargoWise";
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Samuel";
			contact1.OC_Email = "nobody@nowhere.com";
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Jenny";
			contact2.OC_Email = "anybody@anywhere.com";
			GlbCompanyCampaign campaign = new GlbCompanyCampaignTestHelper(Factory).GetCampaignWithoutErrors();

			Factory.Save();

			ZQuery queryFilter = new ZQuery(StmModuleFilterSchema.S9_FilterName, "");
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_ModuleID, "GlbCompanyCampaignContact");
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_IsPublished, false);
			queryFilter.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, campaign.PK);
			var reloadedFilter = Factory.LoadTop1<StmModuleFilter>(queryFilter);
			AssertNull("Precondition", reloadedFilter);

			using (FormForTest form = new FormForTest(campaign))
			{
				form.Show();
				form.CampaignsControl.FilterStripControl.FilterBusinessObject.FilterStrips[0].OrCategory = FilterOrCategory.Green;

				campaign.TemplateEditor.TrackedLinks.AddNew();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				((GlbCampaignContactCollection)form.CampaignsControl.FilterItemModule.GridCollection).Load(new ZQuery(ViewCampaignContactSchema.VCC_OrgFullName, org.OH_FullName));
				SelectContactInDisplayGrid(form, contact1);
				form.CampaignsControl.ScheduleSendButton_Click(null, EventArgs.Empty);
				AssertEquals("Last message should be", "You are about to send this campaign to 1 contact(s). However no hyperlinks/images have been set for monitoring.\r\nIn order to capture a read receipt, destination URL's and/or images must be set for tracking. Would you like to do so?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Campaign should not be sent", 0, campaign.CampaignsItemsSent.Count);
				AssertEquals("Grid collection items should be cleared", 0, form.CampaignsControl.FilterItemModule.GridCollection.Count);
				AssertEquals("CampaignItemScheduleForm should popup", typeof(CampaignItemScheduleForm), ZFormModaliser.LastFormShownForTest.GetType());
				ZFormModaliser.LastFormShownDialogForTest = null;
			}
		}

		#endregion

		#region Contact Details

		public void TestShowContactDetails()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "d";
			contact.OC_Phone = GlbCompanyCampaignTest.GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			OrgColdCallRegister inquiryContact = Factory.NewWithValidTestData<OrgColdCallRegister>();
			inquiryContact.O1_ContactName = "e";
			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			using (FormForTest form = new FormForTest(campaign))
			{
				form.Show();

				ZQuery query = new ZQuery();
				query.AddToFilter(ViewCampaignContactSchema.PK, contact.PK);
				query.AddToFilter(JoinCondition.Or, ViewCampaignContactSchema.PK, inquiryContact.PK);

				GlbCampaignContactCollection collection = new GlbCampaignContactCollection(campaign);
				collection.Load(query);
				collection.Sort(ViewCampaignContactSchema.VCC_ContactName.Name, System.ComponentModel.ListSortDirection.Ascending);
				form.CampaignsControl.FilterItemModule.GridCollection.AddRange(collection);

				MenuItem contactDetailsMenuItem = form.CampaignsControl.FilterStripControl.Grid.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(s => s.Text == "&View");
				AssertNotNull(contactDetailsMenuItem);

				form.CampaignsControl.FilterStripControl.Grid.ListManager.Position = 0;
				contactDetailsMenuItem.PerformClick();
				AssertEquals("Form Shown", typeof(MasterFiles.GUI.ZOrganisationsForm), form.CampaignsControl.FilterItemModule.LastShownForm.GetType());
				MasterFiles.GUI.ZOrganisationsForm activeForm = (MasterFiles.GUI.ZOrganisationsForm)form.CampaignsControl.FilterItemModule.LastShownForm;
				activeForm.Close();

				form.CampaignsControl.FilterStripControl.Grid.ListManager.Position = 1;
				contactDetailsMenuItem.PerformClick();
				AssertEquals("Form Shown", typeof(MasterFiles.GUI.SalesEnquiryForm), form.CampaignsControl.FilterItemModule.LastShownForm.GetType());
				MasterFiles.GUI.SalesEnquiryForm activeForm2 = (MasterFiles.GUI.SalesEnquiryForm)form.CampaignsControl.FilterItemModule.LastShownForm;
				activeForm2.Close();
			}
		}

		#endregion
	}
}
