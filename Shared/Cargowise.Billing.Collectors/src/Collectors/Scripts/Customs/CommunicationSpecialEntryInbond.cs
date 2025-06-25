namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class CommunicationSpecialEntryInbond : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "INB";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs & Other Government Communication";
		public override string FunctionName => "Special Entry Types";
		public override string FeatureName => "InBond / Underbond Movement Request (From Customs Menu) (Inbond)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "bm.BM_SystemCreateTimeUtc";
		public override string BillingReference1 => "bh.BH_JobReference";
		public override string BillingReference2 => "ce.CE_EntryNum";
		public override string GuidReference => "bm.BM_PK";
		public override string CreatingUserCode => "bm.BM_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					CusInbondHeader bh
					INNER JOIN dbo.CusInbondMoveHeader bm on bm.BM_BH = bh.BH_PK
					LEFT JOIN ( select CE_ParentID, CE_EntryNum from dbo.CusEntryNum where CE_ParentTable = 'CusInBondMoveHeader' and CE_EntryType = 'INB') ce on ce.CE_ParentID = bm.BM_PK
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = bh.BH_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"
					bh.BH_ApplicationCode = 'INB'
					AND gc.GC_RN_NKCountryCode in ('US', 'PR')";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.11.21.50";
	}
	#endregion
}
