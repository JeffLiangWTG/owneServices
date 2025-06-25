namespace CargoWise.Billing.Collectors.ProcessManagement
{
	public class RaisedExceptions : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "EXR";
		public override string RoleName => "Process and Workflow";
		public override string ModuleName => "Process and Workflow";
		public override string FunctionName => "Process Management";
		public override string FeatureName => "Raised Exceptions";
		public override string FromClause => $@"
(
SELECT GC_Code AS CompanyCode,
	GB_Code AS BranchCode,
	P9_SystemCreateUser AS UserCode,
	ExceptionCount
FROM (
	SELECT P9_GC,
		P9_SystemCreateUser,
		COUNT(*) AS ExceptionCount
	FROM dbo.ProcessTasks
	WHERE P9_Type = 'EXC'
		AND P9_GC IS NOT NULL
		AND P9_SystemCreateTimeUtc >= {Constants.StartDateTimeInclusiveParamName} AND P9_SystemCreateTimeUtc < {Constants.EndDateTimeExclusiveParamName}
	GROUP BY P9_GC, P9_SystemCreateUser
	) ExceptionsGrouped
INNER JOIN dbo.GlbStaff ON GS_Code = P9_SystemCreateUser
LEFT JOIN dbo.GlbBranch ON GB_PK = GS_GB_HomeBranch
LEFT JOIN dbo.GlbCompany ON P9_GC = GC_PK
) AggregatedExceptions
";
		public override string CompanyCode => "CompanyCode";
		public override string BranchCode => "BranchCode";
		public override string CreatingUserCode => "UserCode";
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.StartDateTimeInclusiveParamName})*10000) + (DATEPART(month, {Constants.StartDateTimeInclusiveParamName})*100) + DATEPART(day, {Constants.StartDateTimeInclusiveParamName}) as varbinary(16)) as uniqueidentifier)";
		public override string TransactionCount => "ExceptionCount";
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string PreparationScript => Constants.MonthAsDateVariableScript;
		public override string BillingReference1 => "BranchCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string BillingReference2 => "UserCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string WhereClause => string.Empty;
	}
}
