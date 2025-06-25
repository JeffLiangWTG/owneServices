namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	sealed public class ForwarderBookableContainerMaximumTEU : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "BKO";
		public override string RoleName => "Forwarding Consolidation/Container";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "Bookable Container - Maximum TEU";
		public override string FeatureName => "Progressive maximum TEU count billing at Booking, SI, SO & Departure points";
		public override string DataGranularity => "TRN";

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "JK_PK";
		public override string TransactionDateUtc => "SL_PostedTimeUtc";

		public override string BillingReference1 => "JK_UniqueConsignRef";
		public override string BillingReference2 => "JK_BookingReference";
		public override string BillingReference3 => "JK_MasterBillNum";
		public override string BillingReference4 => "CMP + ' / ' + TYP + '[' + STA + ']'";

		public override string TransactionCount => "IIF(TRY_CONVERT(DECIMAL, QTY) IS NULL, 0, CONVERT(DECIMAL, QTY))";
		public override string AdditionalRefs => @"
CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX),
(
	SELECT
		EventReference = SL_Reference,
		ConsolType = JK_AgentType,
		TransportMode = JK_TransportMode,
		'Container Mode' = JK_ConsolMode,
		FirstLoadPort = JK_RL_NKLoadPort,
		LastDischargePort = JK_RL_NKDischargePort
	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)))
";

		public override string PreparationScript => @"
WITH OCBLogs AS
(
	SELECT
		CMP = CASE cmpStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, cmpStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, cmpStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, cmpStartPosition + 1)) - cmpStartPosition - 5) END,
		QTY = CASE qtyStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, qtyStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, qtyStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, qtyStartPosition + 1)) - qtyStartPosition - 5) END,
		STA = CASE staStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, staStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, staStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, staStartPosition + 1)) - staStartPosition - 5) END,
		TYP = CASE typStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, typStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, typStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, typStartPosition + 1)) - typStartPosition - 5) END,
		SL_Parent,
		SL_PK,
		SL_GB_NKBranch,
		SL_PostedTimeUtc,
		SL_Reference
	FROM (
		SELECT
			cmpStartPosition = CHARINDEX('|CMP=', SL_Reference),
			qtyStartPosition = CHARINDEX('|QTY=', SL_Reference),
			staStartPosition = CHARINDEX('|STA=', SL_Reference),
			typStartPosition = CHARINDEX('|TYP=', SL_Reference),
			refLen = len(SL_Reference),
			SL_Reference,
			SL_Parent,
			SL_PK,
			SL_GB_NKBranch,
			SL_PostedTimeUtc
		FROM
			dbo.StmALog
		WHERE
			SL_SE_NKEvent = 'OCB'
			AND SL_Table = 'JobConsol'
			AND SL_PostedTimeUtc >= @StartDateTimeInclusive
			AND SL_PostedTimeUtc < @EndDateTimeExclusive
	) Logs
)";

		public override string FromClause => @"OCBLogs logs
LEFT JOIN dbo.JobConsol ON logs.SL_Parent = JobConsol.JK_PK
LEFT JOIN dbo.GlbBranch Branch ON GB_Code = logs.SL_GB_NKBranch
LEFT JOIN dbo.GlbCompany Company ON GC_PK = GB_GC";
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string MinCW1Version => "22.9.14.237";
		/**
		 * ALP/DPR/STD
		 *		MinCW1Version = "22.9.14.237"
		 *
		 * GP1
		 *		MinCW1Version = "22.8.11.237"
		 *		MaxCW1Version = "22.8.11.999"
		 *
		 * GP2
		 *		MinCW1Version = "22.5.18.358"
		 *		MaxCW1Version = "22.5.18.999"
		 */
		public override string WhereClause => string.Empty;
	}

	#endregion
}
