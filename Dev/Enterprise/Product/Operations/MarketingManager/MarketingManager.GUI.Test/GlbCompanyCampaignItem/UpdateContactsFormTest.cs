using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(UpdateContactsForm))]
	public class UpdateContactsFormTest : ZFormBasherTest
	{
		public void TestSendersGrid_DoubleClick()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Ed";
			contact.OC_Phone = GlbCompanyCampaignTest.GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			Factory.Save();

			var contactWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign);
			GlbCampaignContactCollection campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, campaign);
			contactWithNDR.ContactsCollection.Add(campaignContactCollection[0]);

			using (FormForTest form = new FormForTest(contactWithNDR))
			{
				form.Show();
				form.SendersGridExposed.ListManager.Position = 0;
				form.SendersGridExposed.SelectSingleElement(staff);
				Application.DoEvents();
				form.SendersGridExposed.PerformDoubleClickForTest();

				AssertNotNull(form.StaffControllerExposed.LastShownForm);
				AssertEquals("Form Shown", typeof(MasterFiles.GUI.GlbStaffForm), form.StaffControllerExposed.LastShownForm.GetType());
				MasterFiles.GUI.GlbStaffForm activeForm = (MasterFiles.GUI.GlbStaffForm)form.StaffControllerExposed.LastShownForm;
				activeForm.Close();
			}
		}

		public void TestOnHasChangesChanged_NullReferenceException()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Ed";
			contact.OC_Phone = GlbCompanyCampaignTest.GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			Factory.Save();

			var contactWithNdr = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign);
			GlbCampaignContactCollection campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject> { contact }, campaign);
			contactWithNdr.ContactsCollection.Add(campaignContactCollection[0]);

			MasterFiles.GUI.GlbStaffForm activeForm = null;
			using (var form = new FormForTest(contactWithNdr))
			{
				form.Show();
				form.SendersGridExposed.ListManager.Position = 0;
				form.SendersGridExposed.SelectSingleElement(staff);
				Application.DoEvents();
				form.SendersGridExposed.PerformDoubleClickForTest();

				AssertNotNull(form.StaffControllerExposed.LastShownForm);
				AssertEquals("Form Shown", typeof(MasterFiles.GUI.GlbStaffForm), form.StaffControllerExposed.LastShownForm.GetType());
				activeForm = (MasterFiles.GUI.GlbStaffForm)form.StaffControllerExposed.LastShownForm;
			}

			var formStaff = activeForm.BusinessEntity as GlbStaff;
			formStaff.GS_FullName = "bha";

			using (activeForm)
			{
				activeForm.FireSaveButton();
			}
		}

		public void TestContactsGrid_DoubleClick()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Ed";
			contact.OC_Phone = GlbCompanyCampaignTest.GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			Factory.Save();

			var contactWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory);
			GlbCampaignContactCollection campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, campaign);
			contactWithNDR.ContactsCollection.Add(campaignContactCollection[0]);
			using (FormForTest form = new FormForTest(contactWithNDR))
			{
				form.Show();
				MenuItem contactDetailsMenuItem = form.ContactsGridExposed.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(s => s.Text == "Contact Details");

				AssertNotNull(contactDetailsMenuItem);

				form.ContactsGridExposed.ListManager.Position = 0;
				contactDetailsMenuItem.PerformClick();
				AssertNotNull(form.ContactControllerExposed.LastShownForm);
				AssertEquals("Form Shown", typeof(MasterFiles.GUI.ZOrganisationsForm), form.ContactControllerExposed.LastShownForm.GetType());
				MasterFiles.GUI.ZOrganisationsForm activeForm = (MasterFiles.GUI.ZOrganisationsForm)form.ContactControllerExposed.LastShownForm;
				activeForm.Close();
			}
		}

		public void TestContactsGrid_DoubleClickHR()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Ed";

			var jobApplicant = Factory.New<Enterprise.Integration.Recruiter.IHRJobApplicant>();
			jobApplicant.HA_FullName = "Edd";

			Factory.Save();

			var contactWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory);
			var campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { staff }, campaign);
			contactWithNDR.ContactsCollection.Add(campaignContactCollection[0]);

			using (var form = new FormForTest(contactWithNDR))
			{
				form.Show();
				var contactDetailsMenuItem = form.ContactsGridExposed.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(s => s.Text == "Contact Details");

				AssertNotNull(contactDetailsMenuItem);

				form.ContactsGridExposed.ListManager.Position = 0;
				contactDetailsMenuItem.PerformClick();
				AssertNotNull(form.StaffControllerExposed.LastShownForm);
				AssertEquals("Form Shown", typeof(MasterFiles.GUI.GlbStaffForm), form.StaffControllerExposed.LastShownForm.GetType());
				var activeForm = (MasterFiles.GUI.GlbStaffForm)form.StaffControllerExposed.LastShownForm;
				activeForm.Close();
			}

			var applicantContactWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory);
			var applicantCampaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { (BusinessObject)jobApplicant }, campaign);
			applicantContactWithNDR.ContactsCollection.Add(applicantCampaignContactCollection[0]);

			using (var form = new FormForTest(applicantContactWithNDR))
			{
				form.Show();
				var contactDetailsMenuItem = form.ContactsGridExposed.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(s => s.Text == "Contact Details");

				AssertNotNull(contactDetailsMenuItem);

				form.ContactsGridExposed.ListManager.Position = 0;
				contactDetailsMenuItem.PerformClick();
				AssertNotNull(form.ApplicantControllerExposed.LastShownForm);
				var activeForm = (ZForm)form.ApplicantControllerExposed.LastShownForm;
				activeForm.Close();
			}
		}

		public void TestNoStaffSeucritySendersGridDoubleClick()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Ed";
			contact.OC_Phone = GlbCompanyCampaignTest.GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			Factory.Save();

			var contactWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign);
			GlbCampaignContactCollection campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, campaign);
			contactWithNDR.ContactsCollection.Add(campaignContactCollection[0]);
			Env.Security.StaffLocalAdministratorPlaceholder.IsAllowed = false;
			Env.Security.StaffEdit.IsAllowed = false;
			Env.Security.StaffView.IsAllowed = false;
			using (FormForTest form = new FormForTest(contactWithNDR))
			{
				form.Show();
				form.SendersGridExposed.ListManager.Position = 0;
				form.SendersGridExposed.SelectSingleElement(staff);
				Application.DoEvents();
				form.SendersGridExposed.PerformDoubleClickForTest();
				AssertNull("Senders form should not be opened without the proper rights.", form.StaffControllerExposed.LastShownForm);
			}
		}

		#region Messages

		#region Too Many Results

		public void TestInformationText()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CXE";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "a@a.com";
			contact1.OC_ContactName = "A";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "b@b.com";
			contact2.OC_ContactName = "B";
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);

			ReadOnlyCollection<CampaignContact> contacts = new ReadOnlyCollection<CampaignContact>(campaignContactCollection.Cast<CampaignContact>().ToList());
			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.TargetList;
			ContactsWithNonDeliveryReportsUpdater contactsWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign);
			contactsWithNDR.ContactsCollection.Add(campaignContactCollection[0]);
			contactsWithNDR.ContactsCollection.Add(campaignContactCollection[1]);

			using (FormForTest form = new FormForTest(new Collection<IScheduleItemsProvider>(), campaignContactCollection.Cast<CampaignContact>(), contactsWithNDR))
			{
				string expected = @"0 contact(s) were successfully added.

			The following contacts could not be added to the target list.. Possible reasons include:
			- Contact does not have an Email address
			- Contact has recently received a Non Delivery Receipt
			- Email address is malformed";

				AssertMultilineASCIIEquals("TargetListLabel", expected, form.NumCampaignsSentLabelExposed.Text);
			}

			campaign.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.Broadcast;
			using (FormForTest form = new FormForTest(new Collection<IScheduleItemsProvider>(), campaignContactCollection.Cast<CampaignContact>(), contactsWithNDR))
			{
				string expected = @"0 contact(s) were successfully added to the scheduling list.

			The following contacts could not be added to the scheduling list.. Possible reasons include:
			- Contact does not have an Email address
			- Contact has recently received a Non Delivery Receipt
			- Email address is malformed";

				AssertMultilineASCIIEquals("ScheduleListLabel", expected, form.NumCampaignsSentLabelExposed.Text);
			}

			using (FormForTest form = new FormForTest(4, campaignContactCollection.Cast<CampaignContact>(), contactsWithNDR))
			{
				string expected = @"Campaign was successfully sent to 4 contact(s).

			The campaign however could not be delivered to the following contacts. Possible reasons include:
			- Contact does not have an Email address
			- Contact has recently received a Non Delivery Receipt
			- Email address is malformed";

				AssertMultilineASCIIEquals("CampaignsSentLabel", expected, form.NumCampaignsSentLabelExposed.Text);
			}
		}

		#endregion

		#region Campaign Sending Messages

		public void TestGlbCompanyCampaignFormShouldContinueWithSendingStandAloneCampaign()
		{
			const string textMessage = "Do you want to resend the campaign to the following edited contacts?";
			const string caption = "Send Campaigns";

			AssertGlbCompanyCampaignFormShouldContinueWithSending(CampaignTypeList.Codes.Broadcast, textMessage, caption);
			AssertGlbCompanyCampaignFormShouldContinueWithSending(CampaignTypeList.Codes.Voting, textMessage, caption);
			AssertGlbCompanyCampaignFormShouldContinueWithSending(CampaignTypeList.Codes.Survey, textMessage, caption);
		}

		public void TestGlbCompanyCampaignFormShouldContinueWithSendingMasterCampaign()
		{
			const string textMessage = "Do you want to add the edited contacts to the Campaign list?";
			const string caption = "Campaign List";

			AssertGlbCompanyCampaignFormShouldContinueWithSending(CampaignTypeList.Codes.DripMarketing, textMessage, caption);
			AssertGlbCompanyCampaignFormShouldContinueWithSending(CampaignTypeList.Codes.InsideSales, textMessage, caption);
		}

		void AssertGlbCompanyCampaignFormShouldContinueWithSending(string campaignType, string textMessage, string captionMessage)
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = campaignType;

			var contactsWithNdr = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign);
			using (var form = new FormForTest(contactsWithNdr))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				Assert("Answer was Yes, should return true", form.GlbCompanyCampaignForm_ShouldContinueWithSending(6, null));
				AssertEquals("Last message should be", textMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Caption should be", captionMessage, UnitTestUserNotification.Instance.LastMessage.Caption);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				Assert("Answer was No, should return false", !form.GlbCompanyCampaignForm_ShouldContinueWithSending(6, null));
				AssertEquals("Last message should be", textMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Caption should be", captionMessage, UnitTestUserNotification.Instance.LastMessage.Caption);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				Assert("Answer was Cancel, should return false", !form.GlbCompanyCampaignForm_ShouldContinueWithSending(6, null));
				AssertEquals("Last message should be", textMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Caption should be", captionMessage, UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		public void TestGlbCompanyCampaignFormContactsNotSentCampaign()
		{
			AssertGlbCompanyCampaignFormContactsNotSentCampaign(CampaignTypeList.Codes.Broadcast);
			AssertGlbCompanyCampaignFormContactsNotSentCampaign(CampaignTypeList.Codes.Voting);
			AssertGlbCompanyCampaignFormContactsNotSentCampaign(CampaignTypeList.Codes.Survey);
			AssertGlbCompanyCampaignFormContactsNotSentCampaign(CampaignTypeList.Codes.LinkTracking);
			AssertGlbCompanyCampaignFormContactsNotSentCampaign(CampaignTypeList.Codes.DripMarketing);
			AssertGlbCompanyCampaignFormContactsNotSentCampaign(CampaignTypeList.Codes.InsideSales);
		}

		void AssertGlbCompanyCampaignFormContactsNotSentCampaign(string campaignType)
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_BroadcastVoteSurveyExam = campaignType;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "A";
			contact1.OC_Email = "a@a.com";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "b@b.com";
			contact2.OC_ContactName = "B";
			Factory.Save();

			var campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject> { contact1, contact2 }, campaign);
			var contacts = new ReadOnlyCollection<CampaignContact>(campaignContactCollection.Cast<CampaignContact>().ToList());
			var contactsWithNdr = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign);

			using (var form = new FormForTest(contactsWithNdr))
			{
				form.GlbCompanyCampaignForm_ContactsNotSentCampaign(2, contacts);
				AssertEquals("2 Contacts should be in grid", 2, form.ContactsGridExposed.ListManager.Count);
				AssertEquals(form.InformationTextForTest, form.NumCampaignsSentLabelExposed.Text);
			}
		}

		public void TestGlbCompanyCampaignForm_CampaignSuccessfullySent()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CXE";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "a@a.com";
			contact1.OC_ContactName = "A";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "b@b.com";
			contact2.OC_ContactName = "B";
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);

			ReadOnlyCollection<CampaignContact> contacts = new ReadOnlyCollection<CampaignContact>(campaignContactCollection.Cast<CampaignContact>().ToList());
			ContactsWithNonDeliveryReportsUpdater contactsWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign);
			using (FormForTest form = new FormForTest(contactsWithNDR))
			{
				form.GlbCompanyCampaignForm_CampaignSuccessfullySent(2);
				AssertEquals("There should be no Contacts in grid", 0, form.ContactsGridExposed.ListManager.Count);
			}
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

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			ContactsWithNonDeliveryReportsUpdater contactsWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign);
			GlbCampaignContactCollection campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, campaign);
			contactsWithNDR.ContactsCollection.Add(campaignContactCollection[0]);

			using (FormForTest form = new FormForTest(contactsWithNDR))
			{
				AssertNull(form.LastSendProgressForm);
				form.ResendCampaign();
				AssertNull("Errors so progress form should not get shown", form.LastSendProgressForm);
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

			using (FormForTest form = new FormForTest(contactsWithNDR))
			{
				form.Show();
				AssertNull(form.LastSendProgressForm);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ResendCampaign();

				AssertNotNull(form.LastSendProgressForm);
				Assert(form.LastSendProgressForm.IsDisposed);
			}
		}

		public void TestSendCampaigns()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "CargoWise";
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Samuel";
			contact1.OC_Email = "";
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Jenny";
			contact2.OC_Email = "anybody@anywhere.com";
			GlbCompanyCampaign campaign = new GlbCompanyCampaignTestHelper(Factory).GetCampaignWithoutErrors();

			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);
			ContactsWithNonDeliveryReportsUpdater contactsWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign);
			contactsWithNDR.ContactsCollection.Add(campaignContactCollection[0]);
			contactsWithNDR.ContactsCollection.Add(campaignContactCollection[1]);

			using (FormForTest form = new FormForTest(contactsWithNDR))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.ResendCampaign();
				AssertEquals("Campaign should not be sent", 0, campaign.CampaignsItemsSent.Count);
				AssertEquals("Should not clear any grid collection items", 2, form.ContactsGridExposed.ListManager.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ResendCampaign();
				AssertEquals("Campaign should be sent", 1, campaign.CampaignsItemsSent.Count);
				AssertEquals("Should clear one of the grid collection items", 1, form.ContactsGridExposed.ListManager.Count);
				AssertEquals("UNV", campaign.CampaignsItemsSent[0].G8_TrackingStatus);

				contactsWithNDR.ContactsCollection.Add(campaignContactCollection[0]);
				contactsWithNDR.ContactsCollection.Add(campaignContactCollection[1]);
				campaign.CampaignsItemsSent[0].G8_TrackingStatus = "NDR";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				campaignContactCollection[0].IsEditing = true;
				campaignContactCollection[1].IsEditing = true;
				form.ResendCampaign_Exposed(campaign.CampaignsItemsSent.ToArray().Cast<GlbCompanyCampaignItem>().ToArray());
				AssertEquals("Campaign should be sent", 1, campaign.CampaignsItemsSent.Count);
				AssertEquals("UNV", campaign.CampaignsItemsSent[0].G8_TrackingStatus);
			}
		}

		public void TestSendScheduleCampaigns()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "CargoWise";
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Edward";
			contact1.OC_Email = "";
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Richard";
			contact2.OC_Email = "anybody@anywhere.com";
			GlbCompanyCampaign campaign = new GlbCompanyCampaignTestHelper(Factory).GetCampaignWithoutErrors();

			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1, contact2 }, campaign);
			ContactsWithNonDeliveryReportsUpdater contactsWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign);
			contactsWithNDR.ContactsCollection.Add(campaignContactCollection[0]);
			contactsWithNDR.ContactsCollection.Add(campaignContactCollection[1]);
			contactsWithNDR.ContactsCollection.Sort(ViewCampaignContactSchema.Constants.VCC_ContactName, System.ComponentModel.ListSortDirection.Ascending);

			using (FormForTest form = new FormForTest(new Collection<IScheduleItemsProvider>(), campaignContactCollection.Cast<CampaignContact>(), contactsWithNDR))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.ValidateAndSaveExposed();
				AssertEquals("Campaign should not be sent", 0, campaign.CampaignsItemsSent.Count);
			}

			using (FormForTest form = new FormForTest(new Collection<IScheduleItemsProvider>(), campaignContactCollection.Cast<CampaignContact>(), contactsWithNDR))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ValidateAndSaveExposed();
				AssertEquals("Last message should be", "Do you want to continue scheduling the campaign for 1 edited contact(s)?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Campaign should still not be sent", 0, campaign.CampaignsItemsSent.Count);
				AssertEquals("CampaignItemScheduleForm should popup", typeof(CampaignItemScheduleForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				ZFormModaliser.LastFormShownDialogForTest = null;
			}

			contactsWithNDR.ContactsCollection[1].IsNDR = true;
			using (FormForTest form = new FormForTest(new Collection<IScheduleItemsProvider>(), campaignContactCollection.Cast<CampaignContact>(), contactsWithNDR))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ValidateAndSaveExposed();
				AssertEquals("Campaign should still not be sent", 0, campaign.CampaignsItemsSent.Count);
				AssertNull("CampaignItemScheduleForm should not popup", ZFormModaliser.LastFormShownDialogForTest);
			}

			contactsWithNDR.ContactsCollection[1].IsNDR = false;
			using (FormForTest form = new FormForTest(new Collection<IScheduleItemsProvider>(), campaignContactCollection.Cast<CampaignContact>(), contactsWithNDR))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ValidateAndSaveExposed();
				AssertEquals("Last message should be", "Do you want to continue scheduling the campaign for 1 edited contact(s)?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Campaign should still not be sent", 0, campaign.CampaignsItemsSent.Count);
				AssertEquals("CampaignItemScheduleForm should popup", typeof(CampaignItemScheduleForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				ZFormModaliser.LastFormShownDialogForTest = null;
			}
		}

		public void TestContactsNotTransitionedAndScheduledForDRMSPECampaigns()
		{
			var master = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			master.G0_BroadcastVoteSurveyExam = CampaignTypeList.Codes.DripMarketing;
			master.G0_CampaignName = "TEST_MASTER";

			var touch1a = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			touch1a.G0_HorizontalId = 1;
			touch1a.G0_VerticalId = "A";
			touch1a.G0_G0_Master = master.PK;
			touch1a.SendSettings.GSC_ScheduleType = GlbCompanyCampaignSendSettingsLookups.Codes.IMM;
			touch1a.G0_CampaignName = "TEST_TOUCH1A";
			master.AllTouches.Add(touch1a);

			GlbCompanyCampaignTestHelper.PopulateCampaign(master, Factory);
			GlbCompanyCampaignTestHelper.PopulateCampaign(touch1a, Factory);

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var testContact = org.Contacts.AddNew();
			testContact.OC_ContactName = "Ed";
			testContact.OC_Phone = GlbCompanyCampaignTest.GlbCompanyCampaignForTest.PhoneFilterForLoadingLessObjects;
			testContact.OC_Email = "unit.test@cw1.com";

			var emailAddressWithNDR = Factory.New<GlbEmailAddress>();
			emailAddressWithNDR.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			emailAddressWithNDR.GI_DeliveryReportTimeUtc = ZDateTime.Now;
			emailAddressWithNDR.GI_EmailAddress = testContact.OC_Email;
			Factory.Save();

			var contactWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory, master);
			var campaignContactCollection = GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { testContact }, master);
			contactWithNDR.ContactsCollection.Add(campaignContactCollection[0]);
			using (var form = new FormForTest(0, campaignContactCollection.Cast<CampaignContact>(), contactWithNDR))
			{
				form.Show();
				Assert("Pre-condition", testContact.IsNDR);

				form.BusinessEntity.ContactsCollection[0].IsNDR = false;
				form.BusinessEntity.UpdateContacts();
				Assert("Contact should not have NDR.", !testContact.IsNDR);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ValidateAndSaveExposed();

				var campaignItems = Factory.Load<GlbCompanyCampaignItem>(new ZQuery());
				AssertEquals("Only 1 CampaignItem should be created on the Master. No CampaignItems should be created on Touches", 1, campaignItems.Length);

				var campaignItem = campaignItems[0];
				AssertEquals("CampaignItem should be created for Master ONLY.", master.PK, campaignItem.G8_G0);
				AssertEquals("CampaignItem should not be scheduled.", campaignItem.G8_ScheduleTimeUtc, ZDateTime.Empty);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var bizO = new ContactsWithNonDeliveryReportsUpdater(Factory);
			return new UpdateContactsForm(bizO);
		}

		class FormForTest : UpdateContactsForm
		{
			public FormForTest(ContactsWithNonDeliveryReportsUpdater businessEntity)
				: base(businessEntity)
			{
			}

			public FormForTest(int numOfEmailsActuallySent, IEnumerable<CampaignContact> contactsNotSentTo, ContactsWithNonDeliveryReportsUpdater contactsWithNonDeliveryReport)
				: base(numOfEmailsActuallySent, contactsNotSentTo, contactsWithNonDeliveryReport)
			{
			}

			public FormForTest(Collection<IScheduleItemsProvider> contactsToBeScheduled, IEnumerable<CampaignContact> contactsNotSentTo, ContactsWithNonDeliveryReportsUpdater contactsWithNonDeliveryReport)
				: base(contactsToBeScheduled, contactsNotSentTo, contactsWithNonDeliveryReport)
			{
			}

			internal ZGrid ContactsGridExposed
			{
				get { return this.ContactsGrid; }
			}

			internal ZGrid SendersGridExposed
			{
				get { return this.SendersGrid; }
			}

			internal ZController ContactControllerExposed
			{
				get { return this.ContactController; }
			}

			internal ZController StaffControllerExposed
			{
				get { return this.StaffController; }
			}

			internal ZController ApplicantControllerExposed
			{
				get { return this.ApplicantController; }
			}

			internal void ResendCampaign()
			{
				base.ResendCampaign(null);
			}

			internal void ResendCampaign_Exposed(GlbCompanyCampaignItem[] items)
			{
				base.ResendCampaign(items);
			}

			internal ZLabel NumCampaignsSentLabelExposed
			{
				get { return this.NumCampaignsSentLabel; }
			}

			internal ContinueWithSave ValidateAndSaveExposed()
			{
				return base.ValidateAndSave();
			}

			internal string InformationTextForTest => GetInformationText(ContactsGridExposed.ListManager.Count);
		}

		#endregion
	}
}
