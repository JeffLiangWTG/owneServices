namespace CargoWise.Billing.Collectors.Core
{
	#region SuppressResourceStringsCheckRegion
	public class CoreFinancePayableOrder : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "PPO";
		public override string RoleName => "Core Engine";
		public override string ModuleName => "ediCore Base";
		public override string FunctionName => "Finance and Accounting Engine (Base)";
		public override string FeatureName => "Payable Purchase Order";
		public override string CompanyCode => "gc.GC_Code";
		public override string TransactionDateUtc => "aph.APH_SystemCreateTimeUtc";
		public override string BillingReference1 => "aph.APH_OrderNumber";
		public override string GuidReference => "aph.APH_PK";
		public override string CreatingUserCode => "aph.APH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					AccPayableOrderHeader aph
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = aph.APH_GC";
		public override string WhereClause => "";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string BranchCode => string.Empty;
		public override string MinCW1Version => "22.12.7.91";
	}
	#endregion
}
