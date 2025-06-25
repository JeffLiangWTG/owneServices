namespace CargoWise.Billing.Collectors.Logistics
{
	public class ForwarderFIATAInboundMessages : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion
		public override string FeatureCode => "FHR";
		public override string RoleName => "Forwarding Integrations";
		public override string ModuleName => "Forwarding Sea Freight";
		public override string FunctionName => "FIATA Bill of Lading Message Response Received";
		public override string FeatureName => "Verified electronic FIATA Bill of Lading (eFBL) Message Response Received";
		public override string DataGranularity => "TRN";
		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "ATHLogsWithDetails.EM_PK";
		public override string TransactionDateUtc => "ATHLogsWithDetails.SL_PostedTimeUtc";
		public override string BillingReference1 => "ATHLogsWithDetails.ShipmentUniqueConsignRef";
		public override string BillingReference2 => "'Bill Of Lading'";
		public override string BillingReference3 => "'ATH'";
		public override string BillingReference4 => "ATHLogsWithDetails.FileNameWithoutExtension";
		public override string TransactionCount => "1";

		public override string PreparationScript => @"WITH ATHLogs AS
(
	SELECT
		SL_Parent,
		SL_PostedTimeUtc,
		CAST(dbo.CLRUncompressAsBytes(EM_MessageData) AS XML) AS XMLData,
		EM_PK
	FROM
		dbo.StmALog
	JOIN
		dbo.GenPivot ON SL_PK = XX_Relation1ID AND XX_RelationType = 'XEM'
	JOIN
		dbo.EDIMessage ON EM_PK = XX_Relation2ID
	JOIN
		dbo.EDIInterchange ON EI_PK = EM_EI
	WHERE
		SL_SE_NKEvent = 'ATH'
		AND SL_Table = 'JobDocumentData'
		AND SL_Reference LIKE '%TYP=Bill Of Lading%'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
		AND EI_From = 'FIATA_HBL'
		AND EM_SystemCreateTimeUtc >= DATEADD(DAY, -1, @StartDateTimeInclusive)
		AND EM_SystemCreateTimeUtc < DATEADD(DAY, 1, @EndDateTimeExclusive)
),
ATHLogsWithDetails AS
(
	SELECT
		SL_Parent,
		SL_PostedTimeUtc,
		EM_PK,
		Type,
		EventType,
		ShipmentUniqueConsignRef,
		FileNameWithoutExtension = LEFT(FileName, LEN(FileName) - 4),
		Branch = IIF(LEN(EventBranch) > 0, EventBranch, NULL)
	FROM
	(
		SELECT
			SL_Parent,
			SL_PostedTimeUtc,
			EM_PK,
			X.Y.value('(*:EventParameters/*:Type)[1]', 'VARCHAR(50)') AS Type,
			X.Y.value('(*:EventType)[1]', 'VARCHAR(3)') AS EventType,
			X.Y.value('(*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type=""ForwardingShipment""]/*:Key)[1]', 'VARCHAR(50)') AS ShipmentUniqueConsignRef,
			X.Y.value('(*:ContextCollection/*:Context[*:Type=""EventBranch""]/*:Value)[1]', 'VARCHAR(3)') AS EventBranch,
			X.Y.value('(*:AttachedDocumentCollection/*:AttachedDocument/*:FileName)[1]', 'VARCHAR(50)') AS FileName
		FROM ATHLogs
		OUTER APPLY ATHLogs.XMLData.nodes('/*:UniversalEvent/*:Event') as X(Y)
	) ATHLogsWithUEDetails
),
LastMSNLogs AS
(
	SELECT
		SL_Parent,
		SL_GB_NKBranch
	FROM
	(
		SELECT
			MSN.SL_Parent,
			MSN.SL_GB_NKBranch,
			ROW_NUMBER() OVER (PARTITION BY MSN.SL_Parent ORDER BY MSN.SL_PostedTimeUtc DESC) AS RowNo
		FROM
			dbo.StmALog MSN
		JOIN ATHLogs ATH ON MSN.SL_Parent = ATH.SL_Parent
		WHERE
			MSN.SL_SE_NKEvent = 'MSN'
			AND MSN.SL_Table = 'JobDocumentData'
			AND MSN.SL_Reference LIKE '%MST=Bill Of Lading%'
			AND MSN.SL_PostedTimeUtc >= DATEADD(MONTH, -3, @StartDateTimeInclusive)
			AND MSN.SL_PostedTimeUtc < @EndDateTimeExclusive
	) MSNLogs
	WHERE RowNo = 1
)";

		public override string FromClause => @"ATHLogsWithDetails
	LEFT JOIN LastMSNLogs ON ATHLogsWithDetails.SL_Parent = LastMSNLogs.SL_Parent
	LEFT JOIN dbo.GlbBranch Branch ON GB_Code = LastMSNLogs.SL_GB_NKBranch
	LEFT JOIN dbo.GlbCompany Company ON GC_PK = GB_GC";

		public override string WhereClause => @"ATHLogsWithDetails.Type = 'Bill Of Lading'
	AND ATHLogsWithDetails.EventType = 'ATH'
	AND Company.GC_IsActive = 1
	AND Branch.GB_IsActive = 1";
		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";

		#endregion
	}
}
