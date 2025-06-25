namespace CargoWise.Billing.Collectors.Accounting
{
	public class AccTransactionHeaderUsageCollector : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string FeatureCode => "TX0";
		public override string RoleName => "Accounting";
		public override string ModuleName => "Accounting";
		public override string FunctionName => "Accounting Transaction Count";
		public override string FeatureName => "Accounting Transaction Count";
		public override string TransactionDateUtc => "TransactionDate";
		public override string GuidReference => "AH_PK";
		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string BillingReference1 => "AH_Ledger";
		public override string BillingReference2 => "AH_TransactionType";
		public override string BillingReference3 => "GC_RN_NKCountryCode";
		public override string TransactionCount => "TransactionCount";
		public override string PreparationScript => @"WITH groupedTransactions AS
(
	SELECT AH_PK = MIN(AH_PK), GC_Code, GB_Code, GC_RN_NKCountryCode, AH_Ledger, AH_TransactionType, TransactionDate = CONVERT(date, AH_SystemCreateTimeUtc), TransactionCount = COUNT(*)
	FROM dbo.AccTransactionHeader LEFT JOIN dbo.GlbCompany ON GC_PK = AH_GC LEFT JOIN dbo.GlbBranch ON GB_PK = AH_GB
	GROUP BY GC_Code, GB_Code, GC_RN_NKCountryCode, AH_Ledger, AH_TransactionType, CONVERT(date, AH_SystemCreateTimeUtc)
)";
		public override string FromClause => "groupedTransactions";
		public override string ActiveOn => "ALL";

		public override string WhereClause => string.Empty;

		#endregion // SuppressResourceStringsCheckRegion
	}
}
