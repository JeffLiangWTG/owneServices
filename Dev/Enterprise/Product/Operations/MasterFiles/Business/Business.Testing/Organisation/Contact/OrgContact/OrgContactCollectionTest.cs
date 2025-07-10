using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgContactCollection))]
	sealed class OrgContactCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetContactFromCode_UseOrgCodeFilter_True()
		{
			CombineAssertions(() =>
			{
				var collection = new OrgContactCollection(Factory) { UseOrgCodeFilter = true };

				AssertFindByCodeWorks(collection, "Simple case - Should be found", "TEMPLE", "Indiana Jones", "Indiana Jones (TEMPLE)", shouldBeFound: true);
				AssertFindByCodeWorks(collection, "Contact name also has brackets - Should be found", "LOSTARK", "Indiana (Jones)", "Indiana (Jones) (LOSTARK)", shouldBeFound: true);
				AssertFindByCodeWorks(collection, "Org code is missing - search by name instead", "LSTCRU", "Indiana Junes", "Indiana Junes", shouldBeFound: true);
				AssertFindByCodeWorks(collection, "Contact name has brackets, but since the ORG doesn't exist we use full name", "CRSKULL", "Indiana (Junes)", "Indiana (Junes)", shouldBeFound: true);
			});
		}

		public void TestGetContactFromCode_UseOrgCodeFilter_False()
		{
			CombineAssertions(() =>
			{
				var collection = new OrgContactCollection(Factory) { UseOrgCodeFilter = false };

				AssertFindByCodeWorks(collection, "Without orgcode filtering we should revert to regular code lookups. So codes with the Org in them will not work.", "TEMPLE", "Indiana Jones", "Indiana Jones (TEMPLE)", shouldBeFound: false);
				AssertFindByCodeWorks(collection, "Without orgcode filtering we should revert to regular code lookups. So codes with the Org in them will not work.", "LOSTARK", "Indiana (Jones)", "Indiana (Jones) (LOSTARK)", shouldBeFound: false);

				AssertFindByCodeWorks(collection, "Org code is missing - search by name instead", "LSTCRU", "Indiana Junes", "Indiana Junes", shouldBeFound: true);
				AssertFindByCodeWorks(collection, "Contact name has brackets, but since the ORG doesn't exist we use full name", "CRSKULL", "Indiana (Junes)", "Indiana (Junes)", shouldBeFound: true);
			});
		}

		void AssertFindByCodeWorks(IFindBoxListProvider provider, string message, string orgCode, string contactName, string searchCode, bool shouldBeFound)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = orgCode;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_ContactName = contactName;

			var result = (OrgContact)provider.GetBusinessObjectFromCode(searchCode);
			if (shouldBeFound)
			{
				AssertEquals(message, contact, result);
			}
			else
			{
				AssertNull(message, result);
			}
		}

		public void TestRemoveDuplicatesBasedOnEmailAddress()
		{
			OrgContactCollection contacts = new OrgContactCollection(Factory);

			OrgContact contact1 = contacts.AddNew();
			contact1.OC_ContactName = "Contact 1";
			contact1.OC_Email = "test@example.com";

			OrgContact contact2 = contacts.AddNew();
			contact2.OC_ContactName = "Contact 2";
			contact2.OC_Email = "other@nowhere.com";

			OrgContact contact3 = contacts.AddNew();
			contact3.OC_ContactName = "Contact 3";
			contact3.OC_Email = "test@example.com";

			OrgContact contact4 = contacts.AddNew();
			contact4.OC_ContactName = "Contact 4";
			contact4.OC_Email = "test@example.com";

			OrgContact contact5 = contacts.AddNew();
			contact5.OC_ContactName = "Contact 5";
			contact5.OC_Email = "other@nowhere.com";

			OrgContact contact6 = contacts.AddNew();
			contact6.OC_ContactName = "Contact 6";
			contact6.OC_Email = "";

			OrgContact contact7 = contacts.AddNew();
			contact7.OC_ContactName = "Contact 7";
			contact7.OC_Email = "";

			AssertEquals(7, contacts.Count);

			contacts.RemoveDuplicatesBasedOnEmailAddress();

			AssertEquals(4, contacts.Count);
			AssertEquals(contact4.PK, contacts[0].PK);
			AssertEquals(contact5.PK, contacts[1].PK);
			AssertEquals(contact6.PK, contacts[2].PK);
			AssertEquals(contact7.PK, contacts[3].PK);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgContactCollection(Factory);
		}
	}
}
