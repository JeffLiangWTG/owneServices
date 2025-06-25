namespace CargoWise.Billing.Collectors.Customs
{
	public class AdvanceCargoInformationFilingILForwarderManifest : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override string FeatureCode => "ILM";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs";
		public override string FunctionName => "Embargo / Parties of Interest etc.";
		public override string FeatureName => "Advance Cargo Information Filing - Israel - Forwarder Manifest";
		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string TransactionDateUtc => "AMA_SystemCreateTimeUtc";
		public override string CreatingUserCode => "AMA_SystemCreateUser";
		public override string GuidReference => "AMA_PK";
		public override string BillingReference1 => "'Job Reference:' + AMA_JobReference";
		public override string BillingReference2 => "'Master Bill:' + ABL_BillNumber";
		public override string FromClause => "AsycudaManifestHeader INNER JOIN GlbBranch ON GB_PK = AMA_GB INNER JOIN GlbCompany ON GB_GC = GC_PK LEFT JOIN AsycudaBill ON ABL_AMA = AMA_PK AND ABL_BolType = 'BOL'";
		public override string WhereClause => "AMA_ApplicationCode = 'NVC' AND AMA_RN_NKCountry = 'IL'";

		#endregion
	}
}
