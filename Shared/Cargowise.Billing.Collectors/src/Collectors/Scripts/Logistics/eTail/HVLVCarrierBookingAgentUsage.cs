namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class HVLVCarrierBookingAgentUsage : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "HBA";
		public override string RoleName => "Ecommerce Carrier Bookings";
		public override string ModuleName => "Ecommerce";
		public override string FunctionName => "Ecommerce Carrier Bookings";
		public override string FeatureName => "HVLVCarrierBookingAgentUsage";
		public override string CompanyCode => "''";
		public override string BranchCode => "SL_GB_NKBranch";
		public override string TransactionDateUtc => "SL_PostedTimeUtc";
		public override string BillingReference1 => "HVI_ItemId";
		public override string BillingReference2 => "HVI_CurrentBarcode";
		public override string BillingReference4 => "HVI_ShipperReference";
		public override string GuidReference => "HVI_PK";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"HVLVItem INNER JOIN dbo.StmALog
ON HVI_PK = SL_Parent";
		public override string WhereClause => "SL_SE_NKEvent = 'BKC'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.3.16.344";
	}

	#endregion
}
