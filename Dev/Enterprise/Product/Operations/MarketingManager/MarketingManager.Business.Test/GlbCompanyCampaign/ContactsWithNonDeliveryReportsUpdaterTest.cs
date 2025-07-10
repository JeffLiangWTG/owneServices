using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(ContactsWithNonDeliveryReportsUpdater))]
	sealed class ContactsWithNonDeliveryReportsUpdaterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendersCollection()
		{
			var campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();

			campaign1.G0_GS_NKCampaignCoordinator = staff1.GS_Code;
			campaign2.G0_GS_NKCampaignCoordinator = staff2.GS_Code;
			campaign2.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.SPS;
			campaign2.SenderPool.AddNew().GCP_GS_NKSender = staff3.GS_Code;
			campaign2.SenderPool.AddNew().GCP_GS_NKSender = staff4.GS_Code;

			var updater = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign1);
			AssertEquals(1, updater.SendersCollection.Count);
			AssertEquals(staff1.GS_Code, updater.SendersCollection[0].GS_Code);

			updater = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign2);
			AssertEquals(3, updater.SendersCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { staff2.GS_Code, staff3.GS_Code, staff4.GS_Code }, updater.SendersCollection.Select(s => s.GS_Code));
		}

		public void TestSelectedCampaignItem()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "NGCS";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Swarez";
			Factory.Save();

			var contactsWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign);
			var selectedCampaignItem = contactsWithNDR.SelectedCampaignItem(Factory.Load<CampaignContact>(contact.PK));
			AssertNull(selectedCampaignItem);

			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contact.PK;
			Factory.Save();
			selectedCampaignItem = contactsWithNDR.SelectedCampaignItem(Factory.Load<CampaignContact>(contact.PK));
			AssertNotNull(selectedCampaignItem);
			AssertEquals(campaignItem.PK, selectedCampaignItem.PK);
		}

		public void TestUpdateContacts()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var contactsWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CDE";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "email@gmail.com";
			contact.OC_Title = "Mr";
			contact.OC_ContactName = "AAA";
			contact.IsNDR = true;

			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, campaign);
			var campaignContact = campaignContactCollection[0];
			AssertEquals("email@gmail.com", campaignContact.VCC_Email);
			Assert(campaignContact.IsNDR);

			campaignContact.VCC_Email = "x@wxyz.com";
			campaignContact.IsNDR = false;
			contactsWithNDR.ContactsCollection.Add(campaignContact);
			contactsWithNDR.UpdateContacts();
			GlbCampaignContactCollection newCampaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, campaign);
			var newCampaignContact = newCampaignContactCollection[0];
			AssertEquals("Contact Email should change", "x@wxyz.com", newCampaignContact.VCC_Email);
			AssertEquals("NDR status should change for this contact", false, newCampaignContact.IsNDR);

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_Email = "info@yahoo.com";
			inquiry.IsNDR = true;
			Factory.Save();

			GlbCampaignContactCollection inquiryCampaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { inquiry }, campaign);
			var inquiryCampaignContact = inquiryCampaignContactCollection[0];
			AssertEquals("info@yahoo.com", inquiryCampaignContact.VCC_Email);
			inquiryCampaignContact.VCC_Email = "cc@yahoo.com";
			inquiryCampaignContact.IsNDR = false;
			contactsWithNDR.ContactsCollection.Add(inquiryCampaignContact);
			inquiryCampaignContact.IsEditing = true;
			contactsWithNDR.UpdateContacts();
			AssertEquals("cc@yahoo.com", inquiry.O1_Email);
			Assert(!inquiry.IsNDR);

			var linkedContact = org.Contacts.AddNew();
			linkedContact.OC_Email = "linked@hotmail.com";
			linkedContact.OC_Title = "Dr";
			linkedContact.OC_ContactName = "BBB";
			linkedContact.IsNDR = true;

			inquiry.O1_OC_LinkedContact = linkedContact.PK;
			inquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			Factory.Save();

			GlbCampaignContactCollection linkedCampaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { inquiry }, campaign);
			var linkedCampaignContact = linkedCampaignContactCollection[0];
			AssertEquals("cc@yahoo.com", linkedCampaignContact.VCC_Email);
			linkedCampaignContact.VCC_Email = "newEmail@hotmail.com";
			linkedCampaignContact.IsNDR = false;
			contactsWithNDR.ContactsCollection.Add(linkedCampaignContact);
			linkedCampaignContact.IsEditing = true;
			contactsWithNDR.UpdateContacts();
			AssertEquals("newEmail@hotmail.com", inquiry.O1_Email);
			Assert(!linkedCampaignContact.IsNDR);
		}

		public void TestUpdateContactsHR()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var contactsWithNDR = new ContactsWithNonDeliveryReportsUpdater(Factory, campaign);

			var jobApplicant = Factory.New<Enterprise.Integration.Recruiter.IHRJobApplicant>();
			jobApplicant.HA_FullName = "Harry Hurray";

			Factory.Save();

			var hrContact = Factory.Load<CampaignContact>(jobApplicant.PK);

			var campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { hrContact }, campaign);
			var campaignContact = campaignContactCollection[0];
			AssertEquals("Harry Hurray", campaignContact.VCC_ContactName);
			Assert(!campaignContact.IsNDR);

			campaignContact.VCC_Email = "x@wxyz.com";
			campaignContact.IsNDR = false;
			contactsWithNDR.ContactsCollection.Add(campaignContact);
			contactsWithNDR.UpdateContacts();
			var newCampaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { hrContact }, campaign);
			var newCampaignContact = newCampaignContactCollection[0];
			AssertEquals("Contact Email should change", "x@wxyz.com", newCampaignContact.VCC_Email);
			AssertEquals("NDR status should be false", false, newCampaignContact.IsNDR);

			var staff = Factory.New<GlbStaff>();
			staff.GS_EmailAddress = "info@yahoo.com";
			Factory.Save();

			var staffCampaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { staff }, campaign);
			var staffCampaignContact = staffCampaignContactCollection[0];
			AssertEquals("info@yahoo.com", staffCampaignContact.VCC_Email);
			Assert(!staffCampaignContact.IsNDR);

			staffCampaignContact.VCC_Email = "cc@yahoo.com";
			staffCampaignContact.IsNDR = true;
			contactsWithNDR.ContactsCollection.Add(staffCampaignContact);
			staffCampaignContact.IsEditing = true;
			contactsWithNDR.UpdateContacts();

			var newStaffCampaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { staff }, campaign);
			var newStaffCampaignContact = newStaffCampaignContactCollection[0];
			AssertEquals("Contact Email should change", "cc@yahoo.com", newStaffCampaignContact.VCC_Email);
			AssertEquals("NDR status should be true", true, newStaffCampaignContact.IsNDR);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ContactsWithNonDeliveryReportsUpdater(Factory);
		}

		#endregion
	}
}
