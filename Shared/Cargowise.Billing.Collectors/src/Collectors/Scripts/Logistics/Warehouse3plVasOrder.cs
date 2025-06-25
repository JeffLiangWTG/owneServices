namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class Warehouse3plVasOrder : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "WVO";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Warehouse Functions";
		public override string FeatureName => "Contract (Product) Warehouse (3PL VAS Orders)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "wv.WVO_SystemCreateTimeUtc";
		public override string BillingReference1 => "ww.WW_WarehouseCode + ' - ' + CASE WHEN ww.WW_IsVirtualWarehouse = 1 THEN 'Y' ELSE 'N' END";
		public override string BillingReference2 => "wv.WVO_JobID";
		public override string BillingReference3 => "wv.WVO_CustomerReferenceNo";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(SELECT ww.WW_WarehouseName AS [Warehouse Name] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string GuidReference => "wv.WVO_PK";
		public override string CreatingUserCode => "wv.WVO_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					dbo.WhsVasOrder wv
					INNER JOIN dbo.WhsArea wa ON wv.WVO_WA_ServiceArea = wa.WA_PK
					INNER JOIN dbo.WhsWarehouse ww ON wa.WA_WW_Whs = ww.WW_PK
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = ww.WW_GB_RelatedCompanyBranch
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => string.Empty;
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;

	}

	#endregion
}
