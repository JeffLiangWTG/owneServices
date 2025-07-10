using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRCandidateNotes))]
	class HRCandidateNoteTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHRCandidateNotes()
		{
			var parent = Factory.NewWithValidTestData<HRJobApplication>();
			var notes = new HRCandidateNotes(parent);
			var note1 = notes.AddNew();

			AssertEquals("Note type", typeof(CandidateStmNote), note1.GetType());
			AssertEquals("Visible Note type", typeof(CandidateStmNote), notes.VisibleNotes.TypeOfElements);
			AssertEquals("All Note type", typeof(CandidateStmNote), notes.GetAllNotes().TypeOfElements);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var parent = Factory.NewWithValidTestData<HRJobApplication>();
			return new HRCandidateNotes(parent);
		}

		#endregion
	}
}
