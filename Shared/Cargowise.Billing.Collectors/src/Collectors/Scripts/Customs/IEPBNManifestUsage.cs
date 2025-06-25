namespace CargoWise.Billing.Collectors.Customs
{
	public class IEPBNManifestUsage : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "IEP";

		public override string FeatureName => "IE PBN Manifest";

		public override string TransactionDateUtc => "ama.AMA_SystemCreateTimeUTC";

		public override string GuidReference => "ama.AMA_PK";

		public override string FromClause =>
@"dbo.AsycudaManifestHeader ama
	INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = ama.AMA_GB
	INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";

		public override string RoleName => "IE Customs PBN usage count";

		public override string ModuleName => "Usage Data";

		public override string FunctionName => "IE PBN manifest count";

		public override string CompanyCode => "gc.GC_Code";

		public override string BranchCode => "gb.GB_Code";

		public override string BillingReference1 => "ama.AMA_JobReference";

		public override string WhereClause => "ama.AMA_RN_NKCountry = 'IE' AND ama.AMA_ManifestType = 'PBN'";
	}
}
