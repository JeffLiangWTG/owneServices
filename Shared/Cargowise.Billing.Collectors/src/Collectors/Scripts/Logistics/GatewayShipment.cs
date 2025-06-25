namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class GatewayShipment : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "GSH";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "Gateway Operations";
		public override string FeatureName => "Gateway Shipments";
		public override string DataGranularity => "TRN";
		public override string CompanyCode => "gc.GC_Code";
		public override string TransactionDateUtc => "ABH_EventTimeUtc";
		public override string CreatingUserCode => "ABH_GS_NKEventUser";
		public override string GuidReference => "ABH_ParentId";
		public override string BillingReference1 => "ABH_ParentReferenceNumber";
		public override string BillingReference2 => "ABH_InternalReferenceNumber";
		public override string TransactionCount => "ABH_BillingCounter";
		public override string FromClause => @"
					AccBillingHeader
					LEFT JOIN dbo.GlbCompany gc ON gc.GC_PK = ABH_GC_Company";
		public override string WhereClause => "ABH_BillingCode = 'GSH'";
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string MinCW1Version => "21.11.12.194"; //Minimum CW1 version that contains AccBillingHeader table
		public override string BranchCode => string.Empty;
	}

	#endregion
}
