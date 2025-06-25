namespace CargoWise.Billing.Collectors.Logistics
{
	#region SuppressResourceStringsCheckRegion

	public class CustomsPortStatus : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "PSF";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs and Port Messaging";
		public override string FunctionName => "Forwarding Port Messaging";
		public override string FeatureName => "Port Status";
		public override string DataGranularity => "TRN";

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "BillingLogsWithDetails.EM_PK";
		public override string TransactionDateUtc => "BillingLogsWithDetails.SL_PostedTimeUtc";

		public override string BillingReference1 => "BillingLogsWithDetails.ForwardingId";
		public override string BillingReference2 => "BillingLogsWithDetails.LocationCodeAndMessageType";
		public override string BillingReference3 => "BillingLogsWithDetails.EventType";
		public override string BillingReference4 => "BillingLogsWithDetails.CustomsAndEquipmentReferenceNumber";
		public override string TransactionCount => "1";

		public override string PreparationScript => @"
WITH BillingLogs AS
(
	SELECT
		SL_Parent,
		SL_Table,
		SL_PostedTimeUtc,
		SL_SE_NKEvent,
		CAST(dbo.CLRUncompressAsBytes(EM_MessageData) AS XML) AS XMLData,
		EM_PK
	FROM
		dbo.StmALog
	JOIN
		dbo.GenPivot ON SL_PK = XX_Relation1ID AND XX_RelationType = 'XEM'
	JOIN
		dbo.EDIMessage ON EM_PK = XX_Relation2ID
	JOIN
		dbo.EDIInterchange ON EM_EI = EI_PK
	WHERE
		(
			(
				SL_SE_NKEvent IN ('MAA', 'MRJ', 'MWA')
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
								OR SL_Reference LIKE '%MST=File Creation Request (DOS)%'
								OR SL_Reference LIKE '%MST=Goods Received (CRESA)%'
						)
					)
					OR
					(
						SL_Table = 'JobConsol'
						AND SL_Reference LIKE '%MST=Tracing Request (TRC)%'
					)
					OR
					(
						SL_Table = 'WhsItemReceiveConsignment'
						AND SL_Reference LIKE '%MST=Goods Received (CRESA)%'
					)
				)
			)
			OR
			(
				SL_SE_NKEvent NOT IN ('MAA', 'MRJ', 'MWA', 'IRA', 'IRJ', 'ISN')
				AND SL_Table = 'JobContainer'
			)
		)
		AND SL_PostedTimeUtc >= @StartDateTimeInclusive
		AND SL_PostedTimeUtc < @EndDateTimeExclusive
		AND EM_SystemCreateTimeUtc >= DATEADD(DAY, -1, @StartDateTimeInclusive)
		AND EM_SystemCreateTimeUtc < DATEADD(DAY, 1, @EndDateTimeExclusive)
		AND EI_From IN ('SOGET', 'MGI')
),BillingLogsWithDetails AS
(
	SELECT
		SL_Parent,
		SL_Table,
		SL_PostedTimeUtc,
		EM_PK,
		EventType = SL_SE_NKEvent,
		ForwardingId = CASE
			WHEN LEN(ForwardingConsol) > 0 THEN ForwardingConsol
			WHEN LEN(ForwardingShipment) > 0 THEN ForwardingShipment
			WHEN LEN(TransitReceive) > 0 THEN TransitReceive
			WHEN LEN(CustomsReferenceNumber) > 0 THEN CustomsReferenceNumber
			ELSE NULL END,
		LocationCodeAndMessageType = CASE
			WHEN LEN(LocationCode) > 0 AND LEN(MessageType) > 0 THEN CONCAT(LocationCode, ' - ', MessageType)
			WHEN LEN(LocationCode) > 0 THEN LocationCode
			WHEN LEN(MessageType) > 0 THEN MessageType
			ELSE NULL END,
		CustomsAndEquipmentReferenceNumber = CASE
			WHEN LEN(CustomsReferenceNumber) > 0 AND LEN(EquipmentReferenceNumber) > 0 THEN CONCAT(CustomsReferenceNumber, ' - ', EquipmentReferenceNumber)
			WHEN LEN(CustomsReferenceNumber) > 0 THEN CustomsReferenceNumber
			WHEN LEN(EquipmentReferenceNumber) > 0 THEN EquipmentReferenceNumber
			ELSE NULL END
	FROM
	(
		SELECT
			SL_Parent,
			SL_Table,
			SL_PostedTimeUtc,
			SL_SE_NKEvent,
			EM_PK,
			X.Y.value('(*:EventParameters/*:MessageType)[1]', 'VARCHAR(50)') AS MessageType,
			X.Y.value('(*:EventParameters/*:Location)[1]', 'VARCHAR(10)') AS LocationCode,
			X.Y.value('(*:EventParameters/*:CustomsReferenceNumber)[1]', 'VARCHAR(50)') AS CustomsReferenceNumber,
			X.Y.value('(*:EventParameters/*:EquipmentReferenceNumber)[1]', 'VARCHAR(50)') AS EquipmentReferenceNumber,
			X.Y.value('(*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type=""ForwardingConsol""]/*:Key)[1]', 'VARCHAR(50)') AS ForwardingConsol,
			X.Y.value('(*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type=""ForwardingShipment""]/*:Key)[1]', 'VARCHAR(50)') AS ForwardingShipment,
			X.Y.value('(*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type=""TransitReceive""]/*:Key)[1]', 'VARCHAR(50)') AS TransitReceive
		FROM BillingLogs
		OUTER APPLY BillingLogs.XMLData.nodes('/*:UniversalEvent/*:Event') as X(Y)
	) BillingLogsWithEDIDetails
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
		JOIN BillingLogs Billing ON MSN.SL_Parent = Billing.SL_Parent AND MSN.SL_Table = Billing.SL_Table
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
						OR MSN.SL_Reference LIKE '%MST=File Creation Request (DOS)%'
						OR MSN.SL_Reference LIKE '%MST=Goods Received (CRESA)%'
					)
				)
				OR
				(
					MSN.SL_Table IN ('JobConsol', 'JobContainer')
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

		public override string FromClause => @"BillingLogsWithDetails
	LEFT JOIN LastMSNLogs ON BillingLogsWithDetails.SL_Parent = LastMSNLogs.SL_Parent AND BillingLogsWithDetails.SL_Table = LastMSNLogs.SL_Table
	LEFT JOIN dbo.GlbBranch Branch ON GB_Code = LastMSNLogs.SL_GB_NKBranch
	LEFT JOIN dbo.GlbCompany Company ON GC_PK = GB_GC";

		public override bool UsedInBilling => true;
		public override string ActiveOn => "ALL";
		public override string WhereClause => string.Empty;
	}

	#endregion
}
