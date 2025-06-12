SET NOCOUNT ON;

SELECT * FROM
(
	SELECT
		Sender.CC_ID AS ClientID
		,'POZ' AS PriceItemCode
		,CASE WHEN AM_SenderMessageXML.value('(//*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type="ForwardingConsol"]/*:Key)[1]', 'varchar(50)') IS NOT NULL
		THEN AM_SenderMessageXML.value('(//*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type="ForwardingConsol"]/*:Key)[1]', 'varchar(50)')
		ELSE AM_SenderMessageXML.value('(//*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type="CustomsDeclaration"]/*:Key)[1]', 'varchar(50)') END AS Reference1
		,AM_SenderMessageXML.value('(//*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]', 'varchar(50)') AS Reference2
		,AM_SenderMessageXML.value('(//*:UniversalShipment/*:Shipment/*:DataContext/*:Workflow/*:ActionPurpose)[1]', 'varchar(3)') AS Reference3
		,'' AS Reference4
		,AM_SenderMessageXML.value('(//*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:DataVersion)[1]', 'varchar(10)') AS DataVersion
		,AM_InboxMessageTrackingID AS MessageTrackingID
		,AM_ReceivedFromSenderUTC
		,AM_ArchivedUTC
	FROM eHubArchiveMessage
	INNER JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
	INNER JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientInbox	
	WHERE @startUTC < AM_ArchivedUTC
		AND AM_ArchivedUTC <= @endUTC
		AND Recipient.CC_ID = 'TNPA'
		AND AM_Status < 255
)result
WHERE IsNull(Reference1,'') !=''

UNION ALL

SELECT
	Recipient.CC_ID AS ClientID
	,'PSZ' AS PriceItemCode
	,CASE WHEN UniversalEvents.UniversalEvent.value('(//*:UniversalEvent/*:Event/*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type="ForwardingConsol"]/*:Key)[1]', 'varchar(50)') IS NOT NULL
	THEN UniversalEvents.UniversalEvent.value('(//*:UniversalEvent/*:Event/*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type="ForwardingConsol"]/*:Key)[1]', 'varchar(50)')
	ELSE UniversalEvents.UniversalEvent.value('(//*:UniversalEvent/*:Event/*:DataContext/*:DataTargetCollection/*:DataTarget[*:Type="CustomsDeclaration"]/*:Key)[1]', 'varchar(50)') END AS Reference1
	,UniversalEvents.UniversalEvent.value('(//*:UniversalEvent/*:Event/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]', 'varchar(50)') AS Reference2
	,UniversalEvents.UniversalEvent.value('(//*:UniversalEvent/*:Event/*:EventType)[1]', 'varchar(5)') AS Reference3
	,UniversalEvents.UniversalEvent.value('(//*:UniversalEvent/*:Event/*:EventParameters/*:ReferenceNumber)[1]', 'varchar(50)') AS Reference4
	,'' AS DataVersion
	,AM_InboxMessageTrackingID AS MessageTrackingID
	,AM_ReceivedFromSenderUTC
	,AM_ArchivedUTC
FROM eHubArchiveMessage
	INNER JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox	
	INNER JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientOutbox
CROSS APPLY AM_RecipientMessageXML.nodes('(/*:UniversalInterchange/*:Body/*:UniversalEvent)') UniversalEvents(UniversalEvent)
WHERE @startUTC < AM_ArchivedUTC
	AND AM_ArchivedUTC <= @endUTC
	AND Sender.CC_ID = 'TNPA'
	AND AM_Status < 255
	AND UniversalEvents.UniversalEvent.value('(*:Event/*:EventType)[1]', 'nvarchar(50)') IN ('MAA', 'MWA', 'STU')
OPTION(RECOMPILE)