namespace CargoWise.Billing.Collectors.MasterData;

public class OrgRecordsUsageCollector : RefStlScriptWithDefaults
{
	public override string FeatureCode => "OUC";
	public override string FeatureName => "Org Records";
	public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
	public override string RoleName => "Org Records Usage Tracker";
	public override string ModuleName => "Org Records";
	public override string FunctionName => "Org Records Usage Tracker";
	public override string DataGranularity => RefStlItemGrain.MonthlyCurrentDataOnly;
	public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.StartDateTimeInclusiveParamName})*10000) + (DATEPART(month, {Constants.StartDateTimeInclusiveParamName})*100) + DATEPART(day, {Constants.StartDateTimeInclusiveParamName}) as varbinary(16)) as uniqueidentifier)";
	public override string FromClause => "CT";
	public override string CompanyCode => string.Empty;
	public override string BranchCode => string.Empty;
	public override string BillingReference1 => "OrgNameCount+OrgAddressCount+OrgIdCount";
	public override string WhereClause => string.Empty;
	public override bool UsedInBilling => false;
	public override string ActiveOn => "ALL";
	public override string PreparationScript => Constants.MonthAsDateVariableScript + @";
		WITH OrgNameResult AS (
			SELECT 
				(SELECT COUNT(*)
				 FROM OrgHeader oh 
				 JOIN OrgBrandOrRelatedName ob ON oh.OH_PK = ob.P1_OH
				 WHERE oh.OH_IsActive = 1 AND oh.OH_Code != 'DEMORG') +
				(SELECT COUNT(*)
				 FROM OrgHeader oh 
				 JOIN OrgAddress oa ON oh.OH_PK = oa.OA_OH
				 WHERE oh.OH_IsActive = 1 AND oh.OH_Code != 'DEMORG' AND OA_CompanyNameOverride != '' AND OA_IsActive = 1) +
				(SELECT COUNT(*)
				 FROM OrgHeader
				 WHERE OH_IsActive = 1 AND OH_Code != 'DEMORG') AS OrgNameCount
		),
		OSR AS (SELECT 
					OrgNameResult.OrgNameCount,
					(SELECT COUNT(*) 
					 FROM OrgHeader oh 
					 JOIN OrgAddress oa ON oh.OH_PK = oa.OA_OH
					 WHERE oh.OH_IsActive = 1 AND oh.OH_Code != 'DEMORG' AND OA_IsActive = 1) AS OrgAddressCount,
					(SELECT COUNT(*) 
					 FROM OrgCusCode oc 
					 JOIN OrgHeader oh ON oh.OH_PK = oc.OK_OH
					 WHERE oh.OH_IsActive = 1 AND oh.OH_Code != 'DEMORG' 
					 AND (oc.OK_CodeType = 'DUN' OR oc.OK_CodeType = 'PAS')) AS OrgIdCount
				FROM OrgNameResult),
		OSAR AS (SELECT
						OSR.OrgNameCount,
						OSR.OrgAddressCount,
						OSR.OrgIdCount,
						(SELECT COUNT(OH_PK)
						FROM dbo.OrgHeader WHERE OH_IsActive = 1 AND OH_Code != 'DEMORG') AS OrgActiveRecords
				FROM OSR),
		CT AS (SELECT
						OSAR.OrgNameCount,
						OSAR.OrgAddressCount,
						OSAR.OrgIdCount,
						OSAR.OrgActiveRecords,
						(SELECT COUNT(OH_PK)
						FROM dbo.OrgHeader WHERE OH_IsActive = 0 AND OH_Code != 'DEMORG') AS OrgInactiveRecords
				FROM OSAR)";
	public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(
		SELECT
			OrgNameCount,
			OrgAddressCount,
			OrgIdCount,
			OrgActiveRecords,
			OrgInactiveRecords
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
	)))";
}
