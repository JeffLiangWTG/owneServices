namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class Warehouse3plBookedOrderLine : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "ECB";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Warehouse Functions";
		public override string FeatureName => "Outward Carrier Booked eCommerce Order Line";
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
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
					LEFT JOIN dbo.WhsDocket receive ON receive.WD_WP_ParentPickForReceive = wp.WP_PK";
		public override string WhereClause => @"
					wd.WD_DocketType = 'ORD'
					AND wd.WD_DocketSubType <> 'CUS'
					AND wd.WD_BookedWithCBADateTimeUtc IS NOT NULL
					AND wp.WP_FinalizedDateUtc IS NOT NULL
					AND wd.WD_OrderClassification IN ('ECO', 'UNK')
					AND
					(
						we.WE_WE_ParentDocketLine IS NULL
						OR
						we.WE_WE_ParentDocketLine IS NOT NULL AND receive.WD_PK IS NULL
					)";
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string MinCW1Version => "24.2.10.70";
	}

	#endregion
}
