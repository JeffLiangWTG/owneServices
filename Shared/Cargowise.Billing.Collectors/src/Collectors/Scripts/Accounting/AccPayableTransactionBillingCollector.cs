namespace CargoWise.Billing.Collectors.Accounting
{
	public class AccPayableTransactionBillingCollector : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override bool UsedInBilling => true;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string FeatureCode => "TX2";
		public override string RoleName => "Account Payable Usage Data";
		public override string ModuleName => "Accounting";
		public override string FunctionName => "Payables Transactions";
		public override string FeatureName => "Account Payable Usage Data";
		public override string TransactionDateUtc => "SL_PostedTimeUtc";
		public override string GuidReference => "AH_PK";
		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string CreatingUserCode => "AH_SystemCreateUser";
		public override string BillingReference1 => "AH_TransactionNum";
		public override string BillingReference2 => "AH_ConsolidatedInvoiceRef";
		public override string BillingReference3 => "GC_RN_NKCountryCode";
		public override string BillingReference4 => "AH_TransactionType";
		public override string TransactionCount => "1";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(
	SELECT
		AH_RX_NKTransactionCurrency,
		GC_RX_NKLocalCurrency,
		AH_OSTotal,
		AH_LocalTotal,
		AH_InvoiceAmount,
		AH_TransactionCategory
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)))";
		public override string FromClause => "StmALog JOIN dbo.AccTransactionHeader ON AH_PK = SL_Parent LEFT JOIN dbo.GlbCompany ON GC_PK = AH_GC LEFT JOIN dbo.GlbBranch ON GB_PK = AH_GB";
		public override string WhereClause => "SL_SE_NKEvent IN ('ADD','EDT') AND (SL_Reference LIKE 'AP|___|Posted' OR SL_Reference LIKE 'AP|___|Reversed' OR SL_Reference LIKE 'Compliance Sub Type was%') AND AH_Ledger = 'AP' AND AH_TransactionType IN ('INV','CRD','ADJ')";
		public override string ActiveOn => "ALL";

		#endregion // SuppressResourceStringsCheckRegion
	}
}
