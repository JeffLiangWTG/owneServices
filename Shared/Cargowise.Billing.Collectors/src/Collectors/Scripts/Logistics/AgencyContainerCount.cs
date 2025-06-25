namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion
	public class AgencyContainerCount : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "ACO";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Liner and Agency";
		public override string FunctionName => "Container Management";
		public override string FeatureName => "Shipping Container Count";
		public override string TransactionDateUtc => "jc.JC_SystemCreateTimeUtc";
		public override string CreatingUserCode => "jc.JC_SystemCreateUser";
		public override string GuidReference => "jc.JC_PK";
		public override string BillingReference1 => "js.JS_UniqueConsignRef";
		public override string BillingReference2 => "jc.JC_ContainerNum";
		public override string FromClause => @"
					JobShipment js
					INNER JOIN dbo.JobContainer jc ON jc.JC_JS_FCLBookingOnlyLink = js.JS_PK";
		public override string WhereClause => @"
					js.JS_IsShipping = 1
					AND js.JS_IsForwardRegistered = 0
					AND js.JS_IsCFSRegistered = 0";
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "22.11.8.377";
	}
	#endregion
}
