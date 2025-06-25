namespace CargoWise.Billing.Collectors.Accounting
{
	public class AccEPaymentDealUsageCollector : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.Transactional;
		public override string FeatureCode => "EPD";
		public override string RoleName => "Accounting";
		public override string ModuleName => "Accounting";
		public override string FunctionName => "Payables";
		public override string FeatureName => "Global Electronic Payment";
		public override string MinCW1Version => "21.9.10.185";
		public override string TransactionDateUtc => "d.AED_SystemCreateTimeUtc";
		public override string GuidReference => "d.AED_PK";
		public override string CompanyCode => "c.GC_Code";
		public override string BranchCode => "b.GB_Code";
		public override string CreatingUserCode => "s.TK_GS_NKStaffCode";
		public override string FromClause => @"dbo.AccEPaymentDeal AS d 
				LEFT JOIN dbo.GlbCompany AS c ON d.AED_GC_Company = c.GC_PK 
				LEFT JOIN dbo.AccEPaymentQuote AS q ON d.AED_QU_Quote = q.QU_PK 
				LEFT JOIN dbo.AccPaymentApproval AS p ON q.QU_AV = p.AV_PK 
				LEFT JOIN dbo.AccEPaymentStaffToken AS s ON d.AED_SystemCreateUser = s.TK_GS_NKStaffCode AND s.TK_Scope = 'payments' AND p.AV_AB = s.TK_AB
				LEFT JOIN dbo.GlbBranch AS b ON b.GB_PK = p.AV_GB
				LEFT JOIN dbo.StmALog AS dex ON dex.SL_Parent = d.AED_PK and dex.SL_SE_NKEvent = 'DEX' AND dex.SL_Table='AccEPaymentDeal' AND dex.SL_Reference LIKE 'Purpose: E-Payment Deal % submitted for processing to %'
				LEFT JOIN dbo.StmALog AS iak ON iak.SL_Parent = d.AED_PK and iak.SL_SE_NKEvent = 'IAK' AND iak.SL_Table='AccEPaymentDeal' AND iak.SL_Reference LIKE 'E-Payment Deal % accepted by %.'";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(SELECT 
	Country = c.GC_RN_NKCountryCode,
	DealStatus = d.AED_Status,
	DealMessage = d.AED_ErrorDescription,
	FromCurrency = q.QU_RX_NKFromCurrency,
	ToCurrency = q.QU_RX_NKToCurrency,
	ToAmount = q.QU_ToAmount,
	FeeAmount = q.QU_FeeAmount,
	PaymentCreatedDate = p.AV_SystemCreateTimeUtc,
	QuoteCreatedDate = q.QU_SystemCreateTimeUtc,
	PaymentIsBatch = CASE WHEN p.AV_APB_PaymentBatch IS NOT NULL THEN 1 ELSE 0 END,
	PaymentRequiresApproval = CASE WHEN p.AV_GS_NKApproval1st <> '' OR p.AV_GS_NKApproval2nd <> '' OR p.AV_GS_NKApproval3rd <> '' THEN 1 ELSE 0 END,
	DealID = d.AED_ProviderReference,
	OFXAccountName = s.TK_AccountName,
	RequestTimeSeconds = DATEDIFF(SECOND, dex.SL_PostedTimeUtc, iak.SL_EventTime),
	ResponseProcessingTimeSeconds = DATEDIFF(SECOND, iak.SL_EventTime, iak.SL_PostedTimeUtc),
	TotalTimeSeconds = DATEDIFF(SECOND, dex.SL_PostedTimeUtc, iak.SL_PostedTimeUtc)
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";

		public override string ActiveOn => "ALL";

		public override string BillingReference1 => string.Empty;

		public override string WhereClause => string.Empty;

		#endregion // SuppressResourceStringsCheckRegion
	}
}
