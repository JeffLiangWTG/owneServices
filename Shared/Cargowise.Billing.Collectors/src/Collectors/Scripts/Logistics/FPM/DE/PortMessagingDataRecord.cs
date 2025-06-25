namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	sealed public class PortMessagingDataRecord : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "PMD";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs and Port Messaging";
		public override string FunctionName => "Forwarding Port Messaging";
		public override string FeatureName => "Port Messaging Data Record - DE Ports";
		public override string DataGranularity => "DAY";

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "ISNLogsWithDetails.SL_PK";
		public override string TransactionDateUtc => "ISNLogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "JobConsol.JK_UniqueConsignRef";
		public override string BillingReference2 => "CONCAT(LOC, IIF(LEN(LOC) > 0, ' - ', ''), 'Advanced Logistics Port Order')";
		public override string BillingReference3 => @"CASE STA
	WHEN 'ORG' THEN 'Original'
	WHEN 'AMD' THEN 'Amendment'
	WHEN 'WTH' THEN 'Withdrawal'
	ELSE NULL END";
		public override string BillingReference4 => "EQN";

		public override string TransactionCount => "1";

		public override string PreparationScript => @"WITH ISNLogs AS
(
	SELECT
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_Reference
	FROM
		dbo.StmALog
	WHERE
		SL_SE_NKEvent = 'ISN'
		AND	SL_Table = 'JobDocumentData'
		AND SL_Reference LIKE '%MST=Advanced Logistics Port Order%'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
), ISNLogsWithDetails AS
(
	SELECT
		STA = CASE staStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, staStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, staStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, staStartPosition + 1)) - staStartPosition - 5) END,
		EQN = CASE eqnStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, eqnStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, eqnStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, eqnStartPosition + 1)) - eqnStartPosition - 5) END,
		LOC = CASE locStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, locStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, locStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, locStartPosition + 1)) - locStartPosition - 5) END,
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_GB_NKBranch =
		(
			SELECT TOP 1
				MSNOrMWR.SL_GB_NKBranch
			FROM
				dbo.StmALog MSNOrMWR
			WHERE
				MSNOrMWR.SL_Parent = ISNLog.SL_Parent
				AND MSNOrMWR.SL_SE_NKEvent IN ('MSN', 'MWR')
				AND MSNOrMWR.SL_Table = 'JobDocumentData'
				AND MSNOrMWR.SL_PostedTimeUtc >= DATEADD(MONTH, -1, ISNLog.SL_PostedTimeUtc)
				AND MSNOrMWR.SL_PostedTimeUtc < ISNLog.SL_PostedTimeUtc
				AND MSNOrMWR.SL_Reference LIKE '%MST=Advanced Logistics Port Order%'
			ORDER BY MSNOrMWR.SL_PostedTimeUtc DESC
		)
	FROM
	(
		SELECT
			staStartPosition = CHARINDEX('|STA=', SL_Reference),
			eqnStartPosition = CHARINDEX('|EQN=', SL_Reference),
			locStartPosition = CHARINDEX('|LOC=', SL_Reference),
			refLen = LEN(SL_Reference),
			SL_PK,
			SL_Parent,
			SL_PostedTimeUtc,
			SL_Reference
		FROM
			ISNLogs
	) ISNLog
)";

		public override string FromClause => @"ISNLogsWithDetails
	LEFT JOIN dbo.JobDocumentData ON ISNLogsWithDetails.SL_Parent = JobDocumentData.JDD_PK
	LEFT JOIN dbo.JobConsol ON JobDocumentData.JDD_ParentID = JobConsol.JK_PK
	LEFT JOIN dbo.GlbBranch Branch ON Branch.GB_Code = ISNLogsWithDetails.SL_GB_NKBranch
	LEFT JOIN dbo.GlbCompany Company ON Company.GC_PK = Branch.GB_GC";

		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string WhereClause => string.Empty;
	}

	#endregion
}
