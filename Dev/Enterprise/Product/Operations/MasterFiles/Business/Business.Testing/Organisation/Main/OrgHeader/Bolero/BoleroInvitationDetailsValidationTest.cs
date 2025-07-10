using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class BoleroInvitationDetailsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateSelectedContactPK()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_IsActive = true;

			var contact2 = org.Contacts.AddNew();
			contact2.OC_IsActive = false;

			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_IsActive = true;

			var boleroInvitationDetails = new BoleroInvitationDetails(org);
			boleroInvitationDetails.SelectedContactPK = contact1.PK;
			boleroInvitationDetails.Validation.ValidateSelectedContactPK();
			AssertNoErrors(boleroInvitationDetails.SelectedContactPKInfo);

			boleroInvitationDetails.SelectedContactPK = contact2.PK;
			boleroInvitationDetails.Validation.ValidateSelectedContactPK();
			AssertHasError(boleroInvitationDetails.SelectedContactPKInfo, "Enter a valid contact name.");

			boleroInvitationDetails.SelectedContactPK = contact3.PK;
			boleroInvitationDetails.Validation.ValidateSelectedContactPK();
			AssertHasError(boleroInvitationDetails.SelectedContactPKInfo, "Enter a valid contact name.");

			boleroInvitationDetails.SelectedContactPK = ZGuid.Empty;
			boleroInvitationDetails.Validation.ValidateSelectedContactPK();
			AssertHasError(boleroInvitationDetails.SelectedContactPKInfo, "Please enter a contact name.");
		}

		public void TestValidateSelectedContactEmail()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_IsActive = true;
			contact1.OC_Email = "TestUser@123.com";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_IsActive = true;
			contact2.OC_Email = string.Empty;

			var boleroInvitationDetails = new BoleroInvitationDetails(org);
			boleroInvitationDetails.SelectedContactPK = contact1.PK;
			boleroInvitationDetails.Validation.ValidateSelectedContactEmail();
			AssertNoErrors(boleroInvitationDetails.SelectedContactEmailInfo);

			boleroInvitationDetails.SelectedContactPK = contact2.PK;
			boleroInvitationDetails.Validation.ValidateSelectedContactEmail();
			AssertHasError(boleroInvitationDetails.SelectedContactEmailInfo, "Please enter a contact email address.");
		}

		public void TestValidateYourName()
		{
			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUserPK);
			currentUser.GS_FullName = "Test User";
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var boleroInvitationDetails = new BoleroInvitationDetails(org);
			boleroInvitationDetails.Validation.ValidateYourName();
			AssertNoErrors(boleroInvitationDetails.YourNameInfo);

			currentUser.GS_FullName = string.Empty;
			Factory.Save();
			boleroInvitationDetails.Validation.ValidateYourName();
			AssertHasError(boleroInvitationDetails.YourNameInfo, "Please enter a login user's name.");
		}

		public void TestValidateYourEmail()
		{
			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUserPK);
			currentUser.GS_EmailAddress = "TestUser@123.com";
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var boleroInvitationDetails = new BoleroInvitationDetails(org);
			boleroInvitationDetails.Validation.ValidateYourEmail();
			AssertNoErrors(boleroInvitationDetails.YourEmailInfo);

			currentUser.GS_EmailAddress = string.Empty;
			Factory.Save();
			boleroInvitationDetails.Validation.ValidateYourEmail();
			AssertHasError(boleroInvitationDetails.YourEmailInfo, "Please enter a login user's email address.");
		}

		public void TestValidateAll()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = string.Empty;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var boleroInvitationDetails = new BoleroInvitationDetails(org);

			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUserPK);
			currentUser.GS_FullName = string.Empty;
			currentUser.GS_EmailAddress = string.Empty;
			Factory.Save();

			boleroInvitationDetails.SelectedContactPK = contact.PK;
			boleroInvitationDetails.Validation.ValidateAll();

			AssertHasError(boleroInvitationDetails.SelectedContactPKInfo, "Enter a valid contact name.");
			AssertHasError(boleroInvitationDetails.SelectedContactEmailInfo, "Please enter a contact email address.");
			AssertHasError(boleroInvitationDetails.YourNameInfo, "Please enter a login user's name.");
			AssertHasError(boleroInvitationDetails.YourEmailInfo, "Please enter a login user's email address.");
		}
	}
}
