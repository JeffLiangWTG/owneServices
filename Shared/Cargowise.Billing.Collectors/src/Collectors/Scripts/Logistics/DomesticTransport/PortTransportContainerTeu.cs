namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion
	public class PortTransportContainerTeu : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "PTT";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Domestic Transport";
		public override string FunctionName => "Port Transport";
		public override string FeatureName => "Port Transport Container TEU Count";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "jc.JC_SystemCreateTimeUtc";
		public override string BillingReference1 => "jj.JJ_ConsignmentID";
		public override string BillingReference2 => "jc.JC_ContainerNum";
		public override string GuidReference => "jc.JC_PK";
		public override string CreatingUserCode => "jc.JC_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					JobCartage jj
					INNER JOIN dbo.JobBookedCtgMove ew ON ew.EW_JJ = jj.JJ_PK
					INNER JOIN dbo.JobContainer jc on jc.JC_PK = ew.EW_JC_Container
					INNER JOIN dbo.RefContainer rc ON rc.RC_PK = jc.JC_RC
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = jj.JJ_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string TransactionCount => "rc.RC_TEU";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string WhereClause => string.Empty;
		public override string MinCW1Version => "22.12.29.221";
	}
	#endregion
}
