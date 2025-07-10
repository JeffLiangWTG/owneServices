using CargoWise.Types;

namespace Enterprise.DeniedPartyScreening.Integration
{
	public interface IStmEntityScreeningLog
	{
		ZGuid PK { get; }
		ZString StatusDescription { get; }
		ZDateTime PJ_ScreenDate { get; }
		ZString PJ_Status { get; set; }
		ZString PJ_MatchingData { get; set; }
		ZString ScreenedByFullName { get; }
		ZString PJ_HighConfidenceResults { get; set; }
		ZString PJ_MediumConfidenceResults { get; set; }
		ZInt PJ_LowConfidenceResultsCount { get; set; }
		ZString PJ_ExcludedLists { get; set; }
		ZString PJ_IncludedLists { get; set; }
		ZString PJ_ClearedReason { get; set; }
		ZString PJ_SourceTableCode { get; set; }
		ZGuid PJ_SourceID { get; set; }
		ZString PJ_ParentTableCode { get; set; }
		ZGuid PJ_ParentID { get; set; }
		ZString SourceInformation { get; }
		ZDateTime PJ_SystemCreateTimeUtc { get; set; }
	}
}
