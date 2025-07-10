using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Business
{
	public class HRCandidateNotes : Notes
	{
		public HRCandidateNotes(IStmNoteParent parentBizO)
			: base(parentBizO)
		{
		}

		protected override Type ElementType
		{
			get { return typeof(CandidateStmNote); }
		}

		protected override BusinessObjectCollection GetNewElementsCollection()
		{
			return new CandidateStmNoteDependentCollection(Parent, ((BusinessObject)Parent).Factory);
		}

		protected override BusinessObjectCollection GetNewAllElementsCollection()
		{
			return new CandidateStmNoteCollectionWithRelatedElements(Parent);
		}

		protected override IBusinessObjectCollectionView GetNewVisibleElementsCollectionView()
		{
			return new CandidateStmNoteCollectionView(Parent);
		}
	}
}
