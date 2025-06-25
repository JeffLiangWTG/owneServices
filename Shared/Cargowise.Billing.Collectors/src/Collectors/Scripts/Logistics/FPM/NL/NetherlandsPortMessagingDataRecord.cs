namespace CargoWise.Billing.Collectors.Logistics
{
	public class NetherlandsPortMessagingDataRecord : RefStlScriptWithDefaults
	{
		public override string FeatureCode => "PMN";
		public override string RoleName => "Customs & Country Specific Integrations";
		public override string ModuleName => "Other Parties Customs and Port Messaging";
		public override string FeatureName => "Netherlands Port Messaging Data Record";
		public override string FunctionName => "Forwarding Port Messaging";
		public override string DataGranularity => RefStlItemGrain.Transactional;

		public override string CompanyCode => "Company.GC_Code";
		public override string BranchCode => "Branch.GB_Code";
		public override string GuidReference => "EM_PK";
		public override string TransactionDateUtc => "EM_SystemCreateTimeUtc";

		public override string BillingReference1 => "ForwardingConsolKey";
		public override string BillingReference2 => "CONCAT(OperationalPort_Code, ' - ', DocumentName)";
		public override string BillingReference3 => "CONCAT(ReferenceNumber, ' [', EntryType_Code, ']')";

		public override string PreparationScript => $@"
DROP TABLE IF EXISTS #NLPortBaseNotificationMessage;

WITH InterestedNLPortBaseMessage AS
(
	SELECT
		CLRUncompressedEM_MessageData,
		EM_PK,
		EM_SystemCreateTimeUtc
	FROM
	(
		SELECT
			IsNLPortBaseMessage = CASE
				WHEN CHARINDEX('http://www.cargowise.com/Schemas/Universal/2012/11/PortbaseExportNotification/1', CLRUncompressedEM_MessageData) != 0 THEN 1
				WHEN CHARINDEX('http://www.cargowise.com/Schemas/Universal/2012/11/PortbaseImportNotification/1', CLRUncompressedEM_MessageData) != 0 THEN 1
				ELSE 0
				END,
			CLRUncompressedEM_MessageData,
			EM_PK,
			EM_SystemCreateTimeUtc
		FROM
		(
			SELECT
				CLRUncompressedEM_MessageData = IIF(LEFT(EM_MessageData, 2) = 'PZ', dbo.CLRUncompressAsBytes(EM_MessageData), EM_MessageData),
				EM_PK,
				EM_SystemCreateTimeUtc
			FROM dbo.EDIMessage
			JOIN dbo.EDIInterchange ON EM_EI = EI_PK
			WHERE
				EM_ApplicationCode = 'UDM'
				AND EM_ReceiveTransmit = 'TRX'
				AND EM_SystemCreateTimeUtc >= {Constants.StartDateTimeInclusiveParamName}
				AND EM_SystemCreateTimeUtc < {Constants.EndDateTimeExclusiveParamName}
				AND EM_Status = 'SNT'
				AND EI_To = 'FORWARDING_PORT_MESSAGE'
				AND EM_MessageSubType = 'XUS'
		) CLRUncompressedEDIMessage
	) EDIMessage
	WHERE IsNLPortBaseMessage = 1
), NLPortBaseMessage AS
(
	SELECT
		CLRUncompressedEM_MessageData,
		EM_PK,
		EM_SystemCreateTimeUtc,
		IsSuccess = CASE
			WHEN
			(
				(
					SELECT
						TOP 1 SL_SE_NKEvent
					FROM
						dbo.StmALog
					WHERE
						SL_Parent = DataExportParent
						AND SL_SE_NKEvent IN ('ISN', 'IRJ')
						AND SL_PostedTimeUtc > DataExportPostedTimeUtc
						AND SL_PostedTimeUtc < DATEADD(HOUR, 2, {Constants.EndDateTimeExclusiveParamName})
					ORDER BY SL_PostedTimeUtc ASC
				) = 'ISN'
			) THEN 1
			ELSE 0
			END
	FROM InterestedNLPortBaseMessage
	JOIN dbo.GenPivot ON XX_RelationType = 'XEM' AND EM_PK = XX_Relation2ID
	JOIN
	(
		SELECT
			SL_PK,
			SL_Parent AS DataExportParent,
			SL_PostedTimeUtc AS DataExportPostedTimeUtc
		FROM dbo.StmALog
		WHERE
			SL_PostedTimeUtc >= DATEADD(MINUTE, -30, {Constants.StartDateTimeInclusiveParamName})
			AND SL_PostedTimeUtc < DATEADD(MINUTE, 30, {Constants.EndDateTimeExclusiveParamName})
			AND SL_Table = 'JobDocumentData'
			AND SL_SE_NKEvent = 'DEX'
	) DataExportLog ON XX_Relation1ID = SL_PK
)

SELECT
	CAST(CLRUncompressedEM_MessageData as XML) XMLData,
	EM_PK,
	EM_SystemCreateTimeUtc
INTO #NLPortBaseNotificationMessage
FROM NLPortBaseMessage
WHERE IsSuccess = 1;

WITH NLPortBaseMessageWithDetails AS
(
	SELECT
		EM_PK,
		EM_SystemCreateTimeUtc,
		UniversalShipment.EventBranch,
		UniversalShipment.ForwardingConsolKey,
		UniversalShipment.OperationalPort_Code,
		UniversalShipment.DocumentName,
	
		X.Y.value('(*:AddInfoCollection/*:AddInfo[*:Key=""ReferenceNumber""]/*:Value)[1]', 'VARCHAR(50)') AS ReferenceNumber,
		X.Y.value('(*:AddInfoCollection/*:AddInfo[*:Key=""EntryType_Code""]/*:Value)[1]', 'VARCHAR(50)') AS EntryType_Code
	FROM #NLPortBaseNotificationMessage
	CROSS APPLY (
		SELECT
			XMLData.value('(/*:UniversalShipment/*:Shipment/*:DataContext/*:Workflow/*:EventBranch)[1]', 'VARCHAR(5)') AS EventBranch,
			XMLData.value('(/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type=""ForwardingConsol""]/*:Key)[1]', 'VARCHAR(50)') AS ForwardingConsolKey,
			XMLData.value('(/*:UniversalShipment/*:Shipment/*:AddInfoCollection/*:AddInfo[*:Key=""OperationalPort_Code""]/*:Value)[1]', 'VARCHAR(15)') AS OperationalPort_Code,
			XMLData.value('(/*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]', 'VARCHAR(50)') AS DocumentName
	) UniversalShipment
	CROSS APPLY XMLData.nodes('/*:UniversalShipment/*:Shipment/*:SubShipmentCollection/*:SubShipment') AS X(Y)
)";

		public override string FromClause => @"NLPortBaseMessageWithDetails
	LEFT JOIN dbo.GlbBranch Branch ON NLPortBaseMessageWithDetails.EventBranch = Branch.GB_Code
	LEFT JOIN dbo.GlbCompany Company ON Branch.GB_GC = Company.GC_PK";
		public override string WhereClause => string.Empty;
	}
}
