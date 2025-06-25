namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class OtherPartiesForwarderAirCargoCtoOutward : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "CTW";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs Messaging (non Broker Agent)";
		public override string FunctionName => "Forwarder/CFS/CTO Functions";
		public override string FeatureName => "Import/Export Air Cargo CFS/CTO Customs Functions (Outward)";
		public override string TransactionDateUtc => "ce.CE_SystemCreateTimeUtc";
		public override string BillingReference1 => "'Entry No: ' + ce.CE_EntryNum";
		public override string GuidReference => "ce.CE_PK";
		public override string CreatingUserCode => "ce.CE_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => "CusEntryNum ce";
		public override string WhereClause => @"
					ce.CE_RN_NKCountryCode = 'NZ'
					AND ce.CE_Category = 'CUS'
					AND ce.CE_EntryType = 'ORN'
					AND ce.CE_EntryNum <> '00000000'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "22.12.8.541";
	}
	#endregion
}
