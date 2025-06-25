namespace CargoWise.Billing.Collectors.Logistics
{
	public class ForwarderFIATAOutboundMessages : RefStlScriptWithDefaults
	{
		#region SuppressResourceStringsCheckRegion

		public override string FeatureCode => "FHS";
		public override string RoleName => "Forwarding Integrations";
		public override string ModuleName => "Forwarding Sea Freight";
		public override string FunctionName => "FIATA Bill of Lading Message Sent";
		public override string FeatureName => "Verified electronic FIATA Bill of Lading (eFBL) Message Sent";
		public override string DataGranularity => "TRN";
		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "ISNLogsWithDetails.EM_PK";
		public override string TransactionDateUtc => "ISNLogsWithDetails.SL_PostedTimeUtc";
		public override string BillingReference1 => "ISNLogsWithDetails.ShipmentUniqueConsignRef";
		public override string BillingReference2 => "'Bill Of Lading'";
		public override string BillingReference3 => "ISNLogsWithDetails.Purpose";
		public override string BillingReference4 => "ISNLogsWithDetails.MessageReference";
		public override string TransactionCount => "1";

		public override string PreparationScript => @"WITH ISNLogs AS
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
		SL_SE_NKEvent = 'ISN'
		AND SL_Table = 'JobDocumentData'
		AND SL_Reference LIKE '%MST=Bill Of Lading%'
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
		AND EI_From = 'FIATA_HBL'
		AND EM_SystemCreateTimeUtc >= DATEADD(DAY, -1, @StartDateTimeInclusive)
		AND EM_SystemCreateTimeUtc < DATEADD(DAY, 1, @EndDateTimeExclusive)
),
ISNLogsWithDetails AS
(
	SELECT
		SL_Parent,
		SL_PostedTimeUtc,
		EM_PK,
		MessageType,
		EventType,
		ShipmentUniqueConsignRef,
		MessageReference,
		Branch = IIF(LEN(EventBranch) > 0, EventBranch, NULL),
		Purpose = CASE Purpose
			WHEN 'ORG' THEN 'Original'
			WHEN 'AMD' THEN 'Amendment'
			WHEN 'WTH' Then 'Withdrawal'
			ELSE '' END
	FROM
	(
		SELECT
			SL_Parent,
			SL_PostedTimeUtc,
			EM_PK,
			X.Y.value('(*:EventParameters/*:MessageType)[1]', 'VARCHAR(50)') AS MessageType,
			X.Y.value('(*:EventType)[1]', 'VARCHAR(3)') AS EventType,
			X.Y.value('(*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type=""ForwardingShipment""]/*:Key)[1]', 'VARCHAR(50)') AS ShipmentUniqueConsignRef,
			X.Y.value('(*:ContextCollection/*:Context[*:Type=""EventBranch""]/*:Value)[1]', 'VARCHAR(10)') AS EventBranch,
			X.Y.value('(*:ContextCollection/*:Context[*:Type=""Purpose""]/*:Value)[1]', 'VARCHAR(10)') AS Purpose,
			X.Y.value('(*:ContextCollection/*:Context[*:Type=""MessageReference""]/*:Value)[1]', 'VARCHAR(50)') AS MessageReference
		FROM ISNLogs
		OUTER APPLY ISNLogs.XMLData.nodes('/*:UniversalEvent/*:Event') as X(Y)
	) ISNLogsWithUEDetails
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
		JOIN ISNLogs ISN ON MSN.SL_Parent = ISN.SL_Parent
		WHERE
			MSN.SL_SE_NKEvent = 'MSN'
			AND MSN.SL_Table = 'JobDocumentData'
			AND MSN.SL_Reference LIKE '%MST=Bill Of Lading%'
			AND MSN.SL_PostedTimeUtc >= DATEADD(MONTH, -3, @StartDateTimeInclusive)
			AND MSN.SL_PostedTimeUtc < @EndDateTimeExclusive
	) MSNLogs
	WHERE RowNo = 1
)";

		public override string FromClause => @"ISNLogsWithDetails
	LEFT JOIN LastMSNLogs ON ISNLogsWithDetails.Branch IS NULL AND ISNLogsWithDetails.SL_Parent = LastMSNLogs.SL_Parent
	LEFT JOIN dbo.GlbBranch Branch ON GB_Code = COALESCE(ISNLogsWithDetails.Branch, LastMSNLogs.SL_GB_NKBranch)
	LEFT JOIN dbo.GlbCompany Company ON GC_PK = GB_GC";

		public override string WhereClause => @"ISNLogsWithDetails.MessageType = 'Bill Of Lading'
	AND ISNLogsWithDetails.EventType = 'ISN'
	AND Company.GC_IsActive = 1
	AND Branch.GB_IsActive = 1";
		public override bool UsedInBilling => false;
		public override string ActiveOn => "ALL";

		#endregion
	}
}
