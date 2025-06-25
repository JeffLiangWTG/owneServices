namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class ConsolidationAdviceSent : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "CAS";
		public override string RoleName => "Forwarding Shipment";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "Ocean Carrier Messaging - Consolidation Advice";
		public override string FeatureName => "CargoWise to CargoWise Consolidation Advice Sent";
		public override string DataGranularity => RefStlItemGrain.Daily;

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "ISNLogsWithDetails.SL_PK";
		public override string TransactionDateUtc => "ISNLogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "JobShipment.JS_UniqueConsignRef";
		public override string BillingReference2 => "JobShipment.JS_BookingReference";
		public override string BillingReference3 => @"(
	SELECT
		CASE WHEN OrgAddress.OA_PK IS NULL OR OrgHeader.OH_Code = 'MISC' THEN BookingPartyDocumentaryAddress.E2_CompanyName
		WHEN OrgAddress.OA_CompanyNameOverride != '' THEN OrgAddress.OA_CompanyNameOverride
		ELSE OrgHeader.OH_FullName END
	FROM
	(
		SELECT TOP 1
			E2_AddressOverride,
			E2_CompanyName,
			E2_OA_Address
		FROM
			dbo.JobDocAddress
		WHERE
			JobDocAddress.E2_ParentID = JobShipment.JS_PK
			AND JobDocAddress.E2_AddressType = 'BKD'
		ORDER BY
			E2_AddressSequence ASC
	) BookingPartyDocumentaryAddress
	LEFT JOIN dbo.OrgAddress ON BookingPartyDocumentaryAddress.E2_AddressOverride = 0 AND BookingPartyDocumentaryAddress.E2_OA_Address = OrgAddress.OA_PK
	LEFT JOIN dbo.OrgHeader ON OrgAddress.OA_OH = OrgHeader.OH_PK)";
		public override string BillingReference4 => "CONCAT('Consolidation Advice', IIF(LEN(STA) > 0, ' - ', ''), STA)";

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
		AND SL_Table = 'JobDocumentData'
		AND SL_Reference LIKE '%MST=Consolidation Advice%'
		AND SL_Reference LIKE '%DEP=CargoWise%'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
), ISNLogsWithDetails AS
(
	SELECT
		STA = CASE staStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, staStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, staStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, staStartPosition + 1)) - staStartPosition - 5) END,
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
				AND MSN.SL_Reference LIKE '%MST=Consolidation Advice%'
			ORDER BY
				MSN.SL_PostedTimeUtc DESC
		)
	FROM
	(
		SELECT
			staStartPosition = CHARINDEX('|STA=', SL_Reference),
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
LEFT JOIN dbo.JobShipment ON JobDocumentData.JDD_ParentID = JobShipment.JS_PK
LEFT JOIN dbo.GlbBranch Branch ON Branch.GB_Code = ISNLogsWithDetails.SL_GB_NKBranch
LEFT JOIN dbo.GlbCompany Company ON Company.GC_PK = Branch.GB_GC";

		public override string WhereClause => string.Empty;
	}

	#endregion
}
