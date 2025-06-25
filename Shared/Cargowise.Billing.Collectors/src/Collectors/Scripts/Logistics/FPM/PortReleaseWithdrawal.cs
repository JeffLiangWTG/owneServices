namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	sealed public class PortReleaseWithdrawal : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "PRW";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs and Port Messaging";
		public override string FunctionName => "Forwarding Port Messaging";
		public override string FeatureName => "Port Messaging Data Record - Withdrawal";
		public override string DataGranularity => "DAY";

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "ATWLogsWithDetails.SL_PK";
		public override string TransactionDateUtc => "ATWLogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "ATWLogsWithDetails.JK_UniqueConsignRef";
		public override string BillingReference2 => "CASE WHEN LEN(LOC) > 0 AND LEN(MST) > 0 THEN CONCAT(LOC, ' - ', MST) WHEN LEN(LOC) > 0 THEN LOC WHEN LEN(MST) > 0 THEN MST ELSE NULL END";
		public override string BillingReference3 => "CONCAT('ATW', IIF(LEN(STA) > 0, ' - ', ''), STA)";
		public override string BillingReference4 => "CASE WHEN LEN(EQN) > 0 AND LEN(RFN) > 0 THEN CONCAT(EQN, ' - ', RFN) WHEN LEN(EQN) > 0 THEN EQN WHEN LEN(RFN) > 0 THEN RFN ELSE NULL END";

		public override string PreparationScript =>
			@"WITH ATWLogs AS
(
	SELECT
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_Reference,
		SL_SE_NKEvent,
		SL_GB_NKBranch
	FROM
		dbo.StmALog
	WHERE
		SL_SE_NKEvent = 'ATW'
		AND	SL_Table = 'JobContainer'
		AND SL_Reference LIKE '%TYP=Container Release%'
		AND SL_Reference LIKE '%DEP=Terminal%'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
), RelatedConsols AS
(
	SELECT DISTINCT
		ATWLogs.SL_Parent,
		JobConsol.JK_UniqueConsignRef,
		JobConsol.JK_OA_ReceivingForwarderAddress
	FROM
		ATWLogs
	LEFT JOIN dbo.JobContainer ON ATWLogs.SL_Parent = JobContainer.JC_PK
	LEFT JOIN dbo.JobConsol ON JobContainer.JC_JK = JobConsol.JK_PK
), ATWLogsWithDetails AS
(
	SELECT
		*,
		PreferredBranch =
		(
			SELECT TOP 1 GB_Code FROM dbo.GlbBranch
			WHERE
			(
				GB_RL_NKHomePort = LOC
				OR GB_PK IN (SELECT GY_GB FROM dbo.GlbBranchExtraPorts WHERE GY_RL_NKAdditionalBranchRelatedPort = LOC)
			)
			ORDER BY
				CASE
					WHEN GB_OH_OrgProxy = OrgHeaderPK AND GB_RL_NKHomePort = LOC THEN 1
					WHEN GB_OH_OrgProxy = OrgHeaderPK THEN 2
					WHEN GB_RL_NKHomePort = LOC THEN 3
					ELSE 4 END ASC,
				GB_IsActive DESC
		)
	FROM
	(
		SELECT
			LOC = CASE locStartPosition
				WHEN 0 THEN ''
				ELSE SUBSTRING(SL_Reference, locStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, locStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, locStartPosition + 1)) - locStartPosition - 5) END,
			MST = CASE mstStartPosition
				WHEN 0 THEN ''
				ELSE SUBSTRING(SL_Reference, mstStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, mstStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, mstStartPosition + 1)) - mstStartPosition - 5) END,
			STA = CASE staStartPosition
				WHEN 0 THEN ''
				ELSE SUBSTRING(SL_Reference, staStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, staStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, staStartPosition + 1)) - staStartPosition - 5) END,
			EQN = CASE eqnStartPosition
				WHEN 0 THEN ''
				ELSE SUBSTRING(SL_Reference, eqnStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, eqnStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, eqnStartPosition + 1)) - eqnStartPosition - 5) END,
			RFN = CASE rfnStartPosition
				WHEN 0 THEN ''
				ELSE SUBSTRING(SL_Reference, rfnStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, rfnStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, rfnStartPosition + 1)) - rfnStartPosition - 5) END,
			SL_PK,
			SL_Parent,
			SL_PostedTimeUtc,
			JK_UniqueConsignRef,
			OrgHeaderPK,
			ATWLogBranch = ATWLog.SL_GB_NKBranch
		FROM
		(
			SELECT
				locStartPosition = CHARINDEX('|LOC=', SL_Reference),
				mstStartPosition = CHARINDEX('|MST=', SL_Reference),
				staStartPosition = CHARINDEX('|STA=', SL_Reference),
				eqnStartPosition = CHARINDEX('|EQN=', SL_Reference),
				rfnStartPosition = CHARINDEX('|RFN=', SL_Reference),
				refLen = LEN(SL_Reference),
				ATWLogs.SL_PK,
				ATWLogs.SL_Parent,
				ATWLogs.SL_PostedTimeUtc,
				ATWLogs.SL_Reference,
				ATWLogs.SL_GB_NKBranch,
				RelatedConsols.JK_UniqueConsignRef,
				OrgHeaderPK = OrgAddress.OA_OH
			FROM
				ATWLogs
			LEFT JOIN RelatedConsols ON RelatedConsols.SL_Parent = ATWLogs.SL_Parent
			LEFT JOIN dbo.OrgAddress ON OrgAddress.OA_PK = RelatedConsols.JK_OA_ReceivingForwarderAddress
		) ATWLog
	) ATWLogWithReferences
)";
		public override string FromClause =>
			@"ATWLogsWithDetails
LEFT JOIN dbo.GlbBranch Branch ON Branch.GB_Code = IIF(PreferredBranch IS NULL, ATWLogBranch, PreferredBranch)
LEFT JOIN dbo.GlbCompany Company ON Branch.GB_GC = Company.GC_PK";

		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string WhereClause => string.Empty;
	}

	#endregion
}
