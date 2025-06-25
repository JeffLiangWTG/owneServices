namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class ExtensionsUsaProtest : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "USP";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "ediCustomsExtensions";
		public override string FunctionName => "US Customs";
		public override string FeatureName => "Protest";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "je.JE_SystemCreateTimeUtc";
		public override string BillingReference1 => "je.JE_DeclarationReference";
		public override string GuidReference => "je.JE_PK";
		public override string CreatingUserCode => "je.JE_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					JobDeclaration je
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = je.JE_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"
					je.JE_MessageType = 'PRO'
					AND gc.GC_RN_NKCountryCode in ('US', 'PR')";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.6.388";
	}
	#endregion
}
