SET NOCOUNT ON;

BEGIN
	WITH XMLNAMESPACES ('http://cargowise.com/ehub/products/railinc/2011/06' AS ns0), FilteredEHubArchiveMessage AS (
		SELECT
			Sender.CC_ID AS SenderID, 
			Recipient.CC_ID AS RecipientID,
			AM_ReceivedFromSenderUTC,
			AM_ArchivedUTC,
			AM_InboxMessageTrackingID,
			AM_OutboxMessageTrackingID,
			CLU.DTL.value('substring(CustomInformation[1],1,string-length(CustomInformation[1])-2)','varchar(20)') AS CLU_Consol,
			CLU.DTL.value('concat(EquipmentNumber[1],substring(CustomInformation[1],string-length(CustomInformation[1])))', 'varchar(11)') AS CLU_Container,
			CLM.DTL.value('(*:Event/*:DataContext/*:DataTargetCollection/*:DataTarget/*:Key)[1]','varchar(50)') as CLM_Consol,
			CLM.DTL.value('(*:Event/*:ContextCollection/*:Context[*:Type="ContainerNumber"]/*:Value)[1]','varchar(50)') AS CLM_Container,
			CLM.DTL.value('(*:Event/*:EventType)[1]','varchar(3)') AS CLM_EventType,
			CLM.DTL.value('(*:Event/*:IsEstimate)[1]','varchar(5)') AS CLM_IsEstimate,
			CLM.DTL.value('(*:Event/*:EventParameters/*:Location)[1]','varchar(50)') as CLM_Location
		FROM eHubArchiveMessage
			JOIN dbo.eHubClient Sender ON AM_CC_SenderInbox = Sender.CC_PK
			JOIN dbo.eHubClient Recipient ON AM_CC_RecipientOutbox = Recipient.CC_PK
			OUTER APPLY AM_RecipientMessageXML.nodes('/ns0:FleetUpdate/CLUDetail[FleetStatusCode = "L" or FleetStatusCode = "P"]') AS CLU(DTL)
			OUTER APPLY AM_RecipientMessageXML.nodes('/*:UniversalInterchange/*:Body/*:UniversalEvent') AS CLM(DTL)
        WHERE 
            @startUTC < AM_ArchivedUTC AND AM_ArchivedUTC <= @endUTC
			AND (Recipient.CC_ID = 'RAILINCFC' OR Sender.CC_ID = 'RAILINCFC')
            AND AM_Status < 255
			AND
			(
				AM_RecipientMessageXml.value('count(//*[local-name()="UniversalEvent"])', 'int') > 0
				OR
				AM_RecipientMessageXml.value('count(/ns0:FleetUpdate/CLUDetail)', 'int') > 0
			)
    )

	SELECT
		SenderID AS ClientID,
		'CLU' AS MsgType,
		CLU_Consol as Consol,
		CLU_Container AS Container,
		AM_ReceivedFromSenderUTC,
		AM_InboxMessageTrackingID AS MessageTrackingID,
		NULL AS EventType,
		NULL AS IsEstimate,
		NULL as Location,
		AM_ArchivedUTC
	FROM FilteredEHubArchiveMessage
	WHERE RecipientID = 'RAILINCFC'

	UNION ALL

	SELECT
		RecipientID AS ClientID,
		'CLM' AS MsgType,
		CLM_Consol as Consol,
		CLM_Container AS Container,
		AM_ReceivedFromSenderUTC,
		AM_OutboxMessageTrackingID AS MessageTrackingID,
		CLM_EventType AS EventType,
		CLM_IsEstimate AS IsEstimate,
		CLM_Location as Location,
		AM_ArchivedUTC
	FROM FilteredEHubArchiveMessage
	WHERE RecipientID <> 'RAILINCFC' AND RecipientID <> 'CONTAINER_TRACKING'

	OPTION(RECOMPILE)
END