namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class ETradeCountTransaction : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "ETD";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs";
		public override string FunctionName => "E-Trade Bills";
		public override string FeatureName => "E-Trade Bills Count";
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "ce.CE_SystemCreateTimeUtc";
		public override string CreatingUserCode => "ama.AMA_SystemCreateUser";
		public override string GuidReference => "ama.AMA_PK";
		public override string BillingReference1 => "ce.CE_EntryNum";
		public override string BillingReference2 => "ama.AMA_JobReference";
		public override string BillingReference3 => "abl.ABL_BillNumber";
		public override string TransactionCount => "1";
		public override string FromClause => @"
					AsycudaBill abl
					INNER JOIN AsycudaManifestHeader ama ON ama.AMA_PK = abl.ABL_AMA AND ama.AMA_ApplicationCode = 'ETR'
					INNER JOIN CusEntryNum ce ON ce.CE_ParentID = ama.AMA_PK AND ce.CE_ParentTable = 'AsycudaManifestHeader' AND ce.CE_EntryType = 'ASY'
					INNER JOIN GlbBranch gb ON gb.GB_PK = ama.AMA_GB
					INNER JOIN GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"gc.GC_RN_NKCountryCode = 'TR' AND abl.ABL_BolType != 'BOL'";
		public override bool WithOptionRecompile => false;
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string DateType => RefStlDateType.DateTime;
	}

	#endregion
}
