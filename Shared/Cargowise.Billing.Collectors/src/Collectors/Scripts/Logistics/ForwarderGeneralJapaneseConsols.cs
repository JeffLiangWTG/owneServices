namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion
	public class ForwarderGeneralJapaneseConsols : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "JPC";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "General Forwarding Engine";
		public override string FeatureName => "Japanese Consols";
		public override string TransactionDateUtc => "ce.CE_SystemCreateTimeUtc";
		public override string BillingReference1 => "js.JS_UniqueConsignRef";
		public override string BillingReference2 => "ce.CE_EntryNum";
		public override string GuidReference => "ce.CE_PK";
		public override string CreatingUserCode => "ce.CE_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					JobShipment js
					INNER JOIN dbo.CusEntryNum ce
						ON ce.CE_ParentID = js.JS_PK 
						AND ce.CE_EntryType = 'INS'
						AND ce.CE_RN_NKCountryCode = 'JP'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string WhereClause => string.Empty;
		public override string MinCW1Version => "23.1.20.174";
	}
	#endregion
}
