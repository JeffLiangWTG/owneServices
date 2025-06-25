using System;

namespace CargoWise.Billing.Collectors.MasterData;

public class CmdOrgAddressUsageCollector : CmdCollector
{
	protected override string QueryScript => @"
				gb_org_address AS
				(
					SELECT
						[OA_PK],
						CONVERT(VARBINARY(MAX), NULLIF([OA_CompanyNameOverride], '')) CompanyName,
						CONVERT(VARBINARY(MAX), NULLIF([OA_Address1], '')) Address1,
						CONVERT(VARBINARY(MAX), NULLIF([OA_Address2], '')) Address2,
						CONVERT(VARBINARY(MAX), NULLIF([OA_State], '')) State,
						CONVERT(VARBINARY(MAX), NULLIF([OA_PostCode], '')) PostCode, -- PostCode is only nvarchar in this table
						NULLIF([OA_RL_NKRelatedPortCode], '') PortCode,
						[OA_OH] OrgUniqueId,
						NULLIF([OA_IsActive], 1) IsActive,
						NULLIF([OA_RN_NKCountryCode], '') CountryCode,
						CONVERT(VARBINARY(MAX), NULLIF([OA_City], '')) City,
						CASE WHEN [OA_GeofencePolygon].STIsEmpty() = 1 THEN NULL ELSE [OA_GeofencePolygon].STAsText() END GeoPolygon,
						NULLIF([OA_Language], 'EN') [Language],
						CASE WHEN [OA_GeoLocation].STIsEmpty() = 1 OR [OA_GeoLocation].STAsText() = 'POINT (0 0)' THEN NULL ELSE [OA_GeoLocation].STAsText() END GeoLocation,
						[OA_SystemLastEditTimeUtc] SystemLastEditTimeUtc
					FROM dbo.OrgAddress as a
					JOIN gb_org_header as h ON a.OA_OH = h.OH_PK
				)
";
	public override string FeatureCode => "CDA";
	public override string FeatureName => "Org Address Records";
	public override string GuidReference => "OA_PK";
	public override string FromClause => "gb_org_address";
	public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(
		SELECT
			CompanyName, Address1, Address2, State, PostCode, PortCode, OrgUniqueId, IsActive,
			CountryCode, City, GeoPolygon, Language, GeoLocation, SystemLastEditTimeUtc
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
	)))";
}
