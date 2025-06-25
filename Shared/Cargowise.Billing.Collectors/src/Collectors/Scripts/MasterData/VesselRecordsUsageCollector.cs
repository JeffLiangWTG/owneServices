namespace CargoWise.Billing.Collectors.MasterData;

public class VesselRecordsUsageCollector : RefStlScriptWithDefaults
{
	public override string FeatureCode => "VUC";
	public override string FeatureName => "Vessel Records";
	public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
	public override string RoleName => "Vessel Records Usage Tracker";
	public override string ModuleName => "Vessel Records";
	public override string FunctionName => "Vessel Records Usage Tracker";
	public override string DataGranularity => RefStlItemGrain.MonthlyCurrentDataOnly;
	public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.StartDateTimeInclusiveParamName})*10000) + (DATEPART(month, {Constants.StartDateTimeInclusiveParamName})*100) + DATEPART(day, {Constants.StartDateTimeInclusiveParamName}) as varbinary(16)) as uniqueidentifier)";
	public override string FromClause => "CT";
	public override string CompanyCode => string.Empty;
	public override string BranchCode => string.Empty;
	public override string BillingReference1 => "VesselNameCount+VesselIdCount";
	public override string WhereClause => string.Empty;
	public override bool UsedInBilling => false;
	public override string ActiveOn => "ALL";
	public override string PreparationScript => Constants.MonthAsDateVariableScript + @";
		WITH VesselNameResult AS (Select COUNT(*) AS VesselNameCount
					FROM RefVessel
					WHERE RV_IsActive = 1
		),
		VNI AS (SELECT VesselNameResult.VesselNameCount, (SELECT COUNT(*)
					FROM RefVessel WHERE RV_LloydsNumber != '' AND RV_IsActive = 1) AS VesselIdCount FROM VesselNameResult),
		VNIA AS (SELECT VNI.VesselNameCount, VNI.VesselIdCount,
					(SELECT COUNT(RV_PK)
					FROM dbo.RefVessel WHERE RV_IsActive = 1) AS VesselActiveRecords FROM VNI),
		CT AS (SELECT VNIA.VesselNameCount, VNIA.VesselIdCount, VNIA.VesselActiveRecords,
					(SELECT COUNT(RV_PK)
					FROM dbo.RefVessel WHERE RV_IsActive = 0) AS VesselInactiveRecords FROM VNIA)";
	public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(
		SELECT
			VesselNameCount,
			VesselIdCount,
			VesselActiveRecords,
			VesselInactiveRecords
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
	)))";
}
