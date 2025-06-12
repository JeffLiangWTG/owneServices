SET NOCOUNT ON;

SELECT * FROM
(
	SELECT Sender.CC_ID AS ClientID
		,AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:PortMessaging/*:MessagePurpose)[1]', 'varchar(3)') AS MessagePurpose
		,AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSourceCollection/*:DataSource[*:Type="ForwardingConsol"]/*:Key)[1]', 'varchar(50)') AS Reference1
		,AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSourceCollection/*:DataSource[*:Type="ForwardingShipment"]/*:Key)[1]', 'varchar(50)') AS Reference2
		,AM_InboxMessageTrackingID AS MessageTrackingID
		,AM_ReceivedFromSenderUTC
		,AM_ArchivedUTC
		,Recipient.CC_ID AS ServiceProvider
	FROM eHubArchiveMessage
	INNER JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
	INNER JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientOutbox
	WHERE @startUTC < AM_ArchivedUTC
		AND AM_ArchivedUTC <= @endUTC
		AND Recipient.CC_ID = 'DAKOSYHAM'
		AND AM_Status < 255

	UNION ALL

	SELECT Recipient.CC_ID AS ClientID
		,'PM3' AS MessagePurpose
		,UniversalEvents.UniversalEvent.value('(*:Event/*:DataContext/*:DataTargetCollection/*:DataTarget/*:Key)[1]', 'nvarchar(50)') AS Reference1
		,UniversalEvents.UniversalEvent.value('(*:Event/*:ContextCollection/*:Context[*:Type="Z or B-Number"]/*:Value)[1]', 'varchar(50)') AS Reference2
		,AM_InboxMessageTrackingID AS MessageTrackingID
		,AM_ReceivedFromSenderUTC
		,AM_ArchivedUTC
		,Sender.CC_ID AS ServiceProvider
	FROM eHubArchiveMessage
	INNER JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
	INNER JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientOutbox
	CROSS APPLY AM_RecipientMessageXML.nodes('(/*:UniversalInterchange/*:Body/*:UniversalEvent)') UniversalEvents(UniversalEvent)
	WHERE @startUTC < AM_ArchivedUTC
		AND AM_ArchivedUTC <= @endUTC
		AND Sender.CC_ID = 'DAKOSYHAM'
		AND AM_Status < 255
		AND (
			UniversalEvents.UniversalEvent.value('(*:Event/*:EventType)[1]', 'nvarchar(50)') = 'MAA'
			OR (
				UniversalEvents.UniversalEvent.value('(*:Event/*:ContextCollection/*:Context[*:Type="Description"]/*:Value)[1]', 'varchar(50)') = 'ZAPP-Status Notification'
				AND UniversalEvents.UniversalEvent.value('(*:Event/*:EventType)[1]', 'nvarchar(50)') != 'MIS'
				)
			)
)result
WHERE IsNull(Reference1,'') !=''
OPTION(RECOMPILE)
