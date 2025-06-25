namespace CargoWise.Billing.Collectors.ProcessManagement
{
	public class ProcessAndWorkflowProject : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "WKP";
		public override string RoleName => "Process and Workflow";
		public override string ModuleName => "Process and Workflow";
		public override string FunctionName => "Process Management";
		public override string FeatureName => "Project System";
		public override string ActiveOn => "ALL";
		public override string FromClause => $@"
(
SELECT
	GC_Code AS CompanyCode,
	GB_Code AS BranchCode,
	GS_Code AS UserCode,
	ProjectCount
FROM (
	SELECT WKP_SystemCreateUser,
		COUNT(*) AS ProjectCount
	FROM dbo.WorkProject
	WHERE WKP_SystemCreateTimeUtc >= {Constants.StartDateTimeInclusiveParamName} AND WKP_SystemCreateTimeUtc < {Constants.EndDateTimeExclusiveParamName}
	GROUP BY WKP_SystemCreateUser
) ProjectsGrouped
INNER JOIN dbo.GlbStaff ON GS_Code = WKP_SystemCreateUser
LEFT JOIN dbo.GlbBranch ON GB_PK = GS_GB_HomeBranch
LEFT JOIN dbo.GlbCompany ON GC_PK = GB_GC
) AggregatedProjects
";
		public override string WhereClause => string.Empty;
		public override string CompanyCode => "CompanyCode";
		public override string BranchCode => "BranchCode";
		public override string CreatingUserCode => "UserCode";
		public override string TransactionCount => "ProjectCount";
		public override string BillingReference1 => "BranchCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string BillingReference2 => "UserCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.StartDateTimeInclusiveParamName})*10000) + (DATEPART(month, {Constants.StartDateTimeInclusiveParamName})*100) + DATEPART(day, {Constants.StartDateTimeInclusiveParamName}) as varbinary(16)) as uniqueidentifier)";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string DateType => RefStlDateType.SmallDateTime;
		public override string PreparationScript => Constants.MonthAsDateVariableScript;
		public override string MinCW1Version => "22.12.7.241";
	}
}
