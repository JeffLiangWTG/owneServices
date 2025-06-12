SET NOCOUNT ON;

SELECT Sender.CC_ID AS ClientID
	,'PMN' AS PriceItemCode
	,AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type="ForwardingConsol"]/*:Key)[1]', 'varchar(50)') AS Reference1
	,SubString(Shipments.Shipment.value('local-name(.)', 'varchar(50)'), 9, 41) AS Reference2
	,Recipient.CC_ID AS Reference3
	,Null AS Reference4
	,AM_InboxMessageTrackingID AS MessageTrackingID
	,AM_ReceivedFromSenderUTC
	,AM_ArchivedUTC
FROM eHubArchiveMessage
INNER JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
INNER JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientInbox
CROSS APPLY AM_SenderMessageXML.nodes('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DocumentaryOverride/*[text()="Y"])') Shipments(Shipment)
WHERE @startUTC < AM_ArchivedUTC
	AND AM_ArchivedUTC <= @endUTC
	AND Recipient.CC_ID = 'PORTBASE'
	AND AM_Status < 255
	AND Shipments.Shipment.value('local-name(.)', 'varchar(50)') LIKE 'SendMRN^_%' ESCAPE '^'

UNION ALL

SELECT *
FROM
(SELECT Sender.CC_ID AS ClientID
	,'PMN' AS PriceItemCode
	,AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type="ForwardingConsol"]/*:Key)[1]', 'varchar(50)') AS Reference1
	,CASE WHEN PackingLines.ImportReferenceNumber.value('.', 'varchar(50)') = ''
	 THEN SubShipments.SubShipment.value('(*:EntryNumberCollection/*:EntryNumber/*:Number)[1]', 'varchar(50)')
	 ELSE PackingLines.ImportReferenceNumber.value('.', 'varchar(50)') END AS Reference2
	,Recipient.CC_ID AS Reference3
	,Null AS Reference4
	,AM_InboxMessageTrackingID AS MessageTrackingID
	,AM_ReceivedFromSenderUTC
	,AM_ArchivedUTC
FROM eHubArchiveMessage
INNER JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
INNER JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientInbox
CROSS APPLY AM_SenderMessageXML.nodes('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:SubShipmentCollection/*:SubShipment)') SubShipments(SubShipment)
CROSS APPLY SubShipments.SubShipment.nodes('(*:PackingLineCollection/*:PackingLine/*:ImportReferenceNumber)') PackingLines(ImportReferenceNumber)
WHERE @startUTC < AM_ArchivedUTC
	AND AM_ArchivedUTC <= @endUTC
	AND Recipient.CC_ID = 'PORTBASE'
	AND AM_Status < 255 ) AS S
GROUP BY 
S.Reference1, 
S.ClientID,
S.PriceItemCode,
S.Reference2,
S.Reference3,
S.Reference4,
S.MessageTrackingID,
S.AM_ReceivedFromSenderUTC,
S.AM_ArchivedUTC

UNION ALL
SELECT * FROM
(
	SELECT Recipient.CC_ID AS ClientID
		,'PSN' AS PriceItemCode
		,CASE WHEN UniversalEvents.UniversalEvent.value('(*:Event/*:DataContext/*:DataTargetCollection/*:DataTarget/*:Key)[1]', 'nvarchar(50)') = '' 
		 THEN  UniversalEvents.UniversalEvent.value('(*:Event/*:ContextCollection/*:Context[*:Type="CarriersBookingReference"]/*:Value)[1]', 'nvarchar(50)')
		 ELSE UniversalEvents.UniversalEvent.value('(*:Event/*:DataContext/*:DataTargetCollection/*:DataTarget/*:Key)[1]', 'nvarchar(50)') END AS Reference1
		,UniversalEvents.UniversalEvent.value('(*:Event/*:EventParameters/*:CustomsReferenceNumber)[1]', 'varchar(50)') AS Reference2
		,Sender.CC_ID AS Reference3
		,UniversalEvents.UniversalEvent.value('(*:Event/*:EventParameters/*:Type)[1]', 'varchar(50)') AS Reference4
		,AM_InboxMessageTrackingID AS MessageTrackingID
		,AM_ReceivedFromSenderUTC
		,AM_ArchivedUTC
	FROM eHubArchiveMessage
	INNER JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientOutbox
	INNER JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
	CROSS APPLY AM_RecipientMessageXML.nodes('(/*:UniversalInterchange/*:Body/*:UniversalEvent)') UniversalEvents(UniversalEvent)
	WHERE @startUTC < AM_ArchivedUTC
		AND AM_ArchivedUTC <= @endUTC
		AND Sender.CC_ID = 'PORTBASE'
		AND AM_Status < 255	
		AND UniversalEvents.UniversalEvent.value('(*:Event/*:EventType)[1]', 'nvarchar(50)') = 'STU'
) result
WHERE IsNull(Reference2,'') !=''
OPTION(RECOMPILE)