using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgDocumentCopyRecipientCollection))]
	public class OrgDocumentCopyRecipientCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgDocumentCopyRecipientCollection>
	{
		public override void TestAdd()
		{
			base.TestAdd();
			Assert("CC", Collection.All(c => c.ODR_RecipientType == Constants.CopyRecipientType.CarbonCopyRecipient));
		}

		public void TestUpdatedEventIsFired()
		{
			// Arrange
			bool isUpdatedFired;
			Collection.Updated += (sender, args) => isUpdatedFired = true;
			// Act
			isUpdatedFired = false;
			var orgDocumentCopyRecipient = Collection.AddNew();
			orgDocumentCopyRecipient.ODR_EmailAddress = "test@test.com";
			// Assert
			Assert("Updated event should be fired when adding an element to the collection", isUpdatedFired);

			// Arrange
			isUpdatedFired = false;
			// Act
			Collection.Delete(orgDocumentCopyRecipient);
			// Assert
			Assert("Updated event should be fired when deleting an element from the collection", isUpdatedFired);
		}

		public void TestValue()
		{
			// Arrange
			AssertEquals("Precondition - The initial value should be empty.", ZString.Empty, Collection.Value);
			// Act
			Collection.Value = "test1@test.com, test2@test.com";
			// Assert
			AssertEquals("There should be two copy recipients created.", 2, Collection.Count);
			AssertEquals("test1@test.com", Collection[0].ODR_EmailAddress);
			AssertEquals("test2@test.com", Collection[1].ODR_EmailAddress);
		}

		#region Implementations

		protected override OrgDocumentCopyRecipientCollection GetCollectionToTest()
		{
			var orgDocument = Factory.NewWithValidTestData<OrgDocument>();
			return new OrgDocumentCopyRecipientCollection(orgDocument, Constants.CopyRecipientType.CarbonCopyRecipient);
		}

		#endregion
	}
}
