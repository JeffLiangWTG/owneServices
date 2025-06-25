namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class Warehouse3plOrderLine_Pre_24_2_10_70 : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "WOL";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Warehouse Functions";
		public override string FeatureName => "Outward Order Line";
		public override string TransactionDateUtc => "wp.WP_FinalizedDateUtc";
		public override string GuidReference => "we.WE_PK";
		public override string BillingReference1 => "ww.WW_WarehouseCode + ' - ' + CASE WHEN ww.WW_IsVirtualWarehouse = 1 THEN 'Y' ELSE 'N' END";
		public override string BillingReference2 => "'#' + convert(varchar(10), we.WE_LineNo) + '.' + convert(varchar(10), we.WE_SubLineNo)";
		public override string BillingReference3 => "wd.WD_DocketID";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(SELECT ww.WW_WarehouseName AS [Warehouse Name] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string CreatingUserCode => "we.WE_SystemCreateUser";
		public override string FromClause => @"
					WhsDocket wd
					INNER JOIN dbo.WhsPick wp ON wp.WP_PK = wd.WD_WP
					INNER JOIN dbo.WhsDocketLine we ON we.WE_WD = wd.WD_PK
					INNER JOIN dbo.WhsWarehouse ww ON ww.WW_PK = wd.WD_WW_WHS
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = ww.WW_GB_RelatedCompanyBranch
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"
					wd.WD_DocketType = 'ORD'
					AND wd.WD_DocketSubType <> 'CUS'
					AND wp.WP_FinalizedDateUtc IS NOT NULL
					AND wd.WD_OrderClassification = 'BLK'";
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string MinCW1Version => "22.8.23.90";
		public override string MaxCW1Version => "24.2.10.69";
	}

	#endregion
}
