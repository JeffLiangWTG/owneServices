using System.Collections.Generic;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.GUI
{
	public interface IDataProvider<T>
	{
		T MasterGlow { get; }
		string GetTitle();
		IDeduplicationBatcher GetBusinessObjectBatcher(IEnumerable<ScoringResult> previousResults, PatternMatchingResultModel[] resultModels, IEnumerable<T> targetGlows);
		IDeduplicationDataSource GetDeduplicationDataSource(IEnumerable<DeduplicationPresenterModel> presenterModels, IEnumerable<T> targetGlows, Dictionary<ScoringResult, PatternMatchingResult> resultDictionary = null);
	}
}
