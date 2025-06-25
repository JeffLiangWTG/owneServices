namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class OtherPartiesForwarderAirCargoNz : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "ANZ";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs Messaging (non Broker Agent)";
		public override string FunctionName => "Forwarder/CFS/CTO Functions";
		public override string FeatureName => "Import/Export Air Cargo Report (NZ)";
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
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"
					cm.CM_ApplicationCode in ('NZE', 'TSW')
					AND gc.GC_RN_NKCountryCode = 'NZ'
					AND cs.CS_IsHVLV = 0";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.8.541";
	}
	#endregion
}
