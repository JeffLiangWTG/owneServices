using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationResponseStatus
	{
		public ScoringResult ScoringResult { get; set; }
		public string Message { get; set; }
		public UserIgnoreStatus IgnoreStatus { get; set; }
		public bool IsSuccessful => Message == DuplicationResponseMessages.Success;
	}
}
