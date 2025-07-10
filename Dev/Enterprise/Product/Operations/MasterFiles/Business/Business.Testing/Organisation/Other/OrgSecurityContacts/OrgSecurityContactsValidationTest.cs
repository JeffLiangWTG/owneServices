using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSecurityContactsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestGrantedWhenNoWebAccess()
		{
			OrgHeader org = OrgHeader.New(Factory);
			OrgContact contact = org.Contacts.AddNew();

			AssertNoErrors("Precondition: No errors on the granted flag", contact.SecurityRightsForBindingOnly[0].OZ_GrantedInfo);

			contact.SecurityRightsForBindingOnly[0].OZ_Granted = true;
			contact.SecurityRightsForBindingOnly[0].Validation.ValidateOZ_Granted();
			AssertHasErrors("Errors on the granted flag as the contact does not have web access", contact.SecurityRightsForBindingOnly[0].OZ_GrantedInfo);

			contact.OC_WebAccessEnabled = true;
			contact.SecurityRightsForBindingOnly[0].Validation.ValidateOZ_Granted();
			AssertNoErrors("No errors on the granted flag as a password is now specified", contact.SecurityRightsForBindingOnly[0].OZ_GrantedInfo);
		}
	}
}
