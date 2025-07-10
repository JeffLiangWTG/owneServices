using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocumentDeliveryCopyRecipientCollection))]
	public class JobDocumentDeliveryCopyRecipientCollectionTest : ActiveBusinessObjectCollectionTestCase<JobDocumentDeliveryCopyRecipientCollection>
	{
		public override void TestAdd()
		{
			base.TestAdd();
			Assert("TO", Collection.All(c => c.JDR_RecipientType == Constants.CopyRecipientType.EmailToRecipient));
		}

		public void TestUpdatedEventIsFired()
		{
			bool isUpdatedFired;
			Collection.Updated += (sender, args) => isUpdatedFired = true;
			isUpdatedFired = false;

			var copyRecipient = Collection.AddNew();
			copyRecipient.JDR_EmailAddress = "dexter@morgan.com";
			Assert("Updated event should be fired when adding an element to the collection", isUpdatedFired);

			isUpdatedFired = false;
			Collection.Delete(copyRecipient);
			Assert("Updated event should be fired when deleting an element from the collection", isUpdatedFired);
		}

		public void TestValue()
		{
			AssertEquals("Precondition - The initial value should be empty.", ZString.Empty, Collection.Value);

			Collection.Value = "dexter@morgan.com, debra@morgan.com";
			AssertEquals("There should be two copy recipients created.", 2, Collection.Count);
			AssertEquals("dexter@morgan.com", Collection[0].JDR_EmailAddress);
			AssertEquals("debra@morgan.com", Collection[1].JDR_EmailAddress);
		}

		#region Implementations

		protected override JobDocumentDeliveryCopyRecipientCollection GetCollectionToTest()
		{
			var jobDocumentDelivery = Factory.NewWithValidTestData<JobDocumentDelivery>();
			return new JobDocumentDeliveryCopyRecipientCollection(jobDocumentDelivery, Constants.CopyRecipientType.EmailToRecipient);
		}

		#endregion
	}
}
