using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class ViewCampaignContactValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckVCC_Email()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CCS";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "eddy";
			contact.OC_Email = "e@e.com";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Empty Contact";
			contact1.OC_Email = "";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Invalid Contact Email";
			contact2.OC_Email = "s@w";
			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, campaign);
			var campaignContact = campaignContactCollection[0];
			campaignContact.Validation.ValidateVCC_Email();
			AssertNoRowErrors("Email address is valid", campaignContact);

			campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact1 }, campaign);
			campaignContact = campaignContactCollection[0];
			campaignContact.Validation.ValidateVCC_Email();
			AssertNoRowErrors("Email address is valid", campaignContact);

			campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact2 }, campaign);
			campaignContact = campaignContactCollection[0];
			campaignContact.Validation.ValidateVCC_Email();
			AssertHasErrors("Invalid email address so error expected", campaignContact.VCC_EmailInfo);
		}
	}
}
