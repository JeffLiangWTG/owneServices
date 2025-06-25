namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	sealed public class ForwardingPortMessagingOutbound : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "PMB";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs and Port Messaging";
		public override string FunctionName => "Forwarding Port Messaging";
		public override string FeatureName => "Port Messaging Data Record - BE Ports";
		public override string DataGranularity => "DAY";

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "ISNLogsWithDetails.SL_PK";
		public override string TransactionDateUtc => "ISNLogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "JobConsol.JK_UniqueConsignRef";
		public override string BillingReference2 => @"
CONCAT
(
	LOC,
	IIF(LEN(LOC) > 0, ' - ', ''),
	CASE ISNLogsWithDetails.DocumentGroupId
		WHEN 0 THEN 'Certified Pickup - Accept/Decline'
		WHEN 1 THEN 'Certified Pickup - Transfer'
		WHEN 2 THEN 'Certified Pickup - Revoke'
		WHEN 3 THEN 'Export Notification (EBADEC)'
		WHEN 4 THEN 'Dangerous Goods Notification - Import'
		WHEN 5 THEN 'Dangerous Goods Notification - Export'
		ELSE '' END
)";
		public override string BillingReference3 => @"CASE STA
		WHEN 'ORG' THEN 'Original'
		WHEN 'AMD' THEN 'Amendment'
		WHEN 'WTH' THEN 'Withdrawal'
		ELSE NULL END";
		public override string BillingReference4 => "EQN";
		public override string TransactionCount => "1";
		public override string PreparationScript => @"
WITH ISNLogs AS
(
	SELECT
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_Reference,
		DocumentGroupId = CASE
			WHEN SL_Reference LIKE '%MST=Certified Pickup - Accept/Decline%' THEN 0
			WHEN SL_Reference LIKE '%MST=Certified Pickup - Transfer%' THEN 1
			WHEN SL_Reference LIKE '%MST=Certified Pickup - Revoke%' THEN 2
			WHEN SL_Reference LIKE '%MST=Export Notification (EBADEC)%' THEN 3
			WHEN SL_Reference LIKE '%MST=Dangerous Goods Notification - Import%' THEN 4
			WHEN SL_Reference LIKE '%MST=Dangerous Goods Notification - Export%' THEN 5
			ELSE 6 END
	FROM
		dbo.StmALog
	WHERE
		SL_SE_NKEvent = 'ISN'
		AND	SL_Table = 'JobDocumentData'
		AND
		(
			SL_Reference LIKE '%MST=Export Notification (EBADEC)%'
			OR SL_Reference LIKE '%MST=Dangerous Goods Notification%'
			OR SL_Reference LIKE '%MST=Certified Pickup%'
		)
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
),ISNLogsWithDetails AS
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
		DocumentGroupId,
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
				AND MSNOrMWR.SL_Reference LIKE
				(
					CASE ISNLog.DocumentGroupId
						WHEN 0 THEN '%MST=Certified Pickup - Accept/Decline%'
						WHEN 1 THEN '%MST=Certified Pickup - Transfer%'
						WHEN 2 THEN '%MST=Certified Pickup - Revoke%'
						WHEN 3 THEN '%MST=Export Notification (EBADEC)%'
						WHEN 4 THEN '%MST=Dangerous Goods Notification - Import%'
						ELSE '%MST=Dangerous Goods Notification - Export%' END
				)
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
			SL_Reference,
			DocumentGroupId
		FROM
			ISNLogs
		WHERE
			DocumentGroupId <= 5
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
