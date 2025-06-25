namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class ExtensionsEuropeanNctsMovement : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "NCT";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "NCTS";
		public override string FunctionName => "EU NCTS Movements";
		public override string FeatureName => "EU NCTS Departure Declaration or Arrival Notification Request";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "bh.BH_SystemCreateTimeUtc";
		public override string BillingReference1 => "'LRN: ' + bh.BH_JobReference";
		public override string BillingReference2 => "'MRN: ' + COALESCE(cen.CE_EntryNum, '(not yet accepted)')";
		public override string BillingReference3 => "gc.GC_RN_NKCountryCode";
		public override string BillingReference4 => "bh.BH_HeaderType";
		public override string GuidReference => "bh.BH_PK";
		public override string CreatingUserCode => "bh.BH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					dbo.CusInbondHeader bh
					LEFT JOIN dbo.CusEntryNum cen on BH_PK = cen.CE_ParentID and cen.CE_ParentTable = 'CusInBondHeader' and [CE_EntryType] = 'MRN'
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = bh.BH_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
					INNER JOIN RefDatabase_RefCusTradeGroupCountry ctgc ON ctgc.ZZB_RN_NKTradeGroupCountryCode = gc.GC_RN_NKCountryCode
					INNER JOIN RefDatabase_RefCusTradeGroup ctg ON ctg.ZZA_PK = ctgc.ZZB_ZZA_TradeGroup";
		public override string WhereClause => @"
					bh.BH_ApplicationCode IN ('NCT', 'NC5')
					AND ctg.ZZA_TradeGroup = 'EUCTP'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.6.388";
	}

	#endregion
}
