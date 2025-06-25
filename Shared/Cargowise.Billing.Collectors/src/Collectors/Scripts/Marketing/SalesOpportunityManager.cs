namespace CargoWise.Billing.Collectors.Marketing
{
	#region SuppressResourceStringsCheckRegion
	public class SalesOpportunityManager : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "OPM";
		public override string RoleName => "CRM / Sales Management / HRM";
		public override string ModuleName => "Sales and Marketing System";
		public override string FunctionName => "Sales and Marketing Power Functions";
		public override string FeatureName => "Opportunity: Create - Success";
		public override string CompanyCode => "gc.GC_Code";
		public override string TransactionDateUtc => "p8.P8_SystemCreateTimeUtc";
		public override string BillingReference1 => "p8.P8_OpportunityID";
		public override string BillingReference2 => @"'CreationSource=' + CASE WHEN p8.P8_O1_Enquiry IS NOT NULL THEN 'Inquiry' WHEN p8.P8_G0 IS NOT NULL THEN 'Campaign' ELSE 'Opportunity' END + 
					'|LeadSource=' + P8_Source";
		public override string BillingReference3 => @"'HasCurrent=' + CASE WHEN P8_DiscountAmount <> 0 THEN 'Y' ELSE 'N' END + 
					'|HasPotential=' + CASE WHEN P8_RentalMultiplier <> 0 THEN 'Y' ELSE 'N' END +
					'|HasTotalEst=' + CASE WHEN P8_EstimatedValue <> 0 THEN 'Y' ELSE 'N' END";
		public override string BillingReference4 => @"'IsAssignedToCreator=' + CASE WHEN P8_GS_NKPrimarySalesPerson = P8_SystemCreateUser THEN 'Y' ELSE 'N' END +
					'|HasContact=' + CASE WHEN P8_OC IS NOT NULL THEN 'Y' ELSE 'N' END";
		public override string GuidReference => "p8.P8_PK";
		public override string CreatingUserCode => "p8.P8_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"OrgOpportunity p8 
					INNER JOIN dbo.GlbCompany gc ON p8.P8_GC = gc.GC_PK";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string BranchCode => string.Empty;
		public override string WhereClause => string.Empty;
		public override string MinCW1Version => "22.11.29.231";
	}
	#endregion
}
