using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgContactUniqueNameHelperTest : TestCaseWithFactory
	{
		public void TestGenerateUniqueContactName_WithNameDictionary()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User 1 (1)";

			var contactNameDict = new Dictionary<string, ZGuid>
			{
				{ contact1.OC_ContactName, contact1.PK },
				{ contact2.OC_ContactName, contact2.PK }
			};

			var uniqueName = OrgContactUniqueNameHelper.GenerateUniqueContactName(contactNameDict, "User 2", null);
			AssertEquals("User 2", uniqueName);

			uniqueName = OrgContactUniqueNameHelper.GenerateUniqueContactName(contactNameDict, "User 1", null);
			AssertEquals("User 1 (2)", uniqueName);

			uniqueName = OrgContactUniqueNameHelper.GenerateUniqueContactName(contactNameDict, "User 1", contact1);
			AssertEquals("User 1", uniqueName);

			uniqueName = OrgContactUniqueNameHelper.GenerateUniqueContactName(contactNameDict, "User 1", contact2);
			AssertEquals("User 1 (1)", uniqueName);

			var contact3 = org.Contacts.AddNew();
			uniqueName = OrgContactUniqueNameHelper.GenerateUniqueContactName(contactNameDict, "User 1", contact3);
			AssertEquals("User 1 (2)", uniqueName);
		}

		public void TestGenerateUniqueContactName_WithExcludingContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User 1 (1)";

			var uniqueName = OrgContactUniqueNameHelper.GenerateUniqueContactName(contact1, "User 2");
			AssertEquals("User 2", uniqueName);

			uniqueName = OrgContactUniqueNameHelper.GenerateUniqueContactName(contact1, "User 1");
			AssertEquals("User 1", uniqueName);

			uniqueName = OrgContactUniqueNameHelper.GenerateUniqueContactName(contact2, "User 1");
			AssertEquals("User 1 (1)", uniqueName);

			uniqueName = OrgContactUniqueNameHelper.GenerateUniqueContactName(contact2, "User 1 (1)");
			AssertEquals("User 1 (1)", uniqueName);

			var contact3 = org.Contacts.AddNew();
			uniqueName = OrgContactUniqueNameHelper.GenerateUniqueContactName(contact3, "User 1");
			AssertEquals("User 1 (2)", uniqueName);
		}

		public void TestGenerateUniqueContactName_WithoutLoadingAllContacts()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "User 1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "User 1 (1)";

			for (int i = 0; i < 10; i++)
			{
				var contact = org.Contacts.AddNew();
				contact.OC_ContactName = $"Contact {i}";
			}

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var orgInAnotherFactory = anotherFactory.Load<OrgHeader>(org.PK);

			var uniqueName = OrgContactUniqueNameHelper.GenerateUniqueContactName(orgInAnotherFactory, null, "User 1");
			AssertEquals("User 1 (2)", uniqueName);
			AssertEquals("Not all contacts are loaded", 2, ((IBusinessObjectFactoryInternals)anotherFactory).AllBusinessObjects.Count(x => (x is OrgContact)));
		}

		public void TestGetNameWithoutNumberSuffix()
		{
			var name = "Sam";
			AssertEquals("Sam", OrgContactUniqueNameHelper.GetNameWithoutNumberSuffix(name));

			name = "Sam (1)";
			AssertEquals("Sam", OrgContactUniqueNameHelper.GetNameWithoutNumberSuffix(name));

			name = "Sam(0)";
			AssertEquals("Sam", OrgContactUniqueNameHelper.GetNameWithoutNumberSuffix(name));

			name = "Sam (999)";
			AssertEquals("Sam", OrgContactUniqueNameHelper.GetNameWithoutNumberSuffix(name));

			name = "Sam (10086)";
			AssertEquals("Sam (10086)", OrgContactUniqueNameHelper.GetNameWithoutNumberSuffix(name));

			name = "Sam ()";
			AssertEquals("Sam ()", OrgContactUniqueNameHelper.GetNameWithoutNumberSuffix(name));
		}
	}
}
