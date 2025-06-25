namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class WarehouseTransitReceiptIntoDepot : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "WTH";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Transit Warehouse";
		public override string FeatureName => "Receipt into Depot / Warehouse";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "wrc.WRC_SystemCreateTimeUtc";
		public override string BillingReference1 => "wrc.WRC_JobID";
		public override string BillingReference2 => "ww.WW_WarehouseCode";
		public override string GuidReference => "wrc.WRC_PK";
		public override string CreatingUserCode => "wrc.WRC_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					WhsItemReceiveConsignment wrc
					INNER JOIN dbo.WhsWarehouse ww ON ww.WW_PK = wrc.WRC_WW_IntendedWarehouse
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = ww.WW_GB_RelatedCompanyBranch
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string WhereClause => string.Empty;
		public override string MinCW1Version => "23.2.24.897";
	}

	#endregion
}
