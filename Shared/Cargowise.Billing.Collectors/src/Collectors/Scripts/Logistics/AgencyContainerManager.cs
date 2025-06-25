namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion
	public class AgencyContainerManager : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "CDM";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Liner and Agency";
		public override string FunctionName => "Container Management";
		public override string FeatureName => "Container Manager";
		public override string TransactionDateUtc => "R6_SystemCreateTimeUtc";
		public override string CreatingUserCode => "R6_SystemCreateUser";
		public override string GuidReference => "R6_PK";
		public override string BillingReference1 => "R6_ContainerNum";
		public override string FromClause => "RefContainerStock";
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string WhereClause => string.Empty;
		public override string MinCW1Version => "22.11.8.377";
	}
	#endregion
}
