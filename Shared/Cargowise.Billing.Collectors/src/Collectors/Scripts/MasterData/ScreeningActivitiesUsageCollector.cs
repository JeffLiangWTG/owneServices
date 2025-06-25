namespace CargoWise.Billing.Collectors.MasterData;

public class ScreeningActivitiesUsageCollector : RefStlScriptWithDefaults
{
	public override string FeatureCode => "SAC";
	public override string FeatureName => "Screening Activities";
	public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
	public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.StartDateTimeInclusiveParamName})*10000) + (DATEPART(month, {Constants.StartDateTimeInclusiveParamName})*100) + DATEPART(day, {Constants.StartDateTimeInclusiveParamName}) as varbinary(16)) as uniqueidentifier)";
	public override string FromClause => "ScreeningActivitiesUsage";
	public override string RoleName => "Screening Activities Usage Tracker";
	public override string ModuleName => "Screening Activities";
	public override string FunctionName => "Screening Activities Usage Tracking";
	public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
	public override string CompanyCode => string.Empty;
	public override string BranchCode => string.Empty;
	public override string BillingReference1 => "FreightSystemSettings";
	public override string AdditionalRefs => "CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), (SELECT DPR, DSS, FreightCompanySettings FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
	public override string WhereClause => string.Empty;
	public override string ActiveOn => "ALL";
	public override bool UsedInBilling => false;

	public override string PreparationScript => Constants.MonthAsDateVariableScript + @";

	DECLARE @FreightMovementRestrictionSystem VARCHAR(MAX) = (SELECT CAST(SD_BinaryValue AS NVARCHAR(MAX)) FROM StmData WHERE SD_Name = 'DPSFreightMovementRestrictions' AND SD_Owner IS NULL);
	WITH DPRResult AS (
		SELECT
			(SELECT
				S5_TaskPeriod AS TaskPeriod,
				S5_TaskPeriodCount AS TaskPeriodCount,
				S5_WeekDaysOnly AS WeeksDaysOnly,
				S5_DayNumber AS DayNumber,
				S5_DayList AS DayList,
				S5_WeekDayOccurrenceNumber AS WeekDayOccurrenceNumber,
				CASE S5_IsActive WHEN 'true' THEN 1 ELSE 0 END AS IsActive
			FROM StmScheduleTask
			WHERE S5_Scheduletype = 'DPR'
			FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
			) AS DPR),
	DSSResult AS (
		SELECT DPR,
			(SELECT
				S5_TaskPeriod AS TaskPeriod,
				S5_TaskPeriodCount AS TaskPeriodCount,
				S5_WeekDaysOnly AS WeeksDaysOnly,
				S5_DayNumber AS DayNumber,
				S5_DayList AS DayList,
				S5_WeekDayOccurrenceNumber AS WeekDayOccurrenceNumber,
				CASE S5_IsActive WHEN 'true' THEN 1 ELSE 0 END AS IsActive
			FROM StmScheduleTask
			WHERE S5_Scheduletype = 'DSS'
			FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
			) AS DSS from DPRResult),
	FreightSystemSettingsResult AS (
		SELECT DPR, DSS,
			(SELECT
				CASE
					WHEN @FreightMovementRestrictionSystem IS NULL THEN 'DEFAULT'
					ELSE @FreightMovementRestrictionSystem
				END
			) AS FreightSystemSettings from DSSResult),
	CountsResult AS (
		SELECT DPR, DSS, FreightSystemSettings,
			(SELECT
				(SELECT COUNT(GC_Code) FROM GLBCompany WHERE GC_IsActive = 1 AND GC_Code <> 'DEM') AS 'TotalCompanies',
				(SELECT COUNT(SD_Owner) FROM StmData JOIN GLBCompany ON GC_PK = SD_Owner WHERE GC_IsActive = 1 AND SD_Name = 'DPSFreightMovementRestrictions' AND CAST(SD_BinaryValue AS NVARCHAR(MAX)) = 'ALL') AS 'ALL',
				(SELECT COUNT(SD_Owner) FROM StmData JOIN GLBCompany ON GC_PK = SD_Owner WHERE GC_IsActive = 1 AND SD_Name = 'DPSFreightMovementRestrictions' AND CAST(SD_BinaryValue AS NVARCHAR(MAX)) = 'NO') AS 'NO',
				(SELECT COUNT(SD_Owner) FROM StmData JOIN GLBCompany ON GC_PK = SD_Owner WHERE GC_IsActive = 1 AND SD_Name = 'DPSFreightMovementRestrictions' AND CAST(SD_BinaryValue AS NVARCHAR(MAX)) = 'EXP') AS 'EXP',
				(SELECT COUNT(SD_Owner) FROM StmData JOIN GLBCompany ON GC_PK = SD_Owner WHERE GC_IsActive = 1 AND SD_Name = 'DPSFreightMovementRestrictions' AND CAST(SD_BinaryValue AS NVARCHAR(MAX)) = 'INT') AS 'INT'
			FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
			) AS FreightCompanySettings from FreightSystemSettingsResult),
	ScreeningActivitiesUsage AS (
		SELECT DPR, DSS, FreightSystemSettings, FreightCompanySettings
		FROM CountsResult)
";
}
