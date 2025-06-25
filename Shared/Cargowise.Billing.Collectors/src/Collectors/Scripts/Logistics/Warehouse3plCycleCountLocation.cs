namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class Warehouse3plCycleCountLocation : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "WCL";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Warehouse Functions";
		public override string FeatureName => "Contract (Product) Warehouse (3PL Cycle Count Locations)";
		public override string CompanyCode => "gc.GC_Code";
		public override string MinCW1Version => "24.9.14.137";	//The version where WCL_JobID became available.
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "wcl.WCL_SystemCreateTimeUtc";
		public override string BillingReference1 => "ww.WW_WarehouseCode + ' - ' + CASE WHEN ww.WW_IsVirtualWarehouse = 1 THEN 'Y' ELSE 'N' END";
		public override string BillingReference2 => "wcl.WCL_JobID";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(SELECT ww.WW_WarehouseName AS [Warehouse Name] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string GuidReference => "wcl.WCL_PK";
		public override string CreatingUserCode => "wcl.WCL_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
			dbo.WhsCycleCountLocation wcl
			INNER JOIN dbo.WhsLocation wl ON wcl.WCL_WL_Location = wl.WL_PK
			INNER JOIN dbo.WhsArea wa ON wl.WL_WA_PutawayArea = wa.WA_PK
			INNER JOIN dbo.WhsWarehouse ww ON wa.WA_WW_Whs = ww.WW_PK
			INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = ww.WW_GB_RelatedCompanyBranch
			INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => "wcl.WCL_EndTime IS NOT NULL";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;

	}

	#endregion
}
