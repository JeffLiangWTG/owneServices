namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	sealed public class PortOtherMessagesForContainerLoadPlan : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "PLC";
		public override string RoleName => "Container Load Plan Submission to CN Ports";
		public override string ModuleName => "Other Parties Customs and Port Messaging";
		public override string FunctionName => "Forwarding Port Messaging";
		public override string FeatureName => "Port Other Messages(PLC)";
		public override string DataGranularity => RefStlItemGrain.Daily;

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "ISNLogsWithDetails.SL_PK";
		public override string TransactionDateUtc => "ISNLogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "JobConsol.JK_UniqueConsignRef";
		public override string BillingReference2 => "CONCAT(LOC, ' - ', MST)";
		public override string BillingReference3 => @"CASE STA
	WHEN 'ORG' THEN 'ORG'
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
		AND SL_Reference LIKE '%MST=Container Load Plan%'
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
		MST = CASE mstStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, mstStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, mstStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, mstStartPosition + 1)) - mstStartPosition - 5) END,
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_GB_NKBranch =
		(
			SELECT TOP 1
				MSN.SL_GB_NKBranch
			FROM
				dbo.StmALog MSN
			WHERE
				MSN.SL_Parent = ISNLog.SL_Parent
				AND MSN.SL_SE_NKEvent = 'MSN'
				AND MSN.SL_Table = 'JobDocumentData'
				AND MSN.SL_PostedTimeUtc >= DATEADD(MONTH, -1, ISNLog.SL_PostedTimeUtc)
				AND MSN.SL_PostedTimeUtc < ISNLog.SL_PostedTimeUtc
				AND MSN.SL_Reference LIKE '%MST=Container Load Plan%'
			ORDER BY MSN.SL_PostedTimeUtc DESC
		)
	FROM
	(
		SELECT
			staStartPosition = CHARINDEX('|STA=', SL_Reference),
			eqnStartPosition = CHARINDEX('|EQN=', SL_Reference),
			locStartPosition = CHARINDEX('|LOC=', SL_Reference),
			mstStartPosition = CHARINDEX('|MST=', SL_Reference),
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

		public override string WhereClause => string.Empty;
	}

	#endregion
}
