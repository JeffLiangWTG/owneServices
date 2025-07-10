using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDeniedPartyResultItemV4
	{
		int LowConfidenceResultsCount { get; }

		string CurrentScreeningStatus { get; }

		string NewScreeningStatus { get; }

		string HighConfidenceResults { get; }

		string MediumConfidenceResults { get; }

		string FullClearingReason { get; }

		BusinessObject ScreenedEntity { get; }

		BusinessObject[] Parents { get; }

		DpsRequestHeaderWithAddressMatching RequestHeaderWithAddressMatching { get; }

		DpsResponse Response { get; }
	}
}
