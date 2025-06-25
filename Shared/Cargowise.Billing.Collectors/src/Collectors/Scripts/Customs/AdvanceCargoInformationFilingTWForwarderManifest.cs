namespace CargoWise.Billing.Collectors.Customs
{
	public class AdvanceCargoInformationFilingTWForwarderManifest : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override string FeatureCode => "TWF";
		public override string FeatureName => "Advance Cargo Information Filing - TW - Forwarder Manifest";
		public override string TransactionDateUtc => "AMA_SystemCreateTimeUtc";
		public override string GuidReference => "AMA_PK";
		public override string FromClause => "AsycudaManifestHeader INNER JOIN dbo.GlbBranch ON GB_PK = AMA_GB INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK LEFT JOIN dbo.AsycudaBill ON ABL_AMA = AMA_PK AND ABL_BolType = 'BOL'";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Pre-Departure, Advanced Filing and Embargo";
		public override string FunctionName => "Embargo / Parties of Interest etc";
		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string BillingReference1 => "'Job Reference:' + AMA_JobReference";
		public override string BillingReference2 => "'Master Bill:' + ABL_BillNumber";
		public override string WhereClause => "AMA_ManifestType = 'MAN' AND AMA_ApplicationCode = 'NVC' AND AMA_RN_NKCountry = 'TW'";
		public override string CreatingUserCode => "AMA_SystemCreateUser";

		#endregion
	}
}
