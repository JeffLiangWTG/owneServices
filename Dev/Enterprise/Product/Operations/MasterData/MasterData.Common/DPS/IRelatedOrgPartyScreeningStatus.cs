using CargoWise.Types;

namespace Enterprise.MasterData.Common
{
	public interface IRelatedOrgPartyScreeningStatus
	{
		ZGuid PK { get; }

		ZString PJ_ParentTableCode { get; set; }

		ZDateTime PJ_ScreenDate { get; }

		ZGuid PJ_ParentID { get; set; }

		ZString RelatedOrganization { get; set; }

		ZDateTime PJ_SystemCreateTimeUtc { get; set; }

		ZString ScreenedByFullName { get; }

		ZString PJ_Status { get; }

		ZString StatusDescription { get; }

		ZString PJ_MatchingData { get; }

		ZString PJ_HighConfidenceResults { get; }

		ZString PJ_MediumConfidenceResults { get; }

		ZInt PJ_LowConfidenceResultsCount { get; }

		ZString PJ_ExcludedLists { get; }

		ZString PJ_IncludedLists { get; }

		ZString PJ_ClearedReason { get; }

		ZString SourceInformation { get; }
	}
}
