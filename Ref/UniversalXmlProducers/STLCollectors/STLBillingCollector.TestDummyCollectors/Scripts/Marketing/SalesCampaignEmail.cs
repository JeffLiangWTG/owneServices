namespace CargoWise.RefDbRepo.STLBillingCollector.TestDummyCollectors.Marketing
{
	#region SuppressResourceStringsCheckRegion
	public class SalesCampaignEmail : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "CME";
		public override string RoleName => "CRM / Sales Management / HRM";
		public override string ModuleName => "Sales and Marketing System";
		public override string FunctionName => "Sales and Marketing Power Functions";
		public override string FeatureName => "Campaign Manager - Email Creation and Transmission";
		public override string CompanyCode => "gc.GC_Code";
		public override string TransactionDateUtc => "g8.G8_SystemCreateTimeUtc";
		public override string BillingReference1 => "g0.G0_CampaignName";
		public override string BillingReference2 => "g8.G8_SystemCreateUser";
		public override string GuidReference => "g0.G0_PK";
		public override string CreatingUserCode => "g8.G8_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					GlbCompanyCampaign g0
					INNER JOIN dbo.GlbCompanyCampaignItem g8 ON g8.G8_G0 = g0.G0_PK
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = g0.G0_GC";
		public override string WhereClause => @"
				g8.G8_DeliveryMethod = 'EML'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "22.11.29.231";
	}
	#endregion
}
