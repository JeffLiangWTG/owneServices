namespace CargoWise.Billing.Collectors.Customs
{
	public class CaptureLATAMBillingCount : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override string FeatureCode => "LMN";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Pre-Departure, Advanced Filing and Embargo";
		public override string FunctionName => "Embargo / Parties of Interest etc";
		public override string FeatureName => "Pre-Departure Manifest (eManifest, AMS, ACI, ICS, AFR etc)";
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "ab.ABL_SystemCreateTimeUtc";
		public override string CreatingUserCode => "ab.ABL_SystemCreateUser";
		public override string GuidReference => "ab.ABL_PK";
		public override string BillingReference1 => "am.AMA_JobReference";
		public override string BillingReference2 => "'MAWB: ' + ab1.ABL_BillNumber";
		public override string BillingReference3 => "'HAWB: ' + ab.ABL_BillNumber";
		public override string BillingReference4 => "am.AMA_RN_NKCountry";
		public override bool WithOptionRecompile => false;
		public override string TransactionCount => "1";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"AsycudaBill ab
								INNER JOIN dbo.AsycudaManifestHeader am ON am.AMA_PK = ab.ABL_AMA
								INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = am.AMA_GB
								INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
								INNER JOIN dbo.AsycudaBill ab1 ON ab1.ABL_AMA = am.AMA_PK
								AND ab1.ABL_BolType = 'BOL'";
		public override string WhereClause => @"ab.ABL_BolType = 'STD'
								AND am.AMA_ManifestType = 'MAN'
								AND am.AMA_RN_NKCountry IN ('UY', 'MX', 'CL', 'AR', 'CO', 'BR', 'CR', 'PA', 'PY', 'PE', 'DO', 'EC')";
		public override bool UsedInBilling => true;

		#endregion
	}
}
