SET NOCOUNT ON;

SELECT Sender.CC_ID AS ClientID
	,'PMR' AS PriceItemCode
	,'eTerminal Release Manifest' AS MessageType
	,SubShipments.SubShipment.value('.', 'varchar(50)') AS Reference1
	,AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:VesselName)[1]', 'varchar(50)') + '-' + AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:VoyageFlightNo)[1]', 'varchar(50)') AS Reference4
	,AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:Purpose)[1]', 'varchar(3)') AS SubmissionType
	,AM_InboxMessageTrackingID AS MessageTrackingID
	,AM_ReceivedFromSenderUTC
	,AM_ArchivedUTC
FROM eHubArchiveMessage
INNER JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
INNER JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientInbox
CROSS APPLY AM_SenderMessageXML.nodes('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:SubShipmentCollection/*:SubShipment/*:SubShipmentCollection/*:SubShipment/*:AdditionalReferenceCollection/*:AdditionalReference[*:Type="FFW"]/*:ReferenceNumber)') SubShipments(SubShipment)
WHERE @startUTC < AM_ArchivedUTC
	AND AM_ArchivedUTC <= @endUTC
	AND Recipient.CC_ID = 'FORWARDING_PORT_MESSAGE'
	AND AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]', 'varchar(50)')  = 'eTerminal Release Manifest'
	AND AM_Status < 255

UNION ALL

SELECT Sender.CC_ID AS ClientID
	,'PML' AS PriceItemCode
	,'Container Load Plan' AS MessageType
	,AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource/*:Key)[1]', 'varchar(50)') AS Reference1
	,Containers.Container.value('.', 'varchar(50)') AS Reference4
	,AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:Purpose)[1]', 'varchar(3)') AS SubmissionType
	,AM_InboxMessageTrackingID AS MessageTrackingID
	,AM_ReceivedFromSenderUTC
	,AM_ArchivedUTC
FROM eHubArchiveMessage
INNER JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
INNER JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientInbox
CROSS APPLY AM_SenderMessageXML.nodes('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:ContainerCollection/*:Container/*:ContainerNumber)') Containers(Container)
WHERE @startUTC < AM_ArchivedUTC
	AND AM_ArchivedUTC <= @endUTC
	AND Recipient.CC_ID = 'FORWARDING_PORT_MESSAGE'
	AND AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]', 'varchar(50)')  = 'Container Load Plan'
	AND AM_Status < 255

OPTION(RECOMPILE)
