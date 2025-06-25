namespace CargoWise.Billing.Collectors.ProcessManagement
{
	public class TriggersDefinedOnTemplates : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "TRT";
		public override string RoleName => "Process and Workflow";
		public override string ModuleName => "Process and Workflow";
		public override string FunctionName => "Process Management";
		public override string FeatureName => "Triggers Defined on Templatess";
		public override string FromClause => $@"
(
SELECT GC_Code AS CompanyCode,
	GB_Code AS BranchCode,
	UserCode,
	IsUniversalTrigger,
	DefinedOnUniversalTemplate,
	TriggerCount
FROM (
	SELECT UserCode,
		IsUniversalTrigger,
		DefinedOnUniversalTemplate,
		COUNT(*) AS TriggerCount
	FROM (
		SELECT UserCode,
			IsUniversalTrigger,
			IIF(P0_IsUniversal = 1, 'Defined on a universal template', 'Defined on a non-universal template') AS DefinedOnUniversalTemplate
		FROM (
			SELECT P9_SystemCreateUser AS UserCode,
				'Non-universal trigger' AS IsUniversalTrigger,
				P0_IsUniversal
			FROM dbo.ProcessTasks
			JOIN dbo.ProcessTaskTemplate ON P9_ParentID = P0_PK
			WHERE P9_Type = 'TRG'
				AND P9_SystemCreateTimeUtc >= {Constants.StartDateTimeInclusiveParamName} AND P9_SystemCreateTimeUtc < {Constants.EndDateTimeExclusiveParamName}
			UNION ALL
			SELECT P9T_SystemCreateUser,
				'Universal trigger',
				P0_IsUniversal
			FROM dbo.ProcessTemplateTrigger
			JOIN dbo.ProcessTaskTemplate ON P9T_P0_Template = P0_PK
				AND P9T_SystemCreateTimeUtc >= {Constants.StartDateTimeInclusiveParamName} AND P9T_SystemCreateTimeUtc < {Constants.EndDateTimeExclusiveParamName}
		) TriggersAndTemplates
	) TriggersAndTemplatesComplete
	GROUP BY UserCode, IsUniversalTrigger, DefinedOnUniversalTemplate
) TriggersGrouped
INNER JOIN dbo.GlbStaff ON GS_Code = UserCode
LEFT JOIN dbo.GlbBranch ON GB_PK = GS_GB_HomeBranch
LEFT JOIN dbo.GlbCompany ON GB_GC = GC_PK
) AggregatedTriggers
";
		public override string CompanyCode => "CompanyCode";
		public override string BranchCode => "BranchCode";
		public override string CreatingUserCode => "UserCode";
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string GuidReference => $"cast(cast((DATEPART(year, {Constants.StartDateTimeInclusiveParamName})*10000) + (DATEPART(month, {Constants.StartDateTimeInclusiveParamName})*100) + DATEPART(day, {Constants.StartDateTimeInclusiveParamName}) as varbinary(16)) as uniqueidentifier)";
		public override string BillingReference1 => "DefinedOnUniversalTemplate";
		public override string BillingReference2 => "IsUniversalTrigger";
		public override string BillingReference3 => "BranchCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string BillingReference4 => "UserCode"; // as the resulting query tries to avoid duplicate records and thus aggregates them by company code, guid reference, billing references, etc., but not by branch and user code, we need this to make records unique
		public override string TransactionCount => "TriggerCount";
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string PreparationScript => Constants.MonthAsDateVariableScript;
		public override string WhereClause => string.Empty;
	}
}
