namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class CfsContainerCount : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "CFN";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "CFS/Freight Shed Manager";
		public override string FunctionName => "Container Freight Station / Air Freight Shed";
		public override string FeatureName => "CFS Container Count";
		public override string TransactionDateUtc => "jc.JC_SystemCreateTimeUtc";
		public override string BillingReference1 => "jk.JK_UniqueConsignRef";
		public override string BillingReference2 => "jc.JC_ContainerNum";
		public override string GuidReference => "jc.JC_PK";
		public override string CreatingUserCode => "jc.JC_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					JobConsol jk
					INNER JOIN dbo.JobContainer jc ON jc.JC_JK = jk.JK_PK";
		public override string WhereClause => "jk.JK_IsCFS = 1";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "23.3.16.344";
	}

	#endregion
}
