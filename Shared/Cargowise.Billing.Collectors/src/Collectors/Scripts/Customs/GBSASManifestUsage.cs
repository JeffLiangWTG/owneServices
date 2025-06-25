namespace CargoWise.Billing.Collectors.Customs
{
	public class GBSASManifestUsage : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "GBS";

		public override string FeatureName => "GB S&S Manifest";

		public override string DataGranularity => RefStlItemGrain.Transactional;

		public override string TransactionDateUtc => "ama.AMA_SystemCreateTimeUTC";

		public override string CreatingUserCode => "ama.AMA_SystemCreateUser";

		public override string GuidReference => "ama.AMA_PK";

		public override string FromClause => @"AsycudaManifestHeader ama INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = ama.AMA_GB INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";

		public override string RoleName => "GB Customs S&S usage count";

		public override string ModuleName => "Usage Data";

		public override string FunctionName => "GB S&S manifest count";

		public override string CompanyCode => "gc.GC_Code";

		public override string BranchCode => "gb.GB_Code";

		public override string BillingReference1 => "ama.AMA_JobReference";

		public override string BillingReference2 => "ama.AMA_ManifestType";

		public override string WhereClause => @"ama.AMA_RN_NKCountry = 'GB' AND ama.AMA_ManifestType = 'S&S' AND ama.AMA_IsActive = 1";

		public override string TransactionCount => "1";

		public override string ActiveOn => "ALL";

		public override string DateType => RefStlDateType.DateTime;
	}
}
