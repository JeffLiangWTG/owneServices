namespace CargoWise.Billing.Collectors.Customs
{
	public class IntransitInBondUnderbondMovementRequestTWTransshipment : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override string FeatureCode => "TWT";
		public override string FeatureName => "Intransit / InBond / Underbond Movement Request - TW - Transshipment";
		public override string TransactionDateUtc => "CE_SystemCreateTimeUtc";
		public override string GuidReference => "BH_PK";
		public override string FromClause => "CusInBondHeader INNER JOIN dbo.GlbBranch ON GB_PK = BH_GB INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK INNER JOIN dbo.CusEntryNum ON CE_ParentID = BH_PK AND CE_Category = 'CUS' AND CE_EntryType = 'TRS' AND CE_RN_NKCountryCode = 'TW' AND CE_ParentTable = 'CusInBondHeader'";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs & Other Government Communication";
		public override string FunctionName => "Special Entry Types";
		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string BillingReference1 => "'Job Reference:' + BH_JobReference";
		public override string BillingReference2 => "'Entry Number:' + CE_EntryNum";
		public override string WhereClause => "BH_ApplicationCode = 'TW' AND GC_RN_NKCountryCode = 'TW'";
		public override string CreatingUserCode => "CE_SystemCreateUser";

		#endregion
	}
}
