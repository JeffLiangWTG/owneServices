namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class ExtensionsMalasiaK4K5Manifest : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "K45";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "ediCustomsExtensions";
		public override string FunctionName => "MY Customs";
		public override string FeatureName => "K4/K5 Manifest";
		public override string TransactionDateUtc => "ce.CE_SystemCreateTimeUtc";
		public override string BillingReference1 => "ce.CE_EntryNum";
		public override string BillingReference2 => "js.JS_UniqueConsignRef";
		public override string GuidReference => "ce.CE_PK";
		public override string CreatingUserCode => "ce.CE_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					CusEntryNum ce
					INNER JOIN dbo.JobShipment js ON js.JS_PK = ce.CE_ParentID";
		public override string WhereClause => @"
					ce.CE_RN_NKCountryCode = 'MY'
					AND ce.CE_EntryType = 'MAN'
					AND ce.CE_Category = 'CUS'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "22.12.6.388";
	}
	#endregion
}
