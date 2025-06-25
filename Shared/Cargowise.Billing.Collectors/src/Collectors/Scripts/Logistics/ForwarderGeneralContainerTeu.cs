namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion
	public class ForwarderGeneralContainerTeu : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "GFT";
		public override string RoleName => "Logistics and Supply Chain";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "General Forwarding Engine";
		public override string FeatureName => "Forwarding TEU Count";
		public override string TransactionDateUtc => "jc.JC_SystemCreateTimeUtc";
		public override string BillingReference1 => "jk.JK_UniqueConsignRef";
		public override string BillingReference2 => "jc.JC_ContainerNum";
		public override string GuidReference => "jc.JC_PK";
		public override string CreatingUserCode => "jc.JC_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					JobConsol jk
					INNER JOIN dbo.JobContainer jc ON jc.JC_JK = jk.JK_PK
					INNER JOIN dbo.RefContainer rc ON rc.RC_PK = jc.JC_RC";
		public override string WhereClause => "jk.JK_IsForwarding = 1";
		public override string TransactionCount => "rc.RC_TEU";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "23.1.20.174";
	}
	#endregion
}
