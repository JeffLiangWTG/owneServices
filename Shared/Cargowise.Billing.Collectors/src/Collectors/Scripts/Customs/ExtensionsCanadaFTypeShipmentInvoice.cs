namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class ExtensionsCanadaFTypeShipmentInvoice : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "LVI";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "ediCustomsExtensions";
		public override string FunctionName => "CA Customs";
		public override string FeatureName => "F-Type Shipment Invoice";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "jz.JZ_SystemCreateTimeUtc";
		public override string BillingReference1 => "je.JE_DeclarationReference";
		public override string BillingReference2 => "jz.JZ_InvoiceNumber";
		public override string GuidReference => "jz.JZ_PK";
		public override string CreatingUserCode => "jz.JZ_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
dbo.JobDeclaration je
INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = je.JE_GB
INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
INNER JOIN dbo.JobComInvoiceHeader jz ON jz.JZ_ClusterKey = je.JE_ClusterKey AND jz.JZ_JE = je.JE_PK AND jz.JZ_GroupInvoice = 0
";
		public override string WhereClause => @"
je.JE_MessageType = 'LVS'
AND gc.GC_RN_NKCountryCode = 'CA'
";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.6.388";
	}
	#endregion
}
