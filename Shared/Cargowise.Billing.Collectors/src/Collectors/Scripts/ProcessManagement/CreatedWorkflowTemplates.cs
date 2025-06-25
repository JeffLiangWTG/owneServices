namespace CargoWise.Billing.Collectors.ProcessManagement
{
	public class CreatedWorkflowTemplates : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "WTC";
		public override string RoleName => "Process and Workflow";
		public override string ModuleName => "Process and Workflow";
		public override string FunctionName => "Process Management";
		public override string FeatureName => "Created Workflow Templates";
		public override string ActiveOn => "ALL";
		public override string FromClause => $@"
(
SELECT GC_Code AS CompanyCode,
	GB_Code AS BranchCode,
	P0_SystemCreateUser AS UserCode,
	TemplateCount
FROM (
	SELECT P0_GC,
		P0_SystemCreateUser,
		COUNT(*) AS TemplateCount
	FROM dbo.ProcessTaskTemplate
	WHERE P0_IsSystem = 0
		AND P0_SystemCreateTimeUtc >= {Constants.StartDateTimeInclusiveParamName} AND P0_SystemCreateTimeUtc < {Constants.EndDateTimeExclusiveParamName}
	GROUP BY P0_GC, P0_SystemCreateUser
	) TemplatesGrouped
INNER JOIN dbo.GlbStaff ON GS_Code = P0_SystemCreateUser
LEFT JOIN dbo.GlbBranch ON GB_PK = GS_GB_HomeBranch
LEFT JOIN dbo.GlbCompany ON P0_GC = GC_PK
) AggregatedTemplates
";
		public override string WhereClause => string.Empty;
		public override string CompanyCode => "CompanyCode";
		public override string BranchCode => "BranchCode";
		public override string CreatingUserCode => "UserCode";
		public override string BillingReference1 => "BranchCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string BillingReference2 => "UserCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.StartDateTimeInclusiveParamName})*10000) + (DATEPART(month, {Constants.StartDateTimeInclusiveParamName})*100) + DATEPART(day, {Constants.StartDateTimeInclusiveParamName}) as varbinary(16)) as uniqueidentifier)";

		public override string TransactionCount => "TemplateCount";

		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string DateType => RefStlDateType.SmallDateTime;
		public override string PreparationScript => Constants.MonthAsDateVariableScript;
		public override string MinCW1Version => "22.12.7.241";
	}
}
