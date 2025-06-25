namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class HVLVOriginLoadListConversion : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "HLC";
		public override string RoleName => "Ecommerce Origin Load List";
		public override string ModuleName => "Ecommerce";
		public override string FunctionName => "Ecommerce Origin Load List Functions";
		public override string FeatureName => "HVLV Origin Load List Conversion";
		public override string TransactionDateUtc => "HCH_SystemCreateTimeUtc";
		public override string GuidReference => "HCH_PK";
		public override string BillingReference1 => "HCH_JobNumber";
		public override string BillingReference2 => "JS_UniqueConsignRef";
		public override string FromClause => "HVLVConsignmentHeader JOIN dbo.StmALog ON HCH_PK = SL_Parent JOIN dbo.JobShipment ON JS_PK = HCH_JS_Shipment";
		public override string WhereClause => "SL_SE_NKEvent = 'ELC'";
		public override bool UsedInBilling => false;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "22.5.18.90";
	}

	#endregion
}
