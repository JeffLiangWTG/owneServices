namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class CfsContainerStorageManagement : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "CFM";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "CFS/Freight Shed Manager";
		public override string FunctionName => "Container Freight Station / Air Freight Shed";
		public override string FeatureName => "Container Storage Management";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "jh.JH_SystemCreateTimeUtc";
		public override string BillingReference1 => "jh.JH_JobNum";
		public override string BillingReference2 => "jc.JC_ContainerJobID";
		public override string GuidReference => "jh.JH_PK";
		public override string CreatingUserCode => "jh.JH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					JobHeader jh
					INNER JOIN dbo.JobContainer jc ON jc.JC_PK = jh.JH_ParentID
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = jh.JH_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		public override string WhereClause => @"
					jc.JC_IsCFSRegistered = 1
					AND jc.JC_Purpose = 'STR'";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.3.16.344";
	}

	#endregion
}
