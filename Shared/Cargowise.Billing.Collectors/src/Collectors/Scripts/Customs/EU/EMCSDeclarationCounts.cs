namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class EMCSDeclarationCounts : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "EMC";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs & Other Government Communication";
		public override string FunctionName => "Special Entry Types";
		public override string FeatureName => "EU27 EMCS Messaging";
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "je.JE_SystemCreateTimeUtc";
		public override string CreatingUserCode => "je.JE_SystemCreateUser";
		public override string GuidReference => "je.JE_PK";
		public override string BillingReference1 => "je.JE_DeclarationReference";
		public override string TransactionCount => "1";
		public override string FromClause => @"
					JobDeclaration je
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = je.JE_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => "je.JE_ApplicationCode = 'EMC'";
		public override bool UsedInBilling => true;
		public override string ActiveOn => RefActiveOn.All;
		public override string DateType => RefStlDateType.DateTime;
	}
	#endregion
}
