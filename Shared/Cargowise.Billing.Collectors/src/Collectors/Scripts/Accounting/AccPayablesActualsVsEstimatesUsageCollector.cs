namespace CargoWise.Billing.Collectors.Accounting
{
	public class AccPayablesActualsVsEstimatesUsageCollector : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string FeatureCode => "PAY";
		public override string RoleName => "Accounting";
		public override string ModuleName => "Payables";
		public override string MinCW1Version => "24.1.29.720";
		public override string FunctionName => "Payables Actuals Vs Estimates Usage Collector";
		public override string FeatureName => "Payables Actuals Vs Estimates";
		public override string TransactionDateUtc => "h.InvoiceCreatedDateTime";
		public override string GuidReference => "h.AH_PK";
		public override string CompanyCode => "h.GC_Code";
		public override string BranchCode => "h.GB_Code";
		public override string CreatingUserCode => "h.InvoiceCreatedUser";
		public override string WhereClause => @"h.InvoiceNumber <= 1";
		public override string PreparationScript => @"WITH h AS (
	SELECT
		GC_RN_NKCountryCode AS CountryCode,
		GC_RX_NKLocalCurrency AS CurrencyCode,
		GC_Name AS CompanyName,
		GC_Code,
		GB_Code,
		AH_PK,
		AH_RX_NKTransactionCurrency AS HeaderCurrencyCode,
		AH_SystemCreateUser AS InvoiceCreatedUser,
		AH_SystemCreateTimeUtc AS InvoiceCreatedDateTime,
		CASE WHEN AIH_AH_PostedTransactionHeader IS NOT NULL THEN 1 ELSE 0 END AS LinkedToDraftInvoice,
		CASE WHEN OH_Category = 'NAT' THEN NULL ELSE OH_FullName END AS OrganizationName,
		OH_IsShippingProvider AS IsCarrier,
		OH_IsForwarder AS IsForwarder,
		OH_IsBroker AS IsBroker,
		OH_IsMiscFreightServices AS IsServices,
		OH_IsAirLine AS IsAirCarrier,
		CASE WHEN (OH_IsShippingLine = 1 OR OH_IsSeaWholesaler = 1) THEN 1 ELSE 0 END AS IsSeaCarrier,
		OH_IsLocalTransport AS IsRoadTransport,
		AC_Code AS ChargeCode,
		AL_Desc AS ChargeDescription,
		AC_ChargeGroup AS ChargeGroupOfTheChargeCode,
		JH_JobNum AS JobNumber,
		AL_RX_NKTransactionCurrency AS InvoiceLineCurrency,
		JR_OSCostAmt AS InvoiceLineOSAmount,
		JR_SystemCreateTimeUtc AS JobChargeCreatedDateTime,
		JR_SystemCreateUser AS JobChargeCreatedUser,
		JR_EstimatedCost AS EstimatedCost,
		CASE WHEN JR_E6 IS NOT NULL THEN 1 ELSE 0 END AS Apportioned,
		JR_CostRated AS CostAutoRated,
		JR_CostRatingOverride AS CostRatingOverride,
		JR_CostRatingOverrideComment AS CostRatingOverrideComment,
		COALESCE(gg.GE_Code, ge.GE_Code) AS Department,
		DENSE_RANK() OVER (PARTITION BY OH_FullName, CONVERT(date, AH_SystemCreateTimeUtc) ORDER BY AH_SystemCreateTimeUtc DESC, AH_PK) AS InvoiceNumber
	FROM dbo.AccTransactionHeader WITH (FORCESEEK, INDEX(NR_RX__AH_SystemCreateTimeUtc))
	JOIN dbo.GlbCompany ON AH_GC = GC_PK
	JOIN dbo.GlbBranch ON AH_GB = GB_PK
	JOIN dbo.OrgHeader ON AH_OH = OH_PK
	JOIN dbo.AccTransactionLines WITH (FORCESEEK, INDEX(FK_RX__AL_AH)) ON AH_PK = AL_AH
	LEFT JOIN dbo.AccChargeCode ON AL_AC = AC_PK
	LEFT JOIN dbo.JobHeader WITH (FORCESEEK, INDEX(PK_UX__JH_PK)) ON AL_JH = JH_PK
	LEFT JOIN dbo.JobCharge WITH (FORCESEEK, INDEX(FK_RX__JR_AL_APLine)) ON AL_PK = JR_AL_APLine
	LEFT JOIN dbo.GlbDepartment ge ON JR_GE = ge.GE_PK
	LEFT JOIN dbo.GlbDepartment gg ON ge.GE_GE = gg.GE_PK
	LEFT JOIN dbo.AccDraftInvoiceHeader WITH (FORCESEEK, INDEX(FK_RX__AIH_AH_PostedTransactionHeader)) ON AH_PK = AIH_AH_PostedTransactionHeader
	WHERE AH_SystemCreateTimeUtc >= @StartDateTimeInclusive AND AH_SystemCreateTimeUtc < @EndDateTimeExclusive
		AND AH_Ledger = 'AP'
		AND AH_TransactionType = 'INV'
		AND AL_JH IS NOT NULL
		AND AH_IsCancelled = 0
)";
		public override string FromClause => "h";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(SELECT 
h.CountryCode,
h.CurrencyCode,
h.CompanyName,
h.HeaderCurrencyCode,
h.InvoiceCreatedUser,
h.InvoiceCreatedDateTime,
h.LinkedToDraftInvoice,
h.OrganizationName,
h.IsCarrier,
h.IsForwarder,
h.IsBroker,
h.IsServices,
h.IsAirCarrier,
h.IsSeaCarrier,
h.IsRoadTransport,
h.ChargeCode,
h.ChargeDescription,
h.ChargeGroupOfTheChargeCode,
h.JobNumber,
h.InvoiceLineCurrency,
h.InvoiceLineOSAmount,
h.JobChargeCreatedDateTime,
h.JobChargeCreatedUser,
h.EstimatedCost,
h.Apportioned,
h.CostAutoRated,
h.CostRatingOverride,
h.CostRatingOverrideComment,
h.Department
				FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string ActiveOn => "ALL";
		public override string BillingReference1 => string.Empty;

		#endregion // SuppressResourceStringsCheckRegion
	}
}
