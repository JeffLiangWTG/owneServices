using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(NonPersistentCopyRecipientCollection))]
	sealed class NonPersistentCopyRecipientCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NonPersistentCopyRecipientCollection>
	{
		public override void TestAdd()
		{
			base.TestAdd();
			Assert(Constants.CopyRecipientType.CarbonCopyRecipient, Collection.All(npcr => ((NonPersistentCopyRecipient)npcr).Type == Constants.CopyRecipientType.CarbonCopyRecipient));
		}

		public void TestUpdatedEventIsFired()
		{
			// Arrange
			bool isUpdatedFired;
			NonPersistentCopyRecipientCollection collection = Collection;
			collection.Updated += (sender, args) => isUpdatedFired = true;
			// Act
			isUpdatedFired = false;
			NonPersistentCopyRecipient copyRecipient = new NonPersistentCopyRecipient(Organization, Constants.CopyRecipientType.CarbonCopyRecipient);
			collection.Add(copyRecipient);
			// Assert
			Assert("Updated event should be fired when adding an element to the collection", isUpdatedFired);

			// Arrange
			isUpdatedFired = false;
			// Act
			Collection.RemoveAndDelete(copyRecipient);
			// Assert
			Assert("Updated event should be fired when deleting an element from the collection", isUpdatedFired);
		}

		public void TestValue()
		{
			// Arrange
			NonPersistentCopyRecipientCollection collection = Collection;
			AssertEquals("Precondition - The initial value should be empty.", ZString.Empty, collection.Value);
			// Act
			collection.Value = "test1@test.com, test2@test.com";
			// Assert
			AssertEquals("There should be two copy recipients created.", 2, collection.Count);
			AssertEquals("test1@test.com", collection[0].EmailAddress);
			AssertEquals("test2@test.com", collection[1].EmailAddress);
		}

		public void TestUpdatingOrganizationsIsSynchronized()
		{
			// Arrange
			NonPersistentCopyRecipientCollection collection = Collection;
			var copyRecipient = collection.AddNew();
			AssertEquals("Precondition - The new copy recipient should belong to the original organization.", Organization, copyRecipient.Organization);
			OrgHeader newOrganization = Factory.NewWithValidTestData<OrgHeader>();
			// Act
			collection.Organization = newOrganization;
			// Assert
			AssertEquals("The new copy recipient should belong to the new organization.", newOrganization, copyRecipient.Organization);
		}

		protected override NonPersistentCopyRecipientCollection GetCollectionToTest()
		{
			return new NonPersistentCopyRecipientCollection(Organization, Constants.CopyRecipientType.CarbonCopyRecipient);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NonPersistentCopyRecipient(Organization, Constants.CopyRecipientType.CarbonCopyRecipient);
		}

		OrgHeader Organization
		{
			get { return organization ?? (organization = Factory.NewWithValidTestData<OrgHeader>()); }
		}

		OrgHeader organization;
	}
}
