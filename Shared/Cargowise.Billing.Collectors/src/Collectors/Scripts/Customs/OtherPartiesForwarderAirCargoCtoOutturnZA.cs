namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion

	public class OtherPartiesForwarderAirCargoCtoOutturnZA : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "ZX4";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs Messaging (non Broker Agent)";
		public override string FunctionName => "Forwarder/CFS/CTO Functions";
		public override string FeatureName => "ZA Air Outturn and Gate In/Out functions";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "ama.AMA_SystemCreateTimeUTC";
		public override string BillingReference1 => "ama.AMA_JobReference";
		public override string BillingReference2 => "ama.AMA_ManifestType";
		public override string GuidReference => "ama.AMA_PK";
		public override string CreatingUserCode => "ama.AMA_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					AsycudaManifestHeader ama
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = ama.AMA_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"
					gc.GC_RN_NKCountryCode = 'ZA' 
					AND ama.AMA_ApplicationCode = 'OUT'
					AND ama.AMA_TransportMode = 'AIR'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.8.541";
	}

	#endregion
}
