namespace CargoWise.Billing.Collectors.Core
{
	#region SuppressResourceStringsCheckRegion
	public class ExtensionFinanceAdvPayment : RefStlScriptWithDefaults
	{
		public override bool UsedInBilling => false;
		public override string FeatureCode => "ADP";
		public override string RoleName => "Core Engine";
		public override string ModuleName => "ediCore Extension Pack 1";
		public override string FunctionName => "Finance and Accounting Engine (Pack1)";
		public override string FeatureName => "Advanced Cheque and Payment";
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
					LEFT JOIN (
						SELECT XB_ParentID
						FROM dbo.GenExportBatchSequence
						WHERE XB_ParentID is not null
						AND XB_Type = 'PPF'
						GROUP BY XB_ParentID) xb ON ah.AH_PK = xb.XB_ParentID
					LEFT JOIN dbo.OrgHeader oh ON oh.OH_PK = ah.AH_OH";
		public override string WhereClause => "(xb.XB_ParentID is not null OR (ah.AH_transactiontype = 'DDB' and ah.AH_Ledger = 'CB'))";
		public override string TransactionCount => "1";
		public override bool WithOptionRecompile => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string DateType => RefStlDateType.DateTime;
		public override string MinCW1Version => "22.12.7.91";
	}
	#endregion
}
