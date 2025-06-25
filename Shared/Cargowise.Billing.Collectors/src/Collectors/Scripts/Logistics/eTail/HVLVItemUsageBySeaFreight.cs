namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class HVLVItemUsageBySeaFreight : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "HTS";
		public override string RoleName => "Ecommerce Transport Modes";
		public override string ModuleName => "Ecommerce";
		public override string FunctionName => "Ecommerce Sea";
		public override string FeatureName => "Ecommerce Sea Freight Transport Mode";
		public override string CompanyCode => "''";
		public override string BranchCode => "''";
		public override string TransactionDateUtc => "HVI_DestinationFirstUsageTimeUtc";
		public override string BillingReference1 => "HVI_ItemId";
		public override string BillingReference2 => "HVI_CurrentBarcode";
		public override string BillingReference4 => "HVI_ShipperReference";
		public override string GuidReference => "HVI_PK";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"HVLVItem
JOIN dbo.JobShipment ON JS_PK = HVI_JS_LoadedOnShipment";
		public override string WhereClause => "JobShipment.JS_TransportMode = 'SEA'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.3.16.344";
	}

	#endregion
}
