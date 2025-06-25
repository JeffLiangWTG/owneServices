namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion
	public class AgencyContainerDetention : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "CDT";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Liner and Agency";
		public override string FunctionName => "Container Management";
		public override string FeatureName => "Container Detention";
		public override string CompanyCode => "gc.GC_Code";
		public override string TransactionDateUtc => "jcd.NC_SystemCreateTimeUtc";
		public override string CreatingUserCode => "jcd.NC_SystemCreateUser";
		public override string GuidReference => "jcd.NC_PK";
		public override string BillingReference1 => "jcd.NC_JobNumber";
		public override string FromClause => @"
					JobContainerDetention jcd
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = jcd.NC_GC";
		public override string BranchCode => string.Empty;
		public override string WhereClause => string.Empty;
		public override string MinCW1Version => "22.11.8.377";
	}
	#endregion
}
