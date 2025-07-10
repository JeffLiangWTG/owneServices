using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class DirtyRecordFinderTest : TestCaseWithFactory
	{
		public void TestIsOrgDirtyForDeduplication_Domains()
		{
			var org = ValidOrgForTest(Factory);
			var dirtyFinder = new DirtyRecordFinderForTest(org);
			AssertEquals("Precondition", false, dirtyFinder.IsOrgDirtyForDeduplication());
			AssertNotContains("Precondition", "Domain uniqueness ratio is too high", dirtyFinder.GetDirtyReason());

			org.Contacts[0].OC_Email = "abc@abc.com";
			org.Contacts[1].OC_Email = "def@def.com";
			org.Contacts[2].OC_Email = "ghi@ghi.com";
			org.Contacts[3].OC_Email = "jkl@jkl.com";
			AssertEquals(true, dirtyFinder.IsOrgDirtyForDeduplication());
			AssertContains("Domain uniqueness ratio is too high", dirtyFinder.GetDirtyReason());
		}

		public void TestIsOrgDirtyForDeduplication_Emails()
		{
			var org = ValidOrgForTest(Factory);
			var dirtyFinder = new DirtyRecordFinderForTest(org);
			AssertEquals("Precondition", false, dirtyFinder.IsOrgDirtyForDeduplication());
			AssertNotContains("Precondition", "Email uniqueness ratio is too low", dirtyFinder.GetDirtyReason());

			org.Contacts[0].OC_Email = "abc@abc.com";
			org.Contacts[1].OC_Email = "abc@abc.com";
			org.Contacts[2].OC_Email = "abc@abc.com";
			org.Contacts[3].OC_Email = "jkl@jkl.com";
			AssertEquals(true, dirtyFinder.IsOrgDirtyForDeduplication());
			AssertContains("Email uniqueness ratio is too low", dirtyFinder.GetDirtyReason());
		}

		public void TestIsOrgDirtyForDeduplication_Phones()
		{
			var org = ValidOrgForTest(Factory);
			var dirtyFinder = new DirtyRecordFinderForTest(org);
			AssertEquals("Precondition", false, dirtyFinder.IsOrgDirtyForDeduplication());
			AssertNotContains("Precondition", "Phone uniqueness ratio is too low", dirtyFinder.GetDirtyReason());

			org.Contacts[0].OC_Phone = "12345";
			org.Contacts[1].OC_Phone = "67890";
			org.Contacts[2].OC_Phone = "12345";
			org.Contacts[3].OC_Phone = "12345";
			AssertEquals(true, dirtyFinder.IsOrgDirtyForDeduplication());
			AssertContains("Phone uniqueness ratio is too low", dirtyFinder.GetDirtyReason());
		}

		public void TestIsOrgDirtyForDeduplication_EmptyFields()
		{
			var org = ValidOrgForTest(Factory);
			var dirtyFinder = new DirtyRecordFinderForTest(org);
			AssertEquals("Precondition", false, dirtyFinder.IsOrgDirtyForDeduplication());
			AssertNotContains("Precondition", "Non-empty fields ratio is too low", dirtyFinder.GetDirtyReason());

			org.Contacts[0].OC_Phone = "12345";
			org.Contacts[0].OC_Email = "";
			org.Contacts[1].OC_Phone = "";
			org.Contacts[1].OC_Email = "";
			org.Contacts[2].OC_Phone = "";
			org.Contacts[2].OC_Email = "";
			org.Contacts[3].OC_Phone = "";
			org.Contacts[3].OC_Email = "";
			AssertEquals(true, dirtyFinder.IsOrgDirtyForDeduplication());
			AssertContains("Non-empty fields ratio is too low", dirtyFinder.GetDirtyReason());
		}

		public void TestIsOrgDirtyForDeduplication_EmptyContacts()
		{
			var org = ValidOrgForTest(Factory);
			var dirtyFinder = new DirtyRecordFinderForTest(org);
			AssertEquals("Precondition", false, dirtyFinder.IsOrgDirtyForDeduplication());
			AssertNotContains("Precondition", "Contacts with insufficient details ratio is too high", dirtyFinder.GetDirtyReason());

			org.Contacts[0].OC_Phone = "";
			org.Contacts[0].OC_Email = "";
			org.Contacts[1].OC_Phone = "";
			org.Contacts[1].OC_Email = "";
			org.Contacts[2].OC_Phone = "";
			org.Contacts[2].OC_Email = "";
			org.Contacts[3].OC_Phone = "";
			org.Contacts[3].OC_Email = "";
			org.Contacts[4].OC_Phone = "";
			org.Contacts[4].OC_Email = "";

			AssertEquals(true, dirtyFinder.IsOrgDirtyForDeduplication());
			AssertContains("Contacts with insufficient details ratio is too high", dirtyFinder.GetDirtyReason());
		}

		public void TestIsOrgDirtyForDeduplication_MediumOrg()
		{
			var org = ValidOrgForTest(Factory);
			org.Contacts[4].Delete();
			Factory.Save();

			var dirtyFinder = new DirtyRecordFinderForTest(org);
			AssertEquals("Precondition", false, dirtyFinder.IsOrgDirtyForDeduplication());
			AssertNotContains("Precondition", "No contacts with sufficient details found", dirtyFinder.GetDirtyReason());

			org.Contacts[0].OC_Phone = "";
			org.Contacts[0].OC_Email = "";
			org.Contacts[1].OC_Phone = "";
			org.Contacts[1].OC_Email = "";
			org.Contacts[2].OC_Phone = "";
			org.Contacts[2].OC_Email = "";
			AssertEquals(false, dirtyFinder.IsOrgDirtyForDeduplication());
			AssertNotContains("No contacts with sufficient details found", dirtyFinder.GetDirtyReason());

			org.Contacts[3].OC_Phone = "";
			org.Contacts[3].OC_Email = "";
			AssertEquals(true, dirtyFinder.IsOrgDirtyForDeduplication());
			AssertContains("No contacts with sufficient details found", dirtyFinder.GetDirtyReason());
		}

		OrgHeader ValidOrgForTest(BusinessObjectFactory factory)
		{
			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABX";
			org.OH_FullName = "TOLL PTY LTD";
			org.OH_RL_NKClosestPort = "AUSYD";

			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Contact A";
			contact1.OC_Email = "abc@abc.com";
			contact1.OC_Phone = "1234";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Contact B";
			contact2.OC_Email = "def@abc.com";
			contact2.OC_Phone = "5678";

			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Contact C";
			contact3.OC_Email = "ghi@abc.com";
			contact3.OC_Phone = "9012";

			var contact4 = org.Contacts.AddNew();
			contact4.OC_ContactName = "Contact D";
			contact4.OC_Email = "jkl@abc.com";
			contact4.OC_Phone = "3456";

			var contact5 = org.Contacts.AddNew();
			contact5.OC_ContactName = "Contact E";
			contact5.OC_Email = "mno@abc.com";
			contact5.OC_Phone = "4567";

			Factory.Save();

			return org;
		}
	}

	public class DirtyRecordFinderForTest : DirtyRecordFinder
	{
		protected override int LargeOrgContactsThreshold => 4;
		protected override int MediumOrgContactsThreshold => 3;
		protected override int NumberOfEmailAddressesThreshold => 3;
		protected override int NumberOfPhoneNumbersThreshold => 3;

		public DirtyRecordFinderForTest(OrgHeader org) : base(org)
		{
		}
	}
}
