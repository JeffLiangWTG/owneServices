namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class CommunicationImportClearanceGeneral_Pre_24_10_22_62 : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "IFG";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Customs & Other Government Communication";
		public override string FunctionName => "Import Formal Customs Entry Compliance";
		public override string FeatureName => "Import Clearance Full / Detailed Formal Entry / Fiscal Report (General)";
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
					(
						je.JE_TransportMode NOT IN ('TRK', 'RAI', 'ROA')
						AND
						(
							(gc.GC_RN_NKCountryCode IN ('US', 'PR', 'GB', 'CA') AND je.JE_MessageType = 'IMP')
							OR (gc.GC_RN_NKCountryCode = 'SG' AND je.JE_MessageType IN ('IPT', 'INP'))
							OR (gc.GC_RN_NKCountryCode = 'AU' AND je.JE_MessageType IN ('IMP', 'EXW'))
							OR (gc.GC_RN_NKCountryCode = 'NZ' AND je.JE_MessageType = 'IMP' AND je.JE_MessageSubType IN ('NOR', 'SIM', 'TMP', 'SIT', 'PER', 'COM', 'IPI'))
						)
					)
				)";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.11.21.50";
		public override string MaxCW1Version => "24.10.22.18";
	}
	#endregion
}
