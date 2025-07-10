using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	[TestedType(typeof(PersonMergeParticipants))]
	public class PersonMergeParticipantsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PersonMergeParticipants(new PersonMergeBusinessObjectCollection(), new PersonMergeBusinessObjectCollection());
		}

		public void TestPersonMergeParticipants()
		{
			var retainedCollection = new PersonMergeBusinessObjectCollection();
			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			var participants = new PersonMergeParticipants(retainedCollection, dissolvedCollection);

			AssertEquals(retainedCollection, participants.RetainedCollection);
			AssertEquals(dissolvedCollection, participants.DissolvedCollection);
		}

		public void TestPersonMergeParticipants_DefaultStatus()
		{
			AssertEquals("Queued", ParticipantStatus.Queued);
			AssertEquals("Merging", ParticipantStatus.Merging);
			AssertEquals("MergedWithErrors", ParticipantStatus.MergedWithErrors);
			AssertEquals("FailedWithCriticalError", ParticipantStatus.FailedWithCriticalError);
			AssertEquals("Completed", ParticipantStatus.Completed);
		}
	}
}
