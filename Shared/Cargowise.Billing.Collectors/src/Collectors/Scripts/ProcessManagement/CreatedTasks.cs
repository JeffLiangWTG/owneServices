namespace CargoWise.Billing.Collectors.ProcessManagement
{
	public class CreatedTasks : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "TSK";
		public override string RoleName => "Process and Workflow";
		public override string ModuleName => "Process and Workflow";
		public override string FunctionName => "Process Management";
		public override string FeatureName => "Created Tasks";
		public override string FromClause => @"
(
	SELECT GC_Code AS CompanyCode, GB_Code AS BranchCode, P9_SystemCreateUser AS UserCode, LinkedToTemplate, TaskCount
	FROM (
		SELECT P9_GC, P9_SystemCreateUser, LinkedToTemplate, COUNT(*) AS TaskCount
		FROM (
			SELECT P9_GC, P9_SystemCreateUser, IIF(P9_ParentTemplateID IS NULL, 'Not linked to a template', 'Linked to a template') AS LinkedToTemplate
			FROM ProcessTasks
			WHERE P9_Type NOT IN ('MIL','TRG','EXC') AND P9_GC IS NOT NULL AND P9_SystemCreateTimeUtc >= @StartDateTimeInclusive AND P9_SystemCreateTimeUtc < @EndDateTimeExclusive
		) Tasks
		GROUP BY P9_GC, P9_SystemCreateUser, LinkedToTemplate
	) TasksGrouped
	INNER JOIN GlbStaff ON GS_Code = P9_SystemCreateUser
	LEFT JOIN GlbBranch ON GB_PK = GS_GB_HomeBranch
	LEFT JOIN GlbCompany ON P9_GC = GC_PK
) AggregatedTasks";
		public override string CompanyCode => "CompanyCode";
		public override string BranchCode => "BranchCode";
		public override string CreatingUserCode => "UserCode";
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.StartDateTimeInclusiveParamName})*10000) + (DATEPART(month, {Constants.StartDateTimeInclusiveParamName})*100) + DATEPART(day, {Constants.StartDateTimeInclusiveParamName}) as varbinary(16)) as uniqueidentifier)";
		public override string BillingReference1 => "LinkedToTemplate";
		public override string BillingReference2 => "BranchCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string BillingReference3 => "UserCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string TransactionCount => "TaskCount";
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string DateType => RefStlDateType.SmallDateTime;
		public override string PreparationScript => Constants.MonthAsDateVariableScript;
		public override string WhereClause => string.Empty;
	}
}
