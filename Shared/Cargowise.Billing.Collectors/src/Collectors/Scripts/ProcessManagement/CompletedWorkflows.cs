namespace CargoWise.Billing.Collectors.ProcessManagement
{
	public class CompletedWorkflows : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "WFW";
		public override string RoleName => "Process and Workflow";
		public override string ModuleName => "Process and Workflow";
		public override string FunctionName => "Process Management";
		public override string FeatureName => "Completed Workflows";
		public override string ActiveOn => "ALL";
		public override string FromClause => $@"
(
SELECT GC_Code AS CompanyCode,
	SL_GB_NKBranch AS BranchCode,
	SL_GS_NKUser AS UserCode,
	WorkflowCount
FROM (
	SELECT SL_GS_NKUser,
		SL_GB_NKBranch,
		COUNT(*) AS WorkflowCount
	FROM dbo.ProcessHeader
	INNER JOIN dbo.StmALog ON FH_PK = SL_Parent
	WHERE FH_FH_ParentHeader IS NOT NULL
		AND SL_SE_NKEvent = 'JCL'
		AND SL_PostedTimeUtc >= {Constants.StartDateTimeInclusiveParamName} AND SL_PostedTimeUtc < {Constants.EndDateTimeExclusiveParamName}
	GROUP BY SL_GS_NKUser, SL_GB_NKBranch
	) WorkflowGrouped
LEFT JOIN dbo.GlbBranch ON GB_Code = SL_GB_NKBranch
LEFT JOIN dbo.GlbCompany ON GB_GC = GC_PK
) AggregatedWorkflows
";
		public override string WhereClause => string.Empty;
		public override string CompanyCode => "CompanyCode";
		public override string BranchCode => "BranchCode";
		public override string CreatingUserCode => "UserCode";
		public override string BillingReference1 => "BranchCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string BillingReference2 => "UserCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.StartDateTimeInclusiveParamName})*10000) + (DATEPART(month, {Constants.StartDateTimeInclusiveParamName})*100) + DATEPART(day, {Constants.StartDateTimeInclusiveParamName}) as varbinary(16)) as uniqueidentifier)";
		public override string TransactionCount => "WorkflowCount";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string DateType => RefStlDateType.SmallDateTime;
		public override string PreparationScript => Constants.MonthAsDateVariableScript;
		public override string MinCW1Version => "22.12.7.241";
	}
}
