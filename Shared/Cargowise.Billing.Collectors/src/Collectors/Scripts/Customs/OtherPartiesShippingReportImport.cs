namespace CargoWise.Billing.Collectors.Customs
{
	#region SuppressResourceStringsCheckRegion
	public class OtherPartiesShippingReportImport : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "SRI";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs Messaging (non Broker Agent)";
		public override string FunctionName => "Shipping/Agency Functions";
		public override string FeatureName => "Import/Export Shipping Customs Report (Import)";
		public override string TransactionDateUtc => "bt.BT_SystemCreateTimeUtc";
		public override string BillingReference1 => "bt.BT_SendersMessageReference";
		public override string BillingReference2 => "bo.BO_OceanBill";
		public override string GuidReference => "bt.BT_PK";
		public override string CreatingUserCode => "bt.BT_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					dbo.CusSeaManTranHead bt
					INNER JOIN dbo.CusSeaManOBLHeader bo on bo.BO_BT = bt.BT_PK";
		public override string WhereClause => "";
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
