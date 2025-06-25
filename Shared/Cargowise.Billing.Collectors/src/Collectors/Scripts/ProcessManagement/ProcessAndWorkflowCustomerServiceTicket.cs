namespace CargoWise.Billing.Collectors.ProcessManagement
{
	public class ProcessAndWorkflowCustomerServiceTicket : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "CST";
		public override string RoleName => "Process and Workflow";
		public override string ModuleName => "Process and Workflow";
		public override string FunctionName => "Process Management";
		public override string FeatureName => "Customer Service Tickets System";
		public override string ActiveOn => "ALL";
		public override string FromClause => $@"
(
SELECT GC_Code AS CompanyCode,
	BranchCode,
	UserCode,
	RequestCount
FROM (
	SELECT COALESCE(ticketBranch.GB_Code, userBranch.GB_Code) AS BranchCode,
		WKR_SystemCreateUser AS UserCode,
		RequestCount
	FROM (
		SELECT WKR_GB_Branch,
			WKR_SystemCreateUser,
			COUNT(*) AS RequestCount
		FROM dbo.WorkRequest
		WHERE WKR_SystemCreateTimeUtc >= {Constants.StartDateTimeInclusiveParamName} AND WKR_SystemCreateTimeUtc < {Constants.EndDateTimeExclusiveParamName}
		GROUP BY WKR_GB_Branch, WKR_SystemCreateUser
		) RequestsGrouped
	LEFT JOIN dbo.GlbBranch ticketBranch ON GB_PK = WKR_GB_Branch
	INNER JOIN dbo.GlbStaff ON GS_Code = WKR_SystemCreateUser
	LEFT JOIN dbo.GlbBranch userBranch ON userBranch.GB_PK = GS_GB_HomeBranch
	) RequestsWithBranches
LEFT JOIN dbo.GlbBranch ON GB_Code = BranchCode
LEFT JOIN dbo.GlbCompany ON GC_PK = GB_GC
) AggregatedRequests
";

		public override string WhereClause => string.Empty;
		public override string CompanyCode => "CompanyCode";
		public override string BranchCode => "BranchCode";
		public override string CreatingUserCode => "UserCode";
		public override string TransactionCount => "RequestCount";
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
