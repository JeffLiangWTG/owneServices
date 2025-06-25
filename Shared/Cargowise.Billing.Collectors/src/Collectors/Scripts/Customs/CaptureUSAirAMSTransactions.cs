namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	sealed public class CaptureUSAirAMSTransactions : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "AM3";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Pre-Departure, Advanced Filing and Embargo";
		public override string FunctionName => "Embargo / Parties of Interest etc";
		public override string FeatureName => "US Air AMS";
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "am.AMA_SystemCreateTimeUtc";
		public override string CreatingUserCode => "am.AMA_SystemCreateUser";
		public override string GuidReference => "am.AMA_PK";
		public override string BillingReference1 => "am.AMA_JobReference";
		public override string BillingReference2 => "'MAWB: ' + ab.ABL_BillNumber";
		public override string TransactionCount => "1";
		public override string FromClause => @"AsycudaBill ab
								INNER JOIN dbo.AsycudaManifestHeader am ON am.AMA_PK = ab.ABL_AMA
								INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = am.AMA_GB
								INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
								LEFT JOIN dbo.StmALog sl ON am.AMA_PK = sl.SL_Parent
									AND CHARINDEX('TYP=HVL', SL_Reference) > 0
									AND SL_IsCancelled = 'N'
									AND SL_SE_NKEvent = 'TRF'
									AND SL_Table = 'AsycudaManifestHeader'";
		public override string WhereClause => @"ab.ABL_BolType = 'BOL'
								AND am.AMA_ManifestType = 'IAM'
								AND sl.SL_PK IS NULL";
		public override bool WithOptionRecompile => false;
		public override bool UsedInBilling => true;
		public override string ActiveOn => RefActiveOn.All;
		public override string DateType => RefStlDateType.DateTime;
	}

	#endregion
}
