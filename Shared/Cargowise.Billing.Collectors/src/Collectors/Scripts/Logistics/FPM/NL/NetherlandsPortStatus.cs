namespace CargoWise.Billing.Collectors.Logistics
{
	public class NetherlandsPortStatus : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "PSN";
		public override string FeatureName => "Netherlands Port Status";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs and Port Messaging";
		public override string FunctionName => "Forwarding Port Messaging";
		public override string DataGranularity => RefStlItemGrain.Daily;

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "StatusUpdatedLogsWithDetails.SL_PK";
		public override string TransactionDateUtc => "StatusUpdatedLogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "JobConsol.JK_UniqueConsignRef";
		public override string BillingReference2 => "CONCAT('STU', ' - ', TYP)";
		public override string BillingReference3 => "'Export Notification'";
		public override string BillingReference4 => "CRF";

		public override string PreparationScript => $@"WITH StatusUpdatedLogsWithDetails AS
(
	SELECT
		TYP = CASE typStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, typStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, typStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, typStartPosition + 1)) - typStartPosition - 5) END,
		CRF = CASE crfStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, crfStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, crfStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, crfStartPosition + 1)) - crfStartPosition - 5) END,
		SL_PK,
		JobDocumentDataPK,
		SL_PostedTimeUtc,
		MSNOrMWRBranch =
			(
				SELECT TOP 1
					MSNOrMWR.SL_GB_NKBranch
				FROM
					dbo.StmALog MSNOrMWR
				WHERE
					MSNOrMWR.SL_Parent = StatusUpdatedLogs.JobDocumentDataPK
					AND MSNOrMWR.SL_SE_NKEvent IN ('MSN', 'MWR')
					AND MSNOrMWR.SL_Table = 'JobDocumentData'
					AND MSNOrMWR.SL_PostedTimeUtc >= DATEADD(MONTH, -3, StatusUpdatedLogs.SL_PostedTimeUtc)
					AND MSNOrMWR.SL_PostedTimeUtc < StatusUpdatedLogs.SL_PostedTimeUtc
				ORDER BY MSNOrMWR.SL_PostedTimeUtc DESC
			),
		SL_GB_NKBranch
	FROM
	(
		SELECT
			typStartPosition = CHARINDEX('|TYP=', SL_Reference),
			crfStartPosition = CHARINDEX('|CRF=', SL_Reference),
			refLen = LEN(SL_Reference),
			SL_PK,
			JobDocumentDataPK = CASE
				WHEN SL_Table = 'JobDocumentData' THEN SL_Parent
				ELSE 
				(
					SELECT JDD_PK
					FROM dbo.JobContainer 
					JOIN dbo.JobDocumentData ON JDD_ParentTableCode = 'JK' AND JDD_ParentID = JC_JK AND JDD_Name = 'PortbaseExportNotification'
					WHERE JC_PK = SL_Parent
				)
				END,
			SL_PostedTimeUtc,
			SL_Reference,
			SL_GB_NKBranch
		FROM
			dbo.StmALog
		WHERE
			SL_PostedTimeUtc >= {Constants.StartDateTimeInclusiveParamName} AND SL_PostedTimeUtc < {Constants.EndDateTimeExclusiveParamName}
			AND SL_Table IN ('JobDocumentData', 'JobContainer')
			AND SL_SE_NKEvent = 'STU'
			AND (SL_Reference LIKE '%MST=Export Notification|%' OR SL_Reference LIKE '%MST=Export Notification')
			AND SL_Reference LIKE '%DEP=Portbase%'
			AND SL_Reference NOT LIKE '%TYP=Reset to Original%'
	) StatusUpdatedLogs
)";

		public override string FromClause => @"StatusUpdatedLogsWithDetails
	LEFT JOIN dbo.JobDocumentData ON StatusUpdatedLogsWithDetails.JobDocumentDataPK = JobDocumentData.JDD_PK
	LEFT JOIN dbo.JobConsol ON JobDocumentData.JDD_ParentID = JobConsol.JK_PK
	LEFT JOIN dbo.GlbBranch Branch
		ON Branch.GB_Code = IIF(StatusUpdatedLogsWithDetails.MSNOrMWRBranch IS NOT NULL AND LEN(StatusUpdatedLogsWithDetails.MSNOrMWRBranch) > 0, StatusUpdatedLogsWithDetails.MSNOrMWRBranch, StatusUpdatedLogsWithDetails.SL_GB_NKBranch)
	LEFT JOIN dbo.GlbCompany Company ON Company.GC_PK = Branch.GB_GC";

		public override string WhereClause => string.Empty;
	}
}
