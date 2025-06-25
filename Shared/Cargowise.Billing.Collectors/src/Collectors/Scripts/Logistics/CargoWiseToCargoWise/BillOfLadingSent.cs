namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class BillOfLadingSent : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "BLS";
		public override string RoleName => "Forwarding Shipment and L&A Bill Of Lading";
		public override string ModuleName => "Forwarder-Liner and Agency";
		public override string FunctionName => "Forwarder-Liner and Agency";
		public override string FeatureName => "Ocean Carrier Messaging - Bill Of Lading";
		public override string DataGranularity => RefStlItemGrain.Transactional;

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "STULogsWithDetails.SL_PK";
		public override string TransactionDateUtc => "STULogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "STULogsWithDetails.ShipmentNumber";
		public override string BillingReference2 => "STULogsWithDetails.ShipperRef";
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
			JobDocAddress.E2_ParentID = STULogsWithDetails.ShipmentPK
			AND JobDocAddress.E2_AddressType = 'BKD'
		ORDER BY
			E2_AddressSequence ASC
	) BookingPartyDocumentaryAddress
	LEFT JOIN dbo.OrgAddress ON BookingPartyDocumentaryAddress.E2_AddressOverride = 0 AND BookingPartyDocumentaryAddress.E2_OA_Address = OrgAddress.OA_PK
	LEFT JOIN dbo.OrgHeader ON OrgAddress.OA_OH = OrgHeader.OH_PK)";
		public override string BillingReference4 => "STULogsWithDetails.RES";

		public override string TransactionCount => "1";

		public override string PreparationScript => @"WITH STULogs AS
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
		AND SL_Table = 'JobShipment'
		AND SL_Reference LIKE '%NEW=CNF%'
		AND SL_Reference LIKE '%TYP=Shipment Status%'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
), STULogsWithDetails AS
(
	SELECT
		RES = CASE resStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, resStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, resStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, resStartPosition + 1)) - resStartPosition - 5) END,
		SL_PK,
		SL_PostedTimeUtc,
		STULogBranch = SL_GB_NKBranch,
		PreferredBranch =
		(
			SELECT
				TOP 1 GB_Code
			FROM
				dbo.GlbBranch
			WHERE
				GB_GC = STULogCompanyPK
				AND
				(
					GB_RL_NKHomePort = OriginPort
					OR GB_PK IN (SELECT GY_GB FROM dbo.GlbBranchExtraPorts WHERE GY_RL_NKAdditionalBranchRelatedPort = OriginPort)
				)
			ORDER BY
				CASE
					WHEN GB_RL_NKHomePort = OriginPort THEN 1
					ELSE 2 END ASC,
				GB_IsActive DESC
		),
		ShipmentPK = JS_PK,
		ShipmentNumber = JS_UniqueConsignRef,
		ShipperRef = JS_BookingReference
	FROM
	(
		SELECT
			resStartPosition = CHARINDEX('|RES=', SL_Reference),
			refLen = LEN(SL_Reference),
			SL_PK,
			JS_PK,
			SL_PostedTimeUtc,
			SL_GB_NKBranch,
			SL_Reference,
			JS_UniqueConsignRef,
			JS_BookingReference,
			OriginPort = JS_RL_NKOrigin,
			STULogCompanyPK = (SELECT GB_GC FROM dbo.GlbBranch WHERE GB_Code = SL_GB_NKBranch)
		FROM
			STULogs
		LEFT JOIN dbo.JobShipment ON SL_Parent = JS_PK
	) STULog
)";

		public override string FromClause => @"STULogsWithDetails
LEFT JOIN dbo.GlbBranch Branch ON Branch.GB_Code = IIF(STULogsWithDetails.PreferredBranch IS NULL, STULogBranch, PreferredBranch)
LEFT JOIN dbo.GlbCompany Company ON Company.GC_PK = Branch.GB_GC";

		public override string WhereClause => string.Empty;
	}

	#endregion
}
