namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	sealed public class ForwardingPortMessagingInbound : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "PSB";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs and Port Messaging";
		public override string FunctionName => "Forwarding Port Messaging";
		public override string FeatureName => "Port Status - BE Ports";
		public override string DataGranularity => "DAY";

		public override string CompanyCode => "GC_Code";
		public override string BranchCode => "GB_Code";
		public override string GuidReference => "SL_PK";
		public override string TransactionDateUtc => "SL_PostedTimeUtc";

		public override string BillingReference1 => "JK_UniqueConsignRef";
		public override string BillingReference2 => "LOCAndMSTOrTYP";
		public override string BillingReference3 => "EventCodeAndSTA";
		public override string BillingReference4 => "EQNAndRFN";

		public override string TransactionCount => "1";

		public override string PreparationScript => @"
WITH EBADECDGNLogs AS
(
	SELECT
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_Reference,
		SL_SE_NKEvent,
		DocumentGroupId = CASE
			WHEN SL_Reference LIKE '%MST=Export Notification (EBADEC)%' THEN 0
			WHEN SL_Reference LIKE '%MST=Dangerous Goods Notification - Import%' THEN 1
			WHEN SL_Reference LIKE '%MST=Dangerous Goods Notification - Export%' THEN 2
			ELSE 3 END
	FROM
		dbo.StmALog
	WHERE
		SL_SE_NKEvent IN ('MAA', 'MPP', 'MRJ', 'MWA')
		AND	SL_Table = 'JobDocumentData'
		AND
		(
			SL_Reference LIKE '%MST=Export Notification (EBADEC)%'
			OR SL_Reference LIKE '%MST=Dangerous Goods Notification%'
		)
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
), EBADECDGNLogsWithDetails AS
(
	SELECT
		LOC = CASE locStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, locStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, locStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, locStartPosition + 1)) - locStartPosition - 5) END,
		EQN = CASE eqnStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, eqnStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, eqnStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, eqnStartPosition + 1)) - eqnStartPosition - 5) END,
		RFN = CASE rfnStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, rfnStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, rfnStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, rfnStartPosition + 1)) - rfnStartPosition - 5) END,
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_SE_NKEvent,
		DocumentGroupId,
		SL_GB_NKBranch =
		(
			SELECT TOP 1
				MSNOrMWR.SL_GB_NKBranch
			FROM
				dbo.StmALog MSNOrMWR
			WHERE
				MSNOrMWR.SL_Parent = BillingLog.SL_Parent
				AND MSNOrMWR.SL_SE_NKEvent =
				(
					CASE BillingLog.SL_SE_NKEvent
					WHEN 'MWA' THEN 'MWR'
					ELSE 'MSN' END
				)
				AND MSNOrMWR.SL_Table = 'JobDocumentData'
				AND MSNOrMWR.SL_PostedTimeUtc >= DATEADD(MONTH, -1, BillingLog.SL_PostedTimeUtc)
				AND MSNOrMWR.SL_PostedTimeUtc < BillingLog.SL_PostedTimeUtc
				AND MSNOrMWR.SL_Reference LIKE
				(
					CASE BillingLog.DocumentGroupId
						WHEN 0 THEN '%MST=Export Notification (EBADEC)%'
						WHEN 1 THEN '%MST=Dangerous Goods Notification - Import%'
						ELSE '%MST=Dangerous Goods Notification - Export%' END
				)
			ORDER BY MSNOrMWR.SL_PostedTimeUtc DESC
		)
	FROM
	(
		SELECT
			locStartPosition = CHARINDEX('|LOC=', SL_Reference),
			eqnStartPosition = CHARINDEX('|QTY=', SL_Reference),
			rfnStartPosition = CHARINDEX('|RFN=', SL_Reference),
			refLen = LEN(SL_Reference),
			SL_PK,
			SL_Parent,
			SL_PostedTimeUtc,
			SL_SE_NKEvent,
			SL_Reference,
			DocumentGroupId
		FROM
			EBADECDGNLogs
		WHERE
			DocumentGroupId <= 2
	) BillingLog
), EBADECDGNFinalLogs AS
(
	SELECT
		SL_PK = EBADECDGNLogsWithDetails.SL_PK,
		SL_PostedTimeUtc = EBADECDGNLogsWithDetails.SL_PostedTimeUtc,
		JK_UniqueConsignRef = JobConsol.JK_UniqueConsignRef,
		LOCAndMSTOrTYP = CONCAT
		(
			LOC,
			IIF(LEN(LOC) > 0, ' - ', ''),
			CASE DocumentGroupId
				WHEN 0 THEN 'Export Notification (EBADEC)'
				WHEN 1 THEN 'Dangerous Goods Notification - Import'
				WHEN 2 THEN 'Dangerous Goods Notification - Export'
				ELSE '' END
		),
		EventCodeAndSTA = EBADECDGNLogsWithDetails.SL_SE_NKEvent,
		EQNAndRFN = CASE
			WHEN LEN(EQN) > 0 AND LEN(RFN) > 0 THEN CONCAT(EQN, ' - ', RFN)
			WHEN LEN(EQN) > 0 THEN EQN
			WHEN LEN(RFN) > 0 THEN RFN
			ELSE NULL END,
		Branch.GB_Code,
		Company.GC_Code
	FROM
		EBADECDGNLogsWithDetails
	LEFT JOIN dbo.JobDocumentData ON EBADECDGNLogsWithDetails.SL_Parent = JobDocumentData.JDD_PK
	LEFT JOIN dbo.JobConsol ON JobDocumentData.JDD_ParentID = JobConsol.JK_PK
	LEFT JOIN dbo.GlbBranch Branch ON Branch.GB_Code = EBADECDGNLogsWithDetails.SL_GB_NKBranch
	LEFT JOIN dbo.GlbCompany Company ON Company.GC_PK = Branch.GB_GC
), PickUpFormLogs AS
(
	SELECT
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_Reference,
		SL_SE_NKEvent,
		DocumentGroupId = CASE
			WHEN SL_Reference LIKE '%MST=Certified Pickup - Accept/Decline%' THEN 0
			WHEN SL_Reference LIKE '%MST=Certified Pickup - Transfer%' THEN 1
			WHEN SL_Reference LIKE '%MST=Certified Pickup - Revoke%' THEN 2
			ELSE 3 END
	FROM
		dbo.StmALog
	WHERE
		SL_SE_NKEvent IN ('MAA', 'MPP', 'MRJ', 'MWA')
		AND	SL_Table = 'JobContainer'
		AND SL_Reference LIKE '%MST=Certified Pickup%'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
), PickUpFormConsols AS
(
	SELECT DISTINCT
		PickUpFormLogs.SL_Parent,
		JobConsol.JK_UniqueConsignRef,
		JobConsol.JK_PK
	FROM
		PickUpFormLogs
	LEFT JOIN dbo.JobContainer ON PickUpFormLogs.SL_Parent = JobContainer.JC_PK
	LEFT JOIN dbo.JobConsol ON JobContainer.JC_JK = JobConsol.JK_PK
	WHERE
		JobConsol.JK_IsForwarding = 1
), PickUpFormLogsWithDetails AS
(
	SELECT
		LOC = CASE locStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, locStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, locStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, locStartPosition + 1)) - locStartPosition - 5) END,
		EQN = CASE eqnStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, eqnStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, eqnStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, eqnStartPosition + 1)) - eqnStartPosition - 5) END,
		RFN = CASE rfnStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, rfnStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, rfnStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, rfnStartPosition + 1)) - rfnStartPosition - 5) END,
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_SE_NKEvent,
		DocumentGroupId,
		JK_UniqueConsignRef,
		SL_GB_NKBranch =
		(
			SELECT TOP 1
				MSNOrMWR.SL_GB_NKBranch
			FROM
				dbo.StmALog MSNOrMWR
			WHERE
				MSNOrMWR.SL_Parent = BillingLog.JK_PK
				AND MSNOrMWR.SL_SE_NKEvent =
				(
					CASE BillingLog.SL_SE_NKEvent
					WHEN 'MWA' THEN 'MWR'
					ELSE 'MSN' END
				)
				AND MSNOrMWR.SL_Table = 'JobConsol'
				AND MSNOrMWR.SL_PostedTimeUtc >= DATEADD(MONTH, -1, BillingLog.SL_PostedTimeUtc)
				AND MSNOrMWR.SL_PostedTimeUtc < BillingLog.SL_PostedTimeUtc
				AND MSNOrMWR.SL_Reference LIKE
				(
					CASE BillingLog.DocumentGroupId
						WHEN 0 THEN '%MST=Certified Pickup - Accept/Decline%'
						WHEN 1 THEN '%MST=Certified Pickup - Transfer%'
						ELSE '%MST=Certified Pickup - Revoke%' END
				)
			ORDER BY MSNOrMWR.SL_PostedTimeUtc DESC
		)
	FROM
	(
		SELECT
			locStartPosition = CHARINDEX('|LOC=', PickUpFormLogs.SL_Reference),
			eqnStartPosition = CHARINDEX('|EQN=', PickUpFormLogs.SL_Reference),
			rfnStartPosition = CHARINDEX('|RFN=', PickUpFormLogs.SL_Reference),
			refLen = LEN(PickUpFormLogs.SL_Reference),
			PickUpFormLogs.SL_PK,
			PickUpFormLogs.SL_Parent,
			PickUpFormLogs.SL_PostedTimeUtc,
			PickUpFormLogs.SL_SE_NKEvent,
			PickUpFormLogs.SL_Reference,
			PickUpFormLogs.DocumentGroupId,
			PickUpFormConsols.JK_PK,
			PickUpFormConsols.JK_UniqueConsignRef
		FROM
			PickUpFormLogs
		LEFT JOIN PickUpFormConsols ON PickUpFormLogs.SL_Parent = PickUpFormConsols.SL_Parent
		WHERE
			DocumentGroupId <= 2
	) BillingLog
), PickUpFormFinalLogs AS
(
	SELECT
			SL_PK = PickUpFormLogsWithDetails.SL_PK,
			SL_PostedTimeUtc = PickUpFormLogsWithDetails.SL_PostedTimeUtc,
			JK_UniqueConsignRef = PickUpFormLogsWithDetails.JK_UniqueConsignRef,
			LOCAndMSTOrTYP = CONCAT
			(
				LOC,
				IIF(LEN(LOC) > 0, ' - ', ''),
				CASE PickUpFormLogsWithDetails.DocumentGroupId
					WHEN 0 THEN 'Certified Pickup - Accept/Decline'
					WHEN 1 THEN 'Certified Pickup - Transfer'
					WHEN 2 THEN 'Certified Pickup - Revoke'
					ELSE '' END
			),
			EventCodeAndSTA = PickUpFormLogsWithDetails.SL_SE_NKEvent,
			EQNAndRFN = CASE
				WHEN LEN(EQN) > 0 AND LEN(RFN) > 0 THEN CONCAT(EQN, ' - ', RFN)
				WHEN LEN(EQN) > 0 THEN EQN
				WHEN LEN(RFN) > 0 THEN RFN
				ELSE NULL END,
			Branch.GB_Code,
			Company.GC_Code
	FROM
		PickUpFormLogsWithDetails
	LEFT JOIN dbo.GlbBranch Branch ON PickUpFormLogsWithDetails.SL_GB_NKBranch = Branch.GB_Code
	LEFT JOIN dbo.GlbCompany Company ON Branch.GB_GC = Company.GC_PK
)";

		public override string FromClause => @"
(SELECT * FROM EBADECDGNFinalLogs
UNION
SELECT * FROM PickUpFormFinalLogs) FinalLogs";

		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string WhereClause => string.Empty;
	}

	#endregion
}
