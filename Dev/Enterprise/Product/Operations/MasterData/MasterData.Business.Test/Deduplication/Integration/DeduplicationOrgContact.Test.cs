using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business.Tests
{
	public class DeduplicationOrgContactTest : TestCaseWithFactory
	{
		public void TestCertificateCollectionsInitializedAsEmptyCollections()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var dedupeContact = new DeduplicationOrgContact(contact, true);

			AssertEquals("Certificate collection should initially be empty", 0, dedupeContact.CertificateOrAccreditations.Count);
		}

		public void TestCertificatesCollectionPopulatedCorrectly()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			var cert1 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			var cert2 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			cert1.XZ_ParentID = contact.PK;
			cert1.XZ_ParentTableCode = OrgContactSchema.Constants.Prefix;
			cert2.XZ_ParentID = contact.PK;
			cert2.XZ_ParentTableCode = OrgContactSchema.Constants.Prefix;

			var dedupeContact = new DeduplicationOrgContact(contact, true);

			AssertContainsExactElementsInAnyOrder("Certificates should match", new[] { cert1, cert2 }.Select(x => x.PK), dedupeContact.CertificateOrAccreditations.Select(x => x.XZ_PK));
		}

		public void TestContactItemsCollectionsInitializedAsEmptyCollections()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var dedupeContact = new DeduplicationOrgContact(contact, true);

			AssertEquals("Certificate collection should initially be empty", 0, dedupeContact.CertificateOrAccreditations.Count);
		}

		public void TestContactItemsCollectionPopulatedCorrectly()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();

			var item1 = Factory.NewWithValidTestData<OrgContactItem>();
			var item2 = Factory.NewWithValidTestData<OrgContactItem>();
			item1.OI_OC = contact.PK;
			item1.OI_ContactItemType = "PHN";
			item1.OI_Description = "MOB1";
			item1.OI_Address = "1234 5678";
			item2.OI_OC = contact.PK;
			item2.OI_ContactItemType = "PHN";
			item2.OI_Description = "MOB2";
			item2.OI_Address = "5678 9876";

			var dedupeContact = new DeduplicationOrgContact(contact, true);

			AssertContainsExactElementsInAnyOrder("Contact items should match", new[] { item1, item2 }.Select(x => x.PK), dedupeContact.OrgContactItems.Select(x => x.OI_PK));
		}

		public void TestIPersonPropertiesImplementedCorrectly()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var dedupeContact = new DeduplicationOrgContact(contact, true);

			AssertEquals("AddressFull", dedupeContact.AddressFull.ColName);
			AssertEquals("OC_Birthday", dedupeContact.BirthDate.ColName);

			AssertEquals(2, dedupeContact.Address.Count);
			AssertEquals("OA_Address1", dedupeContact.Address[0].ColName);
			AssertEquals("OA_Address2", dedupeContact.Address[1].ColName);

			AssertEquals(1, dedupeContact.EmailAddresses.Count);
			AssertEquals("OC_Email", dedupeContact.EmailAddresses[0].ColName);

			AssertEquals("OC_ContactName", dedupeContact.FullName.ColName);

			AssertEquals(5, dedupeContact.PhoneNumbers.Count);
			AssertEquals("OC_Fax", dedupeContact.PhoneNumbers[0].ColName);
			AssertEquals("OC_HomePhone", dedupeContact.PhoneNumbers[1].ColName);
			AssertEquals("OC_Mobile", dedupeContact.PhoneNumbers[2].ColName);
			AssertEquals("OC_OtherPhone", dedupeContact.PhoneNumbers[3].ColName);
			AssertEquals("OC_Phone", dedupeContact.PhoneNumbers[4].ColName);

			AssertEquals("CargoWise.Glow.Model.Interfaces.IOrgContact", dedupeContact.GlowType.FullName);
		}

		public void TestConstructorSetRawNameCorrectly()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "ABC";
			var dedupContact = new DeduplicationOrgContact(contact, true);
			AssertEquals("ABC", dedupContact.OC_ContactName);
			AssertEquals("ABC", dedupContact.RawName);

			dedupContact = new DeduplicationOrgContact("EFG");
			AssertEquals("EFG", dedupContact.OC_ContactName);
			AssertEquals("EFG", dedupContact.RawName);
		}
	}
}
