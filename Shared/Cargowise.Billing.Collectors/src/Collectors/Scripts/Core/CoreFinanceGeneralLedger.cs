namespace CargoWise.Billing.Collectors.Core
{
	#region SuppressResourceStringsCheckRegion
	public class CoreFinanceGeneralLedger : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "LDG";
		public override string RoleName => "Core Engine";
		public override string ModuleName => "ediCore Base";
		public override string FunctionName => "Finance and Accounting Engine (Base)";
		public override string FeatureName => "General Ledger";
		public override string CompanyCode => "gc.GC_Code";
		public override string BranchCode => "gb.GB_Code";
		public override string TransactionDateUtc => "ah.AH_SystemCreateTimeUtc";
		public override string BillingReference1 => "ah.AH_TransactionNum";
		public override string BillingReference2 => "ah.AH_Ledger + ' ' + ah.AH_TransactionType + ' #' + right('000' + convert(varchar(3), ah.AH_TransactionCount), 3) + ' Org:' + isnull(convert(varchar(12), oh.OH_Code), '<none>')";
		public override string GuidReference => "ah.AH_PK";
		public override string CreatingUserCode => "ah.AH_SystemCreateUser";
		public override string ActiveOn => "ALL";
		public override string FromClause => @"
					AccTransactionHeader ah
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = ah.AH_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC
					LEFT JOIN dbo.OrgHeader oh ON oh.OH_PK = ah.AH_OH";
		public override string WhereClause => @"
					(
						(ah.AH_TransactionType IN ('GJL', 'AJL', 'RJL') AND ah.AH_Ledger = 'GL')
						OR (ah.AH_TransactionType = 'JNL' AND ah.AH_Ledger IN ('AR', 'AP'))
						OR (ah.AH_TransactionType IN ('OVP', 'EXX', 'DSC') AND ah.AH_Ledger IN ('AR', 'AP'))
					)";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.7.91";
	}
	#endregion
}
