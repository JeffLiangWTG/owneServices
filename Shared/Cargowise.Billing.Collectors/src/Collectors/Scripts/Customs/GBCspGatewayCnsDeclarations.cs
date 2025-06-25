namespace CargoWise.Billing.Collectors.Customs
{
	public class GBCspGatewayCnsDeclarations : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "CS3";

		public override string RoleName => "Customs & Country Specific Integrations";

		public override string ModuleName => "Customs";

		public override string FunctionName => "Declarations by CSP Gateway (GB)";

		public override string FeatureName => "CNS - Community Network Services Count";

		public override string DataGranularity => "TRN";

		public override string CompanyCode => "gc.GC_Code";

		public override string BranchCode => "gb.GB_Code";

		public override string TransactionDateUtc => "je.JE_SystemCreateTimeUtc";

		public override string CreatingUserCode => "je.JE_SystemCreateUser";

		public override string GuidReference => "je.JE_PK";

		public override string BillingReference1 => "je.JE_DeclarationReference";

		public override string BillingReference3 => "GC_RN_NKCountryCode";

		public override string BillingReference4 => "je.JE_MessageType";

		public override string FromClause => @"JobDeclaration je INNER JOIN GlbBranch gb ON gb.GB_PK = je.JE_GB INNER JOIN GlbCompany gc ON gc.GC_PK = gb.GB_GC CROSS APPLY dbo.csfn_GetAddInfoValueFromCodeInline(JE_AddInfo, 'Gateway') as csp";

		public override string WhereClause => @"gc.GC_RN_NKCountryCode = 'GB' AND csp.Value = 'CNS'";

		public override bool UsedInBilling => false;
	}
}
