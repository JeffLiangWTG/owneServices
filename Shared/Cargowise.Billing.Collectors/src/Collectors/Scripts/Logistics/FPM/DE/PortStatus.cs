namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	sealed public class PortStatus : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "PSD";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs and Port Messaging";
		public override string FunctionName => "Forwarding Port Messaging";
		public override string FeatureName => "Port Status - DE Ports";
		public override string DataGranularity => "DAY";

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "BillingLogsWithDetails.SL_PK";
		public override string TransactionDateUtc => "BillingLogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "JobConsol.JK_UniqueConsignRef";
		public override string BillingReference2 => "CONCAT(LOC, IIF(LEN(LOC) > 0, ' - ', ''), 'Advanced Logistics Port Order')";
		public override string BillingReference3 => "SL_SE_NKEvent";
		public override string BillingReference4 => @"CASE
	WHEN LEN(EQN) > 0 AND LEN(CRF) > 0 THEN CONCAT(EQN, ' - ', CRF)
	WHEN LEN(EQN) > 0 THEN EQN
	WHEN LEN(CRF) > 0 THEN CRF
	ELSE NULL END";

		public override string TransactionCount => "1";

		public override string PreparationScript => @"WITH BillingLogs AS
(
	SELECT
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_Reference,
		SL_SE_NKEvent
	FROM
		dbo.StmALog
	WHERE
		(
			SL_SE_NKEvent IN ('SHL', 'SCM')
			OR
			(
				SL_SE_NKEvent = 'STU'
				AND SL_Reference NOT LIKE '%TYP=Reset To Original%'
			)
		)
		AND	SL_Table = 'JobDocumentData'
		AND SL_Reference LIKE '%MST=Advanced Logistics Port Order%'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
), BillingLogsWithDetails AS
(
	SELECT
		CRF = CASE crfStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, crfStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, crfStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, crfStartPosition + 1)) - crfStartPosition - 5) END,
		EQN = CASE eqnStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, eqnStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, eqnStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, eqnStartPosition + 1)) - eqnStartPosition - 5) END,
		LOC = CASE locStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, locStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, locStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, locStartPosition + 1)) - locStartPosition - 5) END,
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_SE_NKEvent,
		SL_GB_NKBranch =
		(
			SELECT TOP 1
				MSN.SL_GB_NKBranch
			FROM
				dbo.StmALog MSN
			WHERE
				MSN.SL_Parent = BillingLog.SL_Parent
				AND MSN.SL_SE_NKEvent = 'MSN'
				AND MSN.SL_Table = 'JobDocumentData'
				AND MSN.SL_PostedTimeUtc >= DATEADD(MONTH, -1, BillingLog.SL_PostedTimeUtc)
				AND MSN.SL_PostedTimeUtc < BillingLog.SL_PostedTimeUtc
				AND MSN.SL_Reference LIKE '%MST=Advanced Logistics Port Order%'
			ORDER BY MSN.SL_PostedTimeUtc DESC
		)
	FROM
	(
		SELECT
			crfStartPosition = CHARINDEX('|CRF=', SL_Reference),
			eqnStartPosition = CHARINDEX('|EQN=', SL_Reference),
			locStartPosition = CHARINDEX('|LOC=', SL_Reference),
			refLen = LEN(SL_Reference),
			SL_PK,
			SL_Parent,
			SL_PostedTimeUtc,
			SL_Reference,
			SL_SE_NKEvent
		FROM
			BillingLogs
	) BillingLog
)";

		public override string FromClause => @"BillingLogsWithDetails
	LEFT JOIN dbo.JobDocumentData ON BillingLogsWithDetails.SL_Parent = JobDocumentData.JDD_PK
	LEFT JOIN dbo.JobConsol ON JobDocumentData.JDD_ParentID = JobConsol.JK_PK
	LEFT JOIN dbo.GlbBranch Branch ON Branch.GB_Code = BillingLogsWithDetails.SL_GB_NKBranch
	LEFT JOIN dbo.GlbCompany Company ON Company.GC_PK = Branch.GB_GC";

		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string WhereClause => string.Empty;
	}

	#endregion
}
