using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(FilteredContactsCollection))]
	sealed class FilteredContactsCollectionTest : BusinessObjectCollectionViewTestCase<FilteredContactsCollection>
	{
		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			AssertEquals(true, collection.AllowNew);

			Env.Security.OrgContactNew.IsAllowed = false;
			AssertEquals(false, collection.AllowNew);
		}

		protected override FilteredContactsCollection GetCollectionToTest()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.Contacts.AddNew();
			return org.FilteredContacts;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrgContact>();
		}

		public void TestUnFormattedFilterString()
		{
			var collection = GetCollectionToTest();
			AssertUnFormattedFilterString(collection, "-", "");
			AssertUnFormattedFilterString(collection, "+", "+");
			AssertUnFormattedFilterString(collection, "1+", "1");
			AssertUnFormattedFilterString(collection, "@1+", "1");
			AssertUnFormattedFilterString(collection, "+&1+", "+1");
			AssertUnFormattedFilterString(collection, "+1234", "+1234");
			AssertUnFormattedFilterString(collection, "+12 34", "+1234");
			AssertUnFormattedFilterString(collection, "12#34*5 6", "123456");
			AssertUnFormattedFilterString(collection, "+12 34 5 6", "+123456");
			AssertUnFormattedFilterString(collection, "+12(34_5-6", "+123456");
			AssertUnFormattedFilterString(collection, ">a~bcd!efg<", "abcdefg");
			AssertUnFormattedFilterString(collection, ">*&^*!~:<])#*(@()+_=", "");
			AssertUnFormattedFilterString(collection, "86 159 15a4 4239", "8615915a44239");
			AssertUnFormattedFilterString(collection, "86 159 1514 4239", "8615915144239");
			AssertUnFormattedFilterString(collection, "+86 159 1514 4239", "+8615915144239");
		}

		void AssertUnFormattedFilterString(FilteredContactsCollection collection, string filterString, string expectedUnFormattedFilterString)
		{
			collection.FilterString = filterString;
			AssertEquals(collection.unFormattedFilterString, expectedUnFormattedFilterString);
		}

		public void TestFilterWebAccessEnabledContacts()
		{
			var org = Factory.New<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_ContactName = "contact1";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_WebAccessEnabled = false;
			contact2.OC_ContactName = "contact2";

			var collection = org.FilteredContacts;
			org.OnlyShowWebAccessEnabledContacts = true;
			AssertEquals(1, collection.Count);
			AssertEquals("contact1", collection[0].OC_ContactName);
		}
	}
}
