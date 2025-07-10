using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Business
{
	class CandidateStmNoteCollectionWithRelatedElements : StmNoteCollectionWithRelatedElements
	{
		public CandidateStmNoteCollectionWithRelatedElements(IStmNoteParent parentBizO)
			: base(parentBizO)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(CandidateStmNote);
		}
	}
}
