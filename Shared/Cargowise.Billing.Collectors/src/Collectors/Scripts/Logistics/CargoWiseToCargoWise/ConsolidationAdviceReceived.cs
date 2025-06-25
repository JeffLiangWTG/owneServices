namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class ConsolidationAdviceReceived : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "CAD";
		public override string RoleName => "Forwarding Shipment";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "Ocean Carrier Messaging - Consolidation Advice";
		public override string FeatureName => "CargoWise to CargoWise Consolidation Advice Received";
		public override string DataGranularity => RefStlItemGrain.Daily;

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "MRRLogsWithDetails.SL_PK";
		public override string TransactionDateUtc => "MRRLogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "MRRLogsWithDetails.ConsolNumber";
		public override string BillingReference2 => "MRRLogsWithDetails.CoLoadBkgRef";
		public override string BillingReference3 => "MRRLogsWithDetails.CoLoaderC1CCode";
		public override string BillingReference4 => "CONCAT('Consolidation Advice', IIF(LEN(MSB) > 0, ' - ', ''), MSB)";

		public override string TransactionCount => "1";

		public override string PreparationScript => @"WITH MRRLogs AS
(
	SELECT
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_GB_NKBranch,
		SL_Reference
	FROM
		dbo.StmALog
	WHERE
		SL_SE_NKEvent = 'MRR'
		AND	SL_Table = 'JobConsol'
		AND SL_Reference LIKE '%MST=Consolidation Advice%'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
), MRRLogsWithDetails AS
(
	SELECT
		MSB = CASE msbStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, msbStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, msbStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, msbStartPosition + 1)) - msbStartPosition - 5) END,
		SL_PK,
		SL_PostedTimeUtc,
		MRRLogBranch = SL_GB_NKBranch,
		PreferredBranch =
		(
			SELECT TOP 1 GB_Code FROM dbo.GlbBranch
			WHERE
				GB_OH_OrgProxy = SendingAgentOrgHeaderPK
				AND
				(
					GB_RL_NKHomePort = JK_RL_NKLoadPort
					OR GB_PK IN (SELECT GY_GB FROM dbo.GlbBranchExtraPorts WHERE GY_RL_NKAdditionalBranchRelatedPort = JK_RL_NKLoadPort)
				)
			ORDER BY
				CASE
					WHEN GB_RL_NKHomePort = JK_RL_NKLoadPort THEN 1
					ELSE 2 END ASC,
				GB_IsActive DESC
		),
		ConsolNumber,
		CoLoadBkgRef,
		CoLoaderC1CCode
	FROM
	(
		SELECT
			msbStartPosition = CHARINDEX('|MSB=', SL_Reference),
			refLen = LEN(SL_Reference),
			SL_PK,
			ConsolPK = JK_PK,
			SL_PostedTimeUtc,
			SL_GB_NKBranch,
			SL_Reference,
			ConsolNumber = JK_UniqueConsignRef,
			CoLoadBkgRef = JK_CoLoadBookingReference,
			SendingAgentOrgHeaderPK = SendingAgentContact.OC_OH,
			JK_RL_NKLoadPort,
			CoLoaderC1CCode = OK_CustomsRegNo
		FROM
			MRRLogs
		LEFT JOIN dbo.JobConsol ON SL_Parent = JK_PK
		LEFT JOIN dbo.OrgContact SendingAgentContact ON JK_OC_SendingForwarderContact = OC_PK
		LEFT JOIN dbo.OrgAddress CoLoader ON JK_OA_CreditorAddress = CoLoader.OA_PK
		LEFT JOIN dbo.OrgCusCode ON CoLoader.OA_OH = OK_OH AND OK_CodeType = 'C1C'
	) MRRLog
)";

		public override string FromClause => @"MRRLogsWithDetails
LEFT JOIN dbo.GlbBranch Branch ON Branch.GB_Code = IIF(MRRLogsWithDetails.PreferredBranch IS NULL, MRRLogBranch, PreferredBranch)
LEFT JOIN dbo.GlbCompany Company ON Company.GC_PK = Branch.GB_GC";

		public override string WhereClause => string.Empty;
	}

	#endregion
}
