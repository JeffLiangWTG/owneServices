namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class Warehouse4plLongTermBondOutwards : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "W4O";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "WarehouseManager";
		public override string FunctionName => "Warehouse Functions";
		public override string FeatureName => "Contract (Product) Warehouse (4PL and Long Term Bond) - Outwards";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "WD.WD_SystemCreateTimeUtc";
		public override string BillingReference1 => "ww.WW_WarehouseCode + ' - ' + CASE WHEN ww.WW_IsVirtualWarehouse = 1 THEN 'Y' ELSE 'N' END";
		public override string BillingReference2 => "WD.WD_DocketID";
		public override string BillingReference3 => "WD.WD_CustomerReference";
		public override string BillingReference4 => "Attribute.WB_DeclarationReference";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
	(SELECT ww.WW_WarehouseName AS [Warehouse Name] FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string GuidReference => "WD.WD_PK";
		public override string CreatingUserCode => "WD.WD_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"		
					(SELECT DISTINCT 
						WDL.WE_WD, WBA.WB_EntryKey, WBA.WB_DeclarationReference
					FROM 
						dbo.WhsBondedWarehouseAttribute WBA
						INNER JOIN dbo.WhsDocketLine WDL on WDL.WE_PK = WBA.WB_ParentID and WBA.WB_ParentTableCode = 'WE'
					) Attribute
					INNER JOIN dbo.WhsDocket WD on Attribute.WE_WD = WD.WD_PK
					INNER JOIN dbo.WhsWarehouse WW ON WW.WW_PK = WD.WD_WW_WHS
					INNER JOIN dbo.GlbBranch gb ON GB.GB_PK = WW.WW_GB_RelatedCompanyBranch
					INNER JOIN dbo.GlbCompany gc ON GC.GC_PK = GB.GB_GC ";
		public override string WhereClause => @" Attribute.WB_DeclarationReference != ''
					AND WD.WD_CustomerReference != ''
					AND WD.WD_DocketStatus != 'CAN'
					AND WD.WD_DocketType = 'ORD'
					AND WD.WD_DocketSubType = 'CUS' ";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.2.24.897";
	}

	#endregion
}
