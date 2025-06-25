namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class Warehouse3plLoads : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "WLO";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Warehouse Functions";
		public override string FeatureName => "Contract (Product) Warehouse (3PL Loads)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "wlo.WLO_SystemCreateTimeUtc";
		public override string BillingReference1 => "ww.WW_WarehouseCode + ' - ' + CASE WHEN ww.WW_IsVirtualWarehouse = 1 THEN 'Y' ELSE 'N' END";
		public override string BillingReference2 => "wlo.WLO_JobID";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(SELECT ww.WW_WarehouseName AS [Warehouse Name] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string GuidReference => "wlo.WLO_PK";
		public override string CreatingUserCode => "wlo.WLO_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
			dbo.WhsLoad wlo
			INNER JOIN dbo.WhsLocation wl ON wlo.WLO_WL_PlannedDockDoor = wl.WL_PK
			INNER JOIN dbo.WhsArea wa ON wl.WL_WA_PutawayArea = wa.WA_PK
			INNER JOIN dbo.WhsWarehouse ww ON wa.WA_WW_Whs = ww.WW_PK
			INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = ww.WW_GB_RelatedCompanyBranch
			INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => "wlo.WLO_CompleteTime IS NOT NULL";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;

	}

	#endregion
}
