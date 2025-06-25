using System;

namespace CargoWise.Billing.Collectors.MasterData;

public class CmdOrgAddressCapabilityUsageCollector : CmdCollector
{
	protected override string QueryScript => @"
				gb_address_capability AS
				(
					SELECT
						PZ_PK,
						[PZ_AddressType] AddressType,
						[PZ_OA] OrgAddressUniqueId,
						[PZ_IsMainAddress] IsMainAddress,
						[PZ_SystemLastEditTimeUtc] SystemLastEditTimeUtc
					FROM dbo.OrgAddressCapability ac
					JOIN dbo.OrgAddress as a ON ac.PZ_OA = a.OA_PK
					JOIN gb_org_header h ON a.OA_OH = h.OH_PK 
				)
";
	public override string FeatureCode => "CDP";
	public override string FeatureName => "Org Address Capability Records";
	public override string GuidReference => "PZ_PK";
	public override string FromClause => "gb_address_capability";
	public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(
		SELECT
			AddressType,
			OrgAddressUniqueId,
			IsMainAddress,
			SystemLastEditTimeUtc
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
	)))";
}
