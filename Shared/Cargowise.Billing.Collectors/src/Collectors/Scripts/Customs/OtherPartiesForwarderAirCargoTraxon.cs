namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class OtherPartiesForwarderAirCargoTraxon : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "AHK";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs Messaging (non Broker Agent)";
		public override string FunctionName => "Forwarder/CFS/CTO Functions";
		public override string FeatureName => "Import/Export Air Cargo Report (HK ISAC Traxon)";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "em.EM_SystemCreateTimeUtc";
		public override string BillingReference1 => "jk.JK_UniqueConsignRef";
		public override string BillingReference2 => "em.EM_MessageNum";
		public override string GuidReference => "em.EM_PK";
		public override string CreatingUserCode => "em.EM_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					JobConsol jk
					INNER JOIN dbo.EDIMessage em on em.EM_LinkUniqueID = jk.JK_PK
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = em.EM_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"
					em.EM_ApplicationCode = 'TRX'
					AND em.EM_ReceiveTransmit = 'TRX'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.8.541";
	}
	#endregion
}
