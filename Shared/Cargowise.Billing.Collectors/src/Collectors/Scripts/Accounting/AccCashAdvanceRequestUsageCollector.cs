namespace CargoWise.Billing.Collectors.Accounting
{
	public class AccCashAdvanceRequestUsageCollector : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override bool UsedInBilling => false;
		public override string DataGranularity => RefStlItemGrain.Daily;
		public override string FeatureCode => "CAR";
		public override string RoleName => "Accounting";
		public override string ModuleName => "Accounting";
		public override string MinCW1Version => "22.6.15.136";
		public override string FunctionName => "Advance Payment Request Usage Counter";
		public override string FeatureName => "Advance Payment Request";
		public override string TransactionDateUtc => "d.AH_SystemCreateTimeUtc";
		public override string GuidReference => "MIN(d.AH_PK)";
		public override string CompanyCode => "MIN(d.GC_Code)";
		public override string BranchCode => "MIN(d.GB_Code)";
		public override string CreatingUserCode => "MIN(d.AH_SystemCreateUser)";
		public override string WhereClause => @"d.AH_SystemCreateTimeUtc >= @StartDateTimeInclusive
							AND d.AH_SystemCreateTimeUtc < @EndDateTimeExclusive
							AND d.CAH_PK IS NOT NULL GROUP BY d.AH_PK, d.AH_SystemCreateTimeUtc";
		public override string FromClause => @"(SELECT ah.AH_PK,
	ah.AH_RX_NKTransactionCurrency,
	ah.AH_SystemCreateTimeUtc,
	ah.AH_SystemCreateUser,
	ah.AH_Ledger,
	c.GC_Code,
	c.GC_RN_NKCountryCode,
	b.GB_Code,
	AH_OSTotal = ABS(ah.AH_OSTotal),
	CAH_PK = COALESCE(cah_AP.CAH_PK, cah_ar.CAH_PK),
	CAH_Status = COALESCE(cah_AR.CAH_Status, cah_AP.CAH_Status),
	CAH_Currency = COALESCE(cah_AR.CAH_RX_NKTransactionCurrency, cah_AP.CAH_RX_NKTransactionCurrency),
	JH_ParentTableCode = COALESCE(jh_AR.JH_ParentTableCode, jh_AP.JH_ParentTableCode),
	CountOfCashAdvancePayments = (SELECT COUNT(*) FROM dbo.AccTransactionHeader ath WHERE ath.AH_CAH_CashAdvanceRequestHeader = COALESCE(cah_AP.CAH_PK, cah_ar.CAH_PK)),
	ChargeGroups = COALESCE(ac_AR.AC_ChargeGroup, ac_AP.AC_ChargeGroup),
	OSAmount = COALESCE(cal_AR.CAL_OSAmount, cal_AP.CAL_OSAmount),
	CAH_SystemCreateTimeUtc = COALESCE(cah_AR.CAH_SystemCreateTimeUtc, cah_AP.CAH_SystemCreateTimeUtc)
	FROM dbo.AccTransactionHeader ah
		LEFT JOIN dbo.GlbCompany AS c ON ah.AH_GC = c.GC_PK 
		LEFT JOIN dbo.GlbBranch AS b ON b.GB_PK = ah.AH_GB
		INNER JOIN dbo.AccTransactionLines al on al.AL_AH = ah.AH_PK
		LEFT JOIN dbo.JobCharge jr_AP on jr_AP.JR_AL_APLine = al.AL_PK AND jr_AP.JR_CAL_APLine IS NOT NULL
		LEFT JOIN dbo.AccCashAdvanceRequestLine cal_AP on cal_AP.CAL_PK = jr_AP.JR_CAL_APLine
		LEFT JOIN dbo.AccCashAdvanceRequestHeader cah_AP on cah_AP.CAH_PK = cal_AP.CAL_CAH_RequestHeader
		LEFT JOIN dbo.JobHeader jh_AP on jh_aP.JH_PK = cah_AP.CAH_JH_Job
		LEFT JOIN dbo.AccChargeCode ac_AP on ac_AP.AC_PK = jr_AP.JR_AC
		LEFT JOIN dbo.JobCharge jr_AR on jr_AR.JR_AL_ARLine = al.AL_PK AND jr_AR.JR_CAL_ARLine IS NOT NULL
		LEFT JOIN dbo.AccCashAdvanceRequestLine cal_AR on cal_AR.CAL_PK = jr_AR.JR_CAL_ARLine
		LEFT JOIN dbo.AccCashAdvanceRequestHeader cah_AR on cah_AR.CAH_PK = cal_AR.CAL_CAH_RequestHeader
		LEFT JOIN dbo.JobHeader jh_AR on jh_AR.JH_PK = cah_AR.CAH_JH_Job
		LEFT JOIN dbo.AccChargeCode ac_AR on ac_AR.AC_PK = jr_AR.JR_AC
		WHERE ah.AH_TransactionType = 'INV' AND ah.AH_Ledger in ('AR', 'AP')
		AND ah.AH_GC in (select distinct CAH_GC_Company from dbo.AccCashAdvanceRequestHeader)
		AND COALESCE(JR_AR.JR_PK, JR_AP.JR_PK) IS NOT NULL
		) AS d
";
		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(SELECT 
MIN(d.AH_SystemCreateTimeUtc) AS [Create Date],
MIN(d.AH_Ledger) AS [Ledger],
MIN(d.GC_RN_NKCountryCode) AS [Country code],
MIN(d.AH_RX_NKTransactionCurrency) AS [Inv currency],
MIN(d.AH_OSTotal) AS [OS amount],
COUNT(DISTINCT d.CAH_PK) AS [No. of CA],
MIN(d.CAH_SystemCreateTimeUtc) AS [Earliest CA Date],
dbo.CLRCssvAgg(DISTINCT d.CAH_Status) AS [CA Status],
dbo.CLRCssvAgg(DISTINCT d.CAH_Currency) AS [CA Currency],
dbo.CLRCssvAgg(DISTINCT d.JH_ParentTableCode) AS [Job Type],
SUM(d.CountOfCashAdvancePayments) AS [Number of CA payments],
dbo.CLRCssvAgg(DISTINCT d.ChargeGroups) AS [Charge Groups],	
CASE WHEN
	MIN(d.CAH_Currency) = MAX(d.CAH_Currency)
THEN 
	SUM(d.OSAmount) 
ELSE 
	CAST(0 AS MONEY) 
END AS [Total line OS Amount]
				FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";
		public override string ActiveOn => "ALL";
		public override string BillingReference1 => string.Empty;

		#endregion // SuppressResourceStringsCheckRegion
	}
}
