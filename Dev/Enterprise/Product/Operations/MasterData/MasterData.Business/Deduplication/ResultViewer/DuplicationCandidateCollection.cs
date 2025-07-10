using CargoWise.EntityFramework;

namespace Enterprise.MasterData.Business
{
	public class DuplicationCandidateCollection<TCandidate> : NonPersistentBusinessObjectCollection<TCandidate>
		where TCandidate : DuplicationCandidate
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotSupportedException();
		}

		protected override bool AllowNewCore => false;
	}
}
