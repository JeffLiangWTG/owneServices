namespace CargoWise.RefDbRepo.STLBillingCollector.TestDummyCollectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class SPTSDeclarationsCount : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "SPD";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs";
		public override string FunctionName => "SPTS Declarations Intransit Movement Request";
		public override string FeatureName => "SPTS Declarations Count";
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "ce.CE_SystemCreateTimeUtc";
		public override string CreatingUserCode => "bh.BH_SystemCreateUser";
		public override string GuidReference => "bh.BH_PK";
		public override string BillingReference1 => "ce.CE_EntryNum";
		public override string BillingReference2 => "bh.BH_JobReference";
		public override string TransactionCount => "1";
		public override string FromClause => @"CusInBondHeader bh
					INNER JOIN dbo.CusEntryNum ce ON ce.CE_ParentID = bh.BH_PK and ce.CE_EntryType = 'SPT' AND CE_RN_NKCountryCode = 'TR' AND ce.CE_ParentTable = 'CusInBondHeader' AND ce.CE_EntryNum <> ''
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = bh.BH_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"gc.GC_RN_NKCountryCode = 'TR' AND bh.BH_ApplicationCode = 'SPT'";
		public override bool WithOptionRecompile => false;
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string DateType => RefStlDateType.DateTime;
	}

	#endregion
}
