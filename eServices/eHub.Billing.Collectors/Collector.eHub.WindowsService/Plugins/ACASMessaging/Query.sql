SET NOCOUNT ON;

SELECT * FROM
(
	SELECT 
		Sender.CC_ID AS ClientID,
		CASE 
			WHEN DocumentName = 'HVLV ACAS Shipment Report' THEN 
				AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type="HVLVConsignment"]/*:Key)[1]', 'varchar(50)')
			WHEN DocumentName = 'Advanced Cargo Report' THEN 
				AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DataSource[*:Type="ForwardingShipment"]/*:Key)[1]', 'varchar(50)')
			ELSE NULL
		END AS ShipmentNumber,
		AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:Purpose)[1]', 'varchar(50)') AS Purpose,
		AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:WayBillNumber)[1]', 'varchar(50)') AS HAWBNumber,
		DocumentName,
		AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:PortOfFirstArrival)[1]', 'varchar(5)') AS PortOfFirstArrival,
		AM_InboxMessageTrackingID AS MessageTrackingID,
		AM_ReceivedFromSenderUTC,
		AM_ArchivedUTC
	FROM eHubArchiveMessage
	LEFT JOIN [dbo].[eHubClient] Sender ON Sender.CC_PK = AM_CC_SenderInbox
	LEFT JOIN [dbo].[eHubClient] Recipient ON Recipient.CC_PK = AM_CC_RecipientInbox
	LEFT JOIN [dbo].[eHubClient] RecipientOutbox ON RecipientOutbox.CC_PK = AM_CC_RecipientOutbox
	CROSS APPLY (SELECT AM_SenderMessageXML.value('(/*:UniversalInterchange/*:Body/*:UniversalShipment/*:Shipment/*:DataContext/*:DocumentaryOverride/*:DocumentName)[1]', 'varchar(50)') AS DocumentName) AS Doc
	WHERE @startUTC < AM_ArchivedUTC
		AND AM_ArchivedUTC <= @endUTC
		AND Recipient.CC_ID = 'ADVANCE_AIR_CARGO_REPORT'
		AND AM_Status < 255
		AND Sender.CC_ID != RecipientOutbox.CC_ID
)result
WHERE DocumentName = 'HVLV ACAS Shipment Report' OR DocumentName = 'Advanced Cargo Report'
OPTION(RECOMPILE)
