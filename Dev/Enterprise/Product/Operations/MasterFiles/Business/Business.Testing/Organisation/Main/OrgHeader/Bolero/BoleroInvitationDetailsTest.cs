using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(BoleroInvitationDetails))]
	public class BoleroInvitationDetailsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new BoleroInvitationDetails(Factory.New<OrgHeader>());

		public void TestActiveContacts()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_IsActive = true;
			var contact2 = org.Contacts.AddNew();
			contact2.OC_IsActive = false;
			var contact3 = org.Contacts.AddNew();
			contact3.OC_IsActive = true;
			var contact4 = Factory.NewWithValidTestData<OrgContact>();
			contact4.OC_IsActive = true;

			var invitationDetails = new BoleroInvitationDetails(org);
			AssertContainsExactElementsInAnyOrder(new[] { contact1, contact3 }, invitationDetails.ActiveContacts);
		}

		public void TestSelectedEmailPerSelectedContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var invitationDetails = new BoleroInvitationDetails(org);

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "TestUser@123.com";

			invitationDetails.SelectedContactPK = contact.PK;
			AssertEquals("TestUser@123.com", invitationDetails.SelectedContactEmail);

			contact.OC_Email = string.Empty;
			AssertEquals(string.Empty, invitationDetails.SelectedContactEmail);
		}

		public void TestYourNameAndEmail()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var invitationDetails = new BoleroInvitationDetails(org);

			AssertEquals(Env.CurrentUser.FullName, invitationDetails.YourName);
			AssertEquals(Env.CurrentUser.EmailAddress, invitationDetails.YourEmail);
		}
	}
}
