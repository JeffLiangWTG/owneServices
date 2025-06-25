namespace CargoWise.Billing.Collectors.Marketing
{
	#region SuppressResourceStringsCheckRegion
	public class SalesValueAnalysisManager : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string FeatureCode => "VCS";
		public override string RoleName => "CRM / Sales Management / HRM";
		public override string ModuleName => "Sales and Marketing System";
		public override string FunctionName => "Sales and Marketing Power Functions";
		public override string FeatureName => "ValueAnalysis: Create - Success";
		public override string TransactionDateUtc => "s.OW_SystemCreateTimeUtc";
		public override string CreatingUserCode => "s.OW_SystemCreateUser";
		public override string GuidReference => "s.OW_PK";
		public override string BillingReference1 => "p8.P8_OpportunityID";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), (SELECT
	ProductName = mp.MP_Name,
	TypeModeCount = CASE WHEN mp.MP_Name IN ('Transport', 'Warehouse') THEN 'N/A' ELSE convert(varchar(10), Details.TypeModeCount) END,
	DetailsCount = Details.DetailsCount,
	VerticalMarketCount = Details.VerticalMarketCount,
	ActivityPeriodCount = Details.ActivityPeriodCount,
	CompetitorCount = Details.CompetitorCount,
	AgentCount = Details.AgentCount,
	CarrierCount = Details.CarrierCount
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
OrgSales s
INNER JOIN dbo.OrgSalesValueAssociationPivot svp ON svp.SVP_TradeId = s.OW_PK AND svp.SVP_TradeTableCode = 'OW'
INNER JOIN dbo.OrgOpportunity p8 ON svp.SVP_ActivityId = p8.P8_PK AND svp.SVP_ActivityTableCode = 'P8'
INNER JOIN dbo.OrgSalesProduct mp ON s.OW_MP_Product = mp.MP_PK
LEFT JOIN
(
	SELECT PA_OW,
		count(distinct PA_TradeMode + '-' + PA_TradeType) as TypeModeCount,
		count(PAP_PK) as DetailsCount,
		count(CASE WHEN PAP_IndustryVertical <> '' THEN 1 ELSE null END) as VerticalMarketCount,
		count(CASE WHEN PAP_PeriodOfActivity <> '' THEN 1 ELSE null END) as ActivityPeriodCount,
		count(CASE WHEN PAP_OH_Competitor is not null THEN 1 ELSE null END) as CompetitorCount,
		count(CASE WHEN PAP_OH_ControllingAgent is not null THEN 1 ELSE null END) as AgentCount,
		count(CASE WHEN PAP_OH_ServiceProvider is not null THEN 1 ELSE null END) as CarrierCount
	FROM dbo.OrgTradeDetail
	LEFT JOIN dbo.OrgTradeProspect ON PAP_PA = PA_PK
	GROUP BY PA_OW
) Details ON Details.PA_OW = s.OW_PK";
		public override string WhereClause => "s.OW_IsTraded = 0";
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
	}
	#endregion // SuppressResourceStringsCheckRegion
}
