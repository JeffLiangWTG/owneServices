using System;

namespace CargoWise.Billing.Collectors.MasterData;

public class CmdGlbCompanyUsageCollector : RefStlScriptWithDefaults
{
	public override bool UsedInBilling => false;
	public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
	public override string TransactionDateUtc => Constants.MonthAsDateVariableName; // GC_SystemLastEditTimeUtc
	public override string TransactionCount => "1";
	public override string PreparationScript => Constants.MonthAsDateVariableScript + @"
				;WITH gb_company AS
				(
					SELECT
						[GC_PK],
						[GC_Code] CompanyCode,
						CONVERT(VARBINARY(MAX), NULLIF([GC_BusinessRegNo], '')) BusinessRegistrationNumber,
						CONVERT(VARBINARY(MAX), NULLIF([GC_BusinessRegNo2], '')) BusinessRegistrationNumber2,
						CONVERT(VARBINARY(MAX), NULLIF([GC_CustomsRegistrationNo], '')) CustomsRegistrationNumber,
						CONVERT(VARBINARY(MAX), NULLIF([GC_Address1], '')) Address1,
						CONVERT(VARBINARY(MAX), NULLIF([GC_Address2], '')) Address2,
						CONVERT(VARBINARY(MAX), NULLIF([GC_City], '')) City,
						NULLIF([GC_PostCode], '') PostCode,
						CONVERT(VARBINARY(MAX), NULLIF([GC_State], '')) State,
						[GC_OH_OrgProxy] OrgUniqueId,
						NULLIF([GC_RN_NKCountryCode], '') CountryCode,
						NULLIF([GC_IsActive], 1) IsActive,
						CONVERT(VARBINARY(MAX), [GC_Name]) CompanyName,
						NULLIF([GC_WebAddress], '') WebAddress,
						CASE WHEN [GC_GeoLocation].STIsEmpty() = 1 OR [GC_GeoLocation].STAsText() = 'POINT (0 0)' THEN NULL ELSE [GC_GeoLocation].STAsText() END GeoLocation,
						[GC_SystemLastEditTimeUtc] SystemLastEditTimeUtc
					FROM dbo.GlbCompany
				)
";
	public override string FeatureCode => "CDD";
	public override string RoleName => "Logistics Services";
	public override string ModuleName => "Centralized Master Data";
	public override string FunctionName => "Org Related Records Tracker";
	public override string FeatureName => "Company Records";
	public override string CompanyCode => string.Empty;
	public override string BranchCode => string.Empty;
	public override string GuidReference => "GC_PK";
	public override string BillingReference1 => string.Empty;
	public override string FromClause => "gb_company";
	public override string WhereClause => string.Empty;
	public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(
		SELECT
			CompanyCode, BusinessRegistrationNumber, BusinessRegistrationNumber2, CustomsRegistrationNumber,
			Address1, Address2, City, PostCode, State, OrgUniqueId, CountryCode, IsActive, CompanyName,
			WebAddress, GeoLocation, SystemLastEditTimeUtc
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
	)))";
}
