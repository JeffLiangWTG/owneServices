namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class ExtensionsGreatBritainImportAirAgent : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "ACA";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "ediCustomsExtensions";
		public override string FunctionName => "GB Customs";
		public override string FeatureName => "Import Air Cargo Agent";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "cs.CS_SystemCreateTimeUtc";
		public override string BillingReference1 => "'MAWB: ' + cm.CM_MAWB";
		public override string BillingReference2 => "'HAWB: ' + cs.CS_HAWB";
		public override string GuidReference => "cs.CS_PK";
		public override string CreatingUserCode => "cs.CS_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					CusMawb cm
					INNER JOIN dbo.CusHawb cs on cs.CS_CM = cm.CM_PK
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = cm.CM_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
					LEFT JOIN (
						SELECT distinct CS_CM
						FROM dbo.CusHawb
						WHERE CS_CM is not null
						AND CS_IsMasterHouse = 0
					) cshouse ON cshouse.CS_CM = cm.CM_PK";
		public override string WhereClause => @"
					cm.CM_ApplicationCode = 'CUK'
					AND cm.CM_IsCtoMawb = 0
					AND gc.GC_RN_NKCountryCode = 'GB'
					AND cs.CS_FolioReference like 'CUKFFW98%'
					AND (cs.CS_IsMasterHouse = 0 OR cshouse.CS_CM is null)";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.6.388";
	}
	#endregion
}
