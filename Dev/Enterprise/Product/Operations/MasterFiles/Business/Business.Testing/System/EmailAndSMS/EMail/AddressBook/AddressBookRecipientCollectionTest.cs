using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AddressBookRecipientCollection))]
	sealed class AddressBookRecipientCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AddressBookRecipientCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			IAddressBookRecipient recipient = Factory.New<OrgContact>();
			return new AddressBookRecipient(recipient);
		}

		protected override AddressBookRecipientCollection GetCollectionToTest()
		{
			return new AddressBookRecipientCollection(Factory);
		}

		public void TestContains()
		{
			var contact1 = Factory.New<OrgContact>();
			contact1.OC_ContactName = "Jenny";
			var recipient1 = new AddressBookRecipient(contact1);
			var contact2 = Factory.New<OrgContact>();
			contact2.OC_ContactName = "Nguyen";
			var recipient2 = new AddressBookRecipient(contact2);

			var collection = new AddressBookRecipientCollection(Factory);
			collection.Add(recipient1);
			collection.Add(recipient2);

			AssertEquals(2, collection.Count);
			AssertEquals(true, collection.Contains(contact1));
			AssertEquals(true, collection.Contains(contact2));
		}

		public override void TestAdd()
		{
			var contact1 = Factory.New<OrgContact>();
			contact1.OC_ContactName = "Jenny";
			var recipient1 = new AddressBookRecipient(contact1);
			var contact2 = Factory.New<OrgContact>();
			contact2.OC_ContactName = "Nguyen";
			var recipient2 = new AddressBookRecipient(contact2);

			var collection = new AddressBookRecipientCollection(Factory);
			AssertEquals(0, collection.Count);
			collection.Add(recipient1);
			AssertEquals(1, collection.Count);
			collection.Add(recipient2);
			AssertEquals(2, collection.Count);

			AssertEquals(true, collection.Contains(recipient1));
			AssertEquals(true, collection.Contains(recipient2));
		}

		public override void TestDelete()
		{
			var contact1 = Factory.New<OrgContact>();
			contact1.OC_ContactName = "Jenny";
			var recipient1 = new AddressBookRecipient(contact1);
			var contact2 = Factory.New<OrgContact>();
			contact2.OC_ContactName = "Nguyen";
			var recipient2 = new AddressBookRecipient(contact2);

			var collection = new AddressBookRecipientCollection(Factory);
			AssertEquals(0, collection.Count);
			collection.Add(recipient1);
			AssertEquals(1, collection.Count);
			collection.Add(recipient2);
			AssertEquals(2, collection.Count);

			AssertEquals(true, collection.Contains(recipient1));
			AssertEquals(true, collection.Contains(recipient2));

			collection.RemoveAndDelete(recipient1);
			AssertEquals(1, collection.Count);
			AssertEquals(true, collection.Contains(recipient2));
			Assert("recipient1 is deleted", recipient1.IsDeleted);
			Assert("contact1, which created recipient1, is not deleted", !contact1.IsDeleted);
		}

		public override void TestRemoveFromRelationship()
		{
			var contact1 = Factory.New<OrgContact>();
			contact1.OC_ContactName = "Jenny";
			var recipient1 = new AddressBookRecipient(contact1);
			var contact2 = Factory.New<OrgContact>();
			contact2.OC_ContactName = "Nguyen";
			var recipient2 = new AddressBookRecipient(contact2);

			var collection = new AddressBookRecipientCollection(Factory);
			AssertEquals(0, collection.Count);
			collection.Add(recipient1);
			AssertEquals(1, collection.Count);
			collection.Add(recipient2);
			AssertEquals(2, collection.Count);

			AssertEquals(true, collection.Contains(recipient1));
			AssertEquals(true, collection.Contains(recipient2));

			collection.Remove(recipient1);
			AssertEquals(1, collection.Count);
			AssertEquals(true, collection.Contains(recipient2));
			Assert("recipient1 is removed but not deleted", !recipient1.IsDeleted);
			Assert("contact1, which created recipient1, is not deleted", !contact1.IsDeleted);
		}
	}
}
