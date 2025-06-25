namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class CfsContainerPacking : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => true;
		public override string FeatureCode => "CFC";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "CFS/Freight Shed Manager";
		public override string FunctionName => "Container Freight Station / Air Freight Shed";
		public override string FeatureName => "Container Packing";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "jh.JH_SystemCreateTimeUtc";
		public override string BillingReference1 => "jh.JH_JobNum";
		public override string BillingReference2 => "jk.JK_UniqueConsignRef";
		public override string GuidReference => "jh.JH_PK";
		public override string CreatingUserCode => "jh.JH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					JobHeader jh
					INNER JOIN dbo.JobConsol jk ON jk.JK_PK = jh.JH_ParentID
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = jh.JH_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
					LEFT OUTER JOIN dbo.JobContainer jc ON jc.JC_JK = jk.JK_PK";
		public override string WhereClause => "jk.JK_IsCFS = 1";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "23.3.16.344";
	}

	#endregion
}
