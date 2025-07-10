using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;

namespace Enterprise.MasterData.Common
{
	public interface IDeduplicatable
	{
		event EventHandler DeduplicationStarted;

		[SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		event EventHandler<IDuplicationEventArgs> DeduplicationActionOccurred;

		[SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		event EventHandler<IDuplicationEventArgs> DeduplicationEnded;

		[SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		event EventHandler<IDuplicationEventArgs> DuplicationDetected;

		bool IsDuplicateFound { get; }

		void PropagateDeduplicationActionOccurred(DeduplicationAction userAction, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModels, object targetList, bool isExcludingInactiveFromResults = false, bool isExcludingOtherCountriesFromResults = false, bool isShowIgnoredFromResults = false);

		void ValidateDuplicationResult(bool isDuplicatesFound);

		void PropagateDeduplication(IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModel, object targetObjects);

		void PropagateDeduplicationStarted();

		bool IsDeduplicationStarted { get; set; }

		void FindDuplicates();

		void FindDuplicatesBypassErrorChecking(bool isForAdminPanel);

		Task RegeneratePatternTables();

		void PropagateDeduplicationEnded<TBizo>(IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultsModel, object targetObjects, DuplicationStatus lastRunStatus, DeduplicationExclusionManager<TBizo> exclusionManager) where TBizo : BusinessObject, IDeduplicatable;

		bool IsExcludedFromDeduplication { get; set; }

		bool ShouldRunDeduplication { get; set; }

		ZString DeduplicationCountryCode { get; }

		List<ISupportDuplicationFinder> GetSupportedDuplicationFinders(Type targetType, DeduplicationProxyConfig config);

		string Info { get; }

		string FullName { get; }

		bool IsActive { get; }

		Type BizoType { get; }

		ZString NaturalKey { get; }

		BusinessObjectFactory Factory { get; }

		bool IsDeleted { get; }
	}

	public interface ISupportDuplicationFinder
	{
		Type TargetType { get; }

		bool ShouldStopProcessing { get; set; }

		List<ScoringResult> ScoringResults { get; }

		DuplicationStatus LastRunStatus { set; get; }

		Task FindDuplicates();

		void RequestToCancel();
	}

	public enum DuplicationStatus
	{
		OK,
		Timeout,
		ErrorOccurred
	}
}
