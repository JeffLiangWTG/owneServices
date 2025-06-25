namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class WarehouseScanPacking : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "SPK";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Warehouse Functions";
		public override string FeatureName => "Scan Packing";
		public override string TransactionDateUtc => "sl.SL_PostedTimeUtc";
		public override string BillingReference1 => "kj.KJ_JobID";
		public override string GuidReference => "kj.KJ_PK";
		public override string CreatingUserCode => "sl.SL_GS_NKUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					StmALog sl
					INNER JOIN dbo.PkgPackageJob kj ON kj.KJ_PK = sl.SL_Parent";
		public override string WhereClause => "sl.SL_SE_NKEvent = 'ADD'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "23.2.24.897";
	}

	#endregion
}
