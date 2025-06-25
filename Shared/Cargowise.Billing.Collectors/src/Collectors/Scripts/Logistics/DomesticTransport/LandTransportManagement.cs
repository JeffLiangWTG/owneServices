namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class LandTransportManagement : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "LTM";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Domestic Transport";
		public override string FunctionName => "Port Transport";
		public override string FeatureName => "Land Transport Management System";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "jj.JJ_SystemCreateTimeUtc";
		public override string BillingReference1 => "jj.JJ_ConsignmentID";
		public override string GuidReference => "jj.JJ_PK";
		public override string CreatingUserCode => "jj.JJ_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					JobCartage jj
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = jj.JJ_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
					INNER JOIN (SELECT distinct EW_JJ FROM dbo.JobBookedCtgMove WHERE EW_JC_Container is null) looseEw
						ON looseEw.EW_JJ = jj.JJ_PK
					LEFT JOIN dbo.JobBookedCtgMove containerEw
						ON containerEw.EW_JJ = jj.JJ_PK
						AND containerEw.EW_JC_Container is not null";
		public override string WhereClause => @"
					containerEw.EW_JJ is null";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.29.221";
	}

	#endregion
}
