using System;

namespace CargoWise.Billing.Collectors.MasterData;

public class CmdOrgCusCodeUsageCollector : CmdCollector
{
	protected override string QueryScript => @"
				gb_org_cus_code AS
				(
					SELECT
						OK_PK,
						CONVERT(VARBINARY(MAX),[OK_CustomsRegNo]) RegistrationNumber,
						[OK_CodeType] RegistrationNumberType,
						[OK_OH] OrgUniqueId,
						[OK_OA_PremisesAddress] OrgAddressUniqueId,
						[OK_RN_NKCodeCountry] CountryCode,
						[OK_SystemLastEditTimeUtc] SystemLastEditTimeUtc
					FROM dbo.OrgCusCode c
					JOIN gb_org_header h ON c.OK_OH = h.OH_PK 
				)
";
	public override string FeatureCode => "CDN";
	public override string FeatureName => "Org Cus Code Records";
	public override string GuidReference => "OK_PK";
	public override string FromClause => "gb_org_cus_code";
	public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(
		SELECT
			RegistrationNumber,
			RegistrationNumberType,
			OrgUniqueId,
			OrgAddressUniqueId,
			CountryCode,
			SystemLastEditTimeUtc
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
	)))";
}
