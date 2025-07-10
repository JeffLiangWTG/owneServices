using System;
using System.Collections.Generic;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public class DuplicationEventArgs : EventArgs, IDuplicationEventArgs
	{
		public DuplicationEventArgs(object master, object targetObjects, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModels, ZGuid selectedPK)
			: this(master, targetObjects, results, resultsModels)
		{
			SelectedMasterPK = selectedPK;
		}

		public DuplicationEventArgs(object master, object targetObjects, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModels, DuplicationStatus lastRunStatus, DeduplicationExclusionManager<OrgHeader> exclusionManager)
			: this(master, targetObjects, results, resultsModels)
		{
			IsErrorOccurred = lastRunStatus is DuplicationStatus.ErrorOccurred;
			IsTimeout = lastRunStatus is DuplicationStatus.Timeout;
			OrgHeaderExclusionManager = exclusionManager;
		}

		public DuplicationEventArgs(object master, object targetObjects, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModels, DuplicationStatus lastRunStatus, DeduplicationExclusionManager<GlbPerson> exclusionManager)
			: this(master, targetObjects, results, resultsModels)
		{
			IsErrorOccurred = lastRunStatus is DuplicationStatus.ErrorOccurred;
			IsTimeout = lastRunStatus is DuplicationStatus.Timeout;
			PersonExclusionManager = exclusionManager;
		}

		public DuplicationEventArgs(object master, object targetObjects, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModels)
			: this(resultsModels)
		{
			Results = results;
			Master = master;
			TargetObjects = targetObjects;
		}

		public DuplicationEventArgs(object master, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModels)
			: this(resultsModels)
		{
			Results = results;
			Master = master;
		}

		public DuplicationEventArgs(object master, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModels, ZGuid selectedPK)
			: this(master, results, resultsModels)
		{
			SelectedMasterPK = selectedPK;
		}

		public DuplicationEventArgs(IEnumerable<PatternMatchingResultModel> resultsModels)
		{
			ResultsModels = resultsModels;
		}

		public object Master { get; }
		public object TargetObjects { get; }
		public IEnumerable<ScoringResult> Results { get; }
		public IEnumerable<PatternMatchingResultModel> ResultsModels { get; }
		public ZGuid SelectedMasterPK { get; }
		public DeduplicationAction InvokedAction { get; set; }
		public bool IsExcludingOtherCountriesFromResults { get; set; }
		public bool IsExcludingInactiveFromResults { get; set; }
		public bool IsShowIgnoredFromResults { get; set; }
		public bool IsTimeout { get; set; }
		public bool IsErrorOccurred { get; set; }
		public DeduplicationExclusionManager<GlbPerson> PersonExclusionManager { get; }
		public DeduplicationExclusionManager<OrgHeader> OrgHeaderExclusionManager { get; }
	}
}
