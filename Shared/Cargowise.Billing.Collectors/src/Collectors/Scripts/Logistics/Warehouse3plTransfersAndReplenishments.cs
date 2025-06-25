namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class Warehouse3plTransfersAndReplenishments : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "W3T";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Warehouse Functions";
		public override string FeatureName => "Contract (Product) Warehouse (3PL Transfers and Replenishments)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "wd.WD_SystemCreateTimeUtc";
		public override string BillingReference1 => "ww.WW_WarehouseCode + ' - ' + CASE WHEN ww.WW_IsVirtualWarehouse = 1 THEN 'Y' ELSE 'N' END";
		public override string BillingReference2 => "wd.WD_DocketID";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(SELECT ww.WW_WarehouseName AS [Warehouse Name] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string GuidReference => "wd.WD_PK";
		public override string CreatingUserCode => "wd.WD_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					WhsDocket wd
					INNER JOIN dbo.WhsWarehouse ww ON ww.WW_PK = wd.WD_WW_WHS
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = ww.WW_GB_RelatedCompanyBranch
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"
					wd.WD_DocketType = 'TFR'
					AND wd.WD_DocketSubType <> 'CUS'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.2.24.897";
	}
	#endregion
}
