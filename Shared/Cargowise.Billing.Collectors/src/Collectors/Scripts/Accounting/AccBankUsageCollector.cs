namespace CargoWise.Billing.Collectors.Accounting
{
	public class AccBankUsageCollector : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string FeatureCode => "TX5";
		public override string RoleName => "Count Bank Accounts";
		public override string ModuleName => "Accounting";
		public override string FunctionName => "Bank Accounts Statistics";
		public override string FeatureName => "Bank Accounts Statistics";
		public override string TransactionDateUtc => "@StartDateTimeInclusive";
		public override string GuidReference => "AB_PK";
		public override string CompanyCode => string.Empty;
		public override string BranchCode => string.Empty;
		public override string CreatingUserCode => "AB_SystemLastEditUser";
		public override string BillingReference1 => string.Empty;
		public override string TransactionCount => "1";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(
	SELECT
		AB_PK,
		AB_RN_NKBankAccountCountry,
		AB_RX_NKAccountCurrency,
		AB_Desc,
		AB_BankName,
		AB_BSB,
		AB_LastReconcileDate,
		AH_TransactionType,
		AB_SystemLastEditUser,
		AB_SystemLastEditTimeUtc,
		TransactionCount = COUNT(AH_TransactionType)
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string FromClause => "AccBankAccount AB LEFT JOIN AccTransactionHeader AH WITH (FORCESEEK, INDEX (NR_RX__AH_SystemCreateTimeUtc, PK_UC__AH_PK)) ON AH.AH_AB = AB.AB_PK AND AH.AH_Ledger = 'CB' AND AH.AH_SystemCreateTimeUtc >= @StartDateTimeInclusive AND AH.AH_SystemCreateTimeUtc < @EndDateTimeExclusive";
		public override string WhereClause => "AB.AB_IsActive = 1   GROUP BY AB_PK, AB_Code, AB_RN_NKBankAccountCountry, AB_RX_NKAccountCurrency, AB_Desc, AB_BankName, AB_BSB,AB_LastReconcileDate, AH_TransactionType,AB_SystemLastEditUser, AB_SystemLastEditTimeUtc";
		public override string ActiveOn => "ALL";

		#endregion // SuppressResourceStringsCheckRegion
	}
}
