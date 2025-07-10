using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Business
{
	internal class CandidateStmNoteCollectionView : StmNoteCollectionView
	{
		public CandidateStmNoteCollectionView(IStmNoteParent parentBizO)
			: base(parentBizO, typeof(CandidateStmNote))
		{
		}
	}
}
