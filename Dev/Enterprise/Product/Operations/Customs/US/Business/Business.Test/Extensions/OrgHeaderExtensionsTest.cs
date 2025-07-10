using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.OrgConstants;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class OrgHeaderExtensionsTest : TestCase
	{
		public void TestGetContactDetails()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = factory.New<OrgHeader>();
			AssertEquals(null, orgHeader.GetContactDetails(new string[] { ContactAllocationType.USPGA }));

			DeclarationTestHelper.AddPGAContact(orgHeader, "jonh", "smith", "0123456789", "jonh.smith@gmail.com", "0123456788", false);

			var contactDetails = orgHeader.GetContactDetails(new string[] { ContactAllocationType.USPGA });
			AssertNull(contactDetails);

			DeclarationTestHelper.AddPGAContact(orgHeader, "jonh", "smith", "0123456789", "jonh.smith@gmail.com", "0123456788", true);

			contactDetails = orgHeader.GetContactDetails(new string[] { ContactAllocationType.USPGA });
			AssertEquals("jonh smith", contactDetails.Name);
			AssertEquals("0123456789", contactDetails.PhoneNumber);
			AssertEquals("jonh.smith@gmail.com", contactDetails.EmailAddress);
			AssertEquals("0123456788", contactDetails.Fax);

			DeclarationTestHelper.AddFSVPContact(orgHeader.MainAddress, "mike", "jordon", "1123456789", "mike.jordon@gmail.com", "1123456788", false);

			contactDetails = orgHeader.GetContactDetails(new string[] { ContactAllocationType.USPGA });
			AssertEquals("jonh smith", contactDetails.Name);
			AssertEquals("0123456789", contactDetails.PhoneNumber);
			AssertEquals("jonh.smith@gmail.com", contactDetails.EmailAddress);
			AssertEquals("0123456788", contactDetails.Fax);

			DeclarationTestHelper.AddFSVPContact(orgHeader.MainAddress, "mike", "jordon", "1123456789", "mike.jordon@gmail.com", "1123456788", true);

			contactDetails = orgHeader.GetContactDetails(new string[] { ContactAllocationType.USFSV, ContactAllocationType.USPGA });
			AssertEquals("mike jordon", contactDetails.Name);
			AssertEquals("1123456789", contactDetails.PhoneNumber);
			AssertEquals("mike.jordon@gmail.com", contactDetails.EmailAddress);
			AssertEquals("1123456788", contactDetails.Fax);
		}
	}
}
