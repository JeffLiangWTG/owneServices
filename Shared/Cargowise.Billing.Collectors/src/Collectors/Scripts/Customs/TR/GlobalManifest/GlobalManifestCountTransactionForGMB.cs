namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class GlobalManifestCountTransactionForGMB : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "GMB";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs & Other Government Communication";
		public override string FunctionName => "Special Entry Types";
		public override string FeatureName => "Turkish Manifest Bill";
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "ce.CE_SystemCreateTimeUtc";
		public override string CreatingUserCode => "abl.ABL_SystemCreateUser";
		public override string GuidReference => "abl.ABL_PK";
		public override string BillingReference1 => "ce.CE_EntryNum";
		public override string BillingReference2 => "ama.AMA_JobReference";
		public override string BillingReference3 => "abl.ABL_BillNumber";
		public override string TransactionCount => "1";
		public override string FromClause => @"AsycudaBill abl 
		INNER JOIN dbo.AsycudaManifestHeader ama ON ama.AMA_PK = abl.ABL_AMA and (ama.AMA_ApplicationCode = 'NVC' or ama.AMA_ApplicationCode = 'VOC') and ama.AMA_RN_NKCountry = 'TR'
		INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = ama.AMA_GB
		INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
		INNER JOIN dbo.CusEntryNum ce ON ce.CE_ParentID = ama.AMA_PK AND ce.CE_ParentTable = 'AsycudaManifestHeader' and ce.CE_EntryType = 'ASY'";
		public override string WhereClause => @"abl.ABL_BolType != 'BOL'";
		public override bool WithOptionRecompile => false;
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string DateType => RefStlDateType.DateTime;
	}

	#endregion
}
