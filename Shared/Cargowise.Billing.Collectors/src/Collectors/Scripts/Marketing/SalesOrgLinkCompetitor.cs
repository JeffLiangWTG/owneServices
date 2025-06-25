namespace CargoWise.Billing.Collectors.Marketing
{
	#region SuppressResourceStringsCheckRegion
	public class SalesOrgLinkCompetitor : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string FeatureCode => "OLC";
		public override string RoleName => "CRM / Sales Management / HRM";
		public override string ModuleName => "Sales and Marketing System";
		public override string FunctionName => "Sales and Marketing Power Functions";
		public override string FeatureName => "Org: LinkCompetitor - Success";
		public override string CompanyCode => "gc.GC_Code";
		public override string TransactionDateUtc => "p.PR_SystemCreateTimeUtc";
		public override string CreatingUserCode => "p.PR_SystemCreateUser";
		public override string GuidReference => "p.PR_PK";
		public override string BillingReference1 => @"case
				when p.PR_PartyType = 'CMB' then 'Customs'
				when p.PR_PartyType = 'CMF' then 'Forwarding'
				when p.PR_PartyType = 'CMD' then 'Land Transport'
				when p.PR_PartyType = 'CMW' then 'Warehouse' end";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), (SELECT
					MainOrgCode = oh.OH_Code
				FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
				OrgRelatedParty p
				INNER JOIN dbo.OrgHeader oh ON oh.OH_PK = p.PR_OH_Parent
				LEFT JOIN dbo.GlbCompany gc ON gc.GC_PK = p.PR_GC";
		public override string WhereClause => @"
				oh.OH_IsActive = 1
				AND p.PR_PartyType IN ('CMB', 'CMF', 'CMD', 'CMW')";
		public override string BranchCode => string.Empty;
	}
	#endregion // SuppressResourceStringsCheckRegion
}
