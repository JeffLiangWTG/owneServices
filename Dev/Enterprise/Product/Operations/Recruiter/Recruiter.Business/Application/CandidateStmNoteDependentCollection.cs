using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Recruiter.Business
{
	class CandidateStmNoteDependentCollection : StmNoteCollection
	{
		public CandidateStmNoteDependentCollection(IStmNoteParent parentBizO, BusinessObjectFactory factory)
			: base(parentBizO, factory)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(CandidateStmNote);
		}
	}
}
