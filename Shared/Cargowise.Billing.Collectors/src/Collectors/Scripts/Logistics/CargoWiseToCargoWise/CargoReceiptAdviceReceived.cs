namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class CargoReceiptAdviceReceived : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "CRR";
		public override string RoleName => "Forwarding Shipment";
		public override string ModuleName => "Forwarder";
		public override string FunctionName => "Ocean Carrier Messaging - Cargo Receipt Advice";
		public override string FeatureName => "CargoWise to CargoWise Cargo Receipt Advice Received";
		public override string DataGranularity => RefStlItemGrain.Daily;

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "MRRLogsWithDetails.SL_PK";
		public override string TransactionDateUtc => "MRRLogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "JobShipment.JS_UniqueConsignRef";
		public override string BillingReference2 => "(SELECT STUFF((SELECT ', ' + CE_EntryNum FROM dbo.CusEntryNum WHERE CE_ParentID = MRRLogsWithDetails.ShipmentPK AND CE_ParentTable = 'JobShipment' AND CE_EntryType = 'BKG' FOR XML PATH('')), 1, 2, ''))";
		public override string BillingReference3 => "OrgCusCode.OK_CustomsRegNo";
		public override string BillingReference4 => "CONCAT('Cargo Receipt Advice', IIF(LEN(MSB) > 0, ' - ', ''), MSB)";

		public override string TransactionCount => "1";

		public override string PreparationScript => @"WITH MRRLogs AS
(
	SELECT
		SL_PK,
		SL_Parent,
		SL_PostedTimeUtc,
		SL_Reference
	FROM
		dbo.StmALog
	WHERE
		SL_SE_NKEvent = 'MRR'
		AND	SL_Table = 'JobShipment'
		AND SL_Reference LIKE '%MST=Cargo Receipt Advice%'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
), MRRLogsWithDetails AS
(
	SELECT
		MSB = CASE msbStartPosition
			WHEN 0 THEN ''
			ELSE SUBSTRING(SL_Reference, msbStartPosition + 5, IIF(CHARINDEX('|', SL_Reference, msbStartPosition + 1) = 0, refLen + 1, CHARINDEX('|', SL_Reference, msbStartPosition + 1)) - msbStartPosition - 5) END,
		SL_PK,
		ShipmentPK,
		SL_PostedTimeUtc,
		SL_GB_NKBranch =
		(
			SELECT TOP 1
				MSN.SL_GB_NKBranch
			FROM
				dbo.StmALog MSN
			WHERE
				MSN.SL_Parent = (SELECT JDD_PK FROM dbo.JobDocumentData WHERE JDD_ParentTableCode = 'JS' AND JDD_ParentID = ShipmentPK AND JDD_Name = 'BookingRequest')
				AND MSN.SL_SE_NKEvent = 'MSN'
				AND MSN.SL_Table = 'JobDocumentData'
				AND MSN.SL_PostedTimeUtc >= DATEADD(MONTH, -1, MRRLog.SL_PostedTimeUtc)
				AND MSN.SL_PostedTimeUtc < MRRLog.SL_PostedTimeUtc
				AND MSN.SL_Reference LIKE '%MST=Booking Request%'
			ORDER BY MSN.SL_PostedTimeUtc DESC
		)
	FROM
	(
		SELECT
			msbStartPosition = CHARINDEX('|MSB=', SL_Reference),
			refLen = LEN(SL_Reference),
			SL_PK,
			ShipmentPK = SL_Parent,
			SL_PostedTimeUtc,
			SL_Reference
		FROM
			MRRLogs
	) MRRLog
)";

		public override string FromClause => @"MRRLogsWithDetails
LEFT JOIN dbo.JobShipment ON ShipmentPK = JS_PK
LEFT JOIN dbo.OrgAddress ON JS_OA_BookedShippingLineAddress = OA_PK
LEFT JOIN dbo.OrgCusCode ON OA_OH = OK_OH AND OK_CodeType = 'C1C'
LEFT JOIN dbo.GlbBranch Branch ON Branch.GB_Code = MRRLogsWithDetails.SL_GB_NKBranch
LEFT JOIN dbo.GlbCompany Company ON Company.GC_PK = Branch.GB_GC";

		public override string WhereClause => string.Empty;
	}

	#endregion
}
