namespace CargoWise.Billing.Collectors.Scripts.Customs.EU
{
	#region SuppressResourceStringsCheckRegion

	public class CaptureH7DeclarationCount : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "H7C";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Import Formal Customs Entry Compliance";
		public override string FunctionName => "European Customs Low Value (H7)";
		public override string FeatureName => "European Customs Low Value Declaration (H7)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "ab.ABL_SystemCreateTimeUtc";
		public override string BillingReference1 => "am.AMA_JobReference";
		public override string BillingReference2 => "ab.ABL_BillNumber";
		public override string BillingReference3 => "am.AMA_RN_NKCountry";
		public override string GuidReference => "ab.ABL_PK";
		public override string CreatingUserCode => "ab.ABL_SystemCreateUser";
		public override string FromClause => @"dbo.AsycudaBill ab
				INNER JOIN dbo.AsycudaManifestHeader am ON ab.ABL_ClusterKey = am.AMA_ClusterKey
				INNER JOIN dbo.GlbStaff gs ON gs.GS_Code = ab.ABL_SystemCreateUser
				INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = am.AMA_GB
				INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
				LEFT JOIN dbo.StmALog sl ON am.AMA_PK = sl.SL_Parent
					AND CHARINDEX('TYP=HVL', SL_Reference) > 0
					AND sl.SL_SE_NKEvent = 'TRF'
					AND sl.SL_Table = 'AsycudaManifestHeader'";
		public override string WhereClause => @"am.AMA_ManifestType = 'EH7'
				AND ab.ABL_BolType <> 'BOL'
				AND sl.SL_PK IS NULL";
	}

	#endregion
}
