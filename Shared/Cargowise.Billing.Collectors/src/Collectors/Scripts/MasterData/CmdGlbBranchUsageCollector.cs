using System;

namespace CargoWise.Billing.Collectors.MasterData;

public class CmdGlbBranchUsageCollector : RefStlScriptWithDefaults
{
	public override bool UsedInBilling => false;
	public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
	public override string TransactionDateUtc => Constants.MonthAsDateVariableName; // "GB_SystemLastEditTimeUtc";
	public override string TransactionCount => "1";

	public override string PreparationScript => Constants.MonthAsDateVariableScript + @"
				;WITH gb_branch AS
				(
					SELECT
						GB_PK,
						NULLIF([GB_Code], '') BranchCode,
						CONVERT(VARBINARY(MAX), NULLIF([GB_BranchName], '')) BranchName,
						CONVERT(VARBINARY(MAX), NULLIF([GB_Address1], '')) Address1,
						CONVERT(VARBINARY(MAX), NULLIF([GB_Address2], '')) Address2,
						CONVERT(VARBINARY(MAX), NULLIF([GB_City], '')) City,
						CONVERT(VARBINARY(MAX), NULLIF([GB_State], '')) State,
						NULLIF([GB_PostCode], '') PostCode,
						NULLIF([GB_WebAddress], '') WebAddress,
						[GB_GC] CompanyUniqueId,
						NULLIF([GB_RL_NKHomePort], '') PortCode,
						[GB_OH_OrgProxy] OrgUniqueId,
						NULLIF([GB_IsActive], 1) IsActive,
						[GB_OA_AddressProxy] OrgAddressUniqueId,
						NULLIF([GB_RN_NKCountryCode], '') CountryCode,
						CASE WHEN [GB_GeoLocation].STIsEmpty() = 1 OR [GB_GeoLocation].STAsText() = 'POINT (0 0)' THEN NULL ELSE [GB_GeoLocation].STAsText() END GeoLocation,
						[GB_SystemLastEditTimeUtc] SystemLastEditTimeUtc
					FROM dbo.GlbBranch
				)
";
	public override string FeatureCode => "CDE";
	public override string RoleName => "Logistics Services";
	public override string ModuleName => "Centralized Master Data";
	public override string FunctionName => "Org Related Records Tracker";
	public override string FeatureName => "Branch Records";
	public override string CompanyCode => string.Empty;
	public override string BranchCode => string.Empty;
	public override string GuidReference => "GB_PK";
	public override string BillingReference1 => string.Empty;
	public override string FromClause => "gb_branch";
	public override string WhereClause => string.Empty;
	public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(
		SELECT
			BranchCode, BranchName, Address1, Address2, City, State, PostCode, WebAddress, CompanyUniqueId,
			PortCode,OrgUniqueId, IsActive, OrgAddressUniqueId, CountryCode, GeoLocation, SystemLastEditTimeUtc
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
	)))";
}
