namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class CommunicationExportClearanceNonIntegrated : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "EFI";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs & Other Government Communication";
		public override string FunctionName => "Export Formal Clearance/Fiscal Report";
		public override string FeatureName => "Core Customs Processing (Export Clearance Non-Integrated)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "je.JE_SystemCreateTimeUtc";
		public override string BillingReference1 => "je.JE_DeclarationReference";
		public override string BillingReference2 => "gc.GC_RN_NKCountryCode";
		public override string GuidReference => "je.JE_PK";
		public override string CreatingUserCode => "je.JE_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
				JobDeclaration je
				INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = je.JE_GB
				INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"
			(
				gc.GC_RN_NKCountryCode NOT IN ('US', 'PR', 'SG', 'AU', 'NZ', 'GB', 'CA') 
				AND je.JE_MessageType = 'EXP' 
				AND je.JE_ApplicationCode IN ('ITF', '')
			)";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.11.21.50";
	}
	#endregion
}
