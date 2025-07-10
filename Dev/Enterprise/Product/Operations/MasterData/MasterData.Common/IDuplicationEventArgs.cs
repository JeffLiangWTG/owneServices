using System.Collections.Generic;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;

namespace Enterprise.MasterData.Common
{
	public interface IDuplicationEventArgs
	{
		object Master { get; }
		object TargetObjects { get; }
		IEnumerable<ScoringResult> Results { get; }
		IEnumerable<PatternMatchingResultModel> ResultsModels { get; }
		ZGuid SelectedMasterPK { get; }
		DeduplicationAction InvokedAction { get; set; }
		bool IsExcludingOtherCountriesFromResults { get; set; }
		bool IsExcludingInactiveFromResults { get; set; }
		bool IsShowIgnoredFromResults { get; set; }
		bool IsTimeout { get; set; }
		bool IsErrorOccurred { get; set; }
	}
}
