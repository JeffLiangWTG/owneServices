namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	sealed public class EManifestSentFromShipments : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "EMN";
		public override string RoleName => "eManifest Submission to China Handling Agent";
		public override string ModuleName => "Forwarding Shipment";
		public override string FunctionName => "Ocean Carrier Messaging - China eManifest";
		public override string FeatureName => "Electronic Manifest message sent to Handling Agents in China";
		public override string DataGranularity => RefStlItemGrain.Daily;

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "ISNLogsWithDetails.SL_PK";
		public override string TransactionDateUtc => "ISNLogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "JobShipment.JS_UniqueConsignRef";
		public override string BillingReference2 => @"
(
	SELECT TOP 1 occ.OK_CustomsRegNo FROM dbo.OrgCusCode occ 
	WHERE 
	occ.OK_OH = (
					SELECT
					TOP 1
					CASE WHEN jda.E2_AddressOverride = 1 THEN (SELECT TOP 1 oh.OH_PK FROM dbo.OrgHeader oh WHERE oh.OH_Code = 'MISC')
					WHEN jda.E2_AddressOverride = 0 AND NOT(jda.E2_OA_Address IS NULL) THEN (SELECT oa.OA_OH FROM dbo.OrgAddress oa WHERE oa.OA_PK = jda.E2_OA_Address)
					ELSE NULL
					END
					FROM dbo.JobConShipLink jcsl
					JOIN dbo.JobConsol jc ON jcsl.JN_JK = jc.JK_PK
					JOIN dbo.JobConsolTransport jct ON jct.JW_ParentGUID  = jc.JK_PK AND jct.JW_TransportMode = 'SEA' AND LEFT(jct.JW_RL_NKLoadPort, 2) = 'CN' AND LEFT(jct.JW_RL_NKDiscPort, 2) != 'CN'
					JOIN dbo.JobDocAddress jda  ON jda.E2_ParentID = jc.JK_PK AND jda.E2_AddressType = 'CHA'
					WHERE jcsl.JN_JS = JobShipment.JS_PK
				)
	AND OK_CodeType = 'C1C'
) 
";
		public override string BillingReference3 => "RFN";
		public override string BillingReference4 => "CONCAT(LOC, ' - ', MST, '[', STA, ']')";

		public override string TransactionCount => "1";

		public override string AdditionalRefs => @"CONVERT(VARBINARY(MAX), CONVERT(VARCHAR(MAX), (SELECT EventReference  = ISNLogsWithDetails.SL_Reference,
	ShipmentType = JobShipment.JS_ShipmentType,
	TransportMode = JobShipment.JS_TransportMode,
	ContainerMode = JobShipment.JS_PackingMode,
	Origin = JobShipment.JS_RL_NKOrigin,
	Destination = JobShipment.JS_RL_NKDestination
FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)))";

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
		AND SL_Reference LIKE '%MST=eManifest%'
		AND SL_Reference LIKE '%DEP=CargoWise%'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
), ISNLogsWithDetails AS
(
	SELECT
		STA = CASE staStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, staStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, staStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, staStartPosition + 1)) - staStartPosition - 5) END,
		LOC = CASE locStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, locStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, locStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, locStartPosition + 1)) - locStartPosition - 5) END,
		MST = CASE mstStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, mstStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, mstStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, mstStartPosition + 1)) - mstStartPosition - 5) END,
		RFN = CASE rfnStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, rfnStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, rfnStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, rfnStartPosition + 1)) - rfnStartPosition - 5) END,
		SL_PK,
		SL_Reference,
		SL_Parent,
		SL_PostedTimeUtc,
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
				AND MSNOrMWR.SL_Reference LIKE '%MST=eManifest%'
			ORDER BY MSNOrMWR.SL_PostedTimeUtc DESC
		)
	FROM
	(
		SELECT
			staStartPosition = CHARINDEX('|STA=', SL_Reference),
			locStartPosition = CHARINDEX('|LOC=', SL_Reference),
			mstStartPosition = CHARINDEX('|MST=', SL_Reference),
			rfnStartPosition = CHARINDEX('|RFN=', SL_Reference),
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

		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string WhereClause => string.Empty;
	}

	#endregion
}
