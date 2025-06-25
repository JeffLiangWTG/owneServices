namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class CaptureNZOCRManifestsBills : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "CTM";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs and Port Messaging";
		public override string FunctionName => "Forwarder/CFS/CTO/CY/Land Transport Message";
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "am.AMA_SystemCreateTimeUtc";
		public override string CreatingUserCode => "am.AMA_SystemCreateUser";
		public override string GuidReference => "am.AMA_PK";
		public override string BillingReference1 => "'Manifest: ' + am.AMA_JobReference";
		public override string BillingReference2 => "'MAWB: ' + ab.ABL_BillNumber";
		public override string TransactionCount => "1";
		public override string FromClause => @"AsycudaBill ab
								INNER JOIN dbo.AsycudaManifestHeader am ON am.AMA_PK = ab.ABL_AMA
								INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = am.AMA_GB
								INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"ab.ABL_BolType = 'BOL'
								AND am.AMA_ManifestType = 'OCR'
								AND am.AMA_RN_NKCountry = 'NZ'";
		public override bool WithOptionRecompile => false;
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string FeatureName => "NZ Manifest OCR";
	}
	#endregion
}
