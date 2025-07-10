using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgDocumentCopyRecipientLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestODR_AvailableEmailAddress_List()
		{
			// Arrange
			var orgDocument = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument.OD_OC = Contact.PK;
			var orgDocumentCopyRecipient = Factory.NewWithValidTestData<OrgDocumentCopyRecipient>();
			orgDocumentCopyRecipient.ODR_OD = orgDocument.PK;
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_Email = "test2@test.com";
			contact2.OC_OH = Organization.PK;
			// Act
			var availableEmailAddresses = orgDocumentCopyRecipient.Lookups.ODR_AvailableEmailAddress_List.ToArray();
			// Assert
			AssertEquals(2, availableEmailAddresses.Length);
			AssertCollectionContains(availableEmailAddresses, cdp => cdp.Code == Contact.Email);
			AssertCollectionContains(availableEmailAddresses, cdp => cdp.Code == contact2.Email);
		}

		#region Implementations

		OrgContact Contact
		{
			get
			{
				if (contact == null)
				{
					contact = Organization.Contacts.AddNew();
					contact.OC_Email = "test1@test.com";
					Organization.Contacts.Add(contact);
				}
				return contact;
			}
		}

		OrgContact contact;

		OrgHeader Organization
		{
			get { return organization ?? (organization = Factory.NewWithValidTestData<OrgHeader>()); }
		}

		OrgHeader organization;

		#endregion
	}
}