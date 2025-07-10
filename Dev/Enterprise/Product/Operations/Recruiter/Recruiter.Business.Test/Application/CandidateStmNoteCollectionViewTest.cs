using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(CandidateStmNoteCollectionView))]
	class CandidateStmNoteCollectionViewTest : BusinessObjectCollectionViewTestCase<CandidateStmNoteCollectionView>
	{
		protected override CandidateStmNoteCollectionView GetCollectionToTest()
		{
			return new CandidateStmNoteCollectionView(Application);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var note = Factory.New<CandidateStmNote>();
			note.ST_Table = Application.TableName;
			note.ST_ParentID = Application.PK;
			return note;
		}

		HRJobApplication Application => application ??= Factory.NewWithValidTestData<HRJobApplication>();
		HRJobApplication application;
	}
}
