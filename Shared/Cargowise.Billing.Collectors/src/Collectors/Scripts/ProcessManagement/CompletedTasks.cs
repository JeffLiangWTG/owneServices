namespace CargoWise.Billing.Collectors.ProcessManagement
{
	public class CompletedTasks : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "WFT";
		public override string RoleName => "Process and Workflow";
		public override string ModuleName => "Process and Workflow";
		public override string FunctionName => "Process Management";
		public override string FeatureName => "Completed Tasks";
		public override string ActiveOn => "ALL";
		public override string FromClause => $@"
(
SELECT GC_Code AS CompanyCode,
	SL_GS_NKUser AS UserCode,
	SL_GB_NKBranch AS BranchCode,
	StatusChangeMode,
	TaskCount
FROM (
	SELECT P9_GC,
		SL_GS_NKUser,
		SL_GB_NKBranch,
		StatusChangeMode,
		COUNT(*) AS TaskCount
	FROM (
		SELECT P9_GC,
			SL_GS_NKUser,
			SL_GB_NKBranch,
			SUBSTRING(SL_Reference, PATINDEX('%|CHM=%', SL_Reference)+5, 3) AS StatusChangeMode
		FROM dbo.ProcessTasks
		INNER JOIN dbo.StmALog ON P9_PK = SL_Parent
		WHERE P9_Type NOT IN ('MIL','TRG','EXC')
			AND P9_GC IS NOT NULL --Exclude template tasks
			AND SL_Table = 'ProcessTasks'
			AND SL_SE_NKEvent = 'STC'
			AND SL_Reference LIKE '%|TO=CLS%'
			AND SL_PostedTimeUtc >= {Constants.StartDateTimeInclusiveParamName} AND SL_PostedTimeUtc < {Constants.EndDateTimeExclusiveParamName}
		) Tasks
	GROUP BY P9_GC, SL_GS_NKUser, SL_GB_NKBranch, StatusChangeMode
	) TasksGrouped
LEFT JOIN dbo.GlbCompany ON P9_GC = GC_PK
) AggregatedTasks
";
		public override string WhereClause => string.Empty;
		public override string CompanyCode => "CompanyCode";
		public override string BranchCode => "BranchCode";
		public override string CreatingUserCode => "UserCode";
		public override string BillingReference1 => "StatusChangeMode";
		public override string BillingReference2 => "BranchCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string BillingReference3 => "UserCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.StartDateTimeInclusiveParamName})*10000) + (DATEPART(month, {Constants.StartDateTimeInclusiveParamName})*100) + DATEPART(day, {Constants.StartDateTimeInclusiveParamName}) as varbinary(16)) as uniqueidentifier)";
		public override string TransactionCount => "TaskCount";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string PreparationScript => Constants.MonthAsDateVariableScript;
		public override string MinCW1Version => "22.12.7.241";
	}
}
