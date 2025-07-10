namespace CargoWise.RefDbRepo.STLBillingCollector.TestDummyCollectors.Accounting
{
	public class AccBankUsageCollector : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.MonthlyAllowHistoricalData;
		public override string FeatureCode => "TX5";
		public override string RoleName => "Accounting";
		public override string ModuleName => "Bank Accounts";
		public override string FunctionName => "Bank Accounts Usage";
		public override string FeatureName => "Bank Accounts Usage";
		public override string TransactionDateUtc => "@StartDateTimeInclusive";
		public override string GuidReference => "AB_PK";
		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string CreatingUserCode => "AB.AB_SystemCreateUser";
		public override string BillingReference1 => "CT.AH_TransactionType";
		public override string BillingReference2 => "AH_ReceiptType";
		public override string BillingReference3 => "CONVERT(varchar, AH_IsCancelled)";
		public override string BillingReference4 => "";
		public override string TransactionCount => "1";
		public override string PreparationScript => @"WITH CT AS 
(
   SELECT
      AH_AB,
	  AH_TransactionType,
	  AH_ReceiptType,
	  AH_IsCancelled,
      COUNT(*) AS TransactionCount 
   FROM
      dbo.AccTransactionHeader WITH (FORCESEEK, INDEX (NR_RX__AH_SystemCreateTimeUtc))
   WHERE
      AH_Ledger = 'CB' 
	  AND AH_SystemCreateTimeUtc >= @StartDateTimeInclusive
      AND AH_SystemCreateTimeUtc < @EndDateTimeExclusive
   GROUP BY
      AH_AB, AH_ReceiptType, AH_IsCancelled, AH_TransactionType
)";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(
	SELECT
	   AB.AB_PK AS AccountId,
	   GC.GC_Code AS CompanyCode,
	   GC.GC_Name AS CompanyName,
	   GC.GC_RN_NKCountryCode AS CompanyCountryCode,
	   AB.AB_AccountType AS AccountType,
	   AB.AB_IsActive AS BankIsActive,
	   AB.AB_Code AS BankAccountCode,
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)))";
		public override string FromClause => "dbo.AccBankAccount AB LEFT JOIN dbo.GlbCompany GC ON AB.AB_GC = GC.GC_PK LEFT JOIN dbo.GlbBranch GB ON AB.AB_GB = GB.GB_PK LEFT JOIN CT ON AB.AB_PK = CT.AH_AB";
		public override string WhereClause => "AB.AB_IsActive = 1";
		public override string ActiveOn => "ALL";

		#endregion // SuppressResourceStringsCheckRegion
	}
}
