namespace CargoWise.Billing.Collectors.Marketing
{
	#region SuppressResourceStringsCheckRegion
	public class SalesOrgCreation : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string FeatureCode => "SCC";
		public override string RoleName => "CRM / Sales Management / HRM";
		public override string ModuleName => "Sales and Marketing System";
		public override string FunctionName => "Sales and Marketing Power Functions";
		public override string FeatureName => "SalesAndCompetitorOrgs: Create - Success";
		public override string TransactionDateUtc => Constants.MonthAsDateVariableName;
		public override string GuidReference => "o.OH_PK";
		public override string BillingReference1 => "o.TotalCount";
		public override string AdditionalRefs => @"
CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), (SELECT
	CompetitorCount = o.CompetitorCount,
	SalesCount = o.SalesCount,
	SalesCompetitorCount = o.SalesCompetitorCount
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string ActiveOn => "ALL";
		public override string PreparationScript => $@"
{Constants.MonthAsDateVariableScript}
WITH SalesCompetitorOrgs AS (
	SELECT
		COUNT(case when OH_IsCompetitor = 1 and OH_IsSalesLead <> 1 then 1 else null end) AS CompetitorCount,
		COUNT(case when OH_IsCompetitor <> 1 and OH_IsSalesLead = 1 then 1 else null end) AS SalesCount,
		COUNT(case when OH_IsCompetitor = 1 and OH_IsSalesLead = 1 then 1 else null end) AS SalesCompetitorCount,
		COUNT(*) AS TotalCount,
		CONVERT(uniqueidentifier, CONVERT(binary(16), '0x' + REPLICATE('0', 26) + CONVERT(CHAR(6), {Constants.MonthAsDateVariableName}, 112), 1)) AS OH_PK
	FROM dbo.OrgHeader
	WHERE (OH_IsCompetitor = 1 OR OH_IsSalesLead = 1)
		AND OH_IsActive = 1
		AND OH_SystemCreateTimeUtc >= {Constants.StartDateTimeInclusiveParamName}
		AND OH_SystemCreateTimeUtc < {Constants.EndDateTimeExclusiveParamName}
	HAVING COUNT(*) > 0
)";
		public override string FromClause => "SalesCompetitorOrgs o";
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string WhereClause => string.Empty;
	}
	#endregion // SuppressResourceStringsCheckRegion
}
