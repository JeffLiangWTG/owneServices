namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class CustomsPortMessagingOutboundOthers : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "POF";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs and Port Messaging";
		public override string FunctionName => "Forwarding Port Messaging";
		public override string FeatureName => "Port Other Messages";
		public override string DataGranularity => "TRN";

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "ISNLogsWithDetails.EM_PK";
		public override string TransactionDateUtc => "ISNLogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "IIF(LEN(ISNLogsWithDetails.ForwardingId) > 0, ISNLogsWithDetails.ForwardingId, NULL)";
		public override string BillingReference2 => "ISNLogsWithDetails.PortCodeAndMessageType";
		public override string BillingReference3 => "ISNLogsWithDetails.Purpose";

		public override string BillingReference4 => "ISNLogsWithDetails.ContainerNumber";
		public override string TransactionCount => "1";

		public override string PreparationScript => @"
WITH ISNLogs AS
(
	SELECT
		SL_Parent,
		SL_Table,
		SL_PostedTimeUtc,
		CAST(dbo.CLRUncompressAsBytes(EM_MessageData) AS XML) AS XMLData,
		EM_PK
	FROM
		dbo.StmALog
	JOIN
		dbo.GenPivot ON SL_PK = XX_Relation1ID AND XX_RelationType = 'XEM'
	JOIN
		dbo.EDIMessage ON EM_PK = XX_Relation2ID
	WHERE
		SL_SE_NKEvent = 'ISN'
		AND
		(
			(
				SL_Table = 'JobDocumentData'
				AND
				(
					SL_Reference LIKE '%MST=Container Advice to Booking (AMQ)%'
					OR SL_Reference LIKE '%MST=Provisional Unpacking List (LPD)%'
					OR SL_Reference LIKE '%MST=Final Container Manifest (LDE)%'
					OR SL_Reference LIKE '%MST=Outturn Report (CDM)%'
					OR SL_Reference LIKE '%MST=Goods Received (CRESA)%'
				)
			)
			OR
			(
				SL_Table = 'JobContainer'
				AND SL_Reference LIKE '%MST=Tracing Request (TRC)%'
			)
			OR
			(
				SL_Table = 'WhsItemReceiveConsignment'
				AND SL_Reference LIKE '%MST=Goods Received (CRESA)%'
			)
		)
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
		AND EM_SystemCreateTimeUtc >= DATEADD(DAY, -1, @StartDateTimeInclusive)
		AND EM_SystemCreateTimeUtc < DATEADD(DAY, 1, @EndDateTimeExclusive)
),ISNLogsWithDetails AS
(
	SELECT
		SL_Parent,
		SL_Table,
		SL_PostedTimeUtc,
		EM_PK,
		ForwardingId = IIF(LEN(MessageType) > 0 AND MessageType = 'Goods Received (CRESA)', IIF(LEN(ForwardingShipment) > 0, ForwardingShipment, TransitReceive), ForwardingConsol),
		PortCodeAndMessageType = CASE
			WHEN LEN(OperationalPortCode) > 0 AND LEN(MessageType) > 0 THEN CONCAT(OperationalPortCode, ' - ', MessageType)
			WHEN LEN(OperationalPortCode) > 0 THEN OperationalPortCode
			WHEN LEN(MessageType) > 0 THEN MessageType
			ELSE NULL END,
		Branch = IIF(LEN(EventBranch) > 0, EventBranch, NULL),
		Purpose = CASE Purpose
			WHEN 'ORG' THEN 'Original'
			WHEN 'AMD' THEN 'Amendment'
			WHEN 'WTH' Then 'Withdrawal'
			ELSE NULL END,
		ContainerNumber = IIF(LEN(MessageType) > 0 AND MessageType != 'Goods Received (CRESA)' AND LEN(ContainerNumber) > 0, ContainerNumber, NULL)
	FROM
	(
		SELECT
			SL_Parent,
			SL_Table,
			SL_PostedTimeUtc,
			EM_PK,
			X.Y.value('(*:EventParameters/*:MessageType)[1]', 'VARCHAR(50)') AS MessageType,
			X.Y.value('(*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type=""ForwardingConsol""]/*:Key)[1]', 'VARCHAR(50)') AS ForwardingConsol,
			X.Y.value('(*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type=""ForwardingShipment""]/*:Key)[1]', 'VARCHAR(50)') AS ForwardingShipment,
			X.Y.value('(*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type=""TransitReceive""]/*:Key)[1]', 'VARCHAR(50)') AS TransitReceive,
			X.Y.value('(*:ContextCollection/*:Context[*:Type=""OperationalPortCode""]/*:Value)[1]', 'VARCHAR(10)') AS OperationalPortCode,
			X.Y.value('(*:ContextCollection/*:Context[*:Type=""Purpose""]/*:Value)[1]', 'VARCHAR(10)') AS Purpose,
			X.Y.value('(*:ContextCollection/*:Context[*:Type=""ContainerNumber""]/*:Value)[1]', 'VARCHAR(100)') AS ContainerNumber,
			X.Y.value('(*:ContextCollection/*:Context[*:Type=""EventBranch""]/*:Value)[1]', 'VARCHAR(10)') AS EventBranch
		FROM ISNLogs
		OUTER APPLY ISNLogs.XMLData.nodes('/*:UniversalEvent/*:Event') as X(Y)
	) ISNLogsWithEDIDetails
),LastMSNLogs AS
(
	SELECT
		SL_Parent,
		SL_Table,
		SL_GB_NKBranch
	FROM
	(
		SELECT
			MSN.SL_Parent,
			MSN.SL_Table,
			MSN.SL_GB_NKBranch,
			ROW_NUMBER() OVER (PARTITION BY MSN.SL_Parent, MSN.SL_Table ORDER BY MSN.SL_PostedTimeUtc DESC) AS RowNo
		FROM
			dbo.StmALog MSN
		JOIN ISNLogs ISN ON MSN.SL_Parent = ISN.SL_Parent AND MSN.SL_Table = ISN.SL_Table
		WHERE
			MSN.SL_SE_NKEvent = 'MSN'
			AND MSN.SL_IsCancelled = 'N'
			AND
			(
				(
					MSN.SL_Table = 'JobDocumentData'
					AND
					(
						MSN.SL_Reference LIKE '%MST=Container Advice to Booking (AMQ)%'
						OR MSN.SL_Reference LIKE '%MST=Provisional Unpacking List (LPD)%'
						OR MSN.SL_Reference LIKE '%MST=Final Container Manifest (LDE)%'
						OR MSN.SL_Reference LIKE '%MST=Outturn Report (CDM)%'
						OR MSN.SL_Reference LIKE '%MST=Goods Received (CRESA)%'
					)
				)
				OR
				(
					MSN.SL_Table = 'JobContainer'
					AND MSN.SL_Reference LIKE '%MST=Tracing Request (TRC)%'
				)
				OR
				(
					MSN.SL_Table = 'WhsItemReceiveConsignment'
					AND MSN.SL_Reference LIKE '%MST=Goods Received (CRESA)%'
				)
			)
			AND MSN.SL_PostedTimeUtc >= DATEADD(MONTH, -3, @StartDateTimeInclusive)
			AND MSN.SL_PostedTimeUtc < @EndDateTimeExclusive
	) MSNLogs
	WHERE RowNo = 1
)";
		public override string FromClause => @"ISNLogsWithDetails
	LEFT JOIN LastMSNLogs ON ISNLogsWithDetails.Branch IS NULL AND ISNLogsWithDetails.SL_Parent = LastMSNLogs.SL_Parent AND ISNLogsWithDetails.SL_Table = LastMSNLogs.SL_Table
	LEFT JOIN dbo.GlbBranch Branch ON GB_Code = COALESCE(ISNLogsWithDetails.Branch, LastMSNLogs.SL_GB_NKBranch)
	LEFT JOIN dbo.GlbCompany Company ON GC_PK = GB_GC";

		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string WhereClause => string.Empty;
	}

	#endregion
}
