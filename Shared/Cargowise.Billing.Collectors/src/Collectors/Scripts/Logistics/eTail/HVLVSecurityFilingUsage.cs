namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class HVLVSecurityFilingUsage : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "HSF";
		public override string RoleName => "Ecommerce Security Filings";
		public override string ModuleName => "Ecommerce";
		public override string FunctionName => "Ecommerce Security Functions";
		public override string FeatureName => "HVLV Item Security Filing";
		public override string CompanyCode => "''";
		public override string BranchCode => "HXU_BranchCode";
		public override string TransactionDateUtc => "HXU_UsageTimeUtc";
		public override string BillingReference1 => "HVI_ItemId";
		public override string BillingReference2 => "HVI_CurrentBarcode";
		public override string BillingReference3 => "HXU_Code";
		public override string BillingReference4 => "HVI_ShipperReference";
		public override string GuidReference => "HVI_PK";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"dbo.HVLVUsage
					INNER JOIN dbo.HVLVItem ON HVLVUsage.HXU_HVI_ParentItem = HVLVItem.HVI_PK";
		public override string WhereClause => "HXU_Category = 'SEC'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.7.28.306";
	}

	#endregion
}
