SET NOCOUNT ON;

SELECT * FROM
(
	SELECT Sender.CC_ID AS ClientID
		,'POZ' AS PriceItemCode
		,AM_SenderMessageXML.value('(//*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type="ForwardingConsol"]/*:Key)[1]', 'varchar(50)') AS Reference1Prefix
		,AM_SenderMessageXML.value('(//*:UniversalShipment/*:Shipment/*:SubShipmentCollection/*:SubShipment/*:PackingLineCollection/*:PackingLine/*:ImportReferenceNumber)[1]', 'varchar(50)') AS ImportReferenceNumber
		,AM_SenderMessageXML.value('(//*:UniversalShipment/*:Shipment/*:SubShipmentCollection/*:SubShipment/*:PackingLineCollection/*:PackingLine/*:ExportReferenceNumber)[1]', 'varchar(50)') AS ExportReferenceNumber
		,AM_SenderMessageXML.value('(//*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]', 'varchar(50)') AS MessageType
		,AM_SenderMessageXML.value('(//*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:DataVersion)[1]', 'varchar(50)') AS DataVersion
		,AM_SenderMessageXML.value('(//*:UniversalShipment/*:Shipment/*:DataContext/*:Workflow/*:ActionPurpose/*:ActionCode)[1]', 'varchar(50)') AS ActionCode
		,'' AS EventType
		,'' AS OrderNumber
		,AM_InboxMessageTrackingID AS MessageTrackingID
		,AM_ReceivedFromSenderUTC
		,AM_ArchivedUTC
	FROM eHubArchiveMessage
	INNER JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
	INNER JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientOutbox
	WHERE @startUTC < AM_ArchivedUTC
		AND AM_ArchivedUTC <= @endUTC
		AND Recipient.CC_ID = 'TPT'
		AND AM_Status < 255

	UNION ALL

	SELECT Recipient.CC_ID AS ClientID
		,'PSZ' AS PriceItemCode
		,UniversalEvents.UniversalEvent.value('(//*:UniversalEvent/*:Event/*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type="ForwardingConsol"]/*:Key)[1]', 'varchar(50)') AS Reference1Prefix
		,UniversalEvents.UniversalEvent.value('(//*:UniversalEvent/*:Event/*:EventParameters/*:CustomsReferenceNumber)[1]', 'varchar(50)') AS ImportReferenceNumber
		,'' AS ExportReferenceNumber
		,UniversalEvents.UniversalEvent.value('(//*:UniversalEvent/*:Event/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]', 'varchar(50)') AS MessageType
		,'' AS DataVersion
		,'' AS ActionCode
		,UniversalEvents.UniversalEvent.value('(//*:UniversalEvent/*:Event/*:EventType)[1]', 'varchar(50)') AS EventType
		,UniversalEvents.UniversalEvent.value('(//*:UniversalEvent/*:Event/*:EventParameters/*:ReferenceNumber)[1]', 'varchar(50)') AS OrderNumber
		,AM_InboxMessageTrackingID AS MessageTrackingID
		,AM_ReceivedFromSenderUTC
		,AM_ArchivedUTC
	FROM eHubArchiveMessage
	INNER JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
	INNER JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientOutbox
	CROSS APPLY AM_RecipientMessageXML.nodes('(/*:UniversalInterchange/*:Body/*:UniversalEvent)') UniversalEvents(UniversalEvent)
	WHERE @startUTC < AM_ArchivedUTC
		AND AM_ArchivedUTC <= @endUTC
		AND Sender.CC_ID = 'TPT'
		AND AM_Status < 255
		AND UniversalEvents.UniversalEvent.value('(*:Event/*:EventType)[1]', 'nvarchar(50)') IN ('MAA', 'MRJ', 'MWA', 'STU')
)result
WHERE IsNull(Reference1Prefix,'') !=''
OPTION(RECOMPILE)
