namespace CargoWise.RefDbRepo.STLBillingCollector.TestDummyCollectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class GlobalManifestCountTransactionForGMH : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "GMH";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs & Other Government Communication";
		public override string FunctionName => "Special Entry Types";
		public override string FeatureName => "Turkish Manifest Header";
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "ama.AMA_SystemCreateTimeUtc";
		public override string CreatingUserCode => "AMA_SystemCreateUser";
		public override string GuidReference => "ama.AMA_PK";
		public override string BillingReference1 => "ama.AMA_JobReference";
		public override string TransactionCount => "1";
		public override string FromClause => @"AsycudaManifestHeader ama 
		INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = ama.AMA_GB 
		INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"ama.AMA_RN_NKCountry = 'TR' AND (ama.AMA_ApplicationCode = 'NVC' OR ama.AMA_ApplicationCode = 'VOC')";
		public override bool WithOptionRecompile => false;
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string DateType => RefStlDateType.DateTime;
	}

	#endregion
}
