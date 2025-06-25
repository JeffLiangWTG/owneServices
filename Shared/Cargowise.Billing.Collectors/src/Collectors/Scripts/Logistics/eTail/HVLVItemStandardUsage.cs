namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class HVLVItemStandardUsage : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "HVD";
		public override string RoleName => "Ecommerce Standard";
		public override string ModuleName => "Ecommerce";
		public override string FunctionName => "Ecommerce Standard Functions";
		public override string FeatureName => "Ecommerce Standard";
		public override string CompanyCode => "''";
		public override string BranchCode => "StmALog.SL_GB_NKBranch";
		public override string TransactionDateUtc => "HVI_DestinationFirstUsageTimeUtc";
		public override string BillingReference1 => "HVI_ItemId";
		public override string BillingReference2 => "HVI_CurrentBarcode";
		public override string BillingReference4 => "HVI_ShipperReference";
		public override string GuidReference => "HVI_PK";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"HVLVItem
					LEFT JOIN dbo.StmALog ON StmALog.SL_Parent = HVLVItem.HVI_PK
					AND StmALog.SL_SE_NKEvent = 'ADD'";
		public override string WhereClause => "HVI_UsageType = 'S'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.3.16.344";
	}

	#endregion
}
