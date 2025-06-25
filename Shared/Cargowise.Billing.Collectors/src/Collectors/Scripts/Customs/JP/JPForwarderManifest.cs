namespace CargoWise.Billing.Collectors.Customs
{
	public class JPForwarderManifest : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override string FeatureCode => "JPF";
		public override string FeatureName => "Japan Forwarder Manifest";
		public override string TransactionDateUtc => "ABL_SystemCreateTimeUtc";
		public override string GuidReference => "ABL_PK";
		public override string FromClause => "dbo.AsycudaBill INNER JOIN dbo.AsycudaManifestHeader ON AMA_PK = ABL_AMA INNER JOIN dbo.GlbBranch ON GB_PK = AMA_GB INNER JOIN dbo.GlbCompany ON GC_PK = GB_GC";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Pre-Departure, Advanced Filing and Embargo";
		public override string FunctionName => "Embargo / Parties of Interest etc.";
		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string BillingReference1 => "'Job Reference:' + AMA_JobReference";
		public override string BillingReference2 => "'House Bill:' + ABL_BillNumber";
		public override string WhereClause => "AMA_ApplicationCode = 'NVC' AND AMA_RN_NKCountry = 'JP' AND ABL_BolType <> 'BOL'";
		public override string CreatingUserCode => "ABL_SystemCreateUser";

		#endregion
	}
}
