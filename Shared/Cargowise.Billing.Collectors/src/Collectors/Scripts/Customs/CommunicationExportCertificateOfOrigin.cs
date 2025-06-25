namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class CommunicationExportCertificateOfOrigin : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "COO";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs & Other Government Communication";
		public override string FunctionName => "Export Formal Customs/Statistics Entry Compliance";
		public override string FeatureName => "Certificate of Origin";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "je.JE_SystemCreateTimeUtc";
		public override string BillingReference1 => "je.JE_DeclarationReference";

		//We capture JH only for standalone declarations here, as the JobHeader of JE_JS is handled by the ForwarderGeneralShipment(SHP) collector.
		public override string BillingReference4 => "IIF(jh.JH_IsDisbursement = 1, CONVERT(VARCHAR(36), jh.JH_PK) , '')";
		public override string GuidReference => "je.JE_PK";
		public override string CreatingUserCode => "je.JE_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
				JobDeclaration je
				INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = je.JE_GB
				INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
				LEFT  JOIN dbo.JobHeader jh ON jh.JH_ParentID = je.JE_PK AND jh.JH_GC = je.JE_GC";
		public override string WhereClause => @"
				je.JE_MessageType = 'COO'
				AND gc.GC_RN_NKCountryCode = 'SG'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "24.10.22.62";
	}
	#endregion
}
