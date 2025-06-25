namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	sealed public class BookingRequestReceivedByNVOCC : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "CBR";
		public override string RoleName => "Forwarding Booking and Liner & Agency Booking";
		public override string ModuleName => "Forwarder-Liner and Agency";
		public override string FunctionName => "Ocean Carrier Messaging - Booking Request";
		public override string FeatureName => "CargoWise to CargoWise Booking Request Received";
		public override string DataGranularity => RefStlItemGrain.Daily;

		public override string CompanyCode => "STULogsWithDetails.CompanyCode";
		public override string BranchCode => "IIF(STULogsWithDetails.PreferredBranch IS NULL, STULogBranch, PreferredBranch)";
		public override string GuidReference => "STULogsWithDetails.SL_PK";
		public override string TransactionDateUtc => "STULogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "STULogsWithDetails.JS_UniqueConsignRef";
		public override string BillingReference2 => "STULogsWithDetails.JS_BookingReference";
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
		FROM dbo.JobDocAddress, STULogsWithDetails
		WHERE JobDocAddress.E2_ParentID = STULogsWithDetails.JS_PK AND JobDocAddress.E2_AddressType = 'BKD'
		ORDER BY E2_AddressSequence ASC
	) BookingPartyDocumentaryAddress
	LEFT JOIN dbo.OrgAddress ON BookingPartyDocumentaryAddress.E2_AddressOverride = 0 AND BookingPartyDocumentaryAddress.E2_OA_Address = OrgAddress.OA_PK
	LEFT JOIN dbo.OrgHeader ON OrgAddress.OA_OH = OrgHeader.OH_PK)";

		public override string BillingReference4 => "CONCAT(RES, ' - ', EventReference)";

		public override string TransactionCount => "1";

		public override string PreparationScript => @"WITH ISNLogs AS
(
	SELECT
		SL_PK,
		SL_Parent,
		SL_Table,
		SL_PostedTimeUtc,
		SL_GB_NKBranch,
		SL_Reference
	FROM
		dbo.StmALog
	WHERE
		SL_SE_NKEvent = 'STU'
		AND	SL_Table IN ('JobShipment', 'ViewQuotedBooking')
		AND SL_Reference LIKE '%Original%'
		AND SL_Reference LIKE '%NEW=EBK%'
		AND SL_Reference LIKE '%TYP=Shipment Status%'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
), STULogsWithDetails AS
(
	SELECT
		RES = CASE resStartPosition
			WHEN 0 THEN SL_Reference
			ELSE SUBSTRING(SL_Reference, resStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, resStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, resStartPosition + 1)) - resStartPosition - 5) END,
		EventReference = CASE eventreferenceStartPosition
			WHEN 0 THEN SL_Reference
			ELSE LEFT(SL_Reference, eventreferenceStartPosition- 1) END,
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		CompanyCode,
		STULogBranch = SL_GB_NKBranch,
		PreferredBranch =
		(
			SELECT TOP 1 GB_Code FROM dbo.GlbBranch
			WHERE
				GB_GC = CompanyPK
				AND
				(
					GB_RL_NKHomePort = JS_RL_NKOrigin
					OR GB_PK IN (SELECT GY_GB FROM dbo.GlbBranchExtraPorts WHERE GY_RL_NKAdditionalBranchRelatedPort = JS_RL_NKOrigin)
				)
			ORDER BY
				CASE
					WHEN GB_RL_NKHomePort = JS_RL_NKOrigin THEN 1
					ELSE 2 END ASC,
				GB_IsActive DESC
		),
		JS_PK,
		JS_UniqueConsignRef,
		JS_BookingReference
	FROM
	(
		SELECT
			resStartPosition = CHARINDEX('|RES=', SL_Reference),
			eventreferenceStartPosition = CHARINDEX('|', SL_Reference),
			refLen = LEN(SL_Reference),
			SL_PK,
			SL_Parent,
			SL_PostedTimeUtc,
			SL_Reference,
			SL_GB_NKBranch,
			JS_PK,
			JS_UniqueConsignRef,
			JS_BookingReference,
			CompanyPK = GlbCompany.GC_PK,
			CompanyCode = GlbCompany.GC_Code,
			JS_RL_NKOrigin,
			GB_RL_NKHomePort
		FROM
			ISNLogs
		LEFT JOIN dbo.RatingHeader ON SL_table = 'ViewQuotedBooking' AND SL_Parent = TH_PK
		LEFT JOIN dbo.JobShipment ON TH_PK = JS_TH_OneTimeQuote OR SL_Parent = JS_PK
		LEFT JOIN dbo.GlbBranch ON GB_Code = SL_GB_NKBranch
		LEFT JOIN dbo.GlbCompany ON GC_PK = GlbBranch.GB_GC
	) ISNLog
)";

		public override string FromClause => @"STULogsWithDetails";

		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string WhereClause => string.Empty;
	}

	#endregion
}
