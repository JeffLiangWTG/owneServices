using System.Collections.Generic;
using Enterprise.MasterData.Business;

namespace Enterprise.MasterData.GUI
{
	public interface IDeduplicationDataSource
	{
		IEnumerable<PotentialDuplicationModel> GetPotentialDuplicates();

		IEnumerable<DuplicationCandidate> GetDuplicationCandidates();
	}
}
