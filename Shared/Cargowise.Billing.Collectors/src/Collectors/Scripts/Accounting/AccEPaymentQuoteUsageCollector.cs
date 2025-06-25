namespace CargoWise.Billing.Collectors.Accounting
{
	public class AccEPaymentQuoteUsageCollector : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string FeatureCode => "EPQ";
		public override string RoleName => "Accounting";
		public override string ModuleName => "Accounting";
		public override string FunctionName => "Payables";
		public override string FeatureName => "Global Electronic Quote";
		public override string MinCW1Version => "21.9.10.185"; //The version where E-Payment integration with FX service providers became available.
		public override string TransactionDateUtc => "q.QU_SystemCreateTimeUtc";
		public override string GuidReference => "q.QU_PK";
		public override string CompanyCode => "c.GC_Code";
		public override string BranchCode => "b.GB_Code";
		public override string CreatingUserCode => "q.QU_SystemCreateUser";
		public override string FromClause => @"dbo.AccEPaymentQuote AS q
				LEFT JOIN dbo.AccPaymentApproval AS p ON p.AV_PK = q.QU_AV 
				LEFT JOIN dbo.AccEPaymentStaffToken AS s ON q.QU_SystemCreateUser = s.TK_GS_NKStaffCode AND s.TK_Scope = 'payments' AND p.AV_AB = s.TK_AB
				LEFT JOIN dbo.GlbCompany AS c ON c.GC_PK = q.QU_GC 
				LEFT JOIN dbo.GlbBranch AS b ON b.GB_PK = p.AV_GB
				LEFT JOIN dbo.StmALog AS dex ON dex.SL_Parent = q.QU_PK AND dex.SL_SE_NKEvent = 'DEX' AND dex.SL_Table = 'AccEPaymentQuote'
				LEFT JOIN dbo.StmALog AS iak ON iak.SL_Parent = q.QU_PK AND iak.SL_SE_NKEvent = 'IAK' AND iak.SL_Table = 'AccEPaymentQuote'";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
				(SELECT
					PaymentPK = p.AV_PK,
					QuoteType = CASE 
						WHEN q.QU_ExchangeRate <> 0 THEN
							CASE WHEN q.QU_ProviderReference = '' THEN 'Rate' ELSE 'Quote' END
						ELSE
							CASE WHEN p.AV_PaymentType = 'EPA' THEN 'Quote' ELSE 'Rate' END
					END,
					Country = c.GC_RN_NKCountryCode,
					Currency = c.GC_RX_NKLocalCurrency,
					QuoteStatus = q.QU_Status,
					QuoteMessage = q.QU_ErrorDescription,
					FromCurrency = q.QU_RX_NKFromCurrency,
					ToCurrency = q.QU_RX_NKToCurrency,
					ToAmount = q.QU_ToAmount,
					FeeAmount = q.QU_FeeAmount,
					PaymentCreatedDate = p.AV_SystemCreateTimeUtc,
					QuoteCreatedDate = q.QU_SystemCreateTimeUtc,
					LastEditDate = q.QU_SystemLastEditTimeUtc,
					RequestTimeSeconds = DATEDIFF(SECOND, dex.SL_PostedTimeUtc, iak.SL_EventTime),
					ResponseProcessingTimeSeconds = DATEDIFF(SECOND, iak.SL_EventTime, iak.SL_PostedTimeUtc),
					TotalTimeSeconds = DATEDIFF(SECOND, dex.SL_PostedTimeUtc, iak.SL_PostedTimeUtc)
				FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))".Replace("\r\n", " ").Replace("\t", "");
		public override string ActiveOn => "ALL";

		public override string BillingReference1 => string.Empty;

		public override string WhereClause => string.Empty;

		#endregion // SuppressResourceStringsCheckRegion
	}
}
