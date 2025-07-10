using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class EmailDeliveryDetailsProviderTest : TestCaseWithFactory
	{
		public void TestProviderProperties()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ABCD";
			var contactA = org.Contacts.AddNew();
			contactA.OC_ContactName = "Contact A";
			contactA.OC_Email = "aa@test.com";

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem.G8_RecipientID = contactA.PK;
			campaignItem.G8_TrackingStatus = "NDR";
			var campaignItemNote = campaignItem.Notes.AddNew(true, "Email bounce back", ZString.Empty);
			campaignItemNote.ST_NoteDataAsText = "Email bounced back as result of sent campaign";

			var contactB = org.Contacts.AddNew();
			contactB.OC_ContactName = "Contact BB";
			contactB.OC_Email = "bb@test.org";
			contactB.IsNDR = true;
			var bbEmailAddressNote = contactB.EmailAddress.GlbEmailAddress.Notes.AddNew(true, "Email bounce back", ZString.Empty);
			bbEmailAddressNote.ST_NoteDataAsText = "Email bounced back as result of an invalid Contact Email";

			Factory.Save();

			EmailDeliveryDetailsProvider provider = new EmailDeliveryDetailsProvider(campaignItem);
			AssertEquals("BounceBackEmail", "Email bounced back as result of sent campaign", provider.BounceBackEmail);
			AssertEquals("ContactName", "Contact A", provider.ContactName);
			AssertEquals("Email", "aa@test.com", provider.EmailAddress);
			AssertEquals("TrackingStatusDescription", "Non-Delivery Receipt", provider.TrackingStatusDescription);

			provider = new EmailDeliveryDetailsProvider(contactB);
			AssertEquals("BounceBackEmail", "Email bounced back as result of an invalid Contact Email", provider.BounceBackEmail);
			AssertEquals("ContactName", "Contact BB", provider.ContactName);
			AssertEquals("Email", "bb@test.org", provider.EmailAddress);
			AssertEquals("TrackingStatusDescription", "Non-Delivery Receipt", provider.TrackingStatusDescription);
		}
	}
}
