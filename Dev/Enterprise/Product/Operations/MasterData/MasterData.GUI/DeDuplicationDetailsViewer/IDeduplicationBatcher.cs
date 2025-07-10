using System.Collections.Generic;
using CargoWise.Tools.DuplicateDetector;

namespace Enterprise.MasterData.GUI
{
	public interface IDeduplicationBatcher
	{
		List<ScoringResult> ScoreResults();
	}
}
